using System;

namespace CashrewardsOffers.Domain.Common
{
    public static class SydneyTime
    {
        public static DateTimeOffset ToSydneyTime(DateTimeOffset dateTimeOffset) => dateTimeOffset.ToOffset(Offset(dateTimeOffset));

        public static DateTimeOffset ConvertShopGoTimeToDateTimeOffset(DateTime shopGoTime)
        {
            if (shopGoTime == DateTime.MinValue) return DateTimeOffset.MinValue;
            if (shopGoTime == DateTime.MaxValue) return DateTimeOffset.MaxValue;

            return ToDateTimeOffset(shopGoTime);
        }

        private static DateTimeOffset ToDateTimeOffset(DateTime dateTime) => new DateTimeOffset(dateTime, Offset(dateTime));

        private static TimeSpan Offset(DateTime dateTime) =>
            IsDaylightSavingTime(dateTime)
                ? TimeSpan.FromHours(11)
                : TimeSpan.FromHours(10);

        private static TimeSpan Offset(DateTimeOffset dateTimeOffset) =>
            IsDaylightSavingTime(dateTimeOffset)
                ? TimeSpan.FromHours(11)
                : TimeSpan.FromHours(10);

        public static bool IsDaylightSavingTime(DateTime dateTime) =>
            TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time").IsDaylightSavingTime(dateTime);

        public static bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset) =>
            TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time").IsDaylightSavingTime(dateTimeOffset);
    }
}
