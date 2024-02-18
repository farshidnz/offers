using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IS3Source
    {
        Task<string> DownloadTextFileAsync(string fileKey);
        Task<string> DownloadLatestTextFileAsync(string prefix);
        Task<string> GetLatestModifiedFileKey(string prefix);
        Task<IEnumerable<string>> GetFileKeys(string prefix);
    }
}
