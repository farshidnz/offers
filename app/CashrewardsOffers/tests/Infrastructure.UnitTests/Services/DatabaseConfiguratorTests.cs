using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrastructure.Models;
using CashrewardsOffers.Infrastructure.Persistence;
using CashrewardsOffers.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Services
{
    public class DatabaseConfiguratorTests
    {
        private class TestState
        {
            public DatabaseConfigurator DatabaseConfigurator { get; }

            public Mock<IMongoClientFactory> MongoClientFactory { get; } = new();
            public Mock<IMongoDatabase> MongoDatabase { get; } = new();
            public List<DocDbUser> ExistingUsers { get; } = new();

            public TestState(bool configureDatabase = true)
            {
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ConfigureDatabase"] = configureDatabase.ToString(),
                        ["DocDbUsername"] = "offers_user",
                        ["DocDbPassword"] = "pw"
                    })
                    .Build();

                MongoDatabase
                    .Setup(d => d.RunCommandAsync(It.IsAny<Command<DocDbUserInfo>>(), It.IsAny<ReadPreference>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new DocDbUserInfo { users = ExistingUsers });
                var mongoClient = new Mock<IMongoClient>();
                mongoClient.Setup(c => c.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>())).Returns(MongoDatabase.Object);
                MongoClientFactory.Setup(f => f.CreateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(mongoClient.Object);

                var awsSecretsManagerClientFactory = new Mock<IAwsSecretsManagerClientFactory>();
                var awsSecretsManagerClient = new Mock<IAmazonSecretsManager>();
                awsSecretsManagerClient.Setup(c => c.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(
                    new GetSecretValueResponse
                    {
                        SecretString = JsonConvert.SerializeObject(new AwsDocDbSecret { Username = "user", Password = "p@ssword" })
                    });

                awsSecretsManagerClientFactory.Setup(f => f.CreateClient()).Returns(awsSecretsManagerClient.Object);

                DatabaseConfigurator = new(
                    configuration,
                    MongoClientFactory.Object,
                    awsSecretsManagerClientFactory.Object,
                    Mock.Of<IMongoLockService>(),
                    Mock.Of<IAnzItemPersistenceContext>());
            }
        }

        [Test]
        public async Task ConfigureDatabase_ShouldConnectToMongoDbWithSecretAdminCredentials()
        {
            var state = new TestState();

            await state.DatabaseConfigurator.ConfigureDatabase();

            state.MongoClientFactory.Verify(f => f.CreateClient(It.Is<string>(u => u == "user"), It.Is<string>(p => p == "p@ssword")));
        }

        [Test]
        public async Task ConfigureDatabase_ShouldNotConfigureDatabase_GivenConfigurationIsFalse()
        {
            var state = new TestState(configureDatabase: false);

            await state.DatabaseConfigurator.ConfigureDatabase();

            state.MongoClientFactory.Verify(f => f.CreateClient(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ConfigureDatabase_ShouldCreateOffersUser()
        {
            var state = new TestState();
            Command<BsonDocument> createUserCommand = null;
            state.MongoDatabase
                .Setup(f => f.RunCommandAsync(It.IsAny<Command<BsonDocument>>(), It.IsAny<ReadPreference>(), It.IsAny<CancellationToken>()))
                .Callback((Command<BsonDocument> cmd, ReadPreference r, CancellationToken c) => createUserCommand = cmd);

            await state.DatabaseConfigurator.ConfigureDatabase();

            createUserCommand.ToBsonDocument().GetValue("Document").ToBsonDocument().GetValue("createUser").Should().Be("offers_user");
        }

        [Test]
        public async Task ConfigureDatabase_ShouldNotCreateOffersUser_GivenUserAlreadyExists()
        {
            var state = new TestState();
            state.ExistingUsers.Add(new DocDbUser { user = "offers_user" });

            await state.DatabaseConfigurator.ConfigureDatabase();

            state.MongoDatabase.Verify(f => f.RunCommandAsync(It.IsAny<Command<BsonDocument>>(), It.IsAny<ReadPreference>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
