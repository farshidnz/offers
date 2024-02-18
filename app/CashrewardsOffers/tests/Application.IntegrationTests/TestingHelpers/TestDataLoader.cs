using Newtonsoft.Json;
using System.IO;

namespace CashrewardsOffers.Application.IntegrationTests.TestingHelpers
{
    public static class TestDataLoader
    {
        public static T Load<T>(string testDataFileName, JsonSerializerSettings settings = null) => JsonConvert.DeserializeObject<T>(Load(testDataFileName), settings);

        public static string Load(string testDataFileName) => File.ReadAllText(testDataFileName.Replace("\\", "/"));
    }
}
