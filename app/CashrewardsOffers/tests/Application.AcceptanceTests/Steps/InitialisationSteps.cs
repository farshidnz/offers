using CashrewardsOffers.Application.AcceptanceTests.Helpers;
using TechTalk.SpecFlow;

namespace CashrewardsOffers.Application.AcceptanceTests.Steps
{
    [Binding]
    public class InitialisationSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public InitialisationSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void SetpUpScenario()
        {
            _scenarioContext.CreateFixture();
        }
    }
}
