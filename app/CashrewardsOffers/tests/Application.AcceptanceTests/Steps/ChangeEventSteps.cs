using Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.ANZ.Queries.GetAnzItems.v1;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace CashrewardsOffers.Application.AcceptanceTests.Steps
{
    [Binding]
    public class ChangeEventSteps
    {
        private readonly Fixture? _fixture;
        private readonly ScenarioContext _scenarioContext;

        public ChangeEventSteps(ScenarioContext scenarioContext)
        {
            _fixture = scenarioContext.GetFixture();
            _scenarioContext = scenarioContext;
        }

        [Given(@"merchant data change event")]
        public async Task GivenMerchantDataChangeEvent(Table table)
        {
            _fixture.ShouldNotBeNull();

            var merchantChangeEvents = table.CreateSetWithDefaults<MerchantChanged>(new Dictionary<string, object>
            {
                ["Client"] = Client.Cashrewards,
                ["MobileEnabled"] = true
            }).ToList();

            foreach (var merchantChangeEvent in merchantChangeEvents)
            {
                await _fixture.GivenEvent(merchantChangeEvent);
            }
        }

        [Given(@"merchant data change event at '([^']*)'")]
        public async Task GivenMerchantDataChangeEventAt(string updateTimeString, Table table)
        {
            _fixture.ShouldNotBeNull();

            var updateTime = new DateTimeOffset(DateTime.Parse(updateTimeString), TimeSpan.Zero);
            _fixture.GivenNowUtcIs(updateTime);

            await GivenMerchantDataChangeEvent(table);
        }

        [Given(@"offer data change event")]
        public async Task GivenOfferDataChangeEventAsync(Table table)
        {
            _fixture.ShouldNotBeNull();

            var offerChangeEvents = table.CreateDeepSetWithDefaults<OfferChanged>(new Dictionary<string, object>
            {
                ["Client"] = Client.Cashrewards,
                ["Merchant.MobileEnabled"] = true
            }).ToList();
            foreach (var offerChangeEvent in offerChangeEvents)
            {
                await _fixture.GivenEvent(offerChangeEvent);
            }
        }

        [Given(@"offer data change event at '([^']*)'")]
        public async Task GivenOfferDataChangeEventAt(string updateTimeString, Table table)
        {
            _fixture.ShouldNotBeNull();

            var updateTime = new DateTimeOffset(DateTime.Parse(updateTimeString), TimeSpan.Zero);
            _fixture.GivenNowUtcIs(updateTime);

            await GivenOfferDataChangeEventAsync(table);
        }


        [Given(@"merchant delete event")]
        public async Task GivenMerchantDeleteEvent(Table table)
        {
            _fixture.ShouldNotBeNull();

            var merchantDeletedEvents = table.CreateSetWithDefaults<MerchantDeleted>(new Dictionary<string, object>
            {
                ["Client"] = Client.Cashrewards
            }).ToList();

            foreach (var merchantDeletedEvent in merchantDeletedEvents)
            {
                await _fixture.GivenEvent(merchantDeletedEvent);
            }
        }

        [Given(@"merchant delete event at '([^']*)'")]
        public async Task GivenMerchantDeleteEventAt(string updateTimeString, Table table)
        {
            _fixture.ShouldNotBeNull();

            var updateTime = new DateTimeOffset(DateTime.Parse(updateTimeString), TimeSpan.Zero);
            _fixture.GivenNowUtcIs(updateTime);

            await GivenMerchantDeleteEvent(table);
        }

        [Given(@"offer delete event")]
        public async Task GivenOfferDeleteEvent(Table table)
        {
            _fixture.ShouldNotBeNull();

            var offerDeletedEvents = table.CreateDeepSetWithDefaults<OfferDeleted>(new Dictionary<string, object>
            {
                ["Client"] = Client.Cashrewards
            }).ToList();
            foreach (var offerDeletedEvent in offerDeletedEvents)
            {
                await _fixture.GivenEvent(offerDeletedEvent);
            }
        }

        [Given(@"offer delete event at '([^']*)'")]
        public async Task GivenOfferDeleteEventAt(string updateTimeString, Table table)
        {
            _fixture.ShouldNotBeNull();

            var updateTime = new DateTimeOffset(DateTime.Parse(updateTimeString), TimeSpan.Zero);
            _fixture.GivenNowUtcIs(updateTime);

            await GivenOfferDeleteEvent(table);
        }

        // Theses steps are for building up a single offer or merchant change event over multiple steps
        [Given(@"'([^']*)' for ANZ is available")]
        public void GivenForANZIsAvailable(string api)
        {
            _scenarioContext.AddTestContext($"api:{api}");
            if (api.ToLower().Contains("featured"))
            {
                _scenarioContext["changeType"] = "Offer";
                GivenOfferIsSetToCurrentlyFeatured("yes");
            }
            else if (api.ToLower().Contains("in") && api.ToLower().Contains("store"))
            {
                _scenarioContext["changeType"] = "Merchant";
                GivenMerchantsNetworkIsSetTo("instore");
            }
            else if (api.ToLower().Contains("popular") && api.ToLower().Contains("store"))
            {
                _scenarioContext["changeType"] = "Merchant";
                GivenMerchantIsSetTo("yes");
            }
            else
            {
                api.Should().BeOneOf("featured", "in-store", "popular");
            }
        }

        [Given(@"Offer is set to currently featured '([^']*)'")]
        public void GivenOfferIsSetToCurrentlyFeatured(string currentlyFeatured)
        {
            _scenarioContext.AddTestContext($"currentlyFeatured:{currentlyFeatured}");
            var offerChanged = GetOrCreateCurrentOfferChangeEvent();
            offerChanged.IsFeatured = currentlyFeatured.ToBoolean();
        }

        [Given(@"Merchant is set to '([^']*)'")]
        public void GivenMerchantIsSetTo(string popular)
        {
            _scenarioContext.AddTestContext($"popular:{popular}");
            var merchantChanged = GetOrCreateCurrentMerchantChangeEvent();
            merchantChanged.IsPopular = popular.ToBoolean();
            merchantChanged.PopularMerchantRankingForBrowser = popular.ToBoolean() ? 1 : 0;
        }

        [Given(@"Merchants network is set to '([^']*)'")]
        public void GivenMerchantsNetworkIsSetTo(string networkName)
        {
            _scenarioContext.AddTestContext($"networkName:{networkName}");
            var merchantChanged = GetOrCreateCurrentMerchantChangeEvent();
            merchantChanged.IsPopular = networkName.ToTrackingNetwork() == TrackingNetwork.Instore; // instore carousel only inculdes popular merchants
            merchantChanged.NetworkId = networkName.ToTrackingNetwork();
        }

        [Given(@"Merchants Mobile Enabled is set to '([^']*)'")]
        public void GivenMerchantsMobileEnabledIsSetTo(string mobileEnabled)
        {
            _scenarioContext.AddTestContext($"mobileEnabled:{mobileEnabled}");
            var changeType = _scenarioContext["changeType"] as string;
            changeType.ShouldNotBeNull();
            if (changeType == "Merchant")
            {
                var merchantChanged = GetOrCreateCurrentMerchantChangeEvent();
                merchantChanged.MobileEnabled = mobileEnabled.ToBoolean();
            }
            else if (changeType == "Offer")
            {
                var offerChanged = GetOrCreateCurrentOfferChangeEvent();
                offerChanged.Merchant.MobileEnabled = mobileEnabled.ToBoolean();
            }
        }

        [Given(@"Merchants Suppressed for Max member is set to '([^']*)'")]
        public void GivenMerchantsSuppressedForMaxMemberIsSetTo(string isPremiumDisabled)
        {
            _scenarioContext.AddTestContext($"isPremiumDisabled:{isPremiumDisabled}");
            var changeType = _scenarioContext["changeType"] as string;
            changeType.ShouldNotBeNull();
            if (changeType == "Merchant")
            {
                var merchantChanged = GetOrCreateCurrentMerchantChangeEvent();
                merchantChanged.IsPremiumDisabled = isPremiumDisabled.ToBoolean();
            }
            else if (changeType == "Offer")
            {
                var offerChanged = GetOrCreateCurrentOfferChangeEvent();
                offerChanged.Merchant.IsPremiumDisabled = isPremiumDisabled.ToBoolean();
            }
        }

        [Given(@"Merchants Tablet Enabled is set to '([^']*)'")]
        public void GivenMerchantsTabletEnabledIsSetTo(string tabletEnabled)
        {
            _scenarioContext.AddTestContext($"tabletEnabled:{tabletEnabled}");
        }

        [Given(@"Merchants Mobile App Enabled is set to '([^']*)'")]
        public void GivenMerchantsMobileAppEnabledIsSetTo(string mobileAppEnabled)
        {
            _scenarioContext.AddTestContext($"mobileAppEnabled:{mobileAppEnabled}");
            var changeType = _scenarioContext["changeType"] as string;
            changeType.ShouldNotBeNull();
            if (changeType == "Merchant")
            {
                var merchantChanged = GetOrCreateCurrentMerchantChangeEvent();
                merchantChanged.MobileAppEnabled = mobileAppEnabled.ToBoolean();
            }
            else if (changeType == "Offer")
            {
                var offerChanged = GetOrCreateCurrentOfferChangeEvent();
                offerChanged.Merchant.MobileAppEnabled = mobileAppEnabled.ToBoolean();
            }
        }

        [Given(@"Offer is mapped to '([^']*)'")]
        public async Task GivenOfferIsMappedTo(string clientsCommaSeparated)
        {
            _scenarioContext.AddTestContext($"clientsCommaSeparated:{clientsCommaSeparated}");
            _fixture.ShouldNotBeNull();
            var offerChanged = _scenarioContext.GetCurrentByType<OfferChanged>();
            offerChanged.ShouldNotBeNull();

            foreach (var client in clientsCommaSeparated.Split(',').Select(s => s.ToClient()))
            {
                offerChanged.Client = client;
                await _fixture.GivenEvent(offerChanged);
            }
        }

        [Given(@"Merchant is mapped to '([^']*)'")]
        public async Task GivenMerchantIsMappedTo(string clientsCommaSeparated)
        {
            _scenarioContext.AddTestContext($"clientsCommaSeparated:{clientsCommaSeparated}");
            _fixture.ShouldNotBeNull();
            var merchantChanged = _scenarioContext.GetCurrentByType<MerchantChanged>();
            merchantChanged.ShouldNotBeNull();

            foreach (var client in clientsCommaSeparated.Split(',').Select(s => s.ToClient()))
            {
                merchantChanged.Client = client;
                await _fixture.GivenEvent(merchantChanged);
            }
        }

        [When(@"I call the api")]
        public async Task WhenICallTheApi()
        {
            await SendPendingChangeEvent();

            _fixture.ShouldNotBeNull();
            _scenarioContext.SetCurrentByType(
                await _fixture.WhenISendTheQuery<GetAnzItemsQuery, GetAnzItemsResponse>(new GetAnzItemsQuery
                {
                    OffersPerPage = 9999,
                    PageNumber = 1,
                    UpdatedAfter = null
                }));
        }

        private async Task SendPendingChangeEvent()
        {
            if (_scenarioContext.TryGetValue("changeType", out string changeType))
            {
                _fixture.ShouldNotBeNull();
                changeType.ShouldNotBeNull();
                if (changeType == "Merchant")
                {
                    var merchantChanged = _scenarioContext.GetCurrentByType<MerchantChanged>();
                    merchantChanged.ShouldNotBeNull();
                    await _fixture.GivenEvent(merchantChanged);
                }
                else if (changeType == "Offer")
                {
                    var offerChanged = _scenarioContext.GetCurrentByType<OfferChanged>();
                    offerChanged.ShouldNotBeNull();
                    await _fixture.GivenEvent(offerChanged);
                }
            }
        }

        private OfferChanged GetOrCreateCurrentOfferChangeEvent()
        {
            var offerChanged = _scenarioContext.GetCurrentByType<OfferChanged>();
            if (offerChanged == null)
            {
                offerChanged = new OfferChanged
                {
                    Id = "100-300",
                    OfferId = 300,
                    Client = Client.Cashrewards,
                    Merchant = new OfferMerchantChanged
                    {
                        Id = 100,
                        MobileEnabled = true
                    },
                };

                _scenarioContext.SetCurrentByType(offerChanged);
            }

            return offerChanged;
        }

        private MerchantChanged GetOrCreateCurrentMerchantChangeEvent()
        {
            var merchantChanged = _scenarioContext.GetCurrentByType<MerchantChanged>();
            if (merchantChanged == null)
            {
                merchantChanged = new MerchantChanged
                {
                    Id = "100",
                    MerchantId = 100,
                    Client = Client.Cashrewards,
                    MobileEnabled = true
                };

                _scenarioContext.SetCurrentByType(merchantChanged);
            }

            return merchantChanged;
        }
    }
}
