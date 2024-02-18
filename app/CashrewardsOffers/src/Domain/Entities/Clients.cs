using CashrewardsOffers.Domain.Enums;

namespace CashrewardsOffers.Domain.Entities
{
    public class Clients
    {
        public static Client[] All { get; } = new[]
        {
            Client.Cashrewards,
            Client.MoneyMe,
            Client.Anz
        };

        public static Client[] UsingThisMicroservice { get; } = new[]
        {
            Client.Cashrewards
        };

        public static (Client client, Client premiumClient)[] PremiumRelationships { get; } = new (Client client, Client permiumClient)[]
        {
            (Client.Cashrewards, Client.Anz)
        };
    }
}
