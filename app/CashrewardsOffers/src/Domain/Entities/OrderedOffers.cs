using CashrewardsOffers.Domain.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace CashrewardsOffers.Domain.Entities
{
    public class OrderedOffers
    {
        public List<Offer> Offers { get; }

        public OrderedOffers(List<Offer> offers)
        {
            Offers = offers
                .OrderByDescending(o => o.Ranking)
                .ThenBy(o => o.EndDateTime)
                .ThenBy(o => o.Merchant.Id)
                .ThenBy(o => o.OfferId)
                .ToList();
        }

        public void ReorderOffersForExperiments(string userExperimentFeature, List<FavouriteMerchant> favouriteMerchants)
        {
            List<Offer> personalisedOffers = new List<Offer>();
            switch (userExperimentFeature)
            {
                case FeatureToggle.Exp1: ReorderOffersForExperiment1(favouriteMerchants, out personalisedOffers); break;
                case FeatureToggle.Exp2: ReorderOffersForExperiment2(favouriteMerchants, out personalisedOffers); break;
                case FeatureToggle.Exp3: ReorderOffersForExperiment3(favouriteMerchants, out personalisedOffers); break;
            };

            personalisedOffers.ForEach(o => o.IsPersonalised = true);
        }

        private void ReorderOffersForExperiment1(List<FavouriteMerchant> favouriteMerchants, out List<Offer> personalisedOffers)
        {
            var favouriteOffers = SelectFavouriteOffers(favouriteMerchants)
                .OrderByDescending(o => o.Premium?.ClientCommission ?? o.Merchant.ClientCommission)
                .ThenByDescending(o => (int?)o.Premium?.CommissionType ?? (int)o.Merchant.CommissionType)
                .ThenByDescending(o => o.Premium?.IsFlatRate ?? o.Merchant.IsFlatRate)
                .Take(2)
                .ToList();
            favouriteOffers.ForEach(f => Offers.Remove(f));
            Offers.InsertRange(0, favouriteOffers);
            personalisedOffers = favouriteOffers;
        }

        private void ReorderOffersForExperiment2(List<FavouriteMerchant> favouriteMerchants, out List<Offer> personalisedOffers)
        {
            var favouriteOffers = SelectFavouriteOffers(favouriteMerchants)
                .OrderBy(o => o.EndDateTime)
                .ThenByDescending(o => o.Premium?.ClientCommission ?? o.Merchant.ClientCommission)
                .ThenByDescending(o => (int?)o.Premium?.CommissionType ?? (int)o.Merchant.CommissionType)
                .ThenByDescending(o => o.Premium?.IsFlatRate ?? o.Merchant.IsFlatRate)
                .Take(2)
                .ToList();
            favouriteOffers.ForEach(f => Offers.Remove(f));
            Offers.InsertRange(0, favouriteOffers);
            personalisedOffers = favouriteOffers;
        }

        private void ReorderOffersForExperiment3(List<FavouriteMerchant> favouriteMerchants, out List<Offer> personalisedOffers)
        {
            var favouriteOffers = SelectFavouriteOffersOrderedBySelection(favouriteMerchants)
                .Take(2)
                .ToList();
            favouriteOffers.ForEach(f => Offers.Remove(f));
            Offers.InsertRange(0, favouriteOffers);
            personalisedOffers = favouriteOffers;
        }

        private IEnumerable<Offer> SelectFavouriteOffers(List<FavouriteMerchant> favouriteMerchants)
        {
            var favouriteMerchantIds = favouriteMerchants.Select(f => f.MerchantId).ToHashSet();
            var favouriteHyphenatedStrings = favouriteMerchants.Select(f => f.HyphenatedString).Where(h => !string.IsNullOrEmpty(h)).ToHashSet();
            return Offers
                .Where(o => favouriteMerchantIds.Contains(o.Merchant.Id) || favouriteHyphenatedStrings.Contains(o.Merchant.HyphenatedString));
        }

        private IEnumerable<Offer> SelectFavouriteOffersOrderedBySelection(List<FavouriteMerchant> favouriteMerchants)
        {
            var favouriteMerchantIds = favouriteMerchants
                .Select(f => (f.MerchantId, f.SelectionOrder))
                .ToDictionaryIgnoringDuplicates(f => f.MerchantId);

            var favouriteHyphenatedStrings = favouriteMerchants
                .Select(f => (f.HyphenatedString, f.SelectionOrder))
                .Where(f => !string.IsNullOrEmpty(f.HyphenatedString))
                .ToDictionaryIgnoringDuplicates(f => f.HyphenatedString);

            return Offers
                .Where(o => favouriteMerchantIds.ContainsKey(o.Merchant.Id) || favouriteHyphenatedStrings.ContainsKey(o.Merchant.HyphenatedString))
                .OrderBy(o => favouriteMerchantIds.TryGetValue(o.Merchant.Id, out var f1)
                    ? f1.SelectionOrder
                    : favouriteHyphenatedStrings.TryGetValue(o.Merchant.HyphenatedString, out var f2) ? f2.SelectionOrder : 0);
        }
    }
}
