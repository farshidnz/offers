namespace CashrewardsOffers.Application.Merchants.Models
{
    public class ShopGoMerchant
    {
        public int ClientId { get; set; }
        public int MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string HyphenatedString { get; set; }
        public string RegularImageUrl { get; set; }
        public int ClientProgramTypeId { get; set; }
        public int TierCommTypeId { get; set; }
        public int TierTypeId { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public string RewardName { get; set; }
        public bool? IsFlatRate { get; set; }
        public decimal Rate { get; set; }
        public bool? IsPremiumDisabled { get; set; }
        public int NetworkId { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsHomePageFeatured { get; set; }
        public bool IsPopular { get; set; }
        public string BasicTerms { get; set; }
        public string ExtentedTerms { get; set; }
        public bool MobileEnabled { get; set; }
        public bool IsMobileAppEnabled { get; set; }
        public bool IsPaused { get; set; }
    }
}
