using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Configuration;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public interface IAmazonS3ClientFactory
    {
        IAmazonS3 CreateClient();
    }

    public class AmazonS3ClientFactory : IAmazonS3ClientFactory
    {
        private readonly RegionEndpoint _regionEndpoint;

        public AmazonS3ClientFactory(IConfiguration configuration)
        {
            _regionEndpoint = RegionEndpoint.GetBySystemName(configuration["AWS:region"]);
        }

        public IAmazonS3 CreateClient()
        {
            return new AmazonS3Client(_regionEndpoint);
        }
    }
}
