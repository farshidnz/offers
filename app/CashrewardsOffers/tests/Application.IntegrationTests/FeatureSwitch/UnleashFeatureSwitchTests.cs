using CashrewardsOffers.Application.IntegrationTests.TestingHelpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Unleash;

namespace CashrewardsOffers.Application.IntegrationTests.FeatureSwitch
{
    [Category("Integration")]
    public class UnleashFeatureSwitchTests
    {
        private class TestState
        {
            public IUnleash Unleash { get; }

            public TestState()
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.json", true)
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.Development.json", true)
                    .Build();

                var settings = new UnleashSettings()
                {
                    AppName = "ofers-ms-client",
                    Environment = "development",
                    UnleashApi = new Uri(config["UnleashUrl"]),
                    FetchTogglesInterval = TimeSpan.FromSeconds(1),
                    CustomHttpHeaders = new Dictionary<string, string>()
                    {
                        ["Authorization"] = config["UnleashApiKey"]
                    }
                };

                Unleash = new DefaultUnleash(settings);
            }
        }

        [Test]
        [Ignore("Slow test, dependent on Unleash feature with included user")]
        public async Task IsEnabled_ShouldReturnTrue_GivenUserIsAddedToUserIdStrategy()
        {
            var state = new TestState();

            await Task.Delay(TimeSpan.FromSeconds(2));

            state.Unleash.IsEnabled("PersonalisationExp1", new UnleashContext { UserId = "bab146d3-8a64-4c93-958a-3366e219e2dd" }).Should().BeTrue();
        }
    }
}
