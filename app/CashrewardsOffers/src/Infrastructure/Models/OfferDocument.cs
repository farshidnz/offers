using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class OfferDocument : IDocument
    {
        public ObjectId _id { get; set; }
        public int OfferId { get; set; }
        public int Client { get; set; }
        public int? PremiumClient { get; set; }
        public string CouponCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string HyphenatedString { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public string Terms { get; set; }
        public string OfferBackgroundImageUrl { get; set; }
        public string OfferBadge { get; set; }
        public string WasRate { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsCategoryFeatured { get; set; }
        public bool IsCashbackIncreased { get; set; }
        public bool IsPremiumFeature { get; set; }
        public bool? IsMerchantPaused { get; set; }
        public int Ranking { get; set; }
        public OfferMerchantDocument Merchant { get; set; }
        public PremiumOfferDocument Premium { get; set; }
    }

    public class OfferMerchantDocument
    {
        public int MerchantId { get; set; }
        public string Name { get; set; }
        public string HyphenatedString { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public int? OfferCount { get; set; }
        public int RewardType { get; set; }
        public bool IsCustomTracking { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string MerchantBadge { get; set; }
        public int ClientProgramType { get; set; }
        public int CommissionType { get; set; }
        public decimal Rate { get; set; }
        public bool IsFlatRate { get; set; }
        public string RewardName { get; set; }
        public bool MobileEnabled { get; set; }
        public bool? IsMobileAppEnabled { get; set; }
        public int NetworkId { get; set; }
        public bool IsPremiumDisabled { get; set; }
        public string BasicTerms { get; set; }
        public string ExtentedTerms { get; set; }
        public bool? IsHomePageFeatured { get; set; }
        public int[] Categories { get; set; }
        public List<OfferMerchantCategoryDocument> CategoryObjects { get; set; }
        public List<OfferMerchantTierDocument> Tiers { get; set; }
    }

    public class OfferMerchantCategoryDocument
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class OfferMerchantTierDocument
    {
        public string TierName { get; set; }
        public int CommissionType { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public string TierSpecialTerms { get; set; }
    }

    public class PremiumOfferDocument
    {
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public int ClientProgramType { get; set; }
        public int CommissionType { get; set; }
        public decimal Rate { get; set; }
        public bool IsFlatRate { get; set; }
        public int RewardType { get; set; }
        public string RewardName { get; set; }
    }
}
