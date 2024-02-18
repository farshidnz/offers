using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.EDM
{
    public class ShopGoEdmItem
    {
        public int EDMItemId { get; set; }
        public int EDMCampaignId { get; set; }
        public int EDMId { get; set; }
        public string Title { get; set; }
        public int MerchantId { get; set; }
        public int OfferId { get; set; }
        public string Type { get; set; }
    }
}
