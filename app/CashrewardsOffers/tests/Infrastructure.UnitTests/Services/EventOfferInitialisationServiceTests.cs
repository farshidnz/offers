using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Infrastructure.Services;
using Mapster;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Services
{
    public class EventOfferInitialisationServiceTests
    {
        private class TestState
        {
            public EventOfferInitialisationService EventOfferInitialisationService { get; set; }

            public Mock<IEventOutboxPersistenceContext> EventOutboxPersistenceContext { get; } = new();
            Mock<IOffersPersistenceContext> OffersPersistenceContext { get; } = new();

            List<Offer> _offers = new()
            {
                new Offer { OfferId = 123, Client = Client.Cashrewards },
                new Offer { OfferId = 234, Client = Client.Cashrewards },
                new Offer { OfferId = 345, Client = Client.Anz },
                new Offer { OfferId = 456, Client = Client.Cashrewards, PremiumClient = Client.Anz },
            };

            public TestState()
            {
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Domain"));

                OffersPersistenceContext.Setup(m => m.GetAllOffers()).ReturnsAsync(_offers);

                EventOfferInitialisationService = new(OffersPersistenceContext.Object, EventOutboxPersistenceContext.Object);
            }
        }

        [Test]
        public async Task CheckForInitialisationRequests_ShouldGenerateEvents_GivenTriggered()
        {
            var state = new TestState();
            state.EventOfferInitialisationService.IsTriggered = true;
            Infrastructure.DependencyInjection.RegisterMappingProfiles();
            await state.EventOfferInitialisationService.CheckForInitialisationRequests();

            state.EventOutboxPersistenceContext.Verify(e => e.Append(It.IsAny<OfferInitial>()));
        }

        [Test]
        public async Task CheckForInitialisationRequests_ShouldGenerateEventsForCashrewardsOnly_GivenTriggered()
        {
            var state = new TestState();
            state.EventOfferInitialisationService.IsTriggered = true;
            Infrastructure.DependencyInjection.RegisterMappingProfiles();
            await state.EventOfferInitialisationService.CheckForInitialisationRequests();

            state.EventOutboxPersistenceContext.Verify(e => e.Append(It.IsAny<OfferInitial>()), Times.Exactly(2));
        }

        [Test]
        public async Task CheckForInitialisationRequests_ShouldNotRun_GivenNotTriggered()
        {
            var state = new TestState();

            await state.EventOfferInitialisationService.CheckForInitialisationRequests();

            state.EventOutboxPersistenceContext.Verify(e => e.Append(It.IsAny<OfferInitial>()), Times.Never);
        }
    }
}
