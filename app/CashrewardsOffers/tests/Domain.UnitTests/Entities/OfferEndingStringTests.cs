using CashrewardsOffers.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Globalization;

namespace CashrewardsOffers.Domain.UnitTests.Entities
{
    public class OfferEndingStringTests
    {
        [SetUp]
        public void SetUp()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-AU", false);
        }

        /*
         * [from CPS-1050]
         * 
         * If the end date >24h and <= 30d, pass ‘{x} days’
         * else pass ‘Ongoing’
         * If the end date <=24h
         *   If the current DAY != offer end DAY, pass ‘Tomorrow’
         *   If the current DAY = offer end DAY,
         *     If the offer end date <= 8h, pass ‘Soon’
         *     If the offer end date > 8h, pass 'Today'
         */

        [TestCase("01/01/2022 00:00:00", "01/01/2023 00:00:00", "Ongoing")]
        [TestCase("01/01/2022 00:00:00", "31/01/2022 00:00:01", "Ongoing")]
        [TestCase("01/01/2022 00:00:00", "31/01/2022 00:00:00", "30 days")]
        [TestCase("01/01/2022 00:00:00", "30/01/2022 00:00:00", "29 days")]
        [TestCase("01/01/2022 00:00:00", "02/01/2022 00:00:01", "2 days")]
        [TestCase("01/01/2022 00:00:00", "02/01/2022 00:00:00", "Tomorrow")]
        [TestCase("01/01/2022 20:00:00", "02/01/2022 00:00:00", "Tomorrow")]
        [TestCase("01/01/2022 20:00:00", "02/01/2022 04:00:00", "Tomorrow")]
        [TestCase("01/01/2022 00:00:00", "01/01/2022 23:59:59", "Today")]
        [TestCase("01/01/2022 00:00:00", "01/01/2022 08:00:01", "Today")]
        [TestCase("01/01/2022 00:00:00", "01/01/2022 08:00:00", "Soon")]
        [TestCase("01/01/2022 00:00:00", "01/01/2022 00:00:01", "Soon")]
        [TestCase("01/01/2022 00:00:00", "01/01/2022 00:00:00", "Ended")]
        [TestCase("01/01/2022 00:00:00", "02/02/2021 00:00:00", "Ended")]
        public void EdmEndingString_ShouldReturnHumanReadableString_GivenVaryingEndTimes(string now, string offerEnding, string expected)
        {
            var nowDate = DateTimeOffset.Parse(now);
            var offerEndingDate = DateTimeOffset.Parse(offerEnding);

            var endingString = new OfferEndingString(offerEndingDate, nowDate).EdmEndingString;

            endingString.Should().Be(expected);
        }
    }
}
