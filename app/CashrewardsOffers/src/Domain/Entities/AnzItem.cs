using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using KellermanSoftware.CompareNetObjects;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CashrewardsOffers.Domain.Entities
{
    public interface IAnzItemFactory
    {
        AnzItem Create(int merchantId, int? offerId = null);
    }

    public class AnzItemFactory : IAnzItemFactory
    {
        public AnzItem Create(int merchantId, int? offerId = null)
        {
            return new AnzItem(merchantId, offerId, DomainTime);
        }

        public IDomainTime DomainTime { get; set; } = new DateTimeOffsetWrapper();
    }

    public class AnzItem
    {
        private readonly IDomainTime _domainTime;

        public static Uri HostUri { get; set; }

        internal AnzItem(int merchantId, int? offerId, IDomainTime domainTime)
        {
            _domainTime = domainTime;
            Merchant.Id = merchantId;
            Offer.Id = offerId ?? 0;
        }

        public string Id { get; set; }
        public string ItemId => Offer.Id != 0 ? $"{Merchant.Id}-{Offer.Id}" : $"{Merchant.Id}";
        public long LastUpdated { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPopular => !IsDeleted && Offer.Id == 0 && Merchant.PopularMerchantRankingForBrowser != 0;
        public bool IsInstore => !IsDeleted && Offer.Id == 0 && Merchant.NetworkId == TrackingNetwork.Instore;
        public AnzMerchant Merchant { get; set; } = new();
        public AnzOffer Offer { get; set; } = new();

        private static string MakeLink(string merchantHyphenatedString, string offerHyphenatedString = "")
        {
            if (HostUri == null || merchantHyphenatedString == null) return null;

            var EmptyCodeValues = new List<string>() { null, "" };
            var isEmptyCode = (string code) => EmptyCodeValues.Contains(code?.Trim());
            
            var EmptyQueryValues = new List<string>() { null, "?", "" };
            var isEmptyQuery = (string query) => EmptyQueryValues.Contains(query?.Trim());

            var formatCouponParam = (string code) => isEmptyCode(code) ? "" : $"coupon={code}";
            var joinQueries = (string uriBuilderQuery, string additionalQueryParams) =>
                string.Join("&", new List<string> { uriBuilderQuery, additionalQueryParams }.FindAll(query => !isEmptyQuery(query)));

            var uriBuilder = new UriBuilder(HostUri);
            uriBuilder.Path = merchantHyphenatedString;
            uriBuilder.Query = joinQueries(uriBuilder.Query, formatCouponParam(offerHyphenatedString));

            return uriBuilder.Uri.AbsoluteUri;
        }

        private readonly CompareLogic _compareLogic = new();

        public void ApplyChanges(OfferEventBase offerChangedEvent)
        {
            UpdatePropertyIfChanged(t => t.IsDeleted, false);
            UpdatePropertyIfChanged(t => t.Offer.Link, MakeLink(offerChangedEvent.Merchant?.HyphenatedString, offerChangedEvent.HyphenatedString));
            UpdatePropertyIfChanged(t => t.Offer.Title, offerChangedEvent.Title);
            UpdatePropertyIfChanged(t => t.Offer.Terms, offerChangedEvent.Terms);
            UpdatePropertyIfChanged(t => t.Offer.WasRate, offerChangedEvent.WasRate);
            UpdatePropertyIfChanged(t => t.Offer.EndDateTime, offerChangedEvent.EndDateTime);
            UpdatePropertyIfChanged(t => t.Offer.Ranking, offerChangedEvent.Ranking);
            UpdatePropertyIfChanged(t => t.Offer.IsFeatured, offerChangedEvent.IsFeatured);
            UpdatePropertyIfChanged(t => t.Merchant.NetworkId, offerChangedEvent.Merchant.NetworkId);
            UpdatePropertyIfChanged(t => t.Merchant.MobileEnabled, offerChangedEvent.Merchant.MobileEnabled);
            UpdatePropertyIfChanged(t => t.Merchant.IsPremiumDisabled, offerChangedEvent.Merchant.IsPremiumDisabled);
            UpdatePropertyIfChanged(t => t.Merchant.Name, offerChangedEvent.Merchant.Name);
            UpdatePropertyIfChanged(t => t.Merchant.LogoUrl, offerChangedEvent.Merchant.LogoUrl);
            UpdatePropertyIfChanged(t => t.Merchant.CashbackGuidelines, offerChangedEvent.Merchant.BasicTerms);
            UpdatePropertyIfChanged(t => t.Merchant.SpecialTerms, offerChangedEvent.Merchant.ExtentedTerms);
            UpdatePropertyIfChanged(t => t.Merchant.IsHomePageFeatured, offerChangedEvent.Merchant.IsHomePageFeatured);

            UpdatePropertyIfChanged(t => t.Merchant.Link, MakeLink(offerChangedEvent.Merchant.HyphenatedString));
            UpdatePropertyIfChanged(t => t.Merchant.ClientCommissionString, offerChangedEvent.Merchant.ClientCommissionString);

            UpdatePropertyIfChanged(t => t.Merchant.IsPaused, offerChangedEvent.IsMerchantPaused);

            var changeCategories = offerChangedEvent.Merchant.Categories.Adapt<List<AnzMerchantCategory>>();
            if (!_compareLogic.Compare(Merchant.Categories, changeCategories).AreEqual)
            {
                Merchant.Categories = changeCategories;
                SetLastUpdated();
            }
        }

        public void ApplyChanges(MerchantEventBase merchantChangedEvent)
        {
            UpdatePropertyIfChanged(t => t.IsDeleted, false);
            UpdatePropertyIfChanged(t => t.Merchant.Link, MakeLink(merchantChangedEvent.HyphenatedString));
            UpdatePropertyIfChanged(t => t.Merchant.ClientCommissionString, merchantChangedEvent.ClientCommissionString);
            UpdatePropertyIfChanged(t => t.Merchant.IsPopularFlag, merchantChangedEvent.IsPopular);
            UpdatePropertyIfChanged(t => t.Merchant.NetworkId, merchantChangedEvent.NetworkId);
            UpdatePropertyIfChanged(t => t.Merchant.MobileEnabled, merchantChangedEvent.MobileEnabled);
            UpdatePropertyIfChanged(t => t.Merchant.IsPremiumDisabled, merchantChangedEvent.IsPremiumDisabled);
            UpdatePropertyIfChanged(t => t.Merchant.PopularMerchantRankingForBrowser, merchantChangedEvent.PopularMerchantRankingForBrowser);
            UpdatePropertyIfChanged(t => t.Merchant.Name, merchantChangedEvent.Name);
            UpdatePropertyIfChanged(t => t.Merchant.LogoUrl, merchantChangedEvent.LogoUrl);
            UpdatePropertyIfChanged(t => t.Merchant.CashbackGuidelines, merchantChangedEvent.BasicTerms);
            UpdatePropertyIfChanged(t => t.Merchant.SpecialTerms, merchantChangedEvent.ExtentedTerms);
            UpdatePropertyIfChanged(t => t.Merchant.IsHomePageFeatured, merchantChangedEvent.IsHomePageFeatured);
            UpdatePropertyIfChanged(t => t.Merchant.IsFeatured, merchantChangedEvent.IsFeatured);
            UpdatePropertyIfChanged(t => t.Merchant.IsPaused, merchantChangedEvent.IsPaused);

            var changeCategories = merchantChangedEvent.Categories.Adapt<List<AnzMerchantCategory>>();
            if (!_compareLogic.Compare(Merchant.Categories, changeCategories).AreEqual)
            {
                Merchant.Categories = changeCategories;
                SetLastUpdated();
            }
        }

        public void SetAsDeleted()
        {
            UpdatePropertyIfChanged(t => t.IsDeleted, true);
        }

        public void SetLastUpdated()
        {
            LastUpdated = _domainTime.UtcNow.UtcTicks;
        }

        public bool IsUnavailable =>
            IsDeleted
            || Merchant.IsPremiumDisabled
            || Merchant.IsPaused
            || !Merchant.MobileEnabled
            || (!IsPopular && !IsInstore && !Offer.IsFeatured);

        private void UpdatePropertyIfChanged<T>(Expression<Func<AnzItem, T>> expression, T value)
        {
            var existingValue = expression.Compile()(this);
            if ((value == null && existingValue != null) || (value != null && !value.Equals(existingValue)))
            {
                var expressionBody = expression.Body as MemberExpression;
                object container = expressionBody.Expression is MemberExpression containerExpression
                    ? (containerExpression.Member as PropertyInfo).GetValue(this)
                    : this;

                (expressionBody.Member as PropertyInfo).SetValue(container, value);
                SetLastUpdated();
            }
        }
    }

    public class AnzMerchant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public string LogoUrl { get; set; }
        public string ClientCommissionString { get; set; }
        public string SpecialTerms { get; set; }
        public string CashbackGuidelines { get; set; }
        public int NetworkId { get; set; }
        public bool IsHomePageFeatured { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPopularFlag { get; set; }
        public int PopularRanking { get; set; }
        public int InstoreRanking { get; set; }
        public string InstoreTerms { get; set; }
        public bool MobileEnabled { get; set; }
        public bool IsPremiumDisabled { get; set; }
        public bool IsPaused { get; set; }
        public int PopularMerchantRankingForBrowser { get; set; }
        public List<AnzMerchantCategory> Categories { get; set; }
    }

    public class AnzOffer
    {
        // Anz required fields
        public int Id { get; set; }
        public string Title { get; set; }
        public string Terms { get; set; }
        public string Link { get; set; }
        public string WasRate { get; set; }
        public bool IsFeatured { get; set; }
        public int FeaturedRanking { get; set; }

        // Extra fields for required to calculate Anz required fields
        public long EndDateTime { get; set; }
        public int Ranking { get; set; }


    }

    public class AnzMerchantCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
