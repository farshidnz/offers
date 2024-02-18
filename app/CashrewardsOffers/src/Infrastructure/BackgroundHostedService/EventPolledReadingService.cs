using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrastructure.Extensions;
using MassTransit.Mediator;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.BackgroundHostedService
{
    public class EventPolledReadingService : BackgroundService
    {
        private readonly IAWSEventServiceFactory awsEventServiceFactory;
        private readonly IMediator mediator;
        public readonly string ServiceName = "EventPolledReadingService";

        public EventPolledReadingService(
            IAWSEventServiceFactory awsEventServiceFactory,
            IMediator mediator)
        {
            this.awsEventServiceFactory = awsEventServiceFactory;
            this.mediator = mediator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => Log.Information($"{ServiceName} background task is stopping due to CancellationToken."));

            await PolledReadEvents(stoppingToken);

            Log.Information($"{ServiceName} background task is stopping.");
        }

        public async Task PolledReadEvents(CancellationToken stoppingToken)
        {
            var readers = awsEventServiceFactory.GetAWSEventReaders(AwsEventReadMode.PolledRead);

            await Task.WhenAll(readers.Select(x => ReadEvents(x, stoppingToken)));
        }

        private async Task ReadEvents(IAWSEventService eventService, CancellationToken stoppingToken)
        {
            if (eventService != null)
            {
                Log.Information($"{ServiceName} is starting to read and process events from SQS : {eventService.AWSResourceName}.");

                await foreach (var sqsEvent in eventService.ReadEventStream(stoppingToken))
                {
                    var eventType = sqsEvent.Message.MessageAttributes.TryGetValue(EventMessageAttributes.EventType.ToString(), out var eventTypeAttribute) ? eventTypeAttribute.StringValue : string.Empty;
                    var eventId = sqsEvent.Message.MessageAttributes.TryGetValue(EventMessageAttributes.EventID.ToString(), out var eventIdAttribute) ? eventIdAttribute.StringValue : string.Empty;
                    Log.Information("Consuming event {EventType} - {EventID}", eventType, eventId);
                    await ProcessEvent(eventService, sqsEvent);
                }
            }
        }

        private async Task ProcessEvent(IAWSEventService eventService, SQSEvent sqsEvent)
        {
            if (IsValidDomainEvent(sqsEvent))
            {
                if (await PublishToInternalEventHandlers(sqsEvent))
                {
                    await eventService.DeleteEvent(sqsEvent);
                }
            }
            else
            {
                await eventService.DeleteEvent(sqsEvent);
            }
        }

        private bool IsValidDomainEvent(SQSEvent sqsEvent) => sqsEvent.DomainEvent != default;

        public async Task<bool> PublishToInternalEventHandlers(SQSEvent sqsEvent)
        {
            try
            {
                await mediator.PublishEvent(sqsEvent.DomainEvent, sqsEvent.Message);
                return true;
            }
            catch (Exception e)
            {
                var errorMessage = $"Error handling domain event: {sqsEvent.DomainEvent.ToJson()}";
                Log.Error(e, errorMessage);
                return false;
            }
        }
    }
}