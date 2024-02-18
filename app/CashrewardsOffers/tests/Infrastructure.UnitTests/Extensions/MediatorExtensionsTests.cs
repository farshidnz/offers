using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Infrastructure.Extensions;
using CashrewardsOffers.Infrustructure.UnitTests.Helpers;
using MassTransit.Mediator;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Extensions
{
    [TestFixture]
    public class MediatorExtensionsTests
    {
        public class TestState
        {
            public Mock<IMediator> MockMediator { get; } = new();

            public IMediator Mediator => MockMediator.Object;
        }

        [Test]
        public async Task Publish_ShouldPopulateEventMetadata_GivenInternalEventPublish()
        {
            var state = new TestState();
            var domainEvent = new TestEvent();
            var eventID = Guid.NewGuid();
            var raisedDateTime = DateTimeOffset.UtcNow;
            var publishedDateTime = DateTimeOffset.UtcNow;
            var correlationID = Guid.NewGuid();
            ulong seqNum = 9876543210;
            var sqsMessage = new Amazon.SQS.Model.Message()
            {
                MessageAttributes = new Dictionary<string, Amazon.SQS.Model.MessageAttributeValue> {
                    ["EventID"                  ] = new Amazon.SQS.Model.MessageAttributeValue { StringValue = eventID.ToString() },
                    ["EventType"                ] = new Amazon.SQS.Model.MessageAttributeValue { StringValue = "TestEvent" },
                    ["EventSource"              ] = new Amazon.SQS.Model.MessageAttributeValue { StringValue = "CashrewardsOffers" },
                    ["Domain"                   ] = new Amazon.SQS.Model.MessageAttributeValue { StringValue = "Test" },
                    ["CorrelationID"            ] = new Amazon.SQS.Model.MessageAttributeValue { StringValue = correlationID.ToString() },
                    ["EventRaisedDateTimeUTC"   ] = new Amazon.SQS.Model.MessageAttributeValue { StringValue = raisedDateTime.ToString("o") },
                    ["EventPublishedDateTimeUTC"] = new Amazon.SQS.Model.MessageAttributeValue { StringValue = publishedDateTime.ToString("o") },
                    ["ContextualSequenceNumber" ] = new Amazon.SQS.Model.MessageAttributeValue { StringValue = seqNum.ToString() }
                }
            };

            await state.Mediator.PublishEvent(domainEvent, sqsMessage);

            state.MockMediator.Verify(x => x.Publish(It.Is<object>(
                x => hasExpectedMetadata(x, m =>
                    m.EventType == "TestEvent" &&
                    m.EventSource == "CashrewardsOffers" &&
                    m.Domain == "Test" &&
                    m.EventID == eventID &&
                    m.CorrelationID == correlationID &&
                    m.RaisedDateTimeUTC == raisedDateTime &&
                    m.PublishedDateTimeUTC == publishedDateTime &&
                    m.ContextualSequenceNumber == seqNum)), default));
        }

        private bool hasExpectedMetadata(object notification, Func<EventMetadata, bool> comparator)
        {
            var domainEvent = (notification as DomainEventNotification<TestEvent>).DomainEvent;
            return comparator(domainEvent.Metadata);
        }
    }
}
