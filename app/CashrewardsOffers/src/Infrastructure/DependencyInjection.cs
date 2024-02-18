using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Infrastructure.AWS;
using CashrewardsOffers.Infrastructure.BackgroundHostedService;
using CashrewardsOffers.Infrastructure.Identity;
using CashrewardsOffers.Infrastructure.Persistence;
using CashrewardsOffers.Infrastructure.Services;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace CashrewardsOffers.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterMappingProfiles();

            services.AddTransient<IDocumentDbConnection, DocumentDbConnection>();
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && typeof(IDocument).IsAssignableFrom(t))
                .ToList()
                .ForEach(t => services.AddSingleton(typeof(IDocumentPersistenceContext<>).MakeGenericType(t), typeof(DocumentDbPersistenceContext<>).MakeGenericType(t)));
            services.AddSingleton<IDatabaseConfigurator, DatabaseConfigurator>();
            services.AddSingleton<IMongoLockService, MongoLockService>();
            services.AddSingleton<IEventOutboxPersistenceContext, EventOutboxPersistenceContext>();
            services.AddSingleton<IOffersPersistenceContext, OffersPersistenceContext>();
            services.AddSingleton<IRankedMerchantPersistenceContext, RankedMerchantPersistenceContext>();
            services.AddSingleton<IFeaturePersistenceContext, FeaturePersistenceContext>();
            services.AddSingleton<IMerchantsPersistenceContext, MerchantsPersistenceContext>();
            services.AddSingleton<IAnzItemPersistenceContext, AnzItemPersistenceContext>();
            services.AddSingleton<IMerchantHistoryPersistenceContext, MerchantHistoryPersistenceContext>();
            services.AddSingleton<IShopGoSource, ShopGoSource>();
            services.AddSingleton<IS3MerchantHistoryArchive, S3MerchantHistoryArchive>();
            services.AddSingleton<IAmazonS3ClientFactory, AmazonS3ClientFactory>();
            services.AddSingleton<IS3Source, S3Source>();
            services.AddSingleton<IMerchantPreferenceS3Source, MerchantPreferenceS3Source>();
            services.AddSingleton<IPopularMerchantSource, PopularMerchantSource>();
            services.AddSingleton<InMemoryQueue>();
            services.AddSingleton<IEventInitialisationService<OfferInitial>, EventOfferInitialisationService>();
            services.AddSingleton<IEventInitialisationService<MerchantInitial>, EventMerchantInitialisationService>();

            services.AddScoped<IDomainEventService, DomainEventService>();
            services.AddSingleton<IEventTypeResolver, EventTypeResolver>();

            services.AddTransient<IDateTime, DateTimeService>();

            services.AddTransient<IIdentityService, IdentityService>();

            // AWS services
            services.AddAWSService<IAmazonSQS>();
            services.AddAWSService<IAmazonSimpleNotificationService>();
            services.AddSingleton<IAWSEventServiceFactory, AWSEventServiceFactory>();
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton<IAwsSecretsManagerClientFactory, AwsSecretsManagerClientFactory>();

            // Register Hosted Background Services
            services.AddHostedService<EventOutboxMonitoringService>();
            services.AddHostedService<EventPolledReadingService>();
            services.AddHostedService<EventMerchantInitialisationBackgroundService>();
            services.AddHostedService<EventOfferInitialisationBackgroundService>();

            return services;
        }

        public static void RegisterMappingProfiles()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        }
    }
}
