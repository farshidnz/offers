using Amazon;
using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;

namespace CashrewardsOffers.Infrastructure.AWS
{
    public interface IAwsSecretsManagerClientFactory
    {
        IAmazonSecretsManager CreateClient();
    }

    public class AwsSecretsManagerClientFactory : IAwsSecretsManagerClientFactory
    {
        private readonly string _region;

        public AwsSecretsManagerClientFactory(IConfiguration configuration)
        {
            _region = configuration["AWS:Region"];
        }

        public IAmazonSecretsManager CreateClient() => new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region));
    }
}
