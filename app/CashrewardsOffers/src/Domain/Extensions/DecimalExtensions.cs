using System;

namespace CashrewardsOffers.Domain.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal RoundToTwoDecimalPlaces(this decimal d) => Math.Round(d, 2);
    }
}
