using CashrewardsOffers.Domain.Common;

namespace CashrewardsOffers.Infrustructure.UnitTests.Helpers
{
    public class TestEvent : DomainEvent
    {
        public string SomeValue { get; set; }
    }
}
