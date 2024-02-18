using CashrewardsOffers.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace CashrewardsOffers.Infrustructure.UnitTests.Persistence
{
    public class MongoClientFactoryTests
    {
        private class TestState
        {
            public Mock<MongoClientFactory> MongoClientFactory { get; }

            public string MongoConnectionString { get; private set; }

            public TestState()
            {
                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ConnectionStrings:DocumentDbConnectionString"] = "mongodb://{0}:{1}@{2}:27017/?replicaSet=rs0&readPreference=secondaryPreferred&retryWrites=false&tls=false",
                        ["DocDbUsername"] = "username",
                        ["DocDbPassword"] = "password",
                        ["DocumentDBHostWriter"] = "host"
                    })
                    .Build();

                MongoClientFactory = new Mock<MongoClientFactory>(config);
                MongoClientFactory
                    .Setup(f => f.CreateClient(It.IsAny<string>()))
                    .Callback((string connectionString) => MongoConnectionString = connectionString);
            }
        }

        [Test]
        public void CreateClient_ShouldConstructConnectionString_GivenConfiguration()
        {
            var state = new TestState();

            state.MongoClientFactory.Object.CreateClient();

            state.MongoConnectionString.Should().Be("mongodb://username:password@host:27017/?replicaSet=rs0&readPreference=secondaryPreferred&retryWrites=false&tls=false");
        }

        [Test]
        public void CreateClient_ShouldConstructConnectionString_GivenConfigurationAndUsernameAndPassword()
        {
            var state = new TestState();

            state.MongoClientFactory.Object.CreateClient("offers_user", "Password!");

            state.MongoConnectionString.Should().Be("mongodb://offers_user:Password!@host:27017/?replicaSet=rs0&readPreference=secondaryPreferred&retryWrites=false&tls=false");
        }

        [Test]
        public void CreateClient_ShouldEcsapePassword_GivenPasswordWithWeirdCharacters()
        {
            var state = new TestState();

            state.MongoClientFactory.Object.CreateClient("offers_user", "P@ssword%#/:?@[]");

            state.MongoConnectionString.Should().Be("mongodb://offers_user:P%40ssword%25%23%2f%3a%3f%40%5b%5d@host:27017/?replicaSet=rs0&readPreference=secondaryPreferred&retryWrites=false&tls=false");
        }
    }
}
