using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Extensions;
using KellermanSoftware.CompareNetObjects;
using Mapster;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.MerchantSuggestions.Services
{
    public interface ISyncRankedMerchantService
    {
        Task TrySyncMerchantAsync();
    }

    public class SyncRankedMerchantService : ISyncRankedMerchantService
    {
        private readonly IShopGoSource _shopGoSource;
        private readonly IRankedMerchantPersistenceContext _rankedMerchantPersistenceContext;
        private readonly IMerchantPreferenceS3Source _merchantPreferenceS3Source;

        public SyncRankedMerchantService(
            IShopGoSource shopGoSource,
            IRankedMerchantPersistenceContext rankedMerchantPersistenceContext,
            IMerchantPreferenceS3Source merchantPreferenceS3Source)
        {
            _shopGoSource = shopGoSource;
            _rankedMerchantPersistenceContext = rankedMerchantPersistenceContext;
            _merchantPreferenceS3Source = merchantPreferenceS3Source;
        }

        public async Task TrySyncMerchantAsync()
        {
            Log.Information("Ranked merchant sync started at {UtcNow}", DateTime.UtcNow);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await SyncMerchantAsync();
            }
            catch (Exception ex)
            {
                Log.Error("Ranked merchant sync error - {ex}", ex);
            }
            finally
            {
                Log.Information("Ranked merchant sync completed in {Elapsed}", stopwatch.Elapsed);
            }
        }

        private async Task SyncMerchantAsync()
        {
            var shopGoRankedMerchants = (await _shopGoSource.GetRankedMerchants())
                .Where(m => m.ClientId == (int)Client.Cashrewards)
                .GroupBy(m => m.HyphenatedString)
                .ToDictionary(keySelector => keySelector.Key, valueSelector => valueSelector.FirstOrDefault());

            var existingRankedMerchantsList = (await _rankedMerchantPersistenceContext.GetAllRankedMerchants())
                .ToList();

            var existingRankedMerchants = existingRankedMerchantsList
                .ToDictionaryIgnoringDuplicates(m => m.Key);

            var existingRankedMerchantsById = existingRankedMerchants.Values
                .ToDictionaryIgnoringDuplicates(m => m.Id);

            var newRankedMerchants = (await _merchantPreferenceS3Source.DownloadLatestRankedMerchants())
                .ToDictionaryIgnoringDuplicates(m => m.Key);

            var toRemove = existingRankedMerchantsList
                .Where(em => !newRankedMerchants.ContainsKey(em.Key) || !existingRankedMerchantsById.ContainsKey(em.Id))
                .ToList();

            var toAdd = newRankedMerchants
                .Values
                .Where(nm => !existingRankedMerchants.ContainsKey(nm.Key));

            var toSync = newRankedMerchants
                .Values
                .Where(nm => existingRankedMerchants.ContainsKey(nm.Key));

            int removed = 0;
            foreach (var remove in toRemove)
            {
                await _rankedMerchantPersistenceContext.DeleteRankedMerchant(remove);
                removed++;
            }

            int added = 0;
            foreach (var add in toAdd)
            {
                if (!shopGoRankedMerchants.TryGetValue(add.HyphenatedString, out var foundMerchant)) { continue; }

                var toInsert = MapRankedMerchant(add, foundMerchant);
                await _rankedMerchantPersistenceContext.InsertRankedMerchant(toInsert);
                added++;
            }

            var compareLogic = new CompareLogic(new ComparisonConfig { MembersToIgnore = new List<string> { "Id" } });
            int updated = 0;
            int unchanged = 0;
            foreach (var update in toSync)
            {
                if (!shopGoRankedMerchants.TryGetValue(update.HyphenatedString, out var foundMerchant)) { continue; }

                var toUpdate = MapRankedMerchant(update, foundMerchant);
                var existing = existingRankedMerchants[toUpdate.Key];
                if (compareLogic.Compare(existing, toUpdate).AreEqual)
                {
                    unchanged++;
                }
                else
                {
                    toUpdate.Id = existing.Id;
                    await _rankedMerchantPersistenceContext.UpdateRankedMerchant(toUpdate);
                    updated++;
                }
            }

            Log.Information("Ranked merchant sync added:{added} updated:{updated} deleted:{removed} unchanged:{unchanged}", added, updated, removed, unchanged);
        }

        private RankedMerchant MapRankedMerchant(RankedMerchant src, ShopGoRankedMerchant shopGoRankedMerchant)
        {
            var mapped = src.Adapt<RankedMerchant>();
            mapped.IsPremiumDisabled = shopGoRankedMerchant.IsPremiumDisabled ?? false;
            mapped.RegularImageUrl = shopGoRankedMerchant.RegularImageUrl;
            mapped.MerchantId = shopGoRankedMerchant.MerchantId;
            return mapped;
        }
    }
}
