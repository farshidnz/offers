using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrastructure.Persistence;
using CashrewardsOffers.Infrastructure.Services;
using CashrewardsOffers.Infrustructure.UnitTests.Helpers;
using MassTransit.Mediator;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Services
{
    public class DomainEventServiceTests
    {
        private class TestState
        {
            public DomainEventService DomainEventService { get; }

            public List<DomainEvent> Events { get; } = new();

            public List<IAWSEventService> Publishers { get; } = new();

            public Mock<IEventOutboxPersistenceContext> EventOutboxPersistence { get; } = new();

            public Mock<IMediator> Mediator { get; } = new();

            private readonly Mock<IAWSEventServiceFactory> _eventServiceFactory = new();

            public Mock<IMongoLockService> MongoLockService { get; } = new();

            public TestState()
            {
                _eventServiceFactory.Setup(x => x.GetAWSPublishersForEvent(It.IsAny<DomainEvent>())).Returns(Publishers);
                EventOutboxPersistence.Setup(x => x.GetNext()).ReturnsAsync(() => Events.FirstOrDefault());
                EventOutboxPersistence.Setup(x => x.Delete(It.IsAny<Guid>())).Callback((Guid eventId) => Events.Remove(Events.Single(e => e.Metadata.EventID == eventId)));
                MongoLockService.Setup(x => x.LockAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

                DomainEventService = new DomainEventService(EventOutboxPersistence.Object, Mediator.Object, _eventServiceFactory.Object, MongoLockService.Object);
            }
        }

        [Test]
        public async Task PublishEventOutbox_ShouldPublishPendingOutgoingEvents_AndBeRemovedFromOutbox()
        {
            var state = new TestState();
            state.Events.Add(new TestEvent { SomeValue = "bewm" });
            var externalSNSPublisher = new Mock<IAWSEventService>();
            state.Publishers.Add(externalSNSPublisher.Object);
            var externalSQSPublisher = new Mock<IAWSEventService>();
            state.Publishers.Add(externalSQSPublisher.Object);

            await state.DomainEventService.PublishEventOutbox();

            state.Mediator.Verify(x => x.Publish(It.Is<object>(e => IsDomainEventWithSomeValue(e, "bewm")), default));
            externalSNSPublisher.Verify(x => x.Publish(It.Is<TestEvent>(e => e.SomeValue == "bewm")));
            externalSQSPublisher.Verify(x => x.Publish(It.Is<TestEvent>(e => e.SomeValue == "bewm")));
            state.EventOutboxPersistence.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);
        }

        [Test]
        public async Task PublishEventOutbox_ShouldNotRemoveEventFromOutbox_WhenExceptionInPublishingEvent()
        {
            var state = new TestState();
            state.Events.Add(new TestEvent { SomeValue = "bewm" });
            var externalSNSPublisher = new Mock<IAWSEventService>();
            externalSNSPublisher.Setup(x => x.Publish(It.IsAny<DomainEvent>())).Throws(new Exception("error publishing"));
            state.Publishers.Add(externalSNSPublisher.Object);

            await state.DomainEventService.PublishEventOutbox();

            state.EventOutboxPersistence.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Never);
        }

        private static bool IsDomainEventWithSomeValue(object notification, string expectedSomeValue)
        {
            var domainEvent = (notification as DomainEventNotification<TestEvent>).DomainEvent;
            return domainEvent.SomeValue == expectedSomeValue;
        }
    }
}
