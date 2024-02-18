using Amazon.SimpleNotificationService;
using Amazon.SQS;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrastructure.Services;
using CashrewardsOffers.Infrustructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;

namespace CashrewardsOffers.Infrustructure.UnitTests.AWS
{
    [TestFixture]
    public class AWSEventServiceFactoryTests
    {
        private class TestState
        {
            public AWSEventServiceFactory AWSEventServiceFactory { get; }

            public TestState(dynamic appConfig)
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(appConfig))))
                    .Build();

                var eventTypeResolver = new Mock<IEventTypeResolver>();
                eventTypeResolver.Setup(r => r.GetEventType(It.Is<string>(n => n == "TestEvent"))).Returns(typeof(TestEvent));

                AWSEventServiceFactory = new AWSEventServiceFactory(
                    Mock.Of<IAmazonSQS>(),
                    Mock.Of<IAmazonSimpleNotificationService>(),
                    eventTypeResolver.Object,
                    configuration,
                    new InMemoryQueue());
            }
        }

        [Test]
        public void GetAWSPublishersForEvent_ShouldReturnNoPublishers_GivenEmptyAppConfig()
        {
            var state = new TestState(new { });

            var publishers = state.AWSEventServiceFactory.GetAWSPublishersForEvent(new TestEvent());

            publishers.Should().BeEmpty();
        }

        [Test]
        public void GetAWSPublishersForEvent_ShouldReturnNoPublishers_GivenNoMatchingEventDestinationInAppConfig()
        {
            var state = new TestState(new
            {
                EventDestinations = new
                {
                    AWSResources = new[] {
                        new {
                            Type = "SQS",
                            Name = "queueName",
                            EventTypeName = "NotMatchingTestEvent"
                        }
                    }
                }
            });

            var publishers = state.AWSEventServiceFactory.GetAWSPublishersForEvent(new TestEvent());

            publishers.Should().BeEmpty();
        }

        [Test]
        public void GetAWSPublishersForEvent_ShouldReturnNoPublishers_GivenUnknownResourceTypeInAppConfig()
        {
            var state = new TestState(new
            {
                EventDestinations = new
                {
                    AWSResources = new[] {
                        new {
                            Type = "Unknown",
                            Name = "queueName",
                            EventTypeName = "String"
                        }
                    }
                }
            });

            var publishers = state.AWSEventServiceFactory.GetAWSPublishersForEvent(new TestEvent());

            publishers.Should().BeEmpty();
        }

        [TestCase("SNS")]
        public void GetAWSPublishersForEvent_ShouldReturnPublishers_GivenMatchingEventDestinationInAppConfig(string resourceType)
        {
            var state = new TestState(new
            {
                Environment = "test",
                ServiceName = "offers",
                EventDestinations = new
                {
                    AWSResources = new[] {
                        new {
                            Type = resourceType,
                            Domain = "domain",
                            EventTypeName = "TestEvent"
                        }
                    }
                }
            });

            var publishers = state.AWSEventServiceFactory.GetAWSPublishersForEvent(new TestEvent());

            publishers.Count.Should().Be(1);
            publishers[0].AWSResourceName.Should().Be("test-domain");
            var eventType = publishers[0].EventTypes.First();
            eventType.Name.Should().Be("TestEvent");
            eventType.Should().Be(typeof(TestEvent));
        }
    }
}
