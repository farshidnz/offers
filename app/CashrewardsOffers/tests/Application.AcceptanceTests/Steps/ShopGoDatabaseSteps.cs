using Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.AcceptanceTests.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace CashrewardsOffers.Application.AcceptanceTests.Steps
{
    [Binding]
    public class ShopGoDatabaseSteps
    {
        private readonly Fixture? _fixture;

        public ShopGoDatabaseSteps(ScenarioContext scenarioContext)
        {
            _fixture = scenarioContext.GetFixture();
        }

        [Given(@"default merchants exist in the ShopGo database")]
        public void GivenDefaultMerchantsExistInTheShopGoDatabase()
        {
            _fixture.ShouldNotBeNull();
            _fixture.GivenMerchant(merchantId: 1001527, hyphenatedString: "bonds");
            _fixture.GivenMerchant(merchantId: 1001440, hyphenatedString: "the-iconic");
            _fixture.GivenMerchant(merchantId: 1003166, hyphenatedString: "david-jones");
            _fixture.GivenMerchant(merchantId: 1003434, hyphenatedString: "amazon-australia");
            _fixture.GivenMerchant(merchantId: 1003711, hyphenatedString: "groupon");
            _fixture.GivenMerchant(merchantId: 1003824, hyphenatedString: "myer");
        }

        [Given(@"merchants exist in the ShopGo database")]
        public void GivenMerchantsExistInTheShopGoDatabase(Table table)
        {
            _fixture.ShouldNotBeNull();
            var givenMerchants = table.CreateSet<GivenMerchant>().ToList();
            givenMerchants.ForEach(m => _fixture.GivenMerchant(m.MerchantId, m.HyphenatedString));
        }

        private class GivenMerchant
        {
            public int MerchantId { get; set; }
            public string? HyphenatedString { get; set; }
        }

        [Given(@"merchant tiers exist in the ShopGo database")]
        public void GivenMerchantTiersExistInTheShopGoDatabase(Table table)
        {
            _fixture.ShouldNotBeNull();
            var givenMerchants = table.CreateSet<GivenMerchantTier>().ToList();
            givenMerchants.ForEach(m => _fixture.GivenTier(m.MerchantId, terms: m.Terms));
        }

        private class GivenMerchantTier
        {
            public int MerchantId { get; set; }
            public string? Terms { get; set; }
        }

        [Given(@"offers exist in the ShopGo database")]
        public void GivenOffersExistInTheShopGoDatabase(Table table)
        {
            _fixture.ShouldNotBeNull();
            var givenOffers = table.CreateSet<GivenOffer>().ToList();
            givenOffers.ForEach(o => _fixture.GivenOffer(offerId: o.OfferId, merchantId: o.MerchantId, title: o.Title, ranking: o.Ranking, dateEnd: o.EndDateTime, offerString: o.OfferString));
        }

        private class GivenOffer
        {
            public int? OfferId { get; set; }
            public int MerchantId { get; set; }
            public string Title { get; set; } = string.Empty;
            public int Ranking { get; set; }
            public DateTime EndDateTime { get; set; }
            public string? OfferString { get; set; }
        }

        [Given(@"the merchant '([^']*)' is unavailable")]
        public void GivenGivenTheMerchantIsUnavailable(string merchantHyphenatedString)
        {
            _fixture.ShouldNotBeNull();
            _fixture.RemoveMerchant(merchantHyphenatedString);
        }

        [Given(@"the merchant sync job has run")]
        public async Task GivenTheMerchantSyncJobHasRun()
        {
            _fixture.ShouldNotBeNull();
            await _fixture.GivenMerchantSyncJobHasRun();
        }

        [Given(@"the offer sync job has run")]
        public async Task GivenTheOfferSyncJobHasRun()
        {
            _fixture.ShouldNotBeNull();
            await _fixture.GivenOfferSyncJobHasRun();
        }
    }
}
