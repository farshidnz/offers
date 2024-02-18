using System;

namespace CashrewardsOffers.Application.Offers.Services
{
    public class ShopGoOffer
    {
        public int OfferId { get; set; }
        public int ClientId { get; set; }
        public int MerchantId { get; set; }
        public string CouponCode { get; set; }
        public string OfferTitle { get; set; }
        public string OfferDescription { get; set; }
        public string HyphenatedString { get; set; }
        public DateTime DateEnd { get; set; }
        public string MerchantName { get; set; }
        public string RegularImageUrl { get; set; }
        public int? OfferCount { get; set; }
        public int ClientProgramTypeId { get; set; }
        public int TierCommTypeId { get; set; }
        public int TierTypeId { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public string RewardName { get; set; }
        public string MerchantShortDescription { get; set; }
        public string MerchantHyphenatedString { get; set; }
        public string OfferTerms { get; set; }
        public bool? IsFlatRate { get; set; }
        public decimal Rate { get; set; }
        public int Ranking { get; set; }
        public string OfferBackgroundImageUrl { get; set; }
        public string OfferBadgeCode { get; set; }
        public string MerchantBadgeCode { get; set; }
        public string OfferPastRate { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsCategoryFeatured { get; set; }
        public bool IsCashbackIncreased { get; set; }
        public bool IsPremiumFeature { get; set; }
        public bool? MerchantIsPremiumDisabled { get; set; }
        public bool? MobileEnabled { get; set; }
        public bool? IsMobileAppEnabled { get; set; }
        public bool IsMerchantPaused { get; set; }
        public int NetworkId { get; set; }
        public string BasicTerms { get; set; }
        public string ExtentedTerms { get; set; }
        public bool IsPopular { get; set; }
        public bool? IsHomePageFeatured { get; set; }

    }
}
