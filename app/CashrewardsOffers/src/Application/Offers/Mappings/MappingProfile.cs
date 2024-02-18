using CashrewardsOffers.Application.Offers.Queries.GetEdmOffers.v1;
using CashrewardsOffers.Application.Offers.Queries.GetOffers.v1;
using CashrewardsOffers.Application.Offers.Services;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Domain.ValueObjects;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using ShopGoCategory = CashrewardsOffers.Application.Offers.Services.ShopGoCategory;

namespace CashrewardsOffers.Application.Offers.Mappings
{
    public class MappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ShopGoOffer, Offer>()
                .Map(dest => dest.Client, src => src.ClientId)
                .Map(dest => dest.Title, src => src.OfferTitle)
                .Map(dest => dest.Description, src => src.OfferDescription)
                .Map(dest => dest.EndDateTime, src => SydneyTime.ConvertShopGoTimeToDateTimeOffset(src.DateEnd))
                .Map(dest => dest.Terms, src => src.OfferTerms)
                .Map(dest => dest.OfferBackgroundImageUrl, src => string.IsNullOrWhiteSpace(src.OfferBackgroundImageUrl)
                    ? MapContext.Current.Parameters["offerBackgroundImageDefault"] as string
                    : src.OfferBackgroundImageUrl)
                .Map(dest => dest.OfferBadge, src => string.IsNullOrWhiteSpace(src.OfferBadgeCode) ? string.Empty : src.OfferBadgeCode)
                .Map(dest => dest.WasRate, src => string.IsNullOrWhiteSpace(src.OfferPastRate) ? null : src.OfferPastRate)
                .Map(dest => dest.Merchant, src => src);

            config.NewConfig<ShopGoOffer, OfferMerchant>()
                .Map(dest => dest.Id, src => src.MerchantId)
                .Map(dest => dest.Name, src => src.MerchantName)
                .Map(dest => dest.HyphenatedString, src => src.MerchantHyphenatedString)
                .Map(dest => dest.ClientProgramType, src => src.ClientProgramTypeId)
                .Map(dest => dest.CommissionType, src => src.TierCommTypeId)
                .Map(dest => dest.RewardType, src => src.TierTypeId)
                .Map(dest => dest.Description, src => src.MerchantShortDescription)
                .Map(dest => dest.LogoUrl, src => src.RegularImageUrl)
                .Map(dest => dest.IsCustomTracking, src => IsCustomTracking(MapContext.Current.Parameters["customTrackingMerchantList"] as string, src.MerchantId))
                .Map(dest => dest.MerchantBadge, src => string.IsNullOrWhiteSpace(src.MerchantBadgeCode) ? string.Empty : src.MerchantBadgeCode)
                .Map(dest => dest.IsMobileAppEnabled, src => src.IsMobileAppEnabled ?? true)
                .Map(dest => dest.IsPremiumDisabled, src => src.MerchantIsPremiumDisabled ?? false)
                .Map(dest => dest.Categories, src => GetMerchantCategories(MapContext.Current.Parameters["merchantCategoriesLookup"] as Dictionary<int, ShopGoCategory[]>, src.MerchantId))
                .Map(dest => dest.Tiers, src => (MapContext.Current.Parameters["merchantTiers"] as List<ShopGoTier>).Adapt<List<OfferMerchantTier>>());

            config.NewConfig<Offer, OfferPremium>()
                .Map(dest => dest.Commission, src => src.Merchant.Commission)
                .Map(dest => dest.ClientComm, src => src.Merchant.ClientComm)
                .Map(dest => dest.MemberComm, src => src.Merchant.MemberComm)
                .Map(dest => dest.ClientProgramType, src => src.Merchant.ClientProgramType)
                .Map(dest => dest.CommissionType, src => src.Merchant.CommissionType)
                .Map(dest => dest.Rate, src => src.Merchant.Rate)
                .Map(dest => dest.IsFlatRate, src => src.Merchant.IsFlatRate)
                .Map(dest => dest.RewardName, src => src.Merchant.RewardName)
                .Map(dest => dest.RewardType, src => src.Merchant.RewardType);

            config.NewConfig<Offer, OfferInfo>()
                .Map(dest => dest.Id, src => src.OfferId)
                .Map(dest => dest.EndDateTime, src => src.EndDateTime.DateTime)
                .Map(dest => dest.OfferPastRate, src => (string)null)
                .Map(dest => dest.ClientCommissionString, src => src.Merchant.ClientCommissionString)
                .Map(dest => dest.RegularImageUrl, src => src.Merchant.LogoUrl)
                .Map(dest => dest.OfferBadge, src => (MapContext.Current.Parameters["isFeatured"] as bool? ?? false) && src.OfferBadge == BadgeCodes.AnzPremiumOffers ? string.Empty : src.OfferBadge);

            config.NewConfig<OfferMerchant, MerchantInfo>()
                .Map(dest => dest.Commission, src => src.ClientCommission)
                .Map(dest => dest.CommissionType, src => src.CommissionType == CommissionType.Dollar ? "dollar" : "percent")
                .Map(dest => dest.RewardType, src => src.RewardType == RewardType.Discount || src.RewardType == RewardType.MaxDiscount ? "Savings" : "Cashback");

            config.NewConfig<OfferPremium, PremiumInfo>()
                .Map(dest => dest.Commission, src => src.ClientCommission);

            config.NewConfig<Offer, EdmOfferInfo>()
                .Map(dest => dest.EndDateTime, src => src.EndDateTime.DateTime)
                .Map(dest => dest.ClientCommissionString, src => src.Merchant.ClientCommissionString)
                .Map(dest => dest.OfferHyphenatedString, src => src.HyphenatedString)
                .Map(dest => dest.OfferEndString, src => GetEdmEndingString(src))
                .Map(dest => dest.Terms, src => GetTierZeroSpecialTerms(src));

            config.NewConfig<OfferPremium, EdmOfferPremiumInfo>()
                .Map(dest => dest.ClientCommissionString, src => src.ClientCommissionString);

            config.NewConfig<ShopGoTier, OfferMerchantTier>()
                .Map(dest => dest.CommissionType, src => src.TierCommTypeId);
        }

        private string GetTierZeroSpecialTerms(Offer src) =>
            (src.Merchant.Tiers != null && src.Merchant.Tiers.Count > 0)
                ? src.Merchant.Tiers[0].TierSpecialTerms
                : null;

        private static bool IsCustomTracking(string customTrackingMerchantList, int merchantId) =>
            customTrackingMerchantList
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => Convert.ToInt32(n))
                .Contains(merchantId);

        private static ShopGoCategory[] GetMerchantCategories(Dictionary<int, ShopGoCategory[]> merchantCategoriesLookup, int merchantId) =>
            merchantCategoriesLookup.TryGetValue(merchantId, out ShopGoCategory[] categories) ? categories : Array.Empty<ShopGoCategory>();

        private static string GetEdmEndingString(Offer offer)
        {
            var now = DateTimeOffset.Now;
            if (MapContext.Current != null)
            {
                now = MapContext.Current.Parameters.TryGetValue("now", out var n) ? (DateTimeOffset)n : DateTimeOffset.Now;
            }

            return offer.GetEdmEndDateString(now);
        }
    }
}
