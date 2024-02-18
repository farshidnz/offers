using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrastructure.Extensions;
using CashrewardsOffers.Infrastructure.Persistence;
using MassTransit.Mediator;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Services
{
    public interface IDomainEventService
    {
        Task PublishEventOutbox();
    }

    public class DomainEventService : IDomainEventService
    {
        private readonly IEventOutboxPersistenceContext _eventOutboxPersistenceContext;
        private readonly IMediator _mediator;
        private readonly IAWSEventServiceFactory _eventServiceFactory;
        private readonly IMongoLockService _mongoLockService;

        public DomainEventService(
            IEventOutboxPersistenceContext eventOutboxPersistenceContext,
            IMediator mediator,
            IAWSEventServiceFactory eventServiceFactory,
            IMongoLockService mongoLockService)
        {
            _eventOutboxPersistenceContext = eventOutboxPersistenceContext;
            _mediator = mediator;
            _eventServiceFactory = eventServiceFactory;
            _mongoLockService = mongoLockService;
        }

        private async Task Publish(DomainEvent domainEvent)
        {
            Log.Information("Publishing domain event {EventType} - {EventId}", domainEvent.Metadata.EventType, domainEvent.Metadata.EventID);

            domainEvent.Metadata.PublishedDateTimeUTC = DateTime.UtcNow;

            await Task.WhenAll(PublishToExternalEventDestinations(domainEvent),
                               PublishToInternalEventHandlers(domainEvent));
        }

        public async Task PublishToInternalEventHandlers(DomainEvent domainEvent)
        {
            await _mediator.PublishEvent(domainEvent);
        }

        private async Task PublishToExternalEventDestinations(DomainEvent domainEvent)
        {
            var externalPublishers = _eventServiceFactory.GetAWSPublishersForEvent(domainEvent);
            var hasEventPublishers = externalPublishers?.Any() ?? false;
            if (hasEventPublishers)
            {
                await Task.WhenAll(externalPublishers.Select(x => x.Publish(domainEvent)));
            }
        }

        public async Task PublishEventOutbox()
        {
            Guid? lockId = null;
            try
            {
                lockId = await _mongoLockService.LockAsync(_outboxPublishLockName);
                if (lockId != null)
                {
                    await DoPublish(lockId.Value);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Exception in publishing event outbox, Error: {e.Message}");
            }
            finally
            {
                if (lockId != null)
                {
                    await _mongoLockService.ReleaseLockAsync(_outboxPublishLockName, lockId.Value);
                }
            }
        }

        private static readonly string _outboxPublishLockName = "outbox-publish-lock";

        private async Task DoPublish(Guid lockId)
        {
            var domainEvent = await _eventOutboxPersistenceContext.GetNext();
            while (domainEvent != null)
            {
                await _mongoLockService.RelockAsync(_outboxPublishLockName, lockId);
                await Publish(domainEvent);
                await _eventOutboxPersistenceContext.Delete(domainEvent.Metadata.EventID);

                domainEvent = await _eventOutboxPersistenceContext.GetNext();
            }
        }
    }
}
