using System.Collections.Generic;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Enums;

namespace CashrewardsOffers.Domain.Events
{
    public class MerchantEventBase : DomainEvent
    {
        public int MerchantId { get; set; }
        public Client Client { get; set; }
        public string Name { get; set; }
        public string HyphenatedString { get; set; }
        public string ClientCommissionString { get; set; }
        public string PremiumClientCommissionString { get; set; }
        public string LogoUrl { get; set; }
        public int NetworkId { get; set; }
        public bool IsHomePageFeatured { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPopular { get; set; }
        public bool MobileEnabled { get; set; }
        public bool MobileAppEnabled { get; set; }
        public bool IsPremiumDisabled { get; set; }
        public bool IsPaused { get; set; }
        public int PopularMerchantRankingForBrowser { get; set; }
        public int PopularMerchantRankingForMobile { get; set; }
        public string BasicTerms { get; set; }
        public string ExtentedTerms { get; set; }
        public List<MerchantChangedCategoryItem> Categories { get; set; }
    }

    public class MerchantChangedCategoryItem
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }
}
