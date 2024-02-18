using CashrewardsOffers.Application.MerchantSuggestions.Services;
using CashrewardsOffers.Domain.Entities;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashrewardsOffers.Application.UnitTests.MerchantSuggestions
{
    public class RankedMerchantRandomizationServiceTests
    {
        private class TestState
        {
            public Mock<RankedMerchantRandomizationService> MockRankedMerchantRandomizationService { get; } = new();
            public RankedMerchantRandomizationService RankedMerchantRandomizationService { get; }

            public TestState()
            {
                MockRankedMerchantRandomizationService.CallBase = true;
                RankedMerchantRandomizationService = MockRankedMerchantRandomizationService.Object;
            }
        }

        [Test]
        public void ShuffleRankedMerchants_ShouldNotShuffleMerchantList_GivenLessThan5Merchants()
        {
            var state = new TestState();
            var rankedMerchants = new List<RankedMerchant>
            {
                new RankedMerchant { Id = "1", CategoryName = "Travel", GeneratedRank = 1 },
                new RankedMerchant { Id = "2", CategoryName = "Travel", GeneratedRank = 2 },
                new RankedMerchant { Id = "3", CategoryName = "Travel", GeneratedRank = 3 }
            };

            var shuffledRankedMerchants = state.RankedMerchantRandomizationService.ShuffleRankedMerchants(rankedMerchants).ToList();

            shuffledRankedMerchants[0].Id.Should().Be("1");
            shuffledRankedMerchants[1].Id.Should().Be("2");
            shuffledRankedMerchants[2].Id.Should().Be("3");
        }

        [Test]
        public void ShuffleRankedMerchants_ShouldShuffleMerchantList_Given5OrMoreMerchants()
        {
            var state = new TestState();
            var rankedMerchants = new List<RankedMerchant>
            {
                new RankedMerchant { Id = "1", CategoryName = "Liquor", GeneratedRank = 1 },
                new RankedMerchant { Id = "2", CategoryName = "Liquor", GeneratedRank = 2 },
                new RankedMerchant { Id = "3", CategoryName = "Liquor", GeneratedRank = 3 },
                new RankedMerchant { Id = "4", CategoryName = "Liquor", GeneratedRank = 4 },
                new RankedMerchant { Id = "5", CategoryName = "Liquor", GeneratedRank = 5 },
                new RankedMerchant { Id = "6", CategoryName = "Liquor", GeneratedRank = 6 },
                new RankedMerchant { Id = "7", CategoryName = "Liquor", GeneratedRank = 7 }
            };

            var shuffledRankedMerchants = state.RankedMerchantRandomizationService.ShuffleRankedMerchants(rankedMerchants).ToList();

            state.MockRankedMerchantRandomizationService.Verify(r => r.Shuffle(It.IsAny<Random>(), It.IsAny<List<RankedMerchant>>(), It.IsAny<int>(), It.IsAny<double>()), Times.Once());
        }
    }
}
