using CashrewardsOffers.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IFeaturePersistenceContext
    {
        Task<long> Count(string feature);
        Task Enrol(string feature, string cognitoId);
        Task<string> Get(string cognitoId);
    }
}
