using System.Collections.Generic;

namespace CashrewardsOffers.Infrastructure.Models
{
    public class PopularStoreConfig
    {
        public PopularStoreConfig()
        {
            MerchantIds = new List<int>();
        }

        public List<int> MerchantIds { get; set; }
    }
}
