using Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.ANZ.Queries.GetAnzItems.v1;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace CashrewardsOffers.Application.AcceptanceTests.Steps
{
    [Binding]
    public class AnzSteps
    {
        private readonly Fixture? _fixture;
        private readonly ScenarioContext _scenarioContext;

        public AnzSteps(ScenarioContext scenarioContext)
        {
            _fixture = scenarioContext.GetFixture();
            _scenarioContext = scenarioContext;
        }

        [Given(@"API for ANZ is available")]
        public void GivenAPIForANZIsAvailable()
        {
            _fixture.ShouldNotBeNull();
        }

        [When(@"auth token is valid")]
        public void WhenAuthTokenIsValid()
        {
            _fixture.ShouldNotBeNull();
        }

        [When(@"I send an ANZ query")]
        public async Task WhenISendAnANZQuery()
        {
            _fixture.ShouldNotBeNull();
            _scenarioContext.SetCurrentByType(
                await _fixture.WhenISendTheQuery<GetAnzItemsQuery, GetAnzItemsResponse>(new GetAnzItemsQuery
                {
                    OffersPerPage = 9999,
                    PageNumber = 1,
                    UpdatedAfter = null
                }));
        }

        [When(@"I send an ANZ query with update after '([^']*)'")]
        public async Task WhenISendAnANZQueryWithUpdateAfter(string updateAfterString)
        {
            _fixture.ShouldNotBeNull();

            var updateAfter = new DateTimeOffset(DateTime.Parse(updateAfterString), TimeSpan.Zero);

            _scenarioContext.SetCurrentByType(
                await _fixture.WhenISendTheQuery<GetAnzItemsQuery, GetAnzItemsResponse>(new GetAnzItemsQuery
                {
                    OffersPerPage = 9999,
                    PageNumber = 1,
                    UpdatedAfter = updateAfter.UtcTicks
                }));
        }

        [When(@"I send an ANZ query with OffersPerPage = '([^']*)', PageNumber = '([^']*)'")]
        public async Task WhenISendAnANZQuery_OffersPerPageAndPageNumber(int OffersPerPage, int PageNumber)
        {
            _fixture.ShouldNotBeNull();
            _scenarioContext.SetCurrentByType(
                await _fixture.WhenISendTheQuery<GetAnzItemsQuery, GetAnzItemsResponse>(new GetAnzItemsQuery
                {
                    OffersPerPage = OffersPerPage,
                    PageNumber = PageNumber,
                    UpdatedAfter = null
                }));
        }

        [Then(@"I should receive ANZ items from '([^']*)' to '([^']*)'")]
        public void ThenIShouldReceiveANZItemsFromTo(string fromRowString, string toRowString, Table table)
        {
            var response = _scenarioContext.GetCurrentByType<GetAnzItemsResponse>();
            response.ShouldNotBeNull();
            int.TryParse(fromRowString, out var fromRow);
            int.TryParse(toRowString, out var toRow);
            var responseItems = response.Items.Skip(fromRow - 1).Take(toRow - fromRow + 1);

            var expected = table.CreateDeepSet<AnzItemInfo>().ToList();
            responseItems.Should().BeEquivalentTo(expected, options => options
                .Using(new TableMemberSelectionRule(table))
            );
        }

        [Then(@"I should receive ANZ items")]
        public void ThenIShouldReceiveANZItems(Table table)
        {
            var response = _scenarioContext.GetCurrentByType<GetAnzItemsResponse>();
            response.ShouldNotBeNull();

            var expected = table.CreateDeepSet<AnzItemInfo>().ToList();
            response.Items.Should().BeEquivalentTo(expected, options => options
                .Using(new TableMemberSelectionRule(table))
            );
        }

        [Then(@"I should recieve the following response data from the anz api TotalOffersCount = '([^']*)', PageOffersCount = '([^']*)', TotalPageCount = '([^']*)', PageNumber = '([^']*)'")]
        public void ThenIShouldReceiveTheFollowingResponseDataFromTheANZApi(string TotalOffersCount, string PageOffersCount, string TotalPageCount, string PageNumber)
        {
            var response = _scenarioContext.GetCurrentByType<GetAnzItemsResponse>();
            response.ShouldNotBeNull();
            int.TryParse(TotalOffersCount, out int expectedTotalOffersCount).Should().BeTrue();
            int.TryParse(PageOffersCount, out int expectedPageOffersCount).Should().BeTrue();
            int.TryParse(TotalPageCount, out int expectedTotalPageCount).Should().BeTrue();
            int.TryParse(PageNumber, out int expectedPageNumber).Should().BeTrue();

            response.TotalOffersCount.Should().Be(expectedTotalOffersCount);
            response.PageOffersCount.Should().Be(expectedPageOffersCount);
            response.TotalPageCount.Should().Be(expectedTotalPageCount);
            response.PageNumber.Should().Be(expectedPageNumber);
        }

        [Then(@"the API returns the '([^']*)' for Merchant and Offer in the response as '([^']*)'")]
        public void ThenTheAPIReturnsTheForMerchantAndOfferInTheResponseAs(string fieldName, string mandatoryOrOptional)
        {
            var response = _scenarioContext.GetCurrentByType<GetAnzItemsResponse>();
            response.ShouldNotBeNull();

            var fieldPath = fieldName.Split(".").Select(s => s.Trim());
            var currentPathType = response.GetType();
            foreach (var fieldPart in fieldPath)
            {
                var fieldProperty = currentPathType.GetProperties().FirstOrDefault(p => p.Name == fieldPart.Replace("[", "").Replace("]", ""));
                fieldProperty.ShouldNotBeNull($"field {fieldPart} not found");
                currentPathType = fieldProperty.PropertyType.IsGenericType
                    ? fieldProperty.PropertyType.GenericTypeArguments[0]
                    : fieldProperty.PropertyType;
            }
        }

        [Then(@"the API '([^']*)' the Merchant in the response with the specified fields")]
        [Then(@"the API '([^']*)' the offer in the response with the specified fields")]
        [Then(@"the API '([^']*)' the Merchant and Offer in the response with the specified fields")]
        public void ThenTheAPITheOfferInTheResponseWithTheSpecifiedFields(string includeOrExclude)
        {
            var response = _scenarioContext.GetCurrentByType<GetAnzItemsResponse>();
            response.ShouldNotBeNull();

            var testContext = _scenarioContext.GetTestContext();
            includeOrExclude.Should().BeOneOf("include", "exclude", testContext);

            var item = response.Items.Count == 0 ? "exclude" : "include";
            item.Should().Be(includeOrExclude, testContext);
        }
    }
}
