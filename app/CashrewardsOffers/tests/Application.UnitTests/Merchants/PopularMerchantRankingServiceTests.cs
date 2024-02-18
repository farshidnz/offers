using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Merchants.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Merchants
{
    public class PopularMerchantRankingServiceTests
    {
        private class TestState
        {
            public PopularMerchantRankingService PopularMerchantRankingService { get; }
            public Mock<IPopularMerchantSource> PopularMerchantSource { get; } = new();
            public List<int> PopularMerchantIdsForBrowser { get; } = new() { 100, 101, 102, 103 };
            public List<int> PopularMerchantIdsForMobile { get; } = new() { 100, 102, 104, 108 };

            public TestState()
            {
                PopularMerchantSource.Setup(s => s.GetPopularMerchantIds(It.Is<string>(t => t == PopularMerchantTarget.Browser))).ReturnsAsync(PopularMerchantIdsForBrowser);
                PopularMerchantSource.Setup(s => s.GetPopularMerchantIds(It.Is<string>(t => t == PopularMerchantTarget.Mobile))).ReturnsAsync(PopularMerchantIdsForMobile);

                PopularMerchantRankingService = new PopularMerchantRankingService(PopularMerchantSource.Object);
            }
        }

        [Test]
        public async Task LoadRankings_ShouldCallPopularMerchantSourceWithTargets()
        {
            var state = new TestState();

            await state.PopularMerchantRankingService.LoadRankings();

            state.PopularMerchantSource.Verify(s => s.GetPopularMerchantIds(It.Is<string>(s => s == PopularMerchantTarget.Browser)), Times.Once);
            state.PopularMerchantSource.Verify(s => s.GetPopularMerchantIds(It.Is<string>(s => s == PopularMerchantTarget.Mobile)), Times.Once);
            state.PopularMerchantSource.Verify(s => s.GetPopularMerchantIds(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public async Task GetBrowserRanking_ShouldReturnCorrectRanking_AndReturnZeroForUnranked()
        {
            var state = new TestState();
            await state.PopularMerchantRankingService.LoadRankings();

            state.PopularMerchantRankingService.GetBrowserRanking(100).Should().Be(1);
            state.PopularMerchantRankingService.GetBrowserRanking(101).Should().Be(2);
            state.PopularMerchantRankingService.GetBrowserRanking(102).Should().Be(3);
            state.PopularMerchantRankingService.GetBrowserRanking(103).Should().Be(4);
            state.PopularMerchantRankingService.GetBrowserRanking(104).Should().Be(0);
        }

        [Test]
        public async Task GetMobileRanking_ShouldReturnCorrectRanking_AndReturnZeroForUnranked()
        {
            var state = new TestState();
            await state.PopularMerchantRankingService.LoadRankings();

            state.PopularMerchantRankingService.GetMobileRanking(100).Should().Be(1);
            state.PopularMerchantRankingService.GetMobileRanking(101).Should().Be(0);
            state.PopularMerchantRankingService.GetMobileRanking(102).Should().Be(2);
            state.PopularMerchantRankingService.GetMobileRanking(104).Should().Be(3);
            state.PopularMerchantRankingService.GetMobileRanking(108).Should().Be(4);
        }
    }
}
