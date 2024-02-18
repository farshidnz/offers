using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrustructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.AWS
{
    [TestFixture]
    public class SNSEventServiceTests
    {
        private class TestState
        {
            public SNSEventService SNSEventService { get; }

            public Mock<IAmazonSimpleNotificationService> SnsClient { get; } = new();

            public TestState()
            {
                var configuration = new Mock<IConfiguration>();
                configuration.SetupGet(p => p[It.Is<string>(s => s == "Environment")]).Returns("test");

                SnsClient.Setup(x => x.Config.RegionEndpoint).Returns(RegionEndpoint.APSoutheast2);
                SnsClient.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), default)).ReturnsAsync(new PublishResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                });

                var awsEventResource = new AWSEventDestination
                {
                    Type = "SNS",
                    Domain = "Member"
                };

                SNSEventService = new SNSEventService(configuration.Object, awsEventResource, SnsClient.Object);
            }
        }

        [Test]
        public async Task Publish_ShouldFindTopicARN_AndShouldPublishEventToARN()
        {
            var state = new TestState();
            state.SnsClient.SetupSequence(x => x.ListTopicsAsync(It.IsAny<string>(), default))
                .ReturnsAsync(new ListTopicsResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    NextToken = "there is more token",
                    Topics = new List<Topic>
                    {
                        new Topic { TopicArn = "region:account:team1-Member" },
                        new Topic { TopicArn = "region:account:team2-Member" },
                        new Topic { TopicArn = "region:account:team3-Member" }
                    }
                })
                .ReturnsAsync(new ListTopicsResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    NextToken = null,
                    Topics = new List<Topic>
                    {
                        new Topic { TopicArn = "region:account:team4-Member" },
                        new Topic { TopicArn = "region:account:team5-Member" },
                        new Topic { TopicArn = "region:account:test-Member" }
                    }
                });

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new EventPublishingJsonContractResolver(propNamesToIgnore: new[] { "Metadata" })
            };

            var domainEvent = new TestEvent();
            domainEvent.Metadata.EventType = domainEvent.GetType().Name;
            domainEvent.Metadata.EventSource = "CashrewardsOffers";
            domainEvent.Metadata.Domain = "Test";
            domainEvent.Metadata.CorrelationID = Guid.NewGuid();
            domainEvent.Metadata.ContextualSequenceNumber = 987654321;
            domainEvent.Metadata.RaisedDateTimeUTC = DateTime.UtcNow;
            domainEvent.Metadata.PublishedDateTimeUTC = DateTime.UtcNow;

            await state.SNSEventService.Publish(domainEvent);

            state.SnsClient.Verify(x => x.PublishAsync(It.Is<PublishRequest>(x =>
                x.TopicArn == "region:account:test-Member" &&
                x.Message == JsonConvert.SerializeObject(domainEvent, settings) &&
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
        public async Task Publish_ShouldThowARNNotFoundException_GivenCantFindTopicARN()
        {
            var state = new TestState();
            state.SnsClient.Setup(x => x.ListTopicsAsync(It.IsAny<string>(), default)).ReturnsAsync(new ListTopicsResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Topics = new List<Topic>()
            });

            await FluentActions.Invoking(() => state.SNSEventService.Publish(new TestEvent()))
                .Should().ThrowAsync<Exception>()
                .WithMessage(@$"*Error finding ARN ending with name "":test-Member"" in region :{RegionEndpoint.APSoutheast2}*");
        }

        [Test]
        public async Task Publish_ShouldThowARNNotFoundException_GivenErrorResponseToFindingTopicARN()
        {
            var state = new TestState();
            state.SnsClient.Setup(x => x.ListTopicsAsync(It.IsAny<string>(), default)).ReturnsAsync(new ListTopicsResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.BadRequest
            });

            var exception = new Exception($"Get SNS topics Http response code {System.Net.HttpStatusCode.BadRequest}");
            await FluentActions.Invoking(() => state.SNSEventService.Publish(new TestEvent()))
                .Should().ThrowAsync<Exception>()
                .WithMessage(@$"*Error finding ARN ending with name "":test-Member"" in region :{RegionEndpoint.APSoutheast2}, error: {exception}*");
        }

        [Test]
        public async Task Publish_ShouldThowPublishingErrorException_GivenAnErrorOccursWhilePublishing()
        {
            var state = new TestState();
            state.SnsClient.Setup(x => x.ListTopicsAsync(It.IsAny<string>(), default)).ReturnsAsync(new ListTopicsResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Topics = new List<Topic> { new Topic { TopicArn = "region:account:test-Member" } }
            });

            var exception = new Exception("some publishing error");
            state.SnsClient.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), default)).Throws(exception);

            var domainEvent = new TestEvent();
            await FluentActions.Invoking(() => state.SNSEventService.Publish(domainEvent))
                .Should().ThrowAsync<Exception>()
                .WithMessage($"Error publishing domain event: {JsonConvert.SerializeObject(domainEvent)} to SNS {"test-Member"}, error: {exception}*");
        }

        [Test]
        public async Task Publish_ShouldThowPublishingErrorException_GivenErrorResponseInPublishing()
        {
            var state = new TestState();
            state.SnsClient.Setup(x => x.ListTopicsAsync(It.IsAny<string>(), default)).ReturnsAsync(new ListTopicsResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Topics = new List<Topic> { new Topic { TopicArn = "region:account:test-Member" } }
            });

            state.SnsClient.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), default)).ReturnsAsync(new PublishResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
            });

            var domainEvent = new TestEvent();
            var exception = new Exception($"Publish Http response code {System.Net.HttpStatusCode.InternalServerError}");
            await FluentActions.Invoking(() => state.SNSEventService.Publish(domainEvent))
                .Should().ThrowAsync<Exception>()
                .WithMessage($"Error publishing domain event: {JsonConvert.SerializeObject(domainEvent)} to SNS {"test-Member"}, error: {exception}*");
        }
    }
}
