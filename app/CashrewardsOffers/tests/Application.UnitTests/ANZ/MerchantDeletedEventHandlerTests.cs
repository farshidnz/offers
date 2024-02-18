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
    public class MerchantDeletedEventHandlerTests
    {
        private class TestState
        {
            public MerchantDeletedEventHandler MerchantDeletedEventHandler { get; }

            public Mock<IAnzUpdateService> AnzUpdateService { get; } = new();

            public TestState()
            {
                MerchantDeletedEventHandler = new MerchantDeletedEventHandler(AnzUpdateService.Object);
            }

            public async Task WhenConsumeMerchantDeletedEvent(MerchantDeleted merchantDeleted)
            {
                var context = new Mock<ConsumeContext<DomainEventNotification<MerchantDeleted>>>();
                context.Setup(c => c.Message).Returns(new DomainEventNotification<MerchantDeleted>(merchantDeleted, null));
                await MerchantDeletedEventHandler.Consume(context.Object);
            }
        }

        [Test]
        public async Task Consume_ShouldCallDeleteMerchant_GivenMerchantDeletedEvent()
        {
            var state = new TestState();

            await state.WhenConsumeMerchantDeletedEvent(new MerchantDeleted
            {
                MerchantId = 123, Client = Client.Cashrewards
            });

            state.AnzUpdateService.Verify(p => p.DeleteMerchant(It.IsAny<MerchantDeleted>()), Times.Once);
        }
    }
}
