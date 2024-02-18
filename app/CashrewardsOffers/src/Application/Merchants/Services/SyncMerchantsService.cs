using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Merchants.Models;
using CashrewardsOffers.Application.Merchants.Services;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Domain.Extensions;
using KellermanSoftware.CompareNetObjects;
using Mapster;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Offers.Services
{
    public interface ISyncMerchantsService
    {
        Task TrySyncMerchantsAsync();
        Task SyncMerchantsAsync();
    }

    public class SyncMerchantsService : ISyncMerchantsService
    {
        private readonly IShopGoSource _shopGoSource;
        private readonly IMerchantsPersistenceContext _merchantsPersistenceContext;
        private readonly IPopularMerchantRankingService _popularMerchantRankingService;
        private readonly CompareLogic _compareLogic = new(new ComparisonConfig { MembersToIgnore = new List<string> { "Id" } });
        private Dictionary<int, List<ShopGoCategory>> MerchantCategories = new Dictionary<int, List<ShopGoCategory>>();


        public SyncMerchantsService(
            IShopGoSource shopGoSource,
            IMerchantsPersistenceContext merchantsPersistenceContext,
            IPopularMerchantRankingService popularMerchantRankingService)
        {
            _shopGoSource = shopGoSource;
            _merchantsPersistenceContext = merchantsPersistenceContext;
            _popularMerchantRankingService = popularMerchantRankingService;
        }

        public async Task TrySyncMerchantsAsync()
        {
            Log.Information("Merchants sync started at {UtcNow}", DateTime.UtcNow);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await SyncMerchantsAsync();
            }
            catch (Exception ex)
            {
                Log.Error("Merchants sync error - {ex}", ex);
            }
            finally
            {
                Log.Information("Merchants sync completed in {Elapsed}", stopwatch.Elapsed);
            }
        }

        public async Task SyncMerchantsAsync()
        {
            await _popularMerchantRankingService.LoadRankings();

            this.MerchantCategories = (await _shopGoSource.GetMerchantCategories())
                .GroupBy(m => m.MerchantId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var shopGoMerchants = (await _shopGoSource.GetMerchants()).ToList();
            var savedMerchants = (await _merchantsPersistenceContext.GetAllMerchants()).ToList();
            var savedMerchantsByKey = savedMerchants.ToDictionaryIgnoringDuplicates(x => (x.Client, x.PremiumClient, x.MerchantId));
            var merchantsToBeDeleted = savedMerchants.ToDictionary(x => x.Id);
            var syncProgress = new SyncProgress();

            await CheckStandardMerchants(shopGoMerchants, savedMerchantsByKey, merchantsToBeDeleted, syncProgress);

            await CheckPremiumMerchants(shopGoMerchants, savedMerchantsByKey, merchantsToBeDeleted, syncProgress);

            foreach (var merchantToBeDeleted in merchantsToBeDeleted.Values)
            {
                CreateEvents(merchantToBeDeleted, null)
                    .ForEach(e => merchantToBeDeleted.RaiseEvent(e));
                await _merchantsPersistenceContext.DeleteMerchant(merchantToBeDeleted);
                syncProgress.Deleted++;
            }

            Log.Information("Merchants sync inserted:{Inserted} updated:{Updated} deleted:{Deleted} unchanged:{Unchanged}",
                syncProgress.Inserted, syncProgress.Updated, syncProgress.Deleted, syncProgress.Unchanged);
        }

        private async Task CheckStandardMerchants(List<ShopGoMerchant> shopGoMerchants, Dictionary<(Client Client, Client? PremiumClient, int MerchantId), Merchant> savedMerchantsByKey, Dictionary<string, Merchant> merchantsToBeDeleted, SyncProgress syncProgress)
        {
            foreach (var client in Clients.UsingThisMicroservice)
            {
                foreach (var sourcedMerchant in shopGoMerchants
                    .Where(m => m.ClientId == (int)client)
                    .Select(MapToMerchant)
                    .Where(m => m.ClientCommission > 0))
                {
                    await CheckAndUpdateMerchant(savedMerchantsByKey, merchantsToBeDeleted, syncProgress, sourcedMerchant);
                }
            }
        }

        private async Task CheckPremiumMerchants(List<ShopGoMerchant> shopGoMerchants, Dictionary<(Client Client, Client? PremiumClient, int MerchantId), Merchant> savedMerchantsByKey, Dictionary<string, Merchant> merchantsToBeDeleted, SyncProgress syncProgress)
        {
            foreach (var premiumRelationship in Clients.PremiumRelationships)
            {
                var merchantsThatRequireBaseMerchantLookup = new List<Merchant>();

                var premiumMerchants = shopGoMerchants
                    .Where(m => (m.ClientId == (int)premiumRelationship.client || m.ClientId == (int)premiumRelationship.premiumClient)
                        && (!m.IsPremiumDisabled.HasValue || !m.IsPremiumDisabled.Value))
                    .GroupBy(m => m.MerchantId)
                    .Select(g =>
                    {
                        var baseMerchant = MapToMerchant(g.FirstOrDefault(m => m.ClientId == (int)premiumRelationship.client));
                        var premiumMerchant = MapToMerchant(g.FirstOrDefault(m => m.ClientId == (int)premiumRelationship.premiumClient));
                        if (baseMerchant == null)
                        {
                            var merchant = premiumMerchant;
                            merchant.Premium = premiumMerchant.Adapt<MerchantPremium>();
                            merchant.Client = premiumRelationship.client;
                            merchant.PremiumClient = premiumRelationship.premiumClient;
                            merchantsThatRequireBaseMerchantLookup.Add(merchant);
                            return merchant;
                        }
                        else
                        {
                            var merchant = baseMerchant;
                            if (premiumMerchant != null)
                            {
                                merchant.Premium = premiumMerchant.Adapt<MerchantPremium>();
                            }
                            merchant.PremiumClient = premiumRelationship.premiumClient;
                            return merchant;
                        }
                    })
                    .ToList();

                await SetBaseMerchantRatesForPremiumMerchants(merchantsThatRequireBaseMerchantLookup, premiumRelationship.client);

                foreach (var sourcedOffer in premiumMerchants.Where(m => m.ClientCommission > 0))
                {
                    await CheckAndUpdateMerchant(savedMerchantsByKey, merchantsToBeDeleted, syncProgress, sourcedOffer);
                }
            }
        }

        private async Task SetBaseMerchantRatesForPremiumMerchants(List<Merchant> merchants, Client baseClient)
        {
            var merchantIds = merchants.Select(m => m.MerchantId).ToList();
            var merchantRates = (await _shopGoSource.GetMerchantBaseRatesById(merchantIds, (int)baseClient))
                .ToDictionary(m => m.MerchantId);

            foreach (var merchant in merchants)
            {
                if (merchantRates.TryGetValue(merchant.MerchantId, out var baseMerchant))
                {
                    merchant.Commission = baseMerchant.Commission;
                    merchant.ClientComm = baseMerchant.ClientComm;
                    merchant.MemberComm = baseMerchant.MemberComm;
                    merchant.CommissionType = (CommissionType)baseMerchant.TierCommTypeId;
                    merchant.IsFlatRate = baseMerchant.IsFlatRate ?? false;
                    merchant.RewardType = (RewardType)baseMerchant.TierTypeId;
                }
            }
        }

        private async Task CheckAndUpdateMerchant(
            Dictionary<(Client Client, Client? PremiumClient, int MerchantId), Merchant> savedMerchantsByKey,
            Dictionary<string, Merchant> merchantsToBeDeleted,
            SyncProgress syncProgress,
            Merchant sourcedMerchant)
        {
            if (savedMerchantsByKey.TryGetValue(sourcedMerchant.Key, out var savedMerchant))
            {
                if (_compareLogic.Compare(sourcedMerchant, savedMerchant).AreEqual)
                {
                    syncProgress.Unchanged++;
                }
                else
                {
                    sourcedMerchant.Id = savedMerchant.Id;
                    CreateEvents(savedMerchant, sourcedMerchant)
                        .ForEach(e => sourcedMerchant.RaiseEvent(e));
                    await _merchantsPersistenceContext.UpdateMerchant(sourcedMerchant);
                    syncProgress.Updated++;
                }

                merchantsToBeDeleted.Remove(savedMerchant.Id);
            }
            else
            {
                sourcedMerchant.Categories = GetCategoryList(sourcedMerchant.MerchantId);
                CreateEvents(null, sourcedMerchant)
                    .ForEach(e => sourcedMerchant.RaiseEvent(e));
                await _merchantsPersistenceContext.InsertMerchant(sourcedMerchant);
                syncProgress.Inserted++;
            }
        }

        private Merchant MapToMerchant(ShopGoMerchant shopGoMerchant)
        {
            var mappedMerchant = shopGoMerchant?.BuildAdapter().AdaptToType<Merchant>();

            if (mappedMerchant != null)
            {
                mappedMerchant.Categories = GetCategoryList(mappedMerchant.MerchantId);
                mappedMerchant.PopularMerchantRankingForBrowser = _popularMerchantRankingService.GetBrowserRanking(mappedMerchant.MerchantId);
                mappedMerchant.PopularMerchantRankingForMobile = _popularMerchantRankingService.GetMobileRanking(mappedMerchant.MerchantId);
            }

            return mappedMerchant;
        }

        private List<DomainEvent> CreateEvents(Merchant before, Merchant after)
        {
            var events = new List<DomainEvent>();

            if (before?.PremiumClient != null || after?.PremiumClient != null)
            {
                return events;
            }

            if (after == null)
            {
                events.Add(before.Adapt<MerchantDeleted>());
            }
            else
            {
                var beforeEvent = before.Adapt<MerchantChanged>();
                var afterEvent = after.Adapt<MerchantChanged>();
                if (!_compareLogic.Compare(beforeEvent, afterEvent).AreEqual)
                {
                    events.Add(afterEvent);
                }
            }

            return events;
        }

        private MerchantCategory[] GetCategoryList(int merchantId)
        {
            return this.MerchantCategories.TryGetValue(merchantId, out var categories) ?
                categories.Adapt<MerchantCategory[]>() :
                Array.Empty<MerchantCategory>();
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
