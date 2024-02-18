using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class PopularMerchantSource : IPopularMerchantSource
    {
        private readonly string _popularStoreInfoTable;
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public PopularMerchantSource(
            IConfiguration configuration,
            IAmazonDynamoDB dynamoDbClient)
        {
            _popularStoreInfoTable = configuration["Config:PopularStoreInfoTable"];
            _dynamoDbClient = dynamoDbClient;
        }

        public async Task<List<int>> GetPopularMerchantIds(string target)
        {
            var request = new GetItemRequest
            {
                TableName = _popularStoreInfoTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "targetId", new AttributeValue{ S = target} }
                }
            };

            var response = await _dynamoDbClient.GetItemAsync(request);

            return ConvertToPopularStoreConfig(response.Item).MerchantIds;
        }

        private static PopularStoreConfig ConvertToPopularStoreConfig(Dictionary<string, AttributeValue> item)
        {
            var popularStoreConfig = new PopularStoreConfig();
            if (item.ContainsKey("orderedMerchantIds"))
            {
                var orderedMerchantIds = item["orderedMerchantIds"].L;
                if (orderedMerchantIds.Count > 0)
                {
                    popularStoreConfig = new PopularStoreConfig
                    {
                        MerchantIds = orderedMerchantIds.Select(m => int.Parse(m.N)).ToList()
                    };
                }
            }

            return popularStoreConfig;
        }
    }
}
