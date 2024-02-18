using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Persistence
{
    public class PopularMerchantSourceTests
    {
        private class TestState
        {
            public PopularMerchantSource PopularMerchantSource { get; }

            public Mock<IAmazonDynamoDB> AmazonDynamoDB { get; } = new();
            public List<int> PopularMerchantIdsForBrowser { get; } = new();
            public List<int> PopularMerchantIdsForMobile { get; } = new();

            public TestState()
            {
                PopularMerchantSource = new PopularMerchantSource(Mock.Of<IConfiguration>(), AmazonDynamoDB.Object);
                var db = new Dictionary<string, List<int>>
                {
                    [PopularMerchantTarget.Browser] = PopularMerchantIdsForBrowser,
                    [PopularMerchantTarget.Mobile] = PopularMerchantIdsForMobile
                };

                AmazonDynamoDB.Setup(d => d.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
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

                PopularMerchantIdsForBrowser.AddRange(new int[] { 100, 101, 102 });
                PopularMerchantIdsForMobile.AddRange(new int[] { 100, 101, 107 });
            }
        }

        [Test]
        public async Task GetPopularMerchantIds_ShouldReturnBrowserMerchantIds_GivenBrowserTarget()
        {
            var state = new TestState();

            var merchantIds = await state.PopularMerchantSource.GetPopularMerchantIds(PopularMerchantTarget.Browser);

            merchantIds.Should().BeEquivalentTo(new int[] { 100, 101, 102 });
        }

        [Test]
        public async Task GetPopularMerchantIds_ShouldReturnMobileMerchantIds_GivenMobileTarget()
        {
            var state = new TestState();

            var merchantIds = await state.PopularMerchantSource.GetPopularMerchantIds(PopularMerchantTarget.Mobile);

            merchantIds.Should().BeEquivalentTo(new int[] { 100, 101, 107 });
        }

        [Test]
        public async Task GetPopularMerchantIds_ShouldReturnDistinctIds_GivenDuplicateMerchantIds()
        {
            var state = new TestState();
            state.PopularMerchantIdsForBrowser.AddRange(new int[] { 111, 111, 111 });

            var merchantIds = await state.PopularMerchantSource.GetPopularMerchantIds(PopularMerchantTarget.Browser);

            merchantIds.Should().BeEquivalentTo(new int[] { 100, 101, 102, 111 });
        }
    }
}
