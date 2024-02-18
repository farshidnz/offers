using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashrewardsOffers.Domain.Entities;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IMerchantPreferenceS3Source
    {
        Task<IEnumerable<RankedMerchant>> DownloadLatestRankedMerchants();
    }
}
