using System.IO;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IS3MerchantHistoryArchive
    {
        Task Put(int year, int month, int day, string extension, string data);
        Task Put(int year, int month, int day, string extension, Stream stream);
    }
}
