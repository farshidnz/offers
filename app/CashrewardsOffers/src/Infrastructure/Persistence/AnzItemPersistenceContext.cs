using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Infrastructure.Models;
using Mapster;
using MongoDB.Driver;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class AnzItemPersistenceContext : IAnzItemPersistenceContext
    {
        private readonly IDocumentPersistenceContext<AnzItemDocument> _documentPersistenceContext;
        private readonly IAnzItemFactory _anzItemFactory;

        public AnzItemPersistenceContext(
            IDocumentPersistenceContext<AnzItemDocument> documentPersistenceContext,
            IAnzItemFactory anzItemFactory)
        {
            _documentPersistenceContext = documentPersistenceContext;
            _anzItemFactory = anzItemFactory;
        }

        public void Migrate()
        {
            Log.Information("Migrating AnzItemPersistenceContext");
            _documentPersistenceContext.Collection.Indexes.CreateOne(
                new CreateIndexModel<AnzItemDocument>(
                    Builders<AnzItemDocument>.IndexKeys.Ascending(x => x.ItemId),
                    new CreateIndexOptions { Unique = true })
                );
        }

        public async Task<List<AnzItem>> GetAllActiveCarouselItems()
        {
            var items = await _documentPersistenceContext.Find(IsAnzOfferFilter());

            return MapToAnzItems(items);
        }

        public async Task<AnzItem> Get(string itemId) =>
            MapToAnzItem(await _documentPersistenceContext.FindFirst(KeyFilter(itemId)));

        public async Task Insert(AnzItem anzItem) =>
            await _documentPersistenceContext.Insert(anzItem.Adapt<AnzItemDocument>());

        public async Task Update(string itemId, params (string Name, object Value)[] fields) =>
            await _documentPersistenceContext.Update(KeyFilter(itemId), fields);

        public async Task Replace(AnzItem anzItem) =>
            await _documentPersistenceContext.Replace(anzItem.Adapt<AnzItemDocument>());

        public async Task<List<AnzItem>> GetPage(int ? pageSize = null, int pageNumber = 1, long? updatedAfter = null)
        {
            var options = new FindOptions<AnzItemDocument, AnzItemDocument>
            {
                Limit = pageSize,
                Skip = System.Math.Max(pageNumber - 1, 0) * (pageSize ?? 0),
            };

            var items = await _documentPersistenceContext.Find(UpdatedAfterFilter(updatedAfter), options);

            return MapToAnzItems(items);
        }

        public async Task<long> GetCount(long? updatedAfter = null) => await _documentPersistenceContext.Count(UpdatedAfterFilter(updatedAfter));

        private AnzItem MapToAnzItem(AnzItemDocument item) =>
            item
                .BuildAdapter()
                .AddParameters("anzItemFactory", _anzItemFactory)
                .AdaptToType<AnzItem>();

        private List<AnzItem> MapToAnzItems(IEnumerable<AnzItemDocument> items) =>
            items
                .BuildAdapter()
                .AddParameters("anzItemFactory", _anzItemFactory)
                .AdaptToType<List<AnzItem>>();

        private static FilterDefinition<AnzItemDocument> KeyFilter(string itemId) => Builders<AnzItemDocument>.Filter.Eq("ItemId", itemId);

        private static FilterDefinition<AnzItemDocument> UpdatedAfterFilter(long? updatedAfter) =>
            updatedAfter.HasValue ? DeltaFilter(updatedAfter.Value) : IsAnzOfferFilter();

        private static FilterDefinition<AnzItemDocument> DeltaFilter(long updatedAfter) =>
            Builders<AnzItemDocument>.Filter.Gt("LastUpdated", updatedAfter);

        private static FilterDefinition<AnzItemDocument> IsAnzOfferFilter() =>
            Builders<AnzItemDocument>.Filter.And(
                new List<FilterDefinition<AnzItemDocument>>
                {
                    Builders<AnzItemDocument>.Filter.Eq("IsDeleted", false),
                    Builders<AnzItemDocument>.Filter.Eq("Merchant.MobileEnabled", true),
                    Builders<AnzItemDocument>.Filter.Eq("Merchant.IsPremiumDisabled", false),
                    Builders<AnzItemDocument>.Filter.Ne("Merchant.IsPaused", true),
                    IsOnAnyCarouselFilter()
                });

        private static FilterDefinition<AnzItemDocument> IsOnAnyCarouselFilter() =>
            Builders<AnzItemDocument>.Filter.Or(
                new List<FilterDefinition<AnzItemDocument>>
                {
                    IsOnInstoreCarouselFilter(),
                    IsOnPopularCarouselFilter(),
                    Builders<AnzItemDocument>.Filter.Eq("Offer.IsFeatured", true),
                    Builders<AnzItemDocument>.Filter.Eq("Offer.IsExclusive", true)
                });

        private static FilterDefinition<AnzItemDocument> IsOnInstoreCarouselFilter() =>
            Builders<AnzItemDocument>.Filter.And(
                new List<FilterDefinition<AnzItemDocument>>
                {
                    Builders<AnzItemDocument>.Filter.Eq("Offer.Id", 0),
                    Builders<AnzItemDocument>.Filter.Eq("Merchant.NetworkId", TrackingNetwork.Instore)
                });

        private static FilterDefinition<AnzItemDocument> IsOnPopularCarouselFilter() =>
            Builders<AnzItemDocument>.Filter.And(
                new List<FilterDefinition<AnzItemDocument>>
                {
                    Builders<AnzItemDocument>.Filter.Eq("Offer.Id", 0),
                    Builders<AnzItemDocument>.Filter.Eq("Merchant.IsPopularFlag", true),
                    Builders<AnzItemDocument>.Filter.Gt("Merchant.PopularMerchantRankingForBrowser", 0),
                });
    }
}
