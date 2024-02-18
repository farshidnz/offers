using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Events;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.BackgroundHostedService
{
    public class EventMerchantInitialisationBackgroundService : BackgroundService
    {
        private readonly IEventInitialisationService<MerchantInitial> _eventMerchantInitialisationService;

        public EventMerchantInitialisationBackgroundService(
            IEventInitialisationService<MerchantInitial> eventMerchantInitialisationService)
        {
            _eventMerchantInitialisationService = eventMerchantInitialisationService;
        }

        public virtual bool CancellationRequested(CancellationToken stoppingToken) => stoppingToken.IsCancellationRequested;

        public virtual Task TakeBreakBeforeResumingMonitoring(CancellationToken stoppingToken) => Task.Delay(15000, stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!CancellationRequested(stoppingToken))
            {
                await _eventMerchantInitialisationService.CheckForInitialisationRequests();

                await TakeBreakBeforeResumingMonitoring(stoppingToken);
            }
        }
    }
}
