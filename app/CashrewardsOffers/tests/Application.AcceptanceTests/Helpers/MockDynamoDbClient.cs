using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using CashrewardsOffers.Application.Common.Interfaces;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Application.AcceptanceTests.Helpers
{
    public class MockDynamoDbClient : Mock<IAmazonDynamoDB>
    {
        public List<int> PopularMerchantIdsForBrowser { get; } = new();
        public List<int> PopularMerchantIdsForMobile { get; } = new();

        public MockDynamoDbClient()
        {
            var db = new Dictionary<string, List<int>>
            {
                [PopularMerchantTarget.Browser] = PopularMerchantIdsForBrowser,
                [PopularMerchantTarget.Mobile] = PopularMerchantIdsForMobile
            };

            Setup(d => d.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetItemRequest r, CancellationToken c) => new GetItemResponse
                {
                    Item = new Dictionary<string, AttributeValue>
                    {
                        ["orderedMerchantIds"] = new AttributeValue()
                        {
                            L = db.TryGetValue(r.Key["targetId"].S, out var merchantIds)
                                ? merchantIds.Distinct().Select(id => new AttributeValue { N = id.ToString() }).ToList()
                                : null
                        }
                    }
                });
        }
    }
}
