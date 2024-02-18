using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Merchants.Models;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Entities;
using Mapster;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Merchants.Services
{
    public interface IMerchantHistoryArchiveService
    {
        Task TryArchiveAsync();
    }

    public class MerchantHistoryArchiveService : IMerchantHistoryArchiveService
    {
        private readonly IMerchantHistoryPersistenceContext _merchantHistoryPersistenceContext;
        private readonly IS3MerchantHistoryArchive _s3MerchantHistoryArchive;
        private readonly IMerchantHistoryExcelService _merchantHistoryExcelService;
        private readonly IDomainTime _domainTime;

        public MerchantHistoryArchiveService(
            IMerchantHistoryPersistenceContext merchantHistoryPersistenceContext,
            IS3MerchantHistoryArchive s3MerchantHistoryArchive,
            IMerchantHistoryExcelService merchantHistoryExcelService,
            IDomainTime domainTime)
        {
            _merchantHistoryPersistenceContext = merchantHistoryPersistenceContext;
            _s3MerchantHistoryArchive = s3MerchantHistoryArchive;
            _merchantHistoryExcelService = merchantHistoryExcelService;
            _domainTime = domainTime;
        }

        public async Task TryArchiveAsync()
        {
            Log.Information("Merchant History Archive started at {UtcNow}", DateTime.UtcNow);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await ArchiveAsync();
            }
            catch (Exception ex)
            {
                Log.Error("Merchant History Archive error - {ex}", ex);
            }
            finally
            {
                Log.Information("Merchant History Archive completed in {Elapsed}", stopwatch.Elapsed);
            }
        }

        public async Task ArchiveAsync()
        {
            var yesterday = SydneyTime.ToSydneyTime(_domainTime.UtcNow).AddDays(-1);
            var start = new DateTimeOffset(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0, yesterday.Offset).ToUniversalTime();
            var end = start.AddDays(1).ToUniversalTime();
            var changesForYesterday = (await _merchantHistoryPersistenceContext.GetByDateRange(start, end)).Adapt<List<MerchantHistoryInfo>>();

            await _s3MerchantHistoryArchive.Put(yesterday.Year, yesterday.Month, yesterday.Day, "json", JsonConvert.SerializeObject(changesForYesterday));
            Log.Information("Merchant History Archive archived {Count} history records", changesForYesterday.Count);

            using var stream = new MemoryStream();
            await _s3MerchantHistoryArchive.Put(yesterday.Year, yesterday.Month, yesterday.Day, "xlsx", _merchantHistoryExcelService.GetExcelStream(changesForYesterday, stream));
            Log.Information("Merchant History Archive reported {Count} history records", changesForYesterday.Count);

            var deletedCount = await _merchantHistoryPersistenceContext.DeleteByDateRange(start, end);
            Log.Information("Merchant History Archive deleted {deletedCount} history records", deletedCount);
        }
    }
}
