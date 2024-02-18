using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Infrastructure.Services;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Services
{
    public class EventMerchantInitialisationServiceTests
    {
        private class TestState
        {
            public EventMerchantInitialisationService EventMerchantInitialisationService { get; set; }

            public Mock<IEventOutboxPersistenceContext> EventOutboxPersistenceContext { get; } = new();
            Mock<IMerchantsPersistenceContext> MerchantsPersistenceContext { get; } = new();

            List<Merchant> _merchants = new()
            {
                new Merchant { MerchantId = 123, Client = Client.Cashrewards },
                new Merchant { MerchantId = 234, Client = Client.Cashrewards },
                new Merchant { MerchantId = 345, Client = Client.Cashrewards, PremiumClient = Client.Anz },
                new Merchant { MerchantId = 456, Client = Client.Anz },
            };

            public TestState()
            {
                MerchantsPersistenceContext.Setup(m => m.GetAllMerchants()).ReturnsAsync(_merchants);

                EventMerchantInitialisationService = new(MerchantsPersistenceContext.Object, EventOutboxPersistenceContext.Object);
            }
        }

        [Test]
        public async Task CheckForInitialisationRequests_ShouldGenerateEvents_GivenTriggered()
        {
            var state = new TestState();
            state.EventMerchantInitialisationService.IsTriggered = true;

            await state.EventMerchantInitialisationService.CheckForInitialisationRequests();

            state.EventOutboxPersistenceContext.Verify(e => e.Append(It.IsAny<MerchantInitial>()));
        }

        [Test]
        public async Task CheckForInitialisationRequests_ShouldGenerateEventsForCashrewardsOnly_GivenTriggered()
        {
            var state = new TestState();
            state.EventMerchantInitialisationService.IsTriggered = true;

            await state.EventMerchantInitialisationService.CheckForInitialisationRequests();

            state.EventOutboxPersistenceContext.Verify(e => e.Append(It.IsAny<MerchantInitial>()), Times.Exactly(2));
        }

        [Test]
        public async Task CheckForInitialisationRequests_ShouldNotRun_GivenNotTriggered()
        {
            var state = new TestState();

            await state.EventMerchantInitialisationService.CheckForInitialisationRequests();

            state.EventOutboxPersistenceContext.Verify(e => e.Append(It.IsAny<MerchantInitial>()), Times.Never);
        }
    }
}
