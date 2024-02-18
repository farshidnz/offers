using CashrewardsOffers.Domain.Common;
using FluentAssertions;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using System;

namespace CashrewardsOffers.Domain.UnitTests.Common
{
    public class SydneyTimeTests
    {
        [Test]
        public void IsDaylightSavingTime_ShouldReturnTrue_GivenSummerDateTime()
        {
            var summerDateTime = new DateTime(2022, 12, 10);

            SydneyTime.IsDaylightSavingTime(summerDateTime).Should().Be(true);
        }

        [Test]
        public void IsDaylightSavingTime_ShouldReturnFalse_GivenWinterDateTime()
        {
            var winterDateTime = new DateTime(2022, 6, 10);

            SydneyTime.IsDaylightSavingTime(winterDateTime).Should().Be(false);
        }

        [Test]
        public void ConvertShopGoTimeToDateTimeOffset_ShouldReturnAedt_GivenSummerDateTime()
        {
            var summerDateTime = new DateTime(2022, 12, 10);

            SydneyTime.ConvertShopGoTimeToDateTimeOffset(summerDateTime).Should().Be(new DateTimeOffset(2022, 12, 10, 0, 0, 0, TimeSpan.FromHours(11)));
        }

        [Test]
        public void ConvertShopGoTimeToDateTimeOffset_ShouldReturnAest_GivenWinterDateTime()
        {
            var winterDateTime = new DateTime(2022, 6, 10);

            SydneyTime.ConvertShopGoTimeToDateTimeOffset(winterDateTime).Should().Be(new DateTimeOffset(2022, 6, 10, 0, 0, 0, TimeSpan.FromHours(10)));
        }

        [Test]
        public void ToSydneyTime_ShouldReturnAedt_GivenSummerDateTime()
        {
            var summerDateTime = new DateTimeOffset(2022, 12, 10, 20, 0, 0, TimeSpan.Zero);

            SydneyTime.ToSydneyTime(summerDateTime).Should().Be(new DateTimeOffset(2022, 12, 11, 7, 0, 0, TimeSpan.FromHours(11)));
        }

        [Test]
        public void ToSydneyTime_ShouldReturnAest_GivenWinterDateTime()
        {
            var winterDateTime = new DateTimeOffset(2022, 6, 10, 20, 0, 0, TimeSpan.Zero);

            SydneyTime.ToSydneyTime(winterDateTime).Should().Be(new DateTimeOffset(2022, 6, 11, 6, 0, 0, TimeSpan.FromHours(10)));
        }
    }
}
