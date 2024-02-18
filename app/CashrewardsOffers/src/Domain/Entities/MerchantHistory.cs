using CashrewardsOffers.Domain.Enums;
using System;

namespace CashrewardsOffers.Domain.Entities
{
    public class MerchantHistory
    {
        public string Id { get; set; }
        public DateTimeOffset ChangeTime { get; set; }
        public int MerchantId { get; set; }
        public Client Client { get; set; }
        public string Name { get; set; }
        public string HyphenatedString { get; set; }
        public string ClientCommissionString { get; set; }
    }
}
