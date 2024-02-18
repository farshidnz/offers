using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using Mapster;
using MassTransit;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Merchants.Queries.GetEdmMerchants.v1
{
    public class GetEdmMerchantsQuery
    {
        public string NewMemberId { get; set; }
    }

    public class GetEdmMerchantsResponse
    {
        public List<EdmMerchantInfo> Merchants { get; set; }
    }

    public class EdmMerchantInfo
    {
        public int MerchantId { get; set; }
        public string LogoUrl { get; set; }
        public string ClientCommissionString { get; set; }
        public string HyphenatedString { get; set; }
        public EdmMerchantPremiumInfo Premium { get; set; }
    }

    public class EdmMerchantPremiumInfo
    {
        public string ClientCommissionString { get; set; }
    }

    public class GetEdmMerchantsQueryConsumer : IConsumer<GetEdmMerchantsQuery>
    {
        private readonly IMerchantsPersistenceContext _merchantsPersistenceContext;
        private readonly IShopGoSource _shopGoSource;
        private readonly List<int> _defaultEdmMerchantIds = new()
        {
            1003434, // amazon-australia
            1001440, // the-iconic
            1003824, // myer
            1003166, // david-jones
            1001527, // bonds
            1003711  // groupon
        };

        private readonly int _requiredMerchantCount = 6;

        public GetEdmMerchantsQueryConsumer(
            IMerchantsPersistenceContext merchantsPersistenceContext,
            IShopGoSource shopGoSource)
        {
            _merchantsPersistenceContext = merchantsPersistenceContext;
            _shopGoSource = shopGoSource;
        }

        public async Task Consume(ConsumeContext<GetEdmMerchantsQuery> context)
        {
            Log.Information($"Query: {JsonConvert.SerializeObject(context.Message)}");

            await context.RespondAsync(new GetEdmMerchantsResponse
            {
                Merchants = (await GetMerchants(context)).Adapt<List<EdmMerchantInfo>>()
            });
        }

        private async Task<List<Merchant>> GetMerchants(ConsumeContext<GetEdmMerchantsQuery> context)
        {
            var clientId = (int)Client.Cashrewards;

            if (context.Message.NewMemberId == null)
            {
                return (await GetDefaultMerchants(clientId, null)).Take(_requiredMerchantCount).ToList();
            }

            var premiumClientId = await _shopGoSource.GetPremiumStatus(context.Message.NewMemberId) == 1 ? (int?)Client.Anz : null;

            var favourites = await _shopGoSource.GetFavouritesByNewMemberId(context.Message.NewMemberId);

            var merchants = (await _merchantsPersistenceContext
                .GetMerchants(clientId, premiumClientId, favourites.Select(f => f.MerchantId).ToList()))
                .Take(_requiredMerchantCount)
                .ToList();

            if (merchants.Count < _requiredMerchantCount)
            {
                var merchantIds = merchants.Select(m => m.MerchantId);
                var defaultMerchants = (await GetDefaultMerchants(clientId, premiumClientId))
                    .Where(m => !merchantIds.Contains(m.MerchantId))
                    .Take(_requiredMerchantCount - merchants.Count)
                    .ToList();

                merchants.AddRange(defaultMerchants);
            }

            return merchants;
        }

        private async Task<IEnumerable<Merchant>> GetDefaultMerchants(int clientId, int? premiumClientId) =>
            (await _merchantsPersistenceContext.GetMerchants(clientId, premiumClientId, _defaultEdmMerchantIds))
                .OrderBy(m => _defaultEdmMerchantIds.IndexOf(m.MerchantId));
    }
}
