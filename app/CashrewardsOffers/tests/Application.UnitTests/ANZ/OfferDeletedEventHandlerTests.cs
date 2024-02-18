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
    public class OfferDeletedEventHandlerTests
    {
        private class TestState
        {
            public OfferDeletedEventHandler OfferDeletedEventHandler { get; }

            public Mock<IAnzUpdateService> AnzUpdateService { get; } = new();

            public TestState()
            {
                OfferDeletedEventHandler = new OfferDeletedEventHandler(AnzUpdateService.Object);
            }

            public async Task WhenConsumeOfferDeletedEvent(OfferDeleted offerDeleted)
            {
                var context = new Mock<ConsumeContext<DomainEventNotification<OfferDeleted>>>();  
                context.Setup(c => c.Message).Returns(new DomainEventNotification<OfferDeleted>(offerDeleted, null));
                await OfferDeletedEventHandler.Consume(context.Object);
            }
        }

        [Test]
        public async Task Consume_ShouldCallUpdateOffer_GivenOfferDeletedEvent()
        {
            var state = new TestState();

            await state.WhenConsumeOfferDeletedEvent(new OfferDeleted
            {
                Merchant = new OfferMerchantDeleted { Id = 123 }, OfferId = 234, Client = Client.Cashrewards
            });

            state.AnzUpdateService.Verify(p => p.DeleteOffer(It.IsAny<OfferDeleted>()), Times.Once);
        }
    }
}
