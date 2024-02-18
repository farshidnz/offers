using CashrewardsOffers.Infrastructure.Persistence;
using Mongo2Go;
using MongoDB.Driver;

namespace CashrewardsOffers.Application.IntegrationTests.TestingHelpers
{
    public class MongoInMemoryClientFactory : IMongoClientFactory
    {
        private MongoDbRunner _runner;

        public MongoInMemoryClientFactory()
        {
            _runner = MongoDbRunner.Start();
        }

        public IMongoClient CreateClient(string username = null, string password = null)
        {
            return new MongoClient(_runner.ConnectionString);
        }
    }
}
