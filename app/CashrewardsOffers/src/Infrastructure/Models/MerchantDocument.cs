using System.Collections.Generic;
using MongoDB.Bson;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class MerchantDocument : IDocument
    {
        public ObjectId _id { get; set; }
        public int MerchantId { get; set; }
        public int Client { get; set; }
        public int? PremiumClient { get; set; }
        public string Name { get; set; }
        public string HyphenatedString { get; set; }
        public string LogoUrl { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public int ClientProgramType { get; set; }
        public int CommissionType { get; set; }
        public decimal Rate { get; set; }
        public bool IsFlatRate { get; set; }
        public int RewardType { get; set; }
        public string RewardName { get; set; }
        public int NetworkId { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsHomePageFeatured { get; set; }
        public bool IsPopular { get; set; }
        public bool MobileEnabled { get; set; }
        public bool MobileAppEnabled { get; set; }
        public bool IsPremiumDisabled { get; set; }
        public bool IsPaused { get; set; }
        public PremiumMerchantDocument Premium { get; set; }
        public int PopularMerchantRankingForBrowser { get; set; }
        public int PopularMerchantRankingForMobile { get; set; }
        public string BasicTerms { get; set; }
        public string ExtentedTerms { get; set; }
        public List<MerchantCategory> Categories { get; set; }
    }

    public class PremiumMerchantDocument
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

    public class MerchantCategory
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }
}
