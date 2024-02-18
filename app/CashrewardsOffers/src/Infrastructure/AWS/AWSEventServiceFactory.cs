using Amazon.SimpleNotificationService;
using Amazon.SQS;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CashrewardsOffers.Infrastructure.AWS
{
    internal record ReadServiceKey(AwsEventReadMode AWSReadMode, AwsResourceType AWSResourceType, string Domain);

    public interface IAWSEventServiceFactory
    {
        List<IAWSEventService> GetAWSPublishersForEvent(DomainEvent domainEvent);

        List<IAWSEventService> GetAWSEventReaders(AwsEventReadMode readMode);
    }

    public class AWSEventServiceFactory : IAWSEventServiceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly InMemoryQueue _inMemoryQueue;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IEventTypeResolver _eventTypeResolver;
        private readonly ConcurrentDictionary<ReadServiceKey, IAWSEventService> _eventReaders;
        private readonly ConcurrentDictionary<Type, List<IAWSEventService>> _eventPublishers;
        private readonly IAmazonSQS _sqsClient;
        private readonly bool _useInMemoryQueues;

        public AWSEventServiceFactory(
            IAmazonSQS sqsClient,
            IAmazonSimpleNotificationService snsClient,
            IEventTypeResolver eventTypeResolver,
            IConfiguration configuration,
            InMemoryQueue inMemoryQueue)
        {
            _sqsClient = sqsClient;
            _snsClient = snsClient;
            _eventTypeResolver = eventTypeResolver;
            _configuration = configuration;
            _inMemoryQueue = inMemoryQueue;
            _useInMemoryQueues = bool.TryParse(_configuration["UseInMemoryQueues"], out var b) && b;
            _eventReaders = BuildEventReaders(configuration);
            _eventPublishers = BuildEventPublishers(configuration);
        }

        private ConcurrentDictionary<Type, List<IAWSEventService>> BuildEventPublishers(IConfiguration configuration)
        {
            var eventDestinations = GetAwsResources<AWSEventDestination>(configuration, "EventDestinations:AWSResources");

            var publishers = eventDestinations.GroupBy(x => x.EventType)
                                              .ToDictionary(g => g.Key, g => g.Select(x => BuildEventPublishingService(x)).ToList());

            return new ConcurrentDictionary<Type, List<IAWSEventService>>(publishers);
        }

        private ConcurrentDictionary<ReadServiceKey, IAWSEventService> BuildEventReaders(IConfiguration configuration)
        {
            var eventSources = GetAwsResources<AWSEventSource>(configuration, "EventSources:AWSResources");

            var readers = eventSources.GroupBy(x => (x.AWSReadMode, x.AWSResourceType, x.Domain))
                                      .ToDictionary(g => new ReadServiceKey(g.Key.AWSReadMode, g.Key.AWSResourceType, g.Key.Domain),
                                                    g => BuildEventReadingService(g.Key.AWSResourceType, g.Key.Domain, g.ToList()));

            return new ConcurrentDictionary<ReadServiceKey, IAWSEventService>(readers);
        }

        private List<T> GetAwsResources<T>(IConfiguration configuration, string section) where T : AWSEventResource
        {
            return configuration.GetSection(section).Get<List<T>>()?.Select(x =>
            {
                x.EventType = _eventTypeResolver.GetEventType(x.EventTypeName);
                return x;
            }).Where(t => t.EventType != null).ToList() ?? new();
        }

        private IAWSEventService BuildEventReadingService(AwsResourceType awsResourceType, string domain, IEnumerable<AWSEventSource> awsEventResources)
        {
            if (_useInMemoryQueues)
            {
                return _inMemoryQueue;
            }

            return awsResourceType switch
            {
                AwsResourceType.SQS => new SQSEventService(_configuration, domain, awsEventResources, _sqsClient),
                _ => default,
            };
        }

        private IAWSEventService BuildEventPublishingService(AWSEventDestination awsEventResource)
        {
            if (_useInMemoryQueues)
            {
                return _inMemoryQueue;
            }

            return awsEventResource.AWSResourceType switch
            {
                AwsResourceType.SNS => new SNSEventService(_configuration, awsEventResource, _snsClient),
                _ => default,
            };
        }

        public List<IAWSEventService> GetAWSPublishersForEvent(DomainEvent domainEvent)
        {
            _eventPublishers.TryGetValue(domainEvent.GetType(), out var publishers);
            return publishers ?? new();
        }

        public List<IAWSEventService> GetAWSEventReaders(AwsEventReadMode readMode)
        {
            return _eventReaders.Where(x => x.Key.AWSReadMode == readMode).Select(v => v.Value).ToList();
        }
    }
}
