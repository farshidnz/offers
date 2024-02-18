using CashrewardsOffers.Domain.Attributes;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.UnitTests.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace CashrewardsOffers.Domain.UnitTests.Entities
{
    public class DomainEntityTests
    {
        public TestEntity SUT()
        {
            return new TestEntity(new TestEntityId("testId"));
        }

        [Test]
        public void RaiseEvent_ShouldSetEventMetadata()
        {
            var entity = SUT();

            var approximateRaiseTime = DateTimeOffset.UtcNow;
            entity.RaiseEvent(new TestEvent());

            var eventMetadata = entity.DomainEvents.Peek().Metadata;
            eventMetadata.EventID.Should().NotBe(Guid.Empty);
            eventMetadata.EventType.Should().Be(typeof(TestEvent).Name);
            eventMetadata.EventSource.Should().Be("CashrewardsOffers");
            eventMetadata.Domain.Should().Be(entity.DomainName);
            eventMetadata.RaisedDateTimeUTC.Should().BeOnOrAfter(approximateRaiseTime);
        }

        [TestCase(3)]
        public void RaiseEvent_ShouldSetEventSequenceNumber_ToNextAvailable(int numOfEventsRaised)
        {
            var entity = SUT();

            for (var i = 0; i < numOfEventsRaised; i++)
            {
                entity.RaiseEvent(new TestEvent());
                ulong expectedSeqNum = (ulong)(i + 1);
                entity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(expectedSeqNum);
            }
        }

        [EventContextName("SomeBusinessContextForEvent")]
        private class ComplexEvent : DomainEvent
        {
            [EventContextProvider]
            public string Campaign { get; set; }

            [EventContextProvider]
            public Guid memberID { get; set; }

            [EventContextProvider]
            public int transaction { get; set; }

            public string state { get; set; }
        }

        [Test]
        public void RaiseEvent_ShouldIncrementEventSequenceNumberForTheSameEventContext_GivenRaisingEventsWithSameEventContext()
        {
            Guid guid = Guid.NewGuid();
            var evnt1 = new ComplexEvent()
            {
                Campaign = "abcd",
                memberID = guid,
                transaction = 987654,
                state = "created"
            };
            var evnt2 = new ComplexEvent()
            {
                transaction = 987654,
                memberID = guid,
                Campaign = "abcd",
                state = "updated"
            };

            var testEntity = SUT();

            testEntity.RaiseEvent(evnt1);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);

            testEntity.RaiseEvent(evnt2);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(2);
        }

        [Test]
        public void RaiseEvent_ShouldKeepEventSequenceNumberSeparateForDifferentEventContexts_GivenRaisingEventsWithDifferentEventContext()
        {
            Guid guid = Guid.NewGuid();
            var evnt1 = new ComplexEvent()
            {
                Campaign = "abcd",
                memberID = guid,
                transaction = 987654, // different
                state = "created"
            };
            var evnt2 = new ComplexEvent()
            {
                transaction = 987321, // different
                memberID = guid,
                Campaign = "abcd",
                state = "updated"
            };

            var testEntity = SUT();

            testEntity.RaiseEvent(evnt1);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);

            testEntity.RaiseEvent(evnt2);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);
        }

        [EventContextName("SomeBusinessContextForEvent")]
        private class DifferentComplexEvent : DomainEvent
        {
            [EventContextProvider]
            public string Campaign { get; set; }

            [EventContextProvider]
            public Guid memberID { get; set; }

            [EventContextProvider]
            public int transaction { get; set; }

            public string state { get; set; }
        }

        [Test]
        public void RaiseEvent_ShouldIncrementEventSequenceNumberForTheSameEventContext_GivenRaisingDifferentEventsButForSameEventContext()
        {
            Guid guid = Guid.NewGuid();
            var evnt1 = new ComplexEvent
            {
                Campaign = "abcd",
                memberID = guid,
                transaction = 987654,
                state = "created"
            };
            var evnt2 = new DifferentComplexEvent
            {
                transaction = 987654,
                memberID = guid,
                Campaign = "abcd",
                state = "updated"
            };

            var testEntity = SUT();

            testEntity.RaiseEvent(evnt1);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);

            testEntity.RaiseEvent(evnt2);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(2);
        }

        private class SimpleEvent : DomainEvent
        { }

        private class AnotherSimpleEvent : DomainEvent
        { }

        [Test]
        public void RaiseEvent_ShouldUseEventTypeNameToIdentifyContext_GivenRaisingEventsWithUndefinedContextName()
        {
            var evnt1 = new SimpleEvent();
            var evnt2 = new SimpleEvent();
            var evnt3 = new AnotherSimpleEvent();

            var testEntity = SUT();

            testEntity.RaiseEvent(evnt1);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);

            testEntity.RaiseEvent(evnt2);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(2);

            testEntity.RaiseEvent(evnt3);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);
        }
    }
}
