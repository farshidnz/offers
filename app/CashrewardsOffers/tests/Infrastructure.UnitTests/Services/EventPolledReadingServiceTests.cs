using Amazon.SQS.Model;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrastructure.BackgroundHostedService;
using CashrewardsOffers.Infrustructure.UnitTests.Helpers;
using MassTransit.Mediator;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Services
{
    public class EventPolledReadingServiceTests
    {
        private class TestState
        {
            public EventPolledReadingService EventPolledReadingService { get; }

            public Mock<IMediator> Mediator { get; } = new();

            public Mock<IAWSEventService> EventReader { get; } = new();

            public Mock<IAWSEventServiceFactory> EventServiceFactory { get; } = new();

            public TestState()
            {
                EventReader.Setup(x => x.EventTypes).Returns(new List<Type> { typeof(TestEvent) });
                EventReader.Setup(x => x.ReadEventStream(It.IsAny<CancellationToken>())).Returns(GetTestEvent());

                EventServiceFactory = new Mock<IAWSEventServiceFactory>();
                EventServiceFactory.Setup(x => x.GetAWSEventReaders(AwsEventReadMode.PolledRead)).Returns(new List<IAWSEventService>() { EventReader.Object });

                EventPolledReadingService = new EventPolledReadingService(EventServiceFactory.Object, Mediator.Object);
            }
        }

        private static async IAsyncEnumerable<SQSEvent> GetTestEvent(DomainEvent domainEvent = null, Message message = null)
        {
            message ??= new Message
            {
                MessageAttributes = new()
                {
                    ["EventType"] = new MessageAttributeValue { StringValue = domainEvent?.GetType().Name },
                    ["EventID"] = new MessageAttributeValue { StringValue = domainEvent.Metadata.EventID.ToString() }
                }
            };

            var @event = new SQSEvent(domainEvent, message);

            yield return @event;

            await Task.CompletedTask;
        }

        [Test]
        public async Task PolledReadEvents_ShouldDoNothing_GivenNoEventPolledReaders()
        {
            var state = new TestState();
            state.EventServiceFactory.Setup(x => x.GetAWSEventReaders(AwsEventReadMode.PolledRead)).Returns(new List<IAWSEventService>());

            await state.EventPolledReadingService.PolledReadEvents(default);

            state.EventReader.Verify(x => x.ReadEventStream(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task PolledReadEvents_ShouldPublishEventToConsumers_AndShouldDeleteEventAfter_GivenEventReadFromSQSqueue()
        {
            var state = new TestState();
            var testEvent = new TestEvent { SomeValue = "123" };
            state.EventReader.Setup(x => x.ReadEventStream(It.IsAny<CancellationToken>())).Returns(GetTestEvent(testEvent));

            await state.EventPolledReadingService.PolledReadEvents(default);

            state.Mediator.Verify(x => x.Publish(It.Is<object>(x => IsDomainEventNotifaction(x, "123")), default));
            state.EventReader.Verify(x => x.DeleteEvent(It.Is<SQSEvent>(x => x.DomainEvent == testEvent)));
        }

        [Test]
        public async Task PolledReadEvents_ShouldNotPublishEventToConsumers_ButShouldRemoveTheUnknownEvent_GivenInvalidDomainEvent()
        {
            var state = new TestState();
            var unknownEventMessage = new Message { MessageId = "unknownEventMessage" };
            state.EventReader.Setup(x => x.ReadEventStream(It.IsAny<CancellationToken>())).Returns(GetTestEvent(domainEvent: null, unknownEventMessage));

            await state.EventPolledReadingService.PolledReadEvents(default);

            state.Mediator.Verify(x => x.Publish(It.IsAny<object>(), default), Times.Never);
            state.EventReader.Verify(x => x.DeleteEvent(It.Is<SQSEvent>(x => x.Message == unknownEventMessage)));
        }

        [Test]
        public async Task WhenEventReadFromSQSqueue_AndErrorInPublishing_ShouldNotDeleteEvent()
        {
            var state = new TestState();
            var testEvent = new TestEvent { SomeValue = "123" };
            state.EventReader.Setup(x => x.ReadEventStream(It.IsAny<CancellationToken>())).Returns(GetTestEvent(testEvent));
            state.Mediator.Setup(x => x.Publish(It.IsAny<object>(), default)).Throws(new Exception());

            await state.EventPolledReadingService.PolledReadEvents(default);

            state.EventReader.Verify(x => x.DeleteEvent(It.Is<SQSEvent>(x => x.DomainEvent == testEvent)), Times.Never);
        }

        private static bool IsDomainEventNotifaction(object notification, string expectedSomeValue)
        {
            var domainEvent = (notification as DomainEventNotification<TestEvent>).DomainEvent;
            return domainEvent.SomeValue == expectedSomeValue;
        }

    }
}
