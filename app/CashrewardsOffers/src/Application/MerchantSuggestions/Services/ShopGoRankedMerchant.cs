namespace CashrewardsOffers.Application.MerchantSuggestions.Services
{
    public class ShopGoRankedMerchant
    {
        public int ClientId { get; set; }
        public int MerchantId { get; set; }
        public string HyphenatedString { get; set; }
        public string RegularImageUrl { get; set; }
        public bool? IsPremiumDisabled { get; set; }
    }
}
