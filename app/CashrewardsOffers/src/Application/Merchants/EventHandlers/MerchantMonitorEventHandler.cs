using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Merchants.EventHandlers
{
    public class MerchantMonitorEventHandler : IConsumer<DomainEventNotification<MerchantChanged>>, IConsumer<DomainEventNotification<MerchantDeleted>>
    {
        private readonly IMerchantHistoryPersistenceContext _merchantHistoryPersistenceContext;

        public MerchantMonitorEventHandler(
            IMerchantHistoryPersistenceContext merchantHistoryPersistenceContext)
        {
            _merchantHistoryPersistenceContext = merchantHistoryPersistenceContext;
        }

        public async Task Consume(ConsumeContext<DomainEventNotification<MerchantChanged>> context)
        {
            var merchantChanged = context.Message.DomainEvent;
            await _merchantHistoryPersistenceContext.Add(new MerchantHistory
            {
                ChangeTime = DateTimeOffset.UtcNow,
                MerchantId = merchantChanged.MerchantId,
                Client = merchantChanged.Client,
                Name = merchantChanged.Name,
                HyphenatedString = merchantChanged.HyphenatedString,
                ClientCommissionString = merchantChanged.ClientCommissionString
            });
        }

        public async Task Consume(ConsumeContext<DomainEventNotification<MerchantDeleted>> context)
        {
            var merchantDeleted = context.Message.DomainEvent;
            await _merchantHistoryPersistenceContext.Add(new MerchantHistory
            {
                ChangeTime = DateTimeOffset.UtcNow,
                MerchantId = merchantDeleted.MerchantId,
                Client = merchantDeleted.Client,
                HyphenatedString = merchantDeleted.HyphenatedString,
                ClientCommissionString = "0%"
            });
        }
    }
}
