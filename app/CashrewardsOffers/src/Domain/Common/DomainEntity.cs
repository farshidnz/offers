using CashrewardsOffers.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.Domain.Common
{
    public abstract class DomainEntity : IHasDomainEvent
    {
        // keep track of contextual seq numbers, the event context is defined by EventContextProvider
        // property attributes on each event.
        private readonly Dictionary<EventContext, ulong> ContextualEventSequenceNumbers = new();

        public Queue<DomainEvent> DomainEvents { get; } = new();

        public bool HasDomainEvents
        {
            get { return DomainEvents.Count != 0; }
        }

        public string DomainName { get; } = "Offers";

        public void RaiseEvent(DomainEvent domainEvent)
        {
            var eventContext = domainEvent.GetEventContext();
            ContextualEventSequenceNumbers.TryGetValue(eventContext, out ulong prevSeqNum);

            var nextSeqNum = prevSeqNum + 1;
            domainEvent.Metadata.ContextualSequenceNumber = nextSeqNum;
            domainEvent.Metadata.EventID = Guid.NewGuid();
            domainEvent.Metadata.EventType = domainEvent.GetType().Name;
            domainEvent.Metadata.EventSource = "CashrewardsOffers";
            domainEvent.Metadata.RaisedDateTimeUTC = DateTime.UtcNow;
            domainEvent.Metadata.Domain = DomainName;

            ContextualEventSequenceNumbers[eventContext] = nextSeqNum;

            DomainEvents.Enqueue(domainEvent);
        }

        public void AssignEventsCorrelationID(Guid? correlationID)
        {
            correlationID = correlationID ?? Guid.NewGuid();
            foreach (var domainEvent in DomainEvents)
            {
                domainEvent.Metadata.CorrelationID = correlationID.Value;
            }
        }
    }
}
