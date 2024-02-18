using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Infrastructure.BackgroundHostedService;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Services
{
    public class EventOfferInitialisationBackgroundServiceTests
    {
        private class TestState
        {
            public Mock<EventOfferInitialisationBackgroundService> EventOfferInitialisationBackgroundServiceMock { get; } = new();

            public EventOfferInitialisationBackgroundService EventOfferInitialisationBackgroundService => EventOfferInitialisationBackgroundServiceMock.Object;

            public Mock<IEventInitialisationService<OfferInitial>> EventInitialisationService { get; } = new();

            public TestState()
            {
                EventOfferInitialisationBackgroundServiceMock = new Mock<EventOfferInitialisationBackgroundService>(EventInitialisationService.Object)
                {
                    CallBase = true
                };

                EventOfferInitialisationBackgroundServiceMock.SetupSequence(x => x.CancellationRequested(It.IsAny<CancellationToken>()))
                    .Returns(false)
                    .Returns(true);
                EventOfferInitialisationBackgroundServiceMock.Setup(x => x.TakeBreakBeforeResumingMonitoring(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            }
        }

        [Test]
        public async Task ExecuteAsync_ShouldCallEventInitialisationService()
        {
            var state = new TestState();

            await state.EventOfferInitialisationBackgroundService.StartAsync(default);

            state.EventInitialisationService.Verify(e => e.CheckForInitialisationRequests(), Times.Once);
        }
    }
}
