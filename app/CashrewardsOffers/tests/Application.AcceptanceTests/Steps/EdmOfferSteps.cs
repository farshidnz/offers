using Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.Offers.Queries.GetEdmOffers.v1;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace CashrewardsOffers.Application.AcceptanceTests.Steps
{
    [Binding]
    public class EdmOfferSteps
    {
        private readonly Fixture? _fixture;
        private readonly ScenarioContext _scenarioContext;

        public EdmOfferSteps(ScenarioContext scenarioContext)
        {
            _fixture = scenarioContext.GetFixture();
            _scenarioContext = scenarioContext;
        }

        [Given(@"Offer with OfferId '([^']*)' is in EDM campaign with CampaignId '([^']*)'")]
        public void GivenOfferWithOfferIdIsInEDMCampaignWithCampaignId(int offerId, int campaignId)
        {
            _fixture.ShouldNotBeNull();
            _fixture.GivenEdmItem(campaignId, "O", offerId);
        }

        [When(@"I send an EDM offer query")]
        public async Task WhenISendAnEDMOfferQuery()
        {
            _fixture.ShouldNotBeNull();
            _scenarioContext["response"] = await _fixture.WhenISendTheQuery<GetEdmOffersQuery, GetEdmOffersResponse>(new GetEdmOffersQuery());
        }

        [When(@"I send an EDM offer query with NewMemberId '([^']*)'")]
        public async Task WhenISendAnEDMOfferQueryWithNewMemberId(string newMemberId)
        {
            _fixture.ShouldNotBeNull();
            _scenarioContext["response"] = await _fixture.WhenISendTheQuery<GetEdmOffersQuery, GetEdmOffersResponse>(new GetEdmOffersQuery
            {
                NewMemberId = newMemberId
            });
        }

        [When(@"I send an EDM offer query with EDM campaignId '([^']*)'")]
        public async Task WhenISendAnEDMOfferQueryWithEDMCampaignId(int campaignId)
        {
            _fixture.ShouldNotBeNull();
            _scenarioContext["response"] = await _fixture.WhenISendTheQuery<GetEdmOffersQuery, GetEdmOffersResponse>(new GetEdmOffersQuery
            {
                EDMCampaignId = campaignId
            });
        }

        [Then(@"I should receive EDM offers in the correct order")]
        public void ThenIShouldReceiveEDMOffersInTheCorrectOrder(Table table)
        {
            var response = _scenarioContext["response"] as GetEdmOffersResponse;
            response.ShouldNotBeNull();

            response.Offers.Should().BeEquivalentTo(table.CreateSet<ExpectedEdmOffer>().ToList(), options => options
                .ExcludingMissingMembers()
                .WithStrictOrdering());
        }

        private class ExpectedEdmOffer
        {
            public string? Title { get; set; }
            public string? Terms { get; set; }
        }
    }
}
