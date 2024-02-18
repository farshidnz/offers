using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public interface IMongoLockService
    {
        Task<Guid?> LockAsync(string lockName, CancellationToken cancellationToken = default);
        Task RelockAsync(string lockName, Guid lockId, CancellationToken cancellationToken = default);
        Task ReleaseLockAsync(string lockName, Guid lockId, CancellationToken cancellationToken = default);
        Task ReleaseAllLocks();
        void Migrate();
    }

    public class MongoLockService : IMongoLockService
    {
        private readonly IDocumentDbConnection _documentDbConnection;
		private readonly long _lockMaxDuration = TimeSpan.FromSeconds(60).Ticks;

		public MongoLockService(IDocumentDbConnection documentDbConnection)
        {
            _documentDbConnection = documentDbConnection;
        }

        public void Migrate()
        {
            Log.Information("Migrating MongoLockService");
            Collection.Indexes.CreateOne(
                new CreateIndexModel<LockDocument>(
                    Builders<LockDocument>.IndexKeys.Ascending(x => x.LockName),
                    new CreateIndexOptions() { Unique = true })
                );
        }

        public async Task<Guid?> LockAsync(string lockName, CancellationToken cancellationToken = default)
        {
            var lockDocument = new LockDocument
            {
                LockName = lockName
            };

            var filter = Builders<LockDocument>.Filter.And(
                Builders<LockDocument>.Filter.Eq(l => l.LockName, lockName),
                Builders<LockDocument>.Filter.Or(
                    Builders<LockDocument>.Filter.Eq(l => l.LockId, null),
                    Builders<LockDocument>.Filter.Lt(l => l.LockTime, DateTime.UtcNow.Ticks - _lockMaxDuration)
                )
            );

            var update = Builders<LockDocument>.Update
                .Set(l => l.LockId, Guid.NewGuid())
                .Set(l => l.LockTime, DateTime.UtcNow.Ticks)
                .SetOnInsert(l => l.LockName, lockName);

            var options = new FindOneAndUpdateOptions<LockDocument>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            try
            {
                lockDocument = await Collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
                return lockDocument.LockId;
            }
            catch (MongoCommandException e) when (e.Code == 11000)
            {
                return null;
            }
        }

        public async Task RelockAsync(string lockName, Guid lockId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<LockDocument>.Filter.And(
                Builders<LockDocument>.Filter.Eq(e => e.LockName, lockName),
                Builders<LockDocument>.Filter.Eq(e => e.LockId, lockId)
            );

            var update = Builders<LockDocument>.Update
                .Set(e => e.LockTime, DateTime.UtcNow.Ticks);

            var options = new UpdateOptions()
            {
                IsUpsert = false
            };

            var result = await Collection.UpdateOneAsync(filter, update, options, cancellationToken);

            if (result is null || result.ModifiedCount != 1)
                throw new LockException($"unable to relock item '{lockName}'");
        }

        public async Task ReleaseLockAsync(string lockName, Guid lockId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<LockDocument>.Filter.And(
                Builders<LockDocument>.Filter.Eq(e => e.LockName, lockName),
                Builders<LockDocument>.Filter.Eq(e => e.LockId, lockId)
            );

            var update = Builders<LockDocument>.Update
                .Set(e => e.LockId, null)
                .Set(e => e.LockTime, 0);

            var options = new UpdateOptions()
            {
                IsUpsert = false
            };

            var result = await Collection.UpdateOneAsync(filter, update, options, cancellationToken);

            if (result is null || result.ModifiedCount != 1)
                throw new LockException($"unable to release lock on item '{lockName}'");
        }

        public async Task ReleaseAllLocks() =>
            await _documentDbConnection.Database.DropCollectionAsync(CollectionName);

        private static string CollectionName => typeof(LockDocument).Name;

        private IMongoCollection<LockDocument> Collection => _documentDbConnection.Database.GetCollection<LockDocument>(CollectionName);
    }

    public class LockDocument
	{
        public ObjectId _id { get; set; }
        public string LockName { get; set; }
		public Guid? LockId { get; set; }
		public long LockTime { get; set; }
	}

	public class LockException : Exception
	{
		public LockException(string message) : base(message) { }
	}
}
