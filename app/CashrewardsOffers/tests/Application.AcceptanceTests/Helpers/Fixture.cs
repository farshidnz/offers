using CashrewardsOffers.API;
using CashrewardsOffers.Application.Merchants.Models;
using CashrewardsOffers.Application.Offers.Services;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Application.AcceptanceTests.Helpers
{
    public partial class Fixture
    {
        public class EmptyStartup
        {
            public EmptyStartup(IConfiguration _) { }

            public void ConfigureServices(IServiceCollection _) { }

            public void Configure(IApplicationBuilder _) { }
        }

        private readonly IWebHost _host;
        private readonly MockCurrentUserService _userService = new();
        private readonly MockShopGoSource _shopGoSource = new();
        private readonly MockUnleash _unleash = new();
        private readonly MockDynamoDbClient _dynamoDbClient = new();

        public Fixture(Dictionary<string, string>? memoryConfig = null)
        {
            memoryConfig ??= new Dictionary<string, string>();
            memoryConfig.Add("DocumentDbDatabseName", $"testdb-{Guid.NewGuid()}");
            memoryConfig.Add("UseTransactions", "false");

            var config = new ConfigurationBuilder()
                .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.json", true)
                .AddInMemoryCollection(memoryConfig)
                .Build();

            Startup? startup = null;
            _host = WebHost
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((webHostBulderContext, hostingContext) =>
                {
                    startup = new Startup(config);
                })
                .ConfigureServices(services =>
                {
                    startup?.ConfigureServices(services);
                    ReplaceServices(services, config);
                })
                .UseStartup<EmptyStartup>()
                .Build();
        }

        private void ReplaceServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton<IMongoClientFactory, MongoInMemoryClientFactory>();

            // Register mass transit consumers so we can resolve and call in-process
            Assembly.Load("CashrewardsOffers.Application")
                .GetTypes()
                .Where(t => t.IsClass && t.GetInterfaces().Any(i => typeof(IConsumer).IsAssignableFrom(i)))
                .ToList()
                .ForEach(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .ToList()
                    .ForEach(i => services.AddSingleton(typeof(IConsumer<>).MakeGenericType(i.GetGenericArguments()[0]), t)));

            services.AddSingleton(_shopGoSource.Object);
            services.AddSingleton(_userService.Object);
            services.AddSingleton(_unleash.Object);
            services.AddSingleton(_dynamoDbClient.Object);
        }

        private int _nextOfferId = 1;
        public void GivenOffer(string? title = null, int ranking = 0, int merchantId = 0, string? offerString = null, DateTime? dateEnd = null, bool isMerchantPaused = false, int? offerId = null)
        {
            (bool isFlatRate, bool isDollarRate, decimal rate) = DisassembleOfferString(offerString ?? "10%");

            var offer = new ShopGoOffer
            {
                OfferId = offerId ?? _nextOfferId,
                ClientId = (int)Client.Cashrewards,
                MerchantId = merchantId,
                CouponCode = string.Empty,
                OfferTitle = title ?? $"Offer {_nextOfferId}",
                OfferDescription = "offer description",
                HyphenatedString = $"offer-{_nextOfferId}",
                DateEnd = dateEnd ?? DateTime.MinValue,
                MerchantName = "merchant name",
                RegularImageUrl = "//regular-image-url.png",
                OfferCount = 0,
                ClientProgramTypeId = (int)ClientProgramType.CashProgram,
                TierCommTypeId = (int)(isDollarRate ? CommissionType.Dollar : CommissionType.Percentage),
                TierTypeId = (int)RewardType.Cashback1,
                Commission = rate,
                ClientComm = 100,
                MemberComm = 100,
                RewardName = "cashback",
                MerchantShortDescription = "merchant short description",
                MerchantHyphenatedString = $"merch-{merchantId}",
                OfferTerms = "offer terms",
                IsFlatRate = isFlatRate,
                Rate = 1.23m,
                Ranking = ranking,
                OfferBackgroundImageUrl = "//offer-background-url.png",
                OfferBadgeCode = "SUPER OFFER",
                MerchantBadgeCode = "SUPER MERCHANT",
                OfferPastRate = "was 2%",
                IsFeatured = true,
                IsCategoryFeatured = false,
                IsCashbackIncreased = false,
                IsPremiumFeature = false,
                MerchantIsPremiumDisabled = false,
                IsMobileAppEnabled = false,
                NetworkId = 123456,
                IsMerchantPaused = isMerchantPaused
            };

            if (!offerId.HasValue)
            {
                _nextOfferId++;
            }

            _shopGoSource.Offers.Add(offer);
        }

        public void GivenMerchant(int merchantId, string? hyphenatedString = null, string? offerString = null)
        {
            (bool isFlatRate, bool isDollarRate, decimal rate) = DisassembleOfferString(offerString ?? "10%");

            var merchant = new ShopGoMerchant
            {
                ClientId = (int)Client.Cashrewards,
                MerchantId = merchantId,
                HyphenatedString = hyphenatedString ?? $"merchant-{merchantId}",
                RegularImageUrl = "//regular-image-url.png",
                ClientProgramTypeId = (int)ClientProgramType.CashProgram,
                TierCommTypeId = (int)(isDollarRate ? CommissionType.Dollar : CommissionType.Percentage),
                TierTypeId = (int)RewardType.Cashback1,
                Commission = rate,
                ClientComm = 100,
                MemberComm = 100,
                RewardName = "cashback",
                IsFlatRate = isFlatRate,
                Rate = 1.23m,
                IsPremiumDisabled = false
            };

            _shopGoSource.Merchants.Add(merchant);
        }

        public void RemoveMerchant(int merchantId)
        {
            _shopGoSource.Merchants.Remove(_shopGoSource.Merchants.Single(m => m.MerchantId == merchantId));
        }

        public void RemoveMerchant(string merchantHyphenatedString)
        {
            _shopGoSource.Merchants.Remove(_shopGoSource.Merchants.Single(m => m.HyphenatedString == merchantHyphenatedString));
        }

        private int _nextTierId = 1;
        public void GivenTier(int merchantId, string? name = null, string? terms = null, string? rateString = null)
        {
            (bool isFlatRate, bool isDollarRate, decimal rate) = DisassembleOfferString(rateString ?? "10%");
            if (!isFlatRate) throw new Exception("Tiers must have a flat rate. Setting up a tier with an 'up to' type rate is not allowed.");

            var tier = new ShopGoTier
            {
                ClientTierId = _nextTierId,
                MerchantTierId = 0,
                MerchantId = merchantId,
                ClientId = (int)Client.Cashrewards,
                TierName = name ?? $"tier-{_nextTierId}",
                TierCommTypeId = (int)(isDollarRate ? CommissionType.Dollar : CommissionType.Percentage),
                Commission = rate,
                ClientComm = 100,
                MemberComm = 100,
                TierSpecialTerms = terms ?? "Capped at $45 | Ends tonight"
            };

            _nextTierId++;

            _shopGoSource.Tiers.Add(tier);
        }

        private static (bool isFlatRate, bool isDollarRate, decimal rate) DisassembleOfferString(string offerString)
        {
            var isFlatRate = !offerString.ToLower().StartsWith("up to ");
            var isDollarRate = offerString[isFlatRate ? 0 : 6] == '$';
            var rateStart = isFlatRate ? (isDollarRate ? 1 : 0) : (isDollarRate ? 7 : 6);
            var rate = decimal.Parse(offerString.Substring(rateStart, offerString.Length - rateStart - (isDollarRate ? 0 : 1)));
            return (isFlatRate, isDollarRate, rate);
        }

        public void GivenUserFavourite(string cognitoId, int merchantId, string hyphenatedString, int selectionOrder = 0)
        {
            _shopGoSource.Favourites.Add((cognitoId, new ShopGoFavourite
            {
                MerchantId = merchantId,
                HyphenatedString = hyphenatedString,
                SelectionOrder = selectionOrder
            }));
        }

        public void GivenPerson(string personId = "1", string cognitoId = "1", string memberId = "1", string newMemberId = "1", int premiumStatus = 0)
        {
            _userService.UserId = cognitoId;

            _shopGoSource.GivenPerson(personId, cognitoId, memberId, newMemberId, premiumStatus);
        }

        public void GivenUnleashFeatureToggle(string toggleName, bool enabled = true)
        {
            _unleash.Toggles[toggleName] = enabled;
        }

        public async Task GivenMerchantSyncJobHasRun()
        {
            await (_host.Services.GetService<ISyncMerchantsService>())!.SyncMerchantsAsync();
        }

        public async Task GivenOfferSyncJobHasRun()
        {
            await (_host.Services.GetService<ISyncOffersService>())!.SyncOffersAsync();
        }

        public void GivenEdmItem(int edmCampaignId, string type, int merchantId)
        {
            _shopGoSource.GivenEdmItem(edmCampaignId, type, merchantId);
        }

        public async Task<TResponse?> WhenISendTheQuery<TMessage, TResponse>(TMessage message) where TMessage : class where TResponse : class
        {
            var consumer = _host.Services.GetService<IConsumer<TMessage>>();
            if (consumer == null) throw new Exception($"The is no consumer for messages of type {typeof(TMessage).Name}");

            var context = new Mock<ConsumeContext<TMessage>>();
            TResponse? response = null;
            context.Setup(c => c.Message).Returns(message);
            context.Setup(c => c.RespondAsync(It.IsAny<TResponse>())).Callback<TResponse>(r => response = r);
            await consumer.Consume(context.Object);
            return response;
        }
    }
}
