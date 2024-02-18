using Amazon.SQS.Model;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrustructure.UnitTests.Helpers;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.AWS
{
    [TestFixture]
    public class InMemoryQueueTests
    {
        private class TestState
        {
            public Mock<InMemoryQueue> MockInMemoryQueue { get; }
            public InMemoryQueue InMemoryQueue => MockInMemoryQueue.Object;

            public static AWSEventDestination AwsEventDestination => new()
            {
                Type = "SQS",
                Domain = "Member",
                EventType = typeof(TestEvent)
            };

            public static AWSEventDestination AwsEventSource = new()
            {
                Type = "SQS",
                Domain = "Member",
                EventType = typeof(TestEvent)
            };

            public TestState()
            {
                MockInMemoryQueue = new Mock<InMemoryQueue>()
                {
                    CallBase = true
                };

                MockInMemoryQueue.SetupSequence(x => x.CancellationRequested(It.IsAny<CancellationToken>()))
                    .Returns(false)
                    .Returns(true);

                MockInMemoryQueue.Setup(x => x.TakeBreakBeforePollingForEvents(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }
        }

        [Test]
        public async Task Publish_ShouldQueueEvent()
        {
            var state = new TestState();
            var domainEvent = new TestEvent { SomeValue = "state info" };
            domainEvent.Metadata.EventID = Guid.NewGuid();
            domainEvent.Metadata.EventType = domainEvent.GetType().Name;
            domainEvent.Metadata.EventSource = "CashrewardsOffers";
            domainEvent.Metadata.Domain = "Test";
            domainEvent.Metadata.CorrelationID = Guid.NewGuid();
            domainEvent.Metadata.ContextualSequenceNumber = 987654321;
            domainEvent.Metadata.RaisedDateTimeUTC = DateTime.UtcNow;
            domainEvent.Metadata.PublishedDateTimeUTC = DateTime.UtcNow;

            await state.InMemoryQueue.Publish(domainEvent);

            var events = await (state.InMemoryQueue.ReadEventStream(new CancellationToken())).ToListAsync();
            events.Should().BeEquivalentTo(new List<SQSEvent>
            {
                new SQSEvent(domainEvent, new Message
                {
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        [EventMessageAttributes.EventID.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.EventID.ToString(), DataType = "String" },
                        [EventMessageAttributes.EventType.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.EventType, DataType = "String" },
                        [EventMessageAttributes.EventSource.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.EventSource, DataType = "String" },
                        [EventMessageAttributes.Domain.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.Domain, DataType = "String" },
                        [EventMessageAttributes.CorrelationID.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.CorrelationID.ToString(), DataType = "String" },
                        [EventMessageAttributes.ContextualSequenceNumber.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.ContextualSequenceNumber.ToString(), DataType = "String" },
                        [EventMessageAttributes.EventRaisedDateTimeUTC.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.RaisedDateTimeUTC.ToString("o"), DataType = "String" },
                        [EventMessageAttributes.EventPublishedDateTimeUTC.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.PublishedDateTimeUTC.ToString("o"), DataType = "String" }
                    }
                })
            });
        }

        [Test]
        public async Task ReadEventStream_ShouldTakeBreakBeforeContinuingToResumeReadingQueue_GivenNoEventsInQueue()
        {
            var state = new TestState();

            await foreach (var message in state.InMemoryQueue.ReadEventStream(default)) ;

            state.MockInMemoryQueue.Verify(x => x.TakeBreakBeforePollingForEvents(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
