using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public interface IDocumentDbConnection
    {
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
    }

    public class DocumentDbConnection : IDocumentDbConnection
    {
        public DocumentDbConnection(
            IConfiguration configuration, 
            IMongoClientFactory mongoClientFactory)
        {
            Client = mongoClientFactory.CreateClient();
            Database = Client.GetDatabase(configuration["DocumentDbDatabseName"]);
        }

        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }
    }
}
