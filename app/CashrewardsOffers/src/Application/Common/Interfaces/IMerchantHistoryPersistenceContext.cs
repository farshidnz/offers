using CashrewardsOffers.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IMerchantHistoryPersistenceContext
    {
        Task Add(MerchantHistory merchantHistory);
        Task<List<MerchantHistory>> GetByDateRange(DateTimeOffset start, DateTimeOffset end);
        Task<long> DeleteByDateRange(DateTimeOffset start, DateTimeOffset end);
    }
}
