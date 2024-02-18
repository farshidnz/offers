using System;

namespace CashrewardsOffers.Application.Merchants.Models
{
    public class MerchantHistoryInfo
    {
        public DateTimeOffset ChangeInSydneyTime { get; set; }
        public int MerchantId { get; set; }
        public int Client { get; set; }
        public string Name { get; set; }
        public string HyphenatedString { get; set; }
        public string ClientCommissionString { get; set; }
    }
}
