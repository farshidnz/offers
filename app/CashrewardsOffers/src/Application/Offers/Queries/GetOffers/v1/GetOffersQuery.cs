using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Services;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Extensions;
using Mapster;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Offers.Queries.GetOffers.v1
{
    public class GetOffersQuery
    {
        public int ClientId { get; set; }
        public int? PremiumClientId { get; set; }
        public int? CategoryId { get; set; }
        public bool IsMobile { get; set; }
        public bool? IsFeatured { get; set; }
    }

    public class GetOffersResponse
    {
        public int TotalCount { get; set; }
        public int Count { get; set; }
        public OfferInfo[] Data { get; set; }
    }

    public class OfferInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CouponCode { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Description { get; set; }
        public string HyphenatedString { get; set; }
        public bool IsFeatured { get; set; }
        public string Terms { get; set; }
        public int MerchantId { get; set; }
        public string MerchantLogoUrl { get; set; }
        public string OfferBackgroundImageUrl { get; set; }
        public string OfferBadge { get; set; }
        public bool IsCashbackIncreased { get; set; }
        public bool IsPremiumFeature { get; set; }
        public string WasRate { get; set; }
        public MerchantInfo Merchant { get; set; }
        public PremiumInfo Premium { get; set; }
        public string ClientCommissionString { get; set; }
        public string RegularImageUrl { get; set; }
        public string OfferPastRate { get; set; } // not used?
        public string MerchantHyphenatedString { get; set; }
        public bool IsPersonalised { get; set; }
    }

    public class MerchantInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HyphenatedString { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public decimal Commission { get; set; }
        public bool IsFlatRate { get; set; }
        public string CommissionType { get; set; }
        public int? TrackingTimeMin { get; set; } // not used?
        public int? TrackingTimeMax { get; set; } // not used?
        public int? ApprovalTime { get; set; } // not used?
        public string SpecialTerms { get; set; } // not used?
        public string CashbackGuidelines { get; set; } // not used?
        public int? OfferCount { get; set; }
        public string RewardType { get; set; }
        public string MobileTrackingType { get; set; } // not used?
        public bool IsCustomTracking { get; set; }
        public string BackgroundImageUrl { get; set; } // not used?
        public string MerchantBadge { get; set; }
        public bool DesktopEnabled { get; set; } // not used?
        public string MobileTrackingNetwork { get; set; } // not used?
    }

    public class PremiumInfo
    {
        public decimal Commission { get; set; }
        public bool IsFlatRate { get; set; }
        public string ClientCommissionString { get; set; }
    }

    public class GetOffersQueryConsumer : IConsumer<GetOffersQuery>
    {
        private readonly IOffersPersistenceContext _offersPersistenceContext;
        private readonly IFeaturePersistenceContext _featurePersistenceContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnleashService _unleash;
        private readonly IShopGoSource _shopGoSource;
        private readonly int _mobileSpecificNetworkId; // 1000061 -- Button network

        public GetOffersQueryConsumer(
            IOffersPersistenceContext offersPersistenceContext,
            IFeaturePersistenceContext featurePersistenceContext,
            IConfiguration configuration,
            ICurrentUserService currentUserService,
            IUnleashService unleash,
            IShopGoSource shopGoSource)
        {
            _offersPersistenceContext = offersPersistenceContext;
            _featurePersistenceContext = featurePersistenceContext;
            _currentUserService = currentUserService;
            _unleash = unleash;
            _shopGoSource = shopGoSource;
            int.TryParse(configuration["Config:MobileSpecificNetworkId"], out _mobileSpecificNetworkId);
        }

        public async Task Consume(ConsumeContext<GetOffersQuery> context)
        {
            Log.Information($"Query: {JsonConvert.SerializeObject(context.Message)}");
            var clientId = context.Message.ClientId;
            var premiumClientId = context.Message.PremiumClientId;
            var isfeatured = context.Message.IsFeatured;
            var isMobile = context.Message.IsMobile;
            var excludedNetworkIds = isMobile ? Array.Empty<int>() : new int[] { _mobileSpecificNetworkId };
            var categoryId = context.Message.CategoryId;

            bool excludePausedMerchants = _unleash.IsEnabled(FeatureToggle.MerchantPause);

            var offers = await _offersPersistenceContext.GetOffers(clientId, premiumClientId, isfeatured, isMobile, excludedNetworkIds, categoryId, excludePausedMerchants);
            var orderedOffers = new OrderedOffers(offers);

            if (_unleash.IsEnabled(FeatureToggle.Exp1) || _unleash.IsEnabled(FeatureToggle.Exp2) || _unleash.IsEnabled(FeatureToggle.Exp3))
            {
                await ReorderOffersForExperiments(orderedOffers);
            }

            var data = orderedOffers.Offers.BuildAdapter().AddParameters("isFeatured", isfeatured).AdaptToType<OfferInfo[]>() ?? Array.Empty<OfferInfo>();

            await context.RespondAsync(new GetOffersResponse
            {
                TotalCount = data.Length,
                Count = data.Length,
                Data = data
            });
        }

        private async Task ReorderOffersForExperiments(OrderedOffers orderedOffers)
        {
            var userExperimentFeature = (await _featurePersistenceContext.Get(_currentUserService.UserId)) ?? string.Empty;
            if (_unleash.IsEnabled(userExperimentFeature))
            {
                var favourites = (await _shopGoSource.GetFavourites(_currentUserService.UserId)).ToList().Adapt<List<FavouriteMerchant>>();
                orderedOffers.ReorderOffersForExperiments(userExperimentFeature, favourites);
            }
        }
    }
}
