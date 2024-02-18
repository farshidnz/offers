using Amazon.SQS.Model;
using CashrewardsOffers.Domain.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.AWS
{
    public class InMemoryQueue : IAWSEventService
    {
        private readonly SynchronizedCollection<SQSEvent> _events = new();

        public string AWSResourceName => "InMemoryQueue";

        public IEnumerable<Type> EventTypes { get; } = new List<Type>();

        public Task DeleteEvent(SQSEvent sqsEvent)
        {
            _events.Remove(sqsEvent);
            return Task.CompletedTask;
        }

        public Task Publish(DomainEvent domainEvent)
        {
            try
            {
                _events.Add(new SQSEvent(domainEvent, CreateMessageFromDomainEvent(domainEvent)));
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error publishing domain event: {domainEvent.ToJson()} to {AWSResourceName}");
                throw;
            }
        }

        private static Message CreateMessageFromDomainEvent(DomainEvent domainEvent) => new()
        {
            MessageAttributes = new()
            {
                [EventMessageAttributes.EventID.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.EventID.ToString(), DataType = "String" },
                [EventMessageAttributes.EventType.ToString()] = new MessageAttributeValue { StringValue = domainEvent.GetType().Name, DataType = "String" },
                [EventMessageAttributes.EventSource.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.EventSource, DataType = "String" },
                [EventMessageAttributes.Domain.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.Domain, DataType = "String" },
                [EventMessageAttributes.CorrelationID.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.CorrelationID.ToString(), DataType = "String" },
                [EventMessageAttributes.ContextualSequenceNumber.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.ContextualSequenceNumber.ToString(), DataType = "String" },
                [EventMessageAttributes.EventRaisedDateTimeUTC.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.RaisedDateTimeUTC.ToString("o"), DataType = "String" },
                [EventMessageAttributes.EventPublishedDateTimeUTC.ToString()] = new MessageAttributeValue { StringValue = domainEvent.Metadata.PublishedDateTimeUTC.ToString("o"), DataType = "String" }
            }
        };

        public async IAsyncEnumerable<SQSEvent> ReadEventStream([EnumeratorCancellation] CancellationToken stoppingToken)
        {
            while (!CancellationRequested(stoppingToken))
            {
                var events = await PollForEvents();
                if (!events.Any())
                {
                    await TakeBreakBeforePollingForEvents(stoppingToken);
                    continue;
                }

                foreach (var @event in events)
                {
                    yield return @event;
                }
            }

            async Task<IEnumerable<SQSEvent>> PollForEvents()
            {
                try
                {
                    return await ReadEvents();
                }
                catch
                {
                    return new List<SQSEvent>();
                }
            }
        }

        public virtual bool CancellationRequested(CancellationToken stoppingToken) => stoppingToken.IsCancellationRequested;

        public virtual Task TakeBreakBeforePollingForEvents(CancellationToken stoppingToken) => Task.Delay(1000, stoppingToken);

        public Task<IEnumerable<SQSEvent>> ReadEvents()
        {
            try
            {
                if (_events.Any())
                {
                    Log.Information($"Reading events from {AWSResourceName}");
                    return Task.FromResult(_events.Take(10));
                }

                return Task.FromResult(Array.Empty<SQSEvent>() as IEnumerable<SQSEvent>);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error reading from {AWSResourceName}");
                throw;
            }
        }
    }
}
