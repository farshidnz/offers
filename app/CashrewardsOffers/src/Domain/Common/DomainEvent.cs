using CashrewardsOffers.Domain.Attributes;
using CashrewardsOffers.Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CashrewardsOffers.Domain.Common
{
    public interface IHasDomainEvent
    {
        Queue<DomainEvent> DomainEvents { get; }

        void RaiseEvent(DomainEvent domainEvent);
    }

    public abstract class DomainEvent
    {
        public string Id { get; set; }

        public EventMetadata Metadata { get; set; } = new();

        public string ToJson(IContractResolver contractResolver = default) => JsonConvert.SerializeObject(this, new JsonSerializerSettings()
        {
            ContractResolver = contractResolver
        });

        public EventContext GetEventContext()
        {
            var eventType = this.GetType();
            var EventContextName = eventType.GetCustomAttribute<EventContextName>(true)?.ContextName ?? eventType.Name;
            var eventContextPropertyValues = new List<object> { EventContextName };
            eventContextPropertyValues.AddRange(eventType.GetProperties()
                                                         .Where(p => p.IsDefined(typeof(EventContextProvider)))
                                                         .Select(x => x.GetValue(this)));

            return new EventContext(eventContextPropertyValues);
        }
    }

    public class EventMetadata
    {
        public Guid EventID { get; set; }

        public string EventSource { get; set; }

        public string EventType { get; set; }

        public string Domain { get; set; }

        public Guid CorrelationID { get; set; }

        public DateTimeOffset RaisedDateTimeUTC { get; set; }

        public DateTimeOffset PublishedDateTimeUTC { get; set; }

        // the context is per domain entity/event type
        public ulong ContextualSequenceNumber { get; set; }
    }
}
