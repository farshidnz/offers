using CashrewardsOffers.Application.Merchants.Models;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using FluentAssertions;
using Mapster;
using NUnit.Framework;
using System;
using System.Reflection;

namespace CashrewardsOffers.Application.UnitTests.Merchants
{
    public class MerchantHistoryMappingTests
    {
        private static MerchantHistory MerchantHistoryDomainEntity = new MerchantHistory
        {
            Id = "1234567",
            ChangeTime = new DateTimeOffset(2022, 9, 6, 14, 0, 0, TimeSpan.Zero),
            MerchantId = 1234,
            Client = Client.Cashrewards,
            Name = "Merchant 1",
            HyphenatedString = "merchant-1",
            ClientCommissionString = "10%"
        };

        private static MerchantHistoryInfo MerchantHistoryInfo = new MerchantHistoryInfo
        {
            ChangeInSydneyTime = new DateTimeOffset(2022, 9, 7, 0, 0, 0, TimeSpan.FromHours(10)),
            MerchantId = 1234,
            Client = (int)Client.Cashrewards,
            Name = "Merchant 1",
            HyphenatedString = "merchant-1",
            ClientCommissionString = "10%"
        };

        [Test]
        public void Adapt_ShouldMapToInfo_GivenDomainEntity()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            var info = MerchantHistoryDomainEntity.Adapt<MerchantHistoryInfo>();

            info.Should().BeEquivalentTo(MerchantHistoryInfo);
        }
    }
}
