using CashrewardsOffers.Application.ANZ.EventHandlers;
using CashrewardsOffers.Application.ANZ.Services;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Application.Merchants.EventHandlers;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using MassTransit;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.ANZ
{
    public class MerchantChangedEventHandlerTests
    {
        private class TestState
        {
            public MerchantChangedEventHandler MerchantChangedEventHandler { get; }

            public Mock<IAnzUpdateService> AnzUpdateService { get; } = new();

            public TestState()
            {
                MerchantChangedEventHandler = new MerchantChangedEventHandler(AnzUpdateService.Object);
            }

            public async Task WhenConsumeMerchantChangedEvent(MerchantChanged merchantChanged)
            {
                var context = new Mock<ConsumeContext<DomainEventNotification<MerchantChanged>>>();
                context.Setup(c => c.Message).Returns(new DomainEventNotification<MerchantChanged>(merchantChanged, null));
                await MerchantChangedEventHandler.Consume(context.Object);
            }
        }

        [Test]
        public async Task Consume_ShouldCallUpdateMerchant_GivenMerchantChangedEvent()
        {
            var state = new TestState();

            await state.WhenConsumeMerchantChangedEvent(new MerchantChanged
            {
                MerchantId = 123, Client = Client.Cashrewards
            });

            state.AnzUpdateService.Verify(p => p.UpdateMerchant(It.IsAny<MerchantEventBase>()), Times.Once);
        }
    }
}
