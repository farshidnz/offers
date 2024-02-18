using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Domain.Extensions;
using KellermanSoftware.CompareNetObjects;
using Mapster;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Offers.Services
{
    public interface ISyncOffersService
    {
        Task TrySyncOffersAsync();
        Task SyncOffersAsync();
    }

    public class SyncOffersService : ISyncOffersService
    {
        private readonly IShopGoSource _shopGoOffersSource;
        private readonly IOffersPersistenceContext _offersPersistenceContext;
        private readonly string _offerBackgroundImageDefault;
        private readonly string _customTrackingMerchantList;
        private readonly CompareLogic _compareLogic = new(new ComparisonConfig
        {
            CompareDateTimeOffsetWithOffsets = true,
            MembersToIgnore = new List<string> { "Id" }
        });

        public SyncOffersService(
            IShopGoSource shopGoOffersSource,
            IOffersPersistenceContext offersPersistenceContext,
            IConfiguration configuration)
        {
            _shopGoOffersSource = shopGoOffersSource;
            _offersPersistenceContext = offersPersistenceContext;
            _offerBackgroundImageDefault = configuration["Config:OfferBackgroundImageDefault"];
            _customTrackingMerchantList = configuration["Config:CustomTrackingMerchantList"];
        }

        public async Task TrySyncOffersAsync()
        {
            Log.Information("Offers sync started at {UtcNow}", DateTime.UtcNow);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await SyncOffersAsync();
            }
            catch (Exception ex)
            {
                Log.Error("Offers sync error - {ex}", ex);
            }
            finally
            {
                Log.Information("Offers sync completed in {Elapsed}", stopwatch.Elapsed);
            }
        }

        public async Task SyncOffersAsync()
        {
            var shopGoOffers = (await _shopGoOffersSource.GetOffers()).ToList();
            var shopGoTiers = (await _shopGoOffersSource.GetTiers())
                .ToList()
                .GroupBy(t => t.MerchantId)
                .ToDictionary(t => t.Key, t => t.ToList());
            var shopGoCategories = (await _shopGoOffersSource.GetMerchantCategories())
                .ToList()
                .GroupBy(c => c.MerchantId)
                .ToDictionary(c => c.Key, c => c.ToArray());
            var savedOffers = (await _offersPersistenceContext.GetAllOffers()).ToList();
            var savedOffersByOfferKey = savedOffers.ToDictionaryIgnoringDuplicates(x => (x.Client, x.PremiumClient, x.OfferId));
            var offersToBeDeleted = savedOffers.ToDictionary(x => x.Id);
            var syncProgress = new SyncProgress();

            await CheckStandardOffers(shopGoOffers, shopGoTiers, shopGoCategories, savedOffersByOfferKey, offersToBeDeleted, syncProgress);

            await CheckPremiumOffers(shopGoOffers, shopGoTiers, shopGoCategories, savedOffersByOfferKey, offersToBeDeleted, syncProgress);

            foreach (var offerToBeDeleted in offersToBeDeleted.Values)
            {
                CreateEvents(offerToBeDeleted, null)
                    .ForEach(e => offerToBeDeleted.RaiseEvent(e));
                await _offersPersistenceContext.DeleteOffer(offerToBeDeleted);
                syncProgress.Deleted++;
            }

            Log.Information("Offers sync inserted:{Inserted} updated:{Updated} deleted:{Deleted} unchanged:{Unchanged}",
                syncProgress.Inserted, syncProgress.Updated, syncProgress.Deleted, syncProgress.Unchanged);
        }

        private async Task CheckStandardOffers(List<ShopGoOffer> shopGoOffers, Dictionary<int, List<ShopGoTier>> shopGoTiers, Dictionary<int, ShopGoCategory[]> shopGoCategories, Dictionary<(Client Client, Client? PremiumClient, int OfferId), Offer> savedOffersByOfferKey, Dictionary<string, Offer> offersToBeDeleted, SyncProgress syncProgress)
        {
            foreach (var client in Clients.UsingThisMicroservice)
            {
                foreach (var sourcedOffer in shopGoOffers
                    .Where(o => o.ClientId == (int)client)
                    .Select(o => MapToOffer(o, shopGoCategories, shopGoTiers))
                    .Where(o => o.Merchant.ClientCommission > 0))
                {
                    await CheckAndUpdateOffer(savedOffersByOfferKey, offersToBeDeleted, syncProgress, sourcedOffer);
                }
            }
        }

        private async Task CheckPremiumOffers(List<ShopGoOffer> shopGoOffers, Dictionary<int, List<ShopGoTier>> shopGoTiers, Dictionary<int, ShopGoCategory[]> shopGoCategories, Dictionary<(Client Client, Client? PremiumClient, int OfferId), Offer> savedOffersByOfferKey, Dictionary<string, Offer> offersToBeDeleted, SyncProgress syncProgress)
        {
            foreach (var premiumRelationship in Clients.PremiumRelationships)
            {
                var offersThatRequireBaseMerchantLookup = new List<Offer>();

                var premiumOffers = shopGoOffers
                    .Where(o => (o.ClientId == (int)premiumRelationship.client || o.ClientId == (int)premiumRelationship.premiumClient)
                        && (!o.MerchantIsPremiumDisabled.HasValue || !o.MerchantIsPremiumDisabled.Value))
                    .GroupBy(o => o.OfferId)
                    .Select(g =>
                    {
                        var baseOffer = MapToOffer(g.FirstOrDefault(o => o.ClientId == (int)premiumRelationship.client), shopGoCategories, shopGoTiers);
                        var premiumOffer = MapToOffer(g.FirstOrDefault(o => o.ClientId == (int)premiumRelationship.premiumClient), shopGoCategories, shopGoTiers);
                        if (baseOffer == null)
                        {
                            var offer = premiumOffer;
                            offer.Premium = premiumOffer.Merchant.Adapt<OfferPremium>();
                            offer.Client = premiumRelationship.client;
                            offer.PremiumClient = premiumRelationship.premiumClient;
                            offersThatRequireBaseMerchantLookup.Add(offer);
                            return offer;
                        }
                        else
                        {
                            var offer = baseOffer;
                            if (premiumOffer != null)
                            {
                                offer.Premium = premiumOffer.Merchant.Adapt<OfferPremium>();
                            }
                            offer.PremiumClient = premiumRelationship.premiumClient;
                            return offer;
                        }
                    })
                    .ToList();

                await SetBaseMerchantRatesForPremiumOffers(offersThatRequireBaseMerchantLookup, premiumRelationship.client);

                foreach (var sourcedOffer in premiumOffers.Where(o => o.Merchant.ClientCommission > 0))
                {
                    await CheckAndUpdateOffer(savedOffersByOfferKey, offersToBeDeleted, syncProgress, sourcedOffer);
                }
            }
        }

        private async Task SetBaseMerchantRatesForPremiumOffers(List<Offer> offers, Client baseClient)
        {
            var merchantIds = offers.Select(o => o.Merchant.Id).ToList();
            var merchantRates = (await _shopGoOffersSource.GetMerchantBaseRatesById(merchantIds, (int)baseClient))
                .ToDictionary(m => m.MerchantId);

            foreach (var offer in offers)
            {
                if (merchantRates.TryGetValue(offer.Merchant.Id, out var baseMerchant))
                {
                    offer.Merchant.Commission = baseMerchant.Commission;
                    offer.Merchant.ClientComm = baseMerchant.ClientComm;
                    offer.Merchant.MemberComm = baseMerchant.MemberComm;
                    offer.Merchant.CommissionType = (CommissionType)baseMerchant.TierCommTypeId;
                    offer.Merchant.IsFlatRate = baseMerchant.IsFlatRate ?? false;
                    offer.Merchant.RewardType = (RewardType)baseMerchant.TierTypeId;
                }
            }
        }

        private async Task CheckAndUpdateOffer(
            Dictionary<(Client Client, Client? PremiumClient, int OfferId), Offer> savedOffersByOfferKey,
            Dictionary<string, Offer> offersToBeDeleted,
            SyncProgress syncProgress,
            Offer sourcedOffer)
        {
            if (savedOffersByOfferKey.TryGetValue(sourcedOffer.Key, out var savedOffer))
            {
                if (_compareLogic.Compare(sourcedOffer, savedOffer).AreEqual)
                {
                    syncProgress.Unchanged++;
                }
                else
                {
                    sourcedOffer.Id = savedOffer.Id;
                    CreateEvents(savedOffer, sourcedOffer)
                        .ForEach(e => sourcedOffer.RaiseEvent(e));
                    await _offersPersistenceContext.UpdateOffer(sourcedOffer);
                    syncProgress.Updated++;
                }

                offersToBeDeleted.Remove(savedOffer.Id);
            }
            else
            {
                CreateEvents(null, sourcedOffer)
                    .ForEach(e => sourcedOffer.RaiseEvent(e));
                await _offersPersistenceContext.InsertOffer(sourcedOffer);
                syncProgress.Inserted++;
            }
        }

        private Offer MapToOffer(
            ShopGoOffer shopGoOffer,
            Dictionary<int, ShopGoCategory[]> shopGoCategories,
            Dictionary<int, List<ShopGoTier>> shopGoTiers)
        {
            return shopGoOffer?.BuildAdapter()
                .AddParameters("offerBackgroundImageDefault", _offerBackgroundImageDefault)
                .AddParameters("customTrackingMerchantList", _customTrackingMerchantList)
                .AddParameters("merchantCategoriesLookup", shopGoCategories)
                .AddParameters("merchantTiers", shopGoTiers.TryGetValue(shopGoOffer.MerchantId, out var t) ? t.Where(t => t.ClientId == shopGoOffer.ClientId).ToList() : new List<ShopGoTier>())
                .AdaptToType<Offer>();
        }

        private List<DomainEvent> CreateEvents(Offer before, Offer after)
        {
            var events = new List<DomainEvent>();

            if (before?.PremiumClient != null || after?.PremiumClient != null)
            {
                return events;
            }

            if (after == null)
            {
                events.Add(before.Adapt<OfferDeleted>());
            }
            else
            {
                var beforeEvent = before.Adapt<OfferChanged>();
                var afterEvent = after.Adapt<OfferChanged>();
                if (beforeEvent != null && beforeEvent.Merchant.Id != afterEvent.Merchant.Id)
                {
                    events.Add(before.Adapt<OfferDeleted>());
                    events.Add(afterEvent);
                }
                else if (!_compareLogic.Compare(beforeEvent, afterEvent).AreEqual)
                {
                    events.Add(afterEvent);
                }
            }

            return events;
        }

        private class SyncProgress
        {
            public int Inserted { get; set; }
            public int Updated { get; set; }
            public int Deleted { get; set; }
            public int Unchanged { get; set; }
        }
    }
}
