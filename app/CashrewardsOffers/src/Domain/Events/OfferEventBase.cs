using System.Collections.Generic;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Enums;

namespace CashrewardsOffers.Domain.Events
{
    public class OfferEventBase : DomainEvent
    {
        public int OfferId { get; set; }
        public Client Client { get; set; }
        public string HyphenatedString { get; set; }
        public OfferMerchantChanged Merchant { get; set; } = new OfferMerchantChanged();
        public bool IsFeatured { get; set; }
        public bool IsMerchantPaused { get; set; }
        public string Title { get; set; }
        public string Terms { get; set; }
        public string WasRate { get; set; }
        public string Link { get; set; }
        public long EndDateTime { get; set; }
        public int Ranking { get; set; }
    }

    public class OfferMerchantChanged
    {
        public int Id { get; set; }
        public int NetworkId { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string ClientCommissionString { get; set; }
        public string HyphenatedString { get; set; }
        public bool MobileEnabled { get; set; }
        public bool MobileAppEnabled { get; set; }
        public bool IsPremiumDisabled { get; set; }
        public string BasicTerms { get; set; }
        public string ExtentedTerms { get; set; }
        public bool IsHomePageFeatured { get; set; }
        public List<OfferMerchantChangedCategoryItem> Categories { get; set; }
    }

    public class OfferMerchantChangedCategoryItem
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }
}
