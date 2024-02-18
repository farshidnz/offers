using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Services;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using Mapster;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unleash;

namespace CashrewardsOffers.Application.Offers.Queries.GetEdmOffers.v1
{
    public class GetEdmOffersQuery
    {
        public string NewMemberId { get; set; }
        public int? EDMCampaignId { get; set; }
    }

    public class GetEdmOffersResponse
    {
        public List<EdmOfferInfo> Offers { get; set; }
    }

    public class EdmOfferInfo
    {
        public string Title { get; set; }
        public string ClientCommissionString { get; set; }
        public string WasRate { get; set; }
        public string OfferEndString { get; set; }
        public string OfferBackgroundImageUrl { get; set; }
        public string MerchantLogoUrl { get; set; }
        public DateTime EndDateTime { get; set; }
        public string MerchantHyphenatedString { get; set; }
        public string OfferHyphenatedString { get; set; }
        public int MerchantId { get; set; }
        public int OfferId { get; set; }
        public string Terms { get; set; }
        public EdmOfferPremiumInfo Premium { get; set; }
    }

    public class EdmOfferPremiumInfo
    {
        public string ClientCommissionString { get; set; }
    }

    public class GetEdmOffersQueryConsumer : IConsumer<GetEdmOffersQuery>
    {
        private readonly IOffersPersistenceContext _offersPersistenceContext;
        private readonly IFeaturePersistenceContext _featurePersistenceContext;
        private readonly IUnleashService _unleash;
        private readonly IShopGoSource _shopGoSource;
        private readonly int _mobileSpecificNetworkId; // 1000061 -- Button network

        public GetEdmOffersQueryConsumer(
            IOffersPersistenceContext offersPersistenceContext,
            IFeaturePersistenceContext featurePersistenceContext,
            IConfiguration configuration,
            IUnleashService unleash,
            IShopGoSource shopGoSource)
        {
            _offersPersistenceContext = offersPersistenceContext;
            _featurePersistenceContext = featurePersistenceContext;
            _unleash = unleash;
            _shopGoSource = shopGoSource;
            int.TryParse(configuration["Config:MobileSpecificNetworkId"], out _mobileSpecificNetworkId);
        }

        public async Task Consume(ConsumeContext<GetEdmOffersQuery> context)
        {
            Log.Information($"Query: {JsonConvert.SerializeObject(context.Message)}");

            await context.RespondAsync(new GetEdmOffersResponse
            {
                Offers = (await GetEdmOffers(context)).Adapt<List<EdmOfferInfo>>()
            });
        }

        private async Task<List<Offer>> GetEdmOffers(ConsumeContext<GetEdmOffersQuery> context)
        {
            var clientId = (int)Client.Cashrewards;
            var person = context.Message.NewMemberId == null
                ? null
                : await _shopGoSource.GetPersonFromNewMemberId(context.Message.NewMemberId);
            var premiumClientId = person?.PremiumStatus == 1 ? (int ?)Client.Anz : null;
            var isfeatured = true;
            var isMobile = false;
            var excludedNetworkIds = isMobile ? Array.Empty<int>() : new int[] { _mobileSpecificNetworkId };

            var allOffers = await _offersPersistenceContext.GetOffers(clientId, premiumClientId, isfeatured, isMobile, excludedNetworkIds, null);
            var offers = context.Message.EDMCampaignId.HasValue 
                    ? await ExcludeEdmCampaignItems(context.Message.EDMCampaignId.Value, allOffers)
                    : allOffers;
            
            
            var orderedOffers = new OrderedOffers(offers);

            if (person?.CognitoId != null && (_unleash.IsEnabled(FeatureToggle.Exp1) || _unleash.IsEnabled(FeatureToggle.Exp2) || _unleash.IsEnabled(FeatureToggle.Exp3)))
            {
                await ReorderOffersForExperiments(orderedOffers, person.CognitoId.ToString());
            }

            return orderedOffers.Offers;
        }

        private async Task ReorderOffersForExperiments(OrderedOffers orderedOffers, string cognitoId)
        {
            var userExperimentFeature = await _featurePersistenceContext.Get(cognitoId) ?? string.Empty;
            if (_unleash.IsEnabled(userExperimentFeature))
            {
                var favourites = (await _shopGoSource.GetFavourites(cognitoId)).ToList().Adapt<List<FavouriteMerchant>>();
                orderedOffers.ReorderOffersForExperiments(userExperimentFeature, favourites);
            }
        }

        private async Task<List<Offer>> ExcludeEdmCampaignItems(int campaignId, List<Offer> allOffers)
        {
            var offersToExclude = (await _shopGoSource.GetEdmCampaignItems(campaignId)).ToList()
                .Where(e => e.Type == "O")
                .Select(e => e.OfferId);

            return allOffers.Where(o => !offersToExclude.Contains(o.OfferId))
                .ToList();
        }
    }
}
