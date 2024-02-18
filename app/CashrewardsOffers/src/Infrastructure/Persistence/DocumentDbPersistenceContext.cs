using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Common;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public interface IDocument
    {
        public ObjectId _id { get; set; }
    }

    public interface IDocumentPersistenceContext<TDocumentModel>
    {
        IMongoCollection<TDocumentModel> Collection { get; }
        Task<IEnumerable<TDocumentModel>> Find(FilterDefinition<TDocumentModel> filter, FindOptions<TDocumentModel, TDocumentModel> options = null);
        Task<TDocumentModel> FindFirst(FilterDefinition<TDocumentModel> filter);
        Task<long> Count(FilterDefinition<TDocumentModel> filter);
        Task Insert(TDocumentModel document, List<DomainEvent> domainEvents = null);
        Task Update(FilterDefinition<TDocumentModel> filter, params (string Name, object Value)[] fields);
        Task Upsert(TDocumentModel document, FilterDefinition<TDocumentModel> keyFilter, List<DomainEvent> domainEvents = null);
        Task Replace(TDocumentModel document, List<DomainEvent> domainEvents = null);
        Task Delete(TDocumentModel document, List<DomainEvent> domainEvents = null);
        Task DropCollection();
    }

    public class DocumentDbPersistenceContext<TDocumentModel> : IDocumentPersistenceContext<TDocumentModel> where TDocumentModel : IDocument
    {
        private readonly IDocumentDbConnection _documentDbConnection;
        private readonly IEventOutboxPersistenceContext _eventOutboxPersistenceContext;
        private readonly bool _isTransactionsEnabled;

        public DocumentDbPersistenceContext(
            IConfiguration configuration,
            IDocumentDbConnection documentDbConnection,
            IEventOutboxPersistenceContext eventOutboxPersistenceContext)
        {
            _documentDbConnection = documentDbConnection;
            _eventOutboxPersistenceContext = eventOutboxPersistenceContext;
            _isTransactionsEnabled = bool.TryParse(configuration["UseTransactions"], out var b) ? b : true;
        }

        private string CollectionName { get; } = typeof(TDocumentModel).Name;

        public IMongoCollection<TDocumentModel> Collection => _documentDbConnection.Database.GetCollection<TDocumentModel>(CollectionName);

        public async Task<IEnumerable<TDocumentModel>> Find(FilterDefinition<TDocumentModel> filter, FindOptions<TDocumentModel, TDocumentModel> options = null)
        {
            var cursor = await Collection.FindAsync(filter, options);
            return await cursor.ToListAsync();
        }

        public async Task<TDocumentModel> FindFirst(FilterDefinition<TDocumentModel> filter)
        {
            var cursor = await Collection.FindAsync(filter);
            return await cursor.FirstOrDefaultAsync();
        }

        public async Task<long> Count(FilterDefinition<TDocumentModel> filter) =>
            await Collection.CountDocumentsAsync(filter);

        public async Task Insert(TDocumentModel document, List<DomainEvent> domainEvents = null)
        {
            await DoOutboxTransactionalUpdate(() => Collection.InsertOneAsync(document), domainEvents);
        }

        public async Task Update(FilterDefinition<TDocumentModel> filter, params (string Name, object Value)[] fields)
        {
            UpdateDefinition<TDocumentModel> updateDefinition = null;
            foreach (var (Name, Value) in fields)
            {
                if (updateDefinition == null)
                {
                    updateDefinition = Builders<TDocumentModel>.Update.Set(Name, Value);
                }
                else
                {
                    updateDefinition = updateDefinition.Set(Name, Value);
                }
            }

            await Collection.UpdateOneAsync(filter, updateDefinition);
        }

        public async Task Upsert(TDocumentModel document, FilterDefinition<TDocumentModel> keyFilter, List<DomainEvent> domainEvents = null)
        {
            await DoOutboxTransactionalUpdate(() => Collection.ReplaceOneAsync(keyFilter, document, new ReplaceOptions { IsUpsert = true }), domainEvents);
        }

        public async Task Replace(TDocumentModel document, List<DomainEvent> domainEvents = null)
        {
            await DoOutboxTransactionalUpdate(() => Collection.FindOneAndReplaceAsync(IdFilter(document), document), domainEvents);
        }

        public async Task Delete(TDocumentModel document, List<DomainEvent> domainEvents = null)
        {
            await DoOutboxTransactionalUpdate(() => Collection.FindOneAndDeleteAsync(IdFilter(document)), domainEvents);
        }

        public async Task DropCollection() => await _documentDbConnection.Database.DropCollectionAsync(CollectionName);

        private static FilterDefinition<TDocumentModel> IdFilter(TDocumentModel document) => Builders<TDocumentModel>.Filter.Eq("_id", document._id);

        private async Task DoOutboxTransactionalUpdate(Func<Task> update, List<DomainEvent> domainEvents)
        {
            if (domainEvents?.Any() ?? false)
            {
                if (_isTransactionsEnabled)
                {
                    using var session = await _documentDbConnection.Client.StartSessionAsync(new ClientSessionOptions { CausalConsistency = false });

                    await session.WithTransactionAsync(async (sess, cancellationtoken) =>
                    {
                        await update();
                        await PersistEventsToOutbox(domainEvents);
                        return "Transaction committed";
                    });
                }
                else
                {
                    await update();
                    await PersistEventsToOutbox(domainEvents);
                }
            }
            else
            {
                await update();
            }
        }

        private async Task PersistEventsToOutbox(List<DomainEvent> domainEvents)
        {
            if (domainEvents == null) return;

            foreach (var domainEvent in domainEvents)
            {
                await _eventOutboxPersistenceContext.Append(domainEvent);
            }
        }
    }
}
