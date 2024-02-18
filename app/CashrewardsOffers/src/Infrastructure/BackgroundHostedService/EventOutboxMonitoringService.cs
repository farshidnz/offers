using CashrewardsOffers.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.BackgroundHostedService
{
    public class EventOutboxMonitoringService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        public readonly string ServiceName = "CashrewardsOffers-EventOutboxMonitoring";

        public EventOutboxMonitoringService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public virtual bool CancellationRequested(CancellationToken stoppingToken) => stoppingToken.IsCancellationRequested;

        public virtual Task TakeBreakBeforeResumingMonitoring(CancellationToken stoppingToken) => Task.Delay(1000, stoppingToken);

        public virtual IServiceScope CreateServiceScope() => serviceProvider.CreateScope();

        public virtual IDomainEventService GetDomainEventService(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<IDomainEventService>();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => Log.Information($"{ServiceName} background task is stopping due to CancellationToken."));

            while (!CancellationRequested(stoppingToken))
            {
                await MonitorEventOutbox(stoppingToken);
            }

            Log.Information($"{ServiceName} background task is stopping.");
        }

        private async Task MonitorEventOutbox(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = CreateServiceScope();

                await GetDomainEventService(scope).PublishEventOutbox();

                await TakeBreakBeforeResumingMonitoring(stoppingToken);
            }
            catch (Exception e)
            {
                Log.Error(e, $"{ServiceName} failed to monitor and publish Event Outbox, Error: {e.Message}");
            }
        }
    }
}
