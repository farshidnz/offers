using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Infrastructure.BackgroundHostedService;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Services
{
    public class EventMerchantInitialisationBackgroundServiceTests
    {
        private class TestState
        {
            public Mock<EventMerchantInitialisationBackgroundService> EventMerchantInitialisationBackgroundServiceMock { get; } = new();

            public EventMerchantInitialisationBackgroundService EventMerchantInitialisationBackgroundService => EventMerchantInitialisationBackgroundServiceMock.Object;

            public Mock<IEventInitialisationService<MerchantInitial>> EventInitialisationService { get; } = new();

            public TestState()
            {
                EventMerchantInitialisationBackgroundServiceMock = new Mock<EventMerchantInitialisationBackgroundService>(EventInitialisationService.Object)
                {
                    CallBase = true
                };

                EventMerchantInitialisationBackgroundServiceMock.SetupSequence(x => x.CancellationRequested(It.IsAny<CancellationToken>()))
                    .Returns(false)
                    .Returns(true);
                EventMerchantInitialisationBackgroundServiceMock.Setup(x => x.TakeBreakBeforeResumingMonitoring(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            }
        }

        [Test]
        public async Task ExecuteAsync_ShouldCallEventInitialisationService()
        {
            var state = new TestState();

            await state.EventMerchantInitialisationBackgroundService.StartAsync(default);

            state.EventInitialisationService.Verify(e => e.CheckForInitialisationRequests(), Times.Once);
        }
    }
}
