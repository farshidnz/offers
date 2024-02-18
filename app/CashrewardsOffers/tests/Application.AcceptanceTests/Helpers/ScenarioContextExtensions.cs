using Application.AcceptanceTests.Helpers;
using TechTalk.SpecFlow;

namespace CashrewardsOffers.Application.AcceptanceTests.Helpers
{
    public static class ScenarioContextExtensions
    {
        private static readonly string _fixtureKey = "Fixture";
        private static readonly string _testContextKey = "TestContext";
        private static readonly string _currentOfferChangedKeyBase = "CurrentOfferChanged";

        public static Fixture? GetFixture(this ScenarioContext context) => context[_fixtureKey] as Fixture;

        public static void CreateFixture(this ScenarioContext context)
        {
            context[_fixtureKey] = new Fixture();
        }

        public static void AddTestContext(this ScenarioContext context, string testContext)
        {
            context[_testContextKey] = TestContext(context) + $"[{testContext}]";
        }
        public static string GetTestContext(this ScenarioContext context) => $"test context is {TestContext(context)}";
        private static string TestContext(ScenarioContext context) => (context.TryGetValue(_testContextKey, out var c) ? c : string.Empty) as string ?? string.Empty;

        private static string CurrentKey<T>() => $"{_currentOfferChangedKeyBase}-{typeof(T)}";
        public static T? GetCurrentByType<T>(this ScenarioContext context) where T : class => (context.TryGetValue(CurrentKey<T>(), out var c) ? c : null) as T;
        public static void SetCurrentByType<T>(this ScenarioContext context, T current) => context[CurrentKey<T>()] = current;
    }
}
