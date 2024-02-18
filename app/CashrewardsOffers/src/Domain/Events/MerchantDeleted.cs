using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Enums;

namespace CashrewardsOffers.Domain.Events
{
    public class MerchantDeleted : DomainEvent
    {
        public int MerchantId { get; set; }
        public Client Client { get; set; }
        public string HyphenatedString { get; set; }
    }
}
