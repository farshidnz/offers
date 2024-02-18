using Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.AcceptanceTests.Helpers;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace CashrewardsOffers.Application.AcceptanceTests.Steps
{
    [Binding]
    public class PersonSteps
    {
        private readonly Fixture? _fixture;

        public PersonSteps(ScenarioContext scenarioContext)
        {
            _fixture = scenarioContext.GetFixture();
        }

        [Given(@"person with CognitoId '([^']*)' and NewMemberId '([^']*)'")]
        public void GivenPersonWithCognitoIdAndNewMemberId(string cognitoId, string newMemberId)
        {
            _fixture.ShouldNotBeNull();
            _fixture.GivenPerson(cognitoId: cognitoId, newMemberId: newMemberId);
        }

        [Given(@"person has selected favourites")]
        public void GivenPersonHasSelectedFavourites(Table table)
        {
            _fixture.ShouldNotBeNull();
            var personSelectedFavourites = table.CreateSet<GivenPersonSelectedFavourite>().ToList();
            personSelectedFavourites.ForEach(f => _fixture.GivenUserFavourite(f.CognitoId, f.MerchantId, f.HyphenatedString, f.SelectionOrder));
        }

        private class GivenPersonSelectedFavourite
        {
            public string CognitoId { get; set; } = string.Empty;
            public int MerchantId { get; set; }
            public string HyphenatedString { get; set; } = string.Empty;
            public int SelectionOrder { get; set; }
        }
    }
}
