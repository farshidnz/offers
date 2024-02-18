using Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.Merchants.Queries.GetEdmMerchants.v1;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace CashrewardsOffers.Application.AcceptanceTests.Steps
{
    [Binding]
    public class EdmMerchantSteps
    {
        private readonly Fixture? _fixture;
        private readonly ScenarioContext _scenarioContext;

        public EdmMerchantSteps(ScenarioContext scenarioContext)
        {
            _fixture = scenarioContext.GetFixture();
            _scenarioContext = scenarioContext;
        }

        [Given(@"Merchant with MerchantId '([^']*)' is in EDM campaign with CampaignId '([^']*)'")]
        public void GivenMerchantWithMerchantIdIsInEDMCampaignWithCampaignId(int merchantId, int campaignId)
        {
            _fixture.ShouldNotBeNull();
            _fixture.GivenEdmItem(campaignId, "M", merchantId);
        }

        [When(@"I send an EDM merchant query")]
        public async Task WhenISendAnEDMMerchantQuery()
        {
            _fixture.ShouldNotBeNull();
            _scenarioContext["response"] = await _fixture.WhenISendTheQuery<GetEdmMerchantsQuery, GetEdmMerchantsResponse>(new GetEdmMerchantsQuery());
        }

        [When(@"I send an EDM merchant query with NewMemberId '([^']*)'")]
        public async Task WhenISendAnEDMMerchantQueryWithNewMemberId(string newMemberId)
        {
            _fixture.ShouldNotBeNull();
            _scenarioContext["response"] = await _fixture.WhenISendTheQuery<GetEdmMerchantsQuery, GetEdmMerchantsResponse>(new GetEdmMerchantsQuery
            {
                NewMemberId = newMemberId
            });
        }

        [Then(@"I should receive EDM merchants in the correct order")]
        public void ThenIShouldReceiveMerchantsInTheCorrectOrder(Table table)
        {
            var response = _scenarioContext["response"] as GetEdmMerchantsResponse;
            response.ShouldNotBeNull();

            var expectedMerchants = table.CreateSet<ExpectedEdmMerchant>().ToList();
            response.Merchants.Should().BeEquivalentTo(expectedMerchants, options => options
                .ExcludingMissingMembers()
                .WithStrictOrdering()
                .WithMapping("MerchantHyphenatedString", "HyphenatedString"));
        }

        private class ExpectedEdmMerchant
        {
            public int Order { get; set; }
            public string? MerchantHyphenatedString { get; set; }
        }
    }
}
