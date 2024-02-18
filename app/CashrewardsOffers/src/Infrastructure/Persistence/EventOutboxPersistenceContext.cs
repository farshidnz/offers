using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Infrastructure.Models;
using CashrewardsOffers.Infrastructure.Services;
using Mapster;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class EventOutboxPersistenceContext : IEventOutboxPersistenceContext
    {
        private readonly IMongoDatabase _database;
        private readonly IEventTypeResolver _eventTypeResolver;

        public EventOutboxPersistenceContext(
            IConfiguration configuration,
            IEventTypeResolver eventTypeResolver,
            IMongoClientFactory mongoClientFactory)
        {
            _database = mongoClientFactory.CreateClient().GetDatabase(configuration["DocumentDbDatabseName"]);
            _eventTypeResolver = eventTypeResolver;
        }

        public async Task Append(DomainEvent domainEvent)
        {
            await Collection<DomainEventDocument>().InsertOneAsync(domainEvent.Adapt<DomainEventDocument>());
        }

        public async Task Delete(Guid eventId)
        {
            await Collection<DomainEventDocument>().FindOneAndDeleteAsync(Builders<DomainEventDocument>.Filter.Eq("EventID", eventId));
        }

        public async Task<DomainEvent> GetNext()
        {
            var cursor = await Collection<DomainEventDocument>().FindAsync(Builders<DomainEventDocument>.Filter.Empty);
            var domainEventDocument = await cursor.FirstOrDefaultAsync();
            return domainEventDocument
                .BuildAdapter()
                .AddParameters("eventTypeResolver", _eventTypeResolver)
                .AdaptToType<DomainEvent>();
        }

        private IMongoCollection<T> Collection<T>() => _database.GetCollection<T>("EventOutbox");
    }
}
