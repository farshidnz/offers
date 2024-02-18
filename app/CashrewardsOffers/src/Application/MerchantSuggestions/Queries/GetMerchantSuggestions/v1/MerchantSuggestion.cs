using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.MerchantSuggestions.Queries.GetMerchantSuggestions.v1
{
    public class MerchantSuggestion
    {
        public int MerchantId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Rank { get; set; }
        public string MerchantName { get; set; }
        public string HyphenatedString { get; set; }
        public string RegularImageUrl { get; set; }
    }
}
