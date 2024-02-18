using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrustructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.AWS
{
    [TestFixture]
    public class SQSEventServiceTests
    {
        private class TestState
        {
            public SQSEventService SQSEventService => MockSQSEventService.Object;

            public Mock<SQSEventService> MockSQSEventService { get; }

            public Mock<IAmazonSQS> SqsClient { get; } = new();

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

            public string QueueUrl => "test-offers-Member";

            public TestState(AWSEventDestination awsEventResource)
            {
                var configuration = new Mock<IConfiguration>();
                configuration.SetupGet(p => p[It.Is<string>(s => s == "Environment")]).Returns("test");
                configuration.SetupGet(p => p[It.Is<string>(s => s == "ServiceName")]).Returns("microservicetemplatenet5");

                SqsClient.Setup(x => x.Config.RegionEndpoint).Returns(RegionEndpoint.APSoutheast2);
                SqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default)).ReturnsAsync(new SendMessageResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                });
                SqsClient.Setup(x => x.GetQueueUrlAsync(QueueUrl, default)).ReturnsAsync(new GetQueueUrlResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    QueueUrl = "sqsURL"
                });
                SqsClient.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), default)).ReturnsAsync(new DeleteMessageResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                });

                MockSQSEventService = new Mock<SQSEventService>(configuration.Object, AwsEventDestination.Domain, new List<AWSEventResource> { awsEventResource }, SqsClient.Object)
                {
                    CallBase = true
                };
                MockSQSEventService.SetupSequence(x => x.CancellationRequested(It.IsAny<CancellationToken>()))
                    .Returns(false)
                    .Returns(true);
                MockSQSEventService.Setup(x => x.TakeBreakBeforePollingForEvents(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }
        }

        [Test]
        public async Task Publish_ShouldFindSQSUrl_AndPublishEventToURL()
        {
            var state = new TestState(TestState.AwsEventDestination);

            var domainEvent = new TestEvent { SomeValue = "state info" };
            domainEvent.Metadata.EventID = Guid.NewGuid();
            domainEvent.Metadata.EventType = domainEvent.GetType().Name;
            domainEvent.Metadata.EventSource = "CashrewardsOffers";
            domainEvent.Metadata.Domain = "Test";
            domainEvent.Metadata.CorrelationID = Guid.NewGuid();
            domainEvent.Metadata.ContextualSequenceNumber = 987654321;
            domainEvent.Metadata.RaisedDateTimeUTC = DateTime.UtcNow;
            domainEvent.Metadata.PublishedDateTimeUTC = DateTime.UtcNow;

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new EventPublishingJsonContractResolver(propNamesToIgnore: new[] { "Metadata" })
            };

            await state.SQSEventService.Publish(domainEvent);

            state.SqsClient.Verify(x => x.SendMessageAsync(It.Is<SendMessageRequest>(x =>
                x.QueueUrl == "sqsURL" &&
                x.MessageBody == JsonConvert.SerializeObject(domainEvent, settings) &&
                x.MessageAttributes["EventID"].StringValue == domainEvent.Metadata.EventID.ToString() &&
                x.MessageAttributes["EventType"].StringValue == domainEvent.Metadata.EventType &&
                x.MessageAttributes["EventSource"].StringValue == domainEvent.Metadata.EventSource &&
                x.MessageAttributes["Domain"].StringValue == domainEvent.Metadata.Domain &&
                x.MessageAttributes["CorrelationID"].StringValue == domainEvent.Metadata.CorrelationID.ToString() &&
                x.MessageAttributes["ContextualSequenceNumber"].StringValue == "987654321" &&
                x.MessageAttributes["EventRaisedDateTimeUTC"].StringValue == domainEvent.Metadata.RaisedDateTimeUTC.ToString("o") &&
                x.MessageAttributes["EventPublishedDateTimeUTC"].StringValue == domainEvent.Metadata.PublishedDateTimeUTC.ToString("o")), default));
        }

        [Test]
        public async Task Publish_ShouldThowURLNotFoundException_GivenCantFindSQSUrl()
        {
            var state = new TestState(TestState.AwsEventDestination);
            state.SqsClient.Setup(x => x.GetQueueUrlAsync(state.QueueUrl, default)).ReturnsAsync(new GetQueueUrlResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.NotFound
            });

            await FluentActions.Invoking(() => state.SQSEventService.Publish(new TestEvent()))
                .Should().ThrowAsync<Exception>()
                .WithMessage(@$"*Error finding URL for SQS with name "":{state.QueueUrl}"" in region :{RegionEndpoint.APSoutheast2}*");
        }

        [Test]
        public async Task Publish_ShouldThowPublishingErrorException_GivenExceptionInPublishing()
        {
            var state = new TestState(TestState.AwsEventDestination);
            var exception = new Exception("some publishing error");
            state.SqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default)).Throws(exception);

            var domainEvent = new TestEvent();
            await FluentActions.Invoking(() => state.SQSEventService.Publish(domainEvent))
                .Should().ThrowAsync<Exception>()
                .WithMessage($"Error publishing domain event: {JsonConvert.SerializeObject(domainEvent)} to SQS {state.QueueUrl}, error: {exception}*");
        }

        [Test]
        public async Task Publish_ShouldThowPublishingErrorException_GivenErrorResponseInPublishing()
        {
            var state = new TestState(TestState.AwsEventDestination);
            state.SqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default)).ReturnsAsync(new SendMessageResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
            });

            var domainEvent = new TestEvent();
            var exception = new Exception($"Publish Http response code {System.Net.HttpStatusCode.InternalServerError}");
            await FluentActions.Invoking(() => state.SQSEventService.Publish(domainEvent))
                .Should().ThrowAsync<Exception>()
                .WithMessage($"Error publishing domain event: {JsonConvert.SerializeObject(domainEvent)} to SQS {state.QueueUrl}, error: {exception}*");
        }

        [Test]
        public async Task ReadEvents_ShouldReturnEventsFromSqsQueue()
        {
            var state = new TestState(TestState.AwsEventSource);
            var testEvent = new TestEvent { SomeValue = "test" };
            state.SqsClient.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReceiveMessageResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Messages = new List<Message> {
                        new Message {
                            Body = JsonConvert.SerializeObject(testEvent),
                            MessageAttributes = new Dictionary<string, MessageAttributeValue>
                            {
                                ["EventType"] = new MessageAttributeValue { StringValue = "TestEvent", DataType = "String" }
                            }
                        }
                    }
                });

            var readEvents = await state.SQSEventService.ReadEvents();

            readEvents.Count().Should().Be(1);
            var readEvent = readEvents.First().DomainEvent;
            readEvent.GetType().Should().Be(typeof(TestEvent));
            (readEvent as TestEvent).SomeValue.Should().Be(testEvent.SomeValue);
        }

        [Test]
        public async Task ReadEvents_ShouldReturnInvalidNullDomainEvent_GivenUnknownEventType()
        {
            var state = new TestState(TestState.AwsEventSource);
            var testEvent = new TestEvent { SomeValue = "test" };
            state.SqsClient.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReceiveMessageResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Messages = new List<Message> {
                        new Message {
                            Body = JsonConvert.SerializeObject(testEvent),
                            MessageAttributes = new Dictionary<string, MessageAttributeValue>
                            {
                                { "EventType", new MessageAttributeValue { StringValue = "Unknown", DataType = "String" }}
                            }
                        }
                    }
                });

            var readEvents = await state.SQSEventService.ReadEvents();

            readEvents.Count().Should().Be(1);
            var readEvent = readEvents.First().DomainEvent;
            readEvent.Should().BeNull();
        }

        [Test]
        public async Task ReadEvents_ShouldThowReadingErrorException_GivenErrorReadingEvent()
        {
            var state = new TestState(TestState.AwsEventSource);
            state.SqsClient.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), default)).ReturnsAsync(new ReceiveMessageResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
            });

            var exception = new Exception($"Read SQS Http response code {System.Net.HttpStatusCode.InternalServerError}");
            await FluentActions.Invoking(() => state.SQSEventService.ReadEvents())
                .Should().ThrowAsync<Exception>()
                .WithMessage($"Error reading from SQS {state.QueueUrl}, error: { exception }*");
        }

        [Test]
        public async Task DeleteEvent_ShouldDeleteEventFromSqsQueue()
        {
            var state = new TestState(TestState.AwsEventSource);
            var testEvent = new TestEvent { SomeValue = "test" };
            var sqsEvent = new SQSEvent(testEvent, new Message { ReceiptHandle = "eventHandle" });

            await state.SQSEventService.DeleteEvent(sqsEvent);

            state.SqsClient.Verify(x => x.DeleteMessageAsync("sqsURL", "eventHandle", default));
        }

        [Test]
        public async Task DeleteEvent_ShouldThowDeletingErrorException_GivenErrorDeletingEvent()
        {
            var state = new TestState(TestState.AwsEventSource);
            var testEvent = new TestEvent() { SomeValue = "test" };
            var sqsEvent = new SQSEvent(testEvent, new Message() { ReceiptHandle = "eventHandle" });
            state.SqsClient.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .ReturnsAsync(new DeleteMessageResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
                });

            var exception = new Exception($"Delete SQS Http response code {System.Net.HttpStatusCode.InternalServerError}");
            await FluentActions.Invoking(() => state.SQSEventService.DeleteEvent(sqsEvent))
                .Should().ThrowAsync<Exception>()
                .WithMessage(@$"Error deleting event ""{sqsEvent.DomainEvent.ToJson()}"" from SQS {state.QueueUrl}, error: {exception}*");
        }

        [Test]
        public async Task ReadEventStream_ShouldReturnEventsFromSqsQueue()
        {
            var state = new TestState(TestState.AwsEventSource);
            state.SqsClient.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ReceiveMessageResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Messages = new List<Message> {
                        new Message {
                            Body = new TestEvent { SomeValue = "1" }.ToJson(),
                            MessageAttributes = new Dictionary<string, MessageAttributeValue>
                            {
                                { "EventType", new MessageAttributeValue { StringValue = "TestEvent", DataType = "String" }}
                            } },
                        new Message {
                            Body = new TestEvent { SomeValue = "2" }.ToJson(),
                            MessageAttributes = new Dictionary<string, MessageAttributeValue>
                            {
                                { "EventType", new MessageAttributeValue { StringValue = "TestEvent", DataType = "String" }}
                            } }
                    }
                }));

            var msgSeq = 1;
            await foreach (var @event in state.SQSEventService.ReadEventStream(default))
            {
                (@event.DomainEvent as TestEvent).SomeValue.Should().Be(msgSeq.ToString());
                msgSeq++;
            }
        }

        [Test]
        public async Task ReadEventStream_ShouldTakeBreakBeforeContinuingToResumeReadingQueue_GivenNoEventsInQueue()
        {
            var state = new TestState(TestState.AwsEventSource);

            await foreach (var message in state.SQSEventService.ReadEventStream(default)) ;

            state.MockSQSEventService.Verify(x => x.TakeBreakBeforePollingForEvents(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
