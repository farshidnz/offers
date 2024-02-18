using CashrewardsOffers.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashrewardsOffers.Application.ANZ.Services
{
    public abstract class Ranking
    {
        abstract public int? GetItemRank(AnzItem anzItem);
        abstract public AnzItem SetItemRank(AnzItem anzItem, int? newRank);
        abstract public List<AnzItem> Ordering(List<AnzItem> anzItems);
        abstract public bool Filter(AnzItem anzItem);


        public static string GetUniqueId(AnzItem anzItem) {
            return anzItem.ItemId;
        }
        private Func<string, int?> AnzItemPositionTable { get; set; } = _ => throw new NotImplementedException();

        public Ranking BuildAnzPositionTable(List<AnzItem> anzItems)
        {
            AnzItemPositionTable = CalculateOrderTable(anzItems);
            return this;
        }

        private Func<string, int?> CalculateOrderTable(List<AnzItem> anzItems)
        {
            var orderedOffers = Ordering(anzItems);
            var featuredOfferRankingTable = orderedOffers
                .Where(item => Filter(item))
                .Select((item, index) => (item, index))
                .ToDictionary(pair => GetUniqueId(pair.item), pair => pair.index + 1);

            return id => featuredOfferRankingTable.TryGetValue(id, out int position) ? position : null;
        }

        public bool HasPositionChanged(AnzItem anzItem)
        {
            return (AnzItemPositionTable(GetUniqueId(anzItem)) ?? 0) != (GetItemRank(anzItem) ?? 0);
        }

        public AnzItem UpdateAnzItemWithNewPosition(AnzItem anzItem)
        {
            SetItemRank(anzItem, AnzItemPositionTable(GetUniqueId(anzItem)));
            return anzItem;
        }

    }

    public class Rankings
    {
        private readonly List<Ranking> rankings = new List<Ranking>();
        public Rankings()
        {
        }

        public Rankings Add(Ranking ranking)
        {
            rankings.Add(ranking);
            return this;
        }

        public List<AnzItem> AnzItemsToUpdate(List<AnzItem> anzItems)
        {
            rankings.ForEach(item => item.BuildAnzPositionTable(anzItems));

            return anzItems.Where(anzItem => rankings.Any(ranking => ranking.HasPositionChanged(anzItem)))
                .Select(anzItem =>
                {
                    rankings.ForEach(ranking => anzItem = ranking.UpdateAnzItemWithNewPosition(anzItem));
                    return anzItem;
                }).ToList();
        }
    }


    public class FeaturedOffersRanking : Ranking
    {
        override public int? GetItemRank (AnzItem anzItem)
        {
            return anzItem?.Offer?.FeaturedRanking;
        }

        override public AnzItem SetItemRank(AnzItem anzItem, int? newRank)
        {
            anzItem.Offer.FeaturedRanking = newRank.GetValueOrDefault(0);
            return anzItem;
        }

        override public List<AnzItem> Ordering(List<AnzItem> anzItems)
        {
            return anzItems.OrderByDescending(o => o.Offer.Ranking)
                .ThenBy(o => o.Offer.EndDateTime)
                .ThenBy(o => o.Merchant.Id)
                .ThenBy(o => o.Offer.Id)
                .ToList();
        }

        override public bool Filter(AnzItem anzItem)
        {
            return anzItem.Offer.IsFeatured;
        }
    }

    public class InStoreMerchantRanking : Ranking
    {
        override public int? GetItemRank(AnzItem anzItem) => anzItem?.Merchant?.InstoreRanking;

        override public AnzItem SetItemRank(AnzItem anzItem, int? newRank)
        {
            anzItem.Merchant.InstoreRanking = newRank.GetValueOrDefault(0);
            return anzItem;
        }

        override public List<AnzItem> Ordering(List<AnzItem> anzItems)
        {
            return anzItems.OrderByDescending(r => r.Merchant.IsHomePageFeatured)
            .ThenByDescending(r => r.Merchant.IsFeatured)
            .ThenByDescending(r => r.Merchant.IsPopularFlag)
            .ThenBy(r => r.Merchant.Name)
            .ToList();
        }

        override public bool Filter(AnzItem anzItem) => anzItem.IsInstore;
    }

    public class PopularMerchantRanking : Ranking
    {
        override public int? GetItemRank(AnzItem anzItem) => anzItem?.Merchant?.PopularRanking;

        override public AnzItem SetItemRank(AnzItem anzItem, int? newRank)
        {
            anzItem.Merchant.PopularRanking = newRank.GetValueOrDefault(anzItem.Merchant.PopularRanking);
            return anzItem;
        }

        override public List<AnzItem> Ordering(List<AnzItem> anzItems)
        {
            return anzItems.OrderBy(r => r.Merchant.PopularMerchantRankingForBrowser)
                .ToList();
        }

        override public bool Filter(AnzItem anzItem) => anzItem.IsPopular;
    }
}
