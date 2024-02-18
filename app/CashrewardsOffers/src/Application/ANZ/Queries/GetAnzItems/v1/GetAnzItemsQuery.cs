using CashrewardsOffers.Application.Common.Interfaces;
using Mapster;
using MassTransit;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.ANZ.Queries.GetAnzItems.v1
{
    public class GetAnzItemsQuery
    {
        public int OffersPerPage { get; set; }
        public int PageNumber { get; set; }
        public long? UpdatedAfter { get; set; }
    }

    public class GetAnzItemsResponse
    {
        public int TotalOffersCount { get; set; }
        public int PageOffersCount { get; set; }
        public int TotalPageCount { get; set; }
        public int PageNumber { get; set; }
        public long AsAt { get; set; }
        public List<AnzItemInfo> Items { get; set; }
    }

    public class AnzItemInfo
    {
        public string Id { get; set; }
        public bool IsDeleted { get; set; }
        public AnzMerchantInfo Merchant { get; set; }
        public AnzOfferInfo Offer { get; set; }
    }

    public class AnzMerchantInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public string LogoUrl { get; set; }
        public string ClientCommissionString { get; set; }
        public string ExclusiveClientCommissionString { get; set; }
        public string SpecialTerms { get; set; }
        public string CashbackGuidelines { get; set; }
        public bool IsPopular { get; set; }
        public int PopularRanking { get; set; }
        public bool IsInstore { get; set; }
        public int InstoreRanking { get; set; }
        public string InstoreTerms { get; set; }
        public List<AnzMerchantCategoryInfo> Categories { get; set; }
    }

    public class AnzOfferInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Terms { get; set; }
        public string Link { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public string WasRate { get; set; }
        public bool IsFeatured { get; set; }
        public int FeaturedRanking { get; set; }
        public bool IsExclusive { get; set; }
        public int ExclusiveRanking { get; set; }
    }

    public class AnzMerchantCategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GetAnzItemsQueryConsumer : IConsumer<GetAnzItemsQuery>
    {
        private readonly IAnzItemPersistenceContext _anzItemPersistenceContext;

        public GetAnzItemsQueryConsumer(IAnzItemPersistenceContext anzItemPersistenceContext)
        {
            _anzItemPersistenceContext = anzItemPersistenceContext;
        }

        public async Task Consume(ConsumeContext<GetAnzItemsQuery> context)
        {
            Log.Information($"Query: {JsonConvert.SerializeObject(context.Message)}");

            var fetchTime = DateTimeOffset.UtcNow.UtcTicks;

            int? offersPerPage = context.Message.OffersPerPage;
            int pageNumber = Math.Max(context.Message?.PageNumber ?? 1, 1);
            long? updatedAfter = context.Message.UpdatedAfter;

            var items = await _anzItemPersistenceContext.GetPage(offersPerPage, pageNumber, updatedAfter);
            var count = (int)await _anzItemPersistenceContext.GetCount(updatedAfter);

            await context.RespondAsync(new GetAnzItemsResponse
            {
                TotalOffersCount = count,
                PageOffersCount = items.Count,
                TotalPageCount = CalcTotalPageCount(count, offersPerPage),
                PageNumber = pageNumber,
                AsAt = fetchTime,
                Items = items.Adapt<List<AnzItemInfo>>()
            });
        }

        public static int CalcTotalPageCount(int count, int? offersPerPage = null)
        {
            var pagesCount = count > 0 ? 1 : 0;
            count = Math.Max(count, 0);
            Func<int, int, bool> hasRemainder = (n, m) => n % m != 0;
            if (offersPerPage.HasValue && offersPerPage.Value > 0)
            {
                pagesCount = count / offersPerPage.Value;
                pagesCount += hasRemainder(count, offersPerPage.Value) ? 1 : 0;
            }
            return pagesCount;
        }

    }
}
