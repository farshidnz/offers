namespace CashrewardsOffers.Application.Offers.Services
{
    public class ShopGoTier
    {
        public int ClientTierId { get; set; }
        public int MerchantTierId { get; set; }
        public int MerchantId { get; set; }
        public int ClientId { get; set; }
        public string TierName { get; set; }
        public int TierCommTypeId { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public string TierSpecialTerms { get; set; }
    }
}
