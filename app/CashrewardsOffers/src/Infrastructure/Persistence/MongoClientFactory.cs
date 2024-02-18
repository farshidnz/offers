using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public interface IMongoClientFactory
    {
        IMongoClient CreateClient(string username = null, string password = null);
    }

    public class MongoClientFactory : IMongoClientFactory
    {
        private readonly IConfiguration _configuration;

        public MongoClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IMongoClient CreateClient(string username = null, string password = null)
        {
            string un = username ?? _configuration["DocDbUsername"];
            string pw = Escape(password ?? _configuration["DocDbPassword"]);
            string docDbHost = _configuration["DocumentDBHostWriter"];
            string connectionString = string.Format(_configuration.GetConnectionString("DocumentDbConnectionString"), un, pw, docDbHost);

            return CreateClient(connectionString);
        }

        public virtual IMongoClient CreateClient(string connectionString) => new MongoClient(MongoClientSettings.FromUrl(new MongoUrl(connectionString)));

        private static string Escape(string s)
        {
            return s
                .Replace("%", "%25")
                .Replace("#", "%23")
                .Replace("/", "%2f")
                .Replace(":", "%3a")
                .Replace("?", "%3f")
                .Replace("@", "%40")
                .Replace("[", "%5b")
                .Replace("]", "%5d");
        }
    }
}
