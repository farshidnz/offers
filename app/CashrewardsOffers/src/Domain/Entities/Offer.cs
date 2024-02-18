using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.Domain.Entities
{
    public class Offer : DomainEntity
    {
        public string Id { get; set; }
        public (Client, Client?, int) Key => (Client, PremiumClient, OfferId);
        public int OfferId { get; set; }
        public Client Client { get; set; }
        public Client? PremiumClient { get; set; }
        public string CouponCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string HyphenatedString { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public string GetEdmEndDateString(DateTimeOffset now) => new OfferEndingString(EndDateTime, now).EdmEndingString;
        public string Terms { get; set; }
        public string OfferBackgroundImageUrl { get; set; }
        public string OfferBadge { get; set; }
        public string WasRate { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsCategoryFeatured { get; set; }
        public bool IsCashbackIncreased { get; set; }
        public bool IsPremiumFeature { get; set; }
        public bool IsMerchantPaused { get; set; }
        public int Ranking { get; set; }
        public bool IsPersonalised { get; set; }
        public OfferMerchant Merchant { get; set; } = new();
        public OfferPremium Premium { get; set; }
    }

    public class OfferMerchant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HyphenatedString { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public decimal ClientCommission => Math.Round(Commission * (ClientComm / 100) * (MemberComm / 100), 2);
        public int? OfferCount { get; set; }
        public RewardType RewardType { get; set; }
        public bool IsCustomTracking { get; set; }
        public string MerchantBadge { get; set; }
        public ClientProgramType ClientProgramType { get; set; }
        public CommissionType CommissionType { get; set; }
        public decimal Rate { get; set; }
        public bool IsFlatRate { get; set; }
        public string RewardName { get; set; }
        public string ClientCommissionString => new CommissionString(ClientProgramType, CommissionType, ClientCommission, Rate, IsFlatRate, RewardType, RewardName).FormattedString;
        public bool MobileEnabled { get; set; }
        public bool IsMobileAppEnabled { get; set; }
        public int NetworkId { get; set; }
        public bool IsPremiumDisabled { get; set; }
        public string BasicTerms { get; set; }
        public string ExtentedTerms { get; set; }
        public bool IsHomePageFeatured { get; set; }
        public OfferMerchantCategory[] Categories { get; set; }
        public List<OfferMerchantTier> Tiers { get; set; }
    }

    public class OfferMerchantCategory
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class OfferMerchantTier
    {
        public string TierName { get; set; }
        public CommissionType CommissionType { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public decimal ClientCommission => Math.Round(Commission * (ClientComm / 100) * (MemberComm / 100), 2);
        public string ClientCommissionString => new CommissionString(ClientProgramType.CashProgram, CommissionType, ClientCommission, 0, true, RewardType.Cashback1, "cashback").FormattedString;
        public string TierSpecialTerms { get; set; }
    }

    public class OfferPremium
    {
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public decimal ClientCommission => Math.Round(Commission * (ClientComm / 100) * (MemberComm / 100), 2);
        public ClientProgramType ClientProgramType { get; set; }
        public CommissionType CommissionType { get; set; }
        public decimal Rate { get; set; }
        public bool IsFlatRate { get; set; }
        public RewardType RewardType { get; set; }
        public string RewardName { get; set; }
        public string ClientCommissionString => new CommissionString(ClientProgramType, CommissionType, ClientCommission, Rate, IsFlatRate, RewardType, RewardName).FormattedString;
    }
}
