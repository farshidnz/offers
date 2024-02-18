using CashrewardsOffers.Application.ANZ.Services;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Events;
using MassTransit;
using Serilog;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.ANZ.EventHandlers
{
    public class MerchantChangedEventHandler : IConsumer<DomainEventNotification<MerchantChanged>>, IConsumer<DomainEventNotification<MerchantInitial>>
    {
        private readonly IAnzUpdateService _anzUpdateService;

        public MerchantChangedEventHandler(IAnzUpdateService anzUpdateService)
        {
            _anzUpdateService = anzUpdateService;
        }

        public async Task Consume(ConsumeContext<DomainEventNotification<MerchantChanged>> context)
        {
            await DoConsume(context.Message.DomainEvent);
        }

        public async Task Consume(ConsumeContext<DomainEventNotification<MerchantInitial>> context)
        {
            await DoConsume(context.Message.DomainEvent);
        }

        public async Task DoConsume(MerchantEventBase domainEvent)
        {
            Log.Information("Received {EventType} event {EventId} for client: {Client} merchant: {HyphenatedString}",
                domainEvent.GetType().Name,
                domainEvent.Metadata.EventID,
                domainEvent.Client,
                domainEvent.HyphenatedString);

            await _anzUpdateService.UpdateMerchant(domainEvent);
        }
    }
}
