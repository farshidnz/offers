using CashrewardsOffers.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CashrewardsOffers.Infrastructure.Services
{
    public interface IEventTypeResolver
    {
        Type GetEventType(string eventTypeName);
    }

    public class EventTypeResolver : IEventTypeResolver
    {
        private readonly Dictionary<string, Type> _domainEvents;

        public EventTypeResolver()
        {
            _domainEvents = Assembly.Load("CashrewardsOffers.Domain")
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DomainEvent)))
                .Select(t => new KeyValuePair<string, Type>(t.Name, t))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public Type GetEventType(string eventTypeName) => _domainEvents.TryGetValue(eventTypeName, out var t) ? t : null;
    }
}
