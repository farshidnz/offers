using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using Mapster;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class MerchantHistoryPersistenceContext : IMerchantHistoryPersistenceContext
    {
        private readonly IDocumentPersistenceContext<MerchantHistoryDocument> _documentPersistenceContext;

        public MerchantHistoryPersistenceContext(IDocumentPersistenceContext<MerchantHistoryDocument> documentPersistenceContext)
        {
            _documentPersistenceContext = documentPersistenceContext;
        }

        public async Task Add(MerchantHistory merchantHistory)
        {
            await _documentPersistenceContext.Insert(merchantHistory.Adapt<MerchantHistoryDocument>());
        }

        public async Task<List<MerchantHistory>> GetByDateRange(DateTimeOffset start, DateTimeOffset end) =>
            (await _documentPersistenceContext.Find(DateRangeFilter(start, end))).Adapt<List<MerchantHistory>>();

        public async Task<long> DeleteByDateRange(DateTimeOffset start, DateTimeOffset end) =>
            (await _documentPersistenceContext.Collection.DeleteManyAsync(DateRangeFilter(start, end))).DeletedCount;

        private static FilterDefinition<MerchantHistoryDocument> DateRangeFilter(DateTimeOffset start, DateTimeOffset end) =>
            Builders<MerchantHistoryDocument>.Filter.And(
                new List<FilterDefinition<MerchantHistoryDocument>>
                {
                    Builders<MerchantHistoryDocument>.Filter.Gte("ChangeTime", start),
                    Builders<MerchantHistoryDocument>.Filter.Lt("ChangeTime", end)
                });
    }
}
