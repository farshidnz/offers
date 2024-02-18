using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.IntegrationTests.TestingHelpers;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrastructure.Persistence;
using CashrewardsOffers.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.IntegrationTests.OffersApplication
{
    public class DatabaseConfiguratorTests : TestBase
    {
        private class TestState
        {
            public DatabaseConfigurator DatabaseConfigurator { get; }

            public TestState()
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.json", true)
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.Development.json", true)
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["DocumentDbDatabseName"] = $"integratrion-testdb-{Guid.NewGuid()}",
                        ["UseTransactions"] = "false"
                    })
                    .Build();

                DatabaseConfigurator = new DatabaseConfigurator(
                    configuration,
                    new MongoClientFactory(configuration),
                    Mock.Of<IAwsSecretsManagerClientFactory>(),
                    Mock.Of<IMongoLockService>(),
                    Mock.Of<IAnzItemPersistenceContext>());
            }
        }

        [Test]
        [Ignore("requires real Document, cannot run in memory as the APIs are a bit different")]
        public async Task CanCreateAndDeleteUsers()
        {
            var state = new TestState();

            var username = "test";

            if (await state.DatabaseConfigurator.IsExistingUser(username))
            {
                await state.DatabaseConfigurator.DeleteUser(username);
            }

            await state.DatabaseConfigurator.CreateUser(username, "password");
            await state.DatabaseConfigurator.DeleteUser(username);
        }
    }
}
