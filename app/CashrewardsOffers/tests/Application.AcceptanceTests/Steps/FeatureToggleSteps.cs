using Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.Feature.Queries.v1;
using CashrewardsOffers.Domain.Entities;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace CashrewardsOffers.Application.AcceptanceTests.Steps
{
    [Binding]
    public class FeatureToggleSteps
    {
        private readonly Fixture? _fixture;

        public FeatureToggleSteps(ScenarioContext scenarioContext)
        {
            _fixture = scenarioContext.GetFixture();
        }

        [Given(@"Unleash feature toggles")]
        public void GivenUnleashFeatureToggles(Table table)
        {
            _fixture.ShouldNotBeNull();
            var givenToggles = table.CreateSet<GivenFeatureToggle>().ToList();
            givenToggles.ForEach(t => _fixture.GivenUnleashFeatureToggle(toggleName: t.FeatureToggleName));
        }

        private class GivenFeatureToggle
        {
            public string FeatureToggleName { get; set; } = string.Empty;
        }

        [Given(@"person with CognitoId '([^']*)' is enroled in experiment '([^']*)'")]
        public async Task GivenPersonIsEnroledInExperiment(string cognitoId, string experimentFeatureToggle)
        {
            _fixture.ShouldNotBeNull();

            experimentFeatureToggle.Should().BeOneOf(FeatureToggle.Exp1, FeatureToggle.Exp2, FeatureToggle.Exp3);

            await _fixture.WhenISendTheQuery<EnrolFeatureForMembersQuery, EnrolFeatureForMembersResponse>(
                new EnrolFeatureForMembersQuery
                {
                    IdType = MemberIdType.CognitoId.ToString(),
                    Feature = experimentFeatureToggle,
                    Ids = new[] { cognitoId }
                });
        }
    }
}
