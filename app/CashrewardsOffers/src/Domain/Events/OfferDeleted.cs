using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Enums;

namespace CashrewardsOffers.Domain.Events
{
    public class OfferDeleted : DomainEvent
    {
        public int OfferId { get; set; }
        public Client Client { get; set; }
        public string HyphenatedString { get; set; }
        public OfferMerchantDeleted Merchant { get; set; }
    }

    public class OfferMerchantDeleted
    {
        public int Id { get; set; }
        public string HyphenatedString { get; set; }
    }
}
