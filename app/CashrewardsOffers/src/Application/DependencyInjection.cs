using CashrewardsOffers.Application.ANZ.Services;
using CashrewardsOffers.Application.Common.Behaviours;
using CashrewardsOffers.Application.Merchants.Services;
using CashrewardsOffers.Domain.Entities;
using FluentValidation;
using GreenPipes;
using Mapster;
using MassTransit;
using MassTransit.Registration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application
{
    public static class MediatorHttpContextScopeFilterExtensions
    {
        public static void UseHttpContextScopeFilter(this IMediatorConfigurator configurator, IServiceProvider serviceProvider)
        {
            var filter = new HttpContextScopeFilter(serviceProvider.GetRequiredService<IHttpContextAccessor>());

            configurator.ConfigurePublish(x => x.UseFilter(filter));
            configurator.ConfigureSend(x => x.UseFilter(filter));
            configurator.UseFilter(filter);
        }
    }
    public class HttpContextScopeFilter :
        IFilter<PublishContext>,
        IFilter<SendContext>,
        IFilter<ConsumeContext>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextScopeFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddPayload(PipeContext context)
        {
            if (_httpContextAccessor.HttpContext == null)
                return;

            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            context.GetOrAddPayload(() => serviceProvider);
            context.GetOrAddPayload<IServiceScope>(() => new NoopScope(serviceProvider));
        }

        public Task Send(PublishContext context, IPipe<PublishContext> next)
        {
            AddPayload(context);
            return next.Send(context);
        }

        public Task Send(SendContext context, IPipe<SendContext> next)
        {
            AddPayload(context);
            return next.Send(context);
        }

        public Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            AddPayload(context);
            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
        }

        private class NoopScope :
            IServiceScope
        {
            public NoopScope(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }

            public void Dispose()
            {
            }

            public IServiceProvider ServiceProvider { get; }
        }
    }

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            RegisterMappingProfiles();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddMediator(cfg =>
            {
                cfg.AddConsumers(Assembly.GetExecutingAssembly());

                cfg.ConfigureMediator((context, mcfg) =>
                {
                    RegisterMassTransitFilters(context, mcfg);
                    mcfg.UseHttpContextScopeFilter(context);
                });
            });

            services.AddSingleton<IAnzUpdateService, AnzUpdateService>();
            services.AddSingleton<IPopularMerchantRankingService, PopularMerchantRankingService>();
            services.AddSingleton<IAnzItemFactory, AnzItemFactory>();
            services.AddSingleton<IDomainTime, DateTimeOffsetWrapper>();
            services.AddSingleton<IMerchantHistoryArchiveService, MerchantHistoryArchiveService>();
            services.AddSingleton<IMerchantHistoryExcelService, MerchantHistoryExcelService>();

            return services;
        }

        private static void RegisterMappingProfiles()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Domain"));
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        }

        private static void RegisterMassTransitFilters(IMediatorRegistrationContext context, IMediatorConfigurator mcfg)
        {
            // order of execution of the filters is important
            mcfg.UseConsumeFilter(typeof(UnhandledExceptionBehaviour<>), context);

            // is logging of commands, queries and events prior to handling them required ?
            // mcfg.UseConsumeFilter(typeof(LoggingBehaviour<>), context);

            // is custome authorisation logic for commands, queries and events required ?
            // mcfg.UseConsumeFilter(typeof(AuthorisationBehaviour<>), context);

            // enable validators for commands, queries and events when present
            mcfg.UseConsumeFilter(typeof(ValidationBehaviour<>), context);

            // monitor poorly performing commands, queries and events
            mcfg.UseConsumeFilter(typeof(PerformanceBehaviour<>), context);
        }
    }
}