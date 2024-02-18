using Amazon.S3.Model;
using CashrewardsOffers.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class S3MerchantHistoryArchive : IS3MerchantHistoryArchive
    {
        private readonly IAmazonS3ClientFactory _amazonS3ClientFactory;
        private readonly string _bucketName;
        private readonly string _storagePath;

        public S3MerchantHistoryArchive(
            IConfiguration configuration,
            IAmazonS3ClientFactory amazonS3ClientFactory)
        {
            _amazonS3ClientFactory = amazonS3ClientFactory;
            _bucketName = configuration["MerchantHistoryArchiveBucket"];
            _storagePath = configuration["MerchantHistoryArchivePath"];
        }

        public Task Put(int year, int month, int day, string extension, string data) =>
            Put(year, month, day, extension, ToStream(data));

        public async Task Put(int year, int month, int day, string extension, Stream stream)
        {
            using var client = _amazonS3ClientFactory.CreateClient();

            await client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = $"{_storagePath}/history/{year:0000}/{month:00}/{year:0000}-{month:00}-{day:00}-merchant-changes.{extension}",
                InputStream = stream
            });
        }

        private static Stream ToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
