using CashrewardsOffers.Domain.Common;
using System.Collections.Generic;

namespace CashrewardsOffers.Domain.UnitTests.Helpers
{
    public class TestEntityId : ValueObject
    {
        private readonly string id;

        public TestEntityId(string id)
        {
            this.id = id;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return id;
        }
    }

    public class TestEntity : DomainEntity
    {
        public TestEntity(TestEntityId id)
        {
            ID = id;
        }

        public TestEntityId ID { get; }

        public void DoSomethingThatRaisesEvent(string someValue)
        {
            RaiseEvent(new TestEvent { SomeValue = someValue });
        }
    }

    public class TestEvent : DomainEvent
    {
        public string SomeValue { get; set; }
    }
}
