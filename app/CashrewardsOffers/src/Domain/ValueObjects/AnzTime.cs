using System;

namespace CashrewardsOffers.Domain.ValueObjects
{
    public class AnzTime
    {
        public static readonly DateTimeOffset MinValue = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset MaxValue = new(9999, 12, 31, 0, 0, 0, TimeSpan.Zero);
    }
}
