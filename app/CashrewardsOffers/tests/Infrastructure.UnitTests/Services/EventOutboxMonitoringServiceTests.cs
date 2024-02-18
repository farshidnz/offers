using CashrewardsOffers.Infrastructure.BackgroundHostedService;
using CashrewardsOffers.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Services
{
    public class EventOutboxMonitoringServiceTests
    {
        private class TestState
        {
            private Mock<EventOutboxMonitoringService> _mockEventOutboxMonitoringService { get; }
            public EventOutboxMonitoringService EventOutboxMonitoringService => _mockEventOutboxMonitoringService.Object;

            public Mock<IDomainEventService> DomainEventService { get; } = new();

            public TestState()
            {
                // partial mock
                _mockEventOutboxMonitoringService = new Mock<EventOutboxMonitoringService>(Mock.Of<IServiceProvider>())
                {
                    CallBase = true
                };
                _mockEventOutboxMonitoringService.Setup(x => x.CreateServiceScope()).Returns(Mock.Of<IServiceScope>());
                _mockEventOutboxMonitoringService.Setup(x => x.GetDomainEventService(It.IsAny<IServiceScope>())).Returns(DomainEventService.Object);
                _mockEventOutboxMonitoringService.SetupSequence(x => x.CancellationRequested(It.IsAny<CancellationToken>()))
                    .Returns(false)
                    .Returns(true);
                _mockEventOutboxMonitoringService.Setup(x => x.TakeBreakBeforeResumingMonitoring(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            }
        }

        [Test]
        public void StartAsync_ShouldPeriodicallyPublishAnyPendingOutgoingEvents()
        {
            var state = new TestState();

            state.EventOutboxMonitoringService.StartAsync(default);

            state.DomainEventService.Verify(x => x.PublishEventOutbox());
        }
    }
}
