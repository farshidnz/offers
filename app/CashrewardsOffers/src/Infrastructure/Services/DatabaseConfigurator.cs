using Amazon.SecretsManager.Model;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrastructure.Models;
using CashrewardsOffers.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Services
{
    public interface IDatabaseConfigurator
    {
        Task ConfigureDatabase();
    }

    public class DatabaseConfigurator : IDatabaseConfigurator
    {
        private readonly IMongoClientFactory _mongoClientFactory;
        private readonly IAwsSecretsManagerClientFactory _awsSecretsManagerClientFactory;
        private readonly IMongoLockService _mongoLockService;
        private readonly IAnzItemPersistenceContext _anzItemPersistenceContext;
        private readonly string _secretName;
        private readonly string _documentDbDatabseName;
        private readonly bool _configureDatabase;
        private readonly string _docDbUsername;
        private readonly string _docDbPassword;

        public DatabaseConfigurator(
            IConfiguration configuration,
            IMongoClientFactory mongoClientFactory,
            IAwsSecretsManagerClientFactory awsSecretsManagerClientFactory,
            IMongoLockService mongoLockService,
            IAnzItemPersistenceContext anzItemPersistenceContext)
        {
            _mongoLockService = mongoLockService;
            _anzItemPersistenceContext = anzItemPersistenceContext;
            _mongoClientFactory = mongoClientFactory;
            _awsSecretsManagerClientFactory = awsSecretsManagerClientFactory;
            _secretName = configuration["DocDbSecretName"];
            _documentDbDatabseName = configuration["DocumentDbDatabseName"];
            _configureDatabase = bool.TryParse(configuration["ConfigureDatabase"], out var b) && b;
            _docDbUsername = configuration["DocDbUsername"];
            _docDbPassword = configuration["DocDbPassword"];
        }

        public async Task ConfigureDatabase()
        {
            Log.Information("Configuring document database");
            await AddDatabaseUser();
            MigrateDatabase();
        }

        private void MigrateDatabase()
        {
            Log.Information("Migrating document database");
            _mongoLockService.Migrate();
            _anzItemPersistenceContext.Migrate();
        }

        public async Task AddDatabaseUser()
        {
            if (!_configureDatabase)
            {
                Log.Information("Skipping create of document database user");
                return;
            }

            if (await IsExistingUser(_docDbUsername))
            {
                Log.Information("User {DocDbUsername} already exists", _docDbUsername);

            }
            else
            {
                await CreateUser(_docDbUsername, _docDbPassword);
            }
        }

        private async Task<IMongoDatabase> GetDatabase()
        {
            var docDbSecret = await GetSecret();
            var mongoClient = _mongoClientFactory.CreateClient(docDbSecret.Username, docDbSecret.Password);
            return mongoClient.GetDatabase(_documentDbDatabseName);
        }

        public async Task<AwsDocDbSecret> GetSecret()
        {
            using var client = _awsSecretsManagerClientFactory.CreateClient();
            var response = await client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = _secretName });
            return JsonConvert.DeserializeObject<AwsDocDbSecret>(response.SecretString);
        }

        public async Task<bool> IsExistingUser(string username)
        {
            var result = await (await GetDatabase()).RunCommandAsync<DocDbUserInfo>(
                new BsonDocument
                {
                    { "usersInfo", 1 }
                }
            );

            return result.users.Any(u => u.user == username);
        }

        public async Task CreateUser(string username, string password)
        {
            Log.Information("Creating user {username}", username);

            await (await GetDatabase()).RunCommandAsync<BsonDocument>(
                new BsonDocument
                {
                    { "createUser", username },
                    { "pwd", password },
                    { "roles", new BsonArray { new BsonDocument { { "role", "root" }, { "db", "admin" } }} }
                }
            );
        }

        public async Task DeleteUser(string username)
        {
            Log.Information("Deleting user {username}", username);

            await (await GetDatabase()).RunCommandAsync<BsonDocument>(
                new BsonDocument
                {
                    { "dropUser", username }
                }
            );
        }
    }

    public class AwsDocDbSecret
    {
        public string DbClusterIdentifier { get; set; }
        public string Password { get; set; }
        public string Engine { get; set; }
        public string Port { get; set; }
        public string Host { get; set; }
        public string Ssl { get; set; }
        public string Username { get; set; }
    }
}
