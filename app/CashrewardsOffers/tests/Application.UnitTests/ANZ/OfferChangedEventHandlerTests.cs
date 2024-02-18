using CashrewardsOffers.Application.ANZ.EventHandlers;
using CashrewardsOffers.Application.ANZ.Services;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using MassTransit;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.ANZ
{
    public class OfferChangedEventHandlerTests
    {
        private class TestState
        {
            public OfferChangedEventHandler OfferChangedEventHandler { get; }

            public Mock<IAnzUpdateService> AnzUpdateService { get; } = new();

            public TestState()
            {
                OfferChangedEventHandler = new OfferChangedEventHandler(AnzUpdateService.Object);
            }

            public async Task WhenConsumeOfferChangedEvent(OfferChanged offerChanged)
            {
                var context = new Mock<ConsumeContext<DomainEventNotification<OfferChanged>>>();  
                context.Setup(c => c.Message).Returns(new DomainEventNotification<OfferChanged>(offerChanged, null));
                await OfferChangedEventHandler.Consume(context.Object);
            }
        }

        [Test]
        public async Task Consume_ShouldCallUpdateOffer_GivenOfferChangedEvent()
        {
            var state = new TestState();

            await state.WhenConsumeOfferChangedEvent(new OfferChanged
            {
                Merchant = new OfferMerchantChanged { Id = 123 }, OfferId = 234, Client = Client.Cashrewards
            });

            state.AnzUpdateService.Verify(p => p.UpdateOffer(It.IsAny<OfferEventBase>()), Times.Once);
        }
    }
}
