namespace CashrewardsOffers.Application.Offers.Services
{
    public class ShopGoMerchantBaseRate
    {
        public int MerchantId { get; set; }
        public int TierCommTypeId { get; set; }
        public int TierTypeId { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public decimal Rate { get; set; }
        public int ClientProgramTypeId { get; set; }
        public bool? IsFlatRate { get; set; }
        public string RewardName { get; set; }
    }
}
