using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Enums;
using System;

namespace CashrewardsOffers.Domain.Entities
{
    public class Merchant : DomainEntity
    {
        public string Id { get; set; }
        public (Client, Client?, int) Key => (Client, PremiumClient, MerchantId);
        public int MerchantId { get; set; }
        public Client Client { get; set; }
        public Client? PremiumClient { get; set; }
        public string Name { get; set; }
        public string HyphenatedString { get; set; }
        public string LogoUrl { get; set; }
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
        public int NetworkId { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsHomePageFeatured { get; set; }
        public bool IsPopular { get; set; }
        public bool MobileEnabled { get; set; }
        public bool MobileAppEnabled { get; set; }
        public bool IsPremiumDisabled { get; set; }
        public bool IsPaused { get; set; }
        public MerchantPremium Premium { get; set; }
        public int PopularMerchantRankingForBrowser { get; set; }
        public int PopularMerchantRankingForMobile { get; set; }
        public string BasicTerms { get; set; }
        public string ExtentedTerms { get; set; }
        public MerchantCategory[] Categories { get; set; }
    }

    public class MerchantPremium
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

    public class MerchantCategory
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }
}
