using CashrewardsOffers.Domain.Enums;
using FluentAssertions;

namespace CashrewardsOffers.Application.AcceptanceTests.Helpers
{
    public static class StringExtensions
    {
        public static bool ToBoolean(this string s)
        {
            if (s.ToLower().StartsWith('n') || s == "0")
            {
                return false;
            }
            else if (s.ToLower().StartsWith('y') || s == "1")
            {
                return true;
            }

            return bool.Parse(s);
        }

        public static Client ToClient(this string s)
        {
            switch (s.Trim().ToLower())
            {
                case "cr": return Client.Cashrewards;
                case "anz": return Client.Anz;
                case "mme": return Client.MoneyMe;
                default: s.Should().BeOneOf("CR", "ANZ", "MME");
                    break;
            }

            return Client.Cashrewards;
        }

        public static int ToTrackingNetwork(this string s)
        {
            return s.Trim().Replace("-", "").ToLower() switch
            {
                "instore" => TrackingNetwork.Instore,
                "linkshare" => TrackingNetwork.LinkShare,
                _ => TrackingNetwork.LinkShare,
            };
        }
    }
}
