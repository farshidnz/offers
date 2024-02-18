using CashrewardsOffers.Application.ANZ.Services;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Events;
using MassTransit;
using Serilog;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.ANZ.EventHandlers
{
    public class MerchantDeletedEventHandler : IConsumer<DomainEventNotification<MerchantDeleted>>
    {
        private readonly IAnzUpdateService _anzUpdateService;

        public MerchantDeletedEventHandler(IAnzUpdateService anzUpdateService)
        {
            _anzUpdateService = anzUpdateService;
        }

        public async Task Consume(ConsumeContext<DomainEventNotification<MerchantDeleted>> context)
        {
            var domainEvent = context.Message.DomainEvent;

            Log.Information("Received {EventType} event {EventId} for client: {Client} merchant: {HyphenatedString}",
                domainEvent.GetType().Name,
                domainEvent.Metadata.EventID,
                domainEvent.Client,
                domainEvent.HyphenatedString);

            await _anzUpdateService.DeleteMerchant(domainEvent);
        }
    }
}
