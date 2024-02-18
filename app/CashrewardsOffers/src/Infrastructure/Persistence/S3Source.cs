using Amazon.S3.Model;
using CashrewardsOffers.Application.Common.Interfaces;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class S3Source : IS3Source
    {
        protected string BucketName { get; set; }

        private string _storagePath;
        private readonly IAmazonS3ClientFactory _amazonS3ClientFactory;

        protected string StoragePath
        {
            get => _storagePath;
            set => _storagePath = value.EndsWith('/') ? value : $"{value}/";
        }

        public S3Source(IAmazonS3ClientFactory amazonS3ClientFactory)
        {
            _amazonS3ClientFactory = amazonS3ClientFactory;
        }

        public async Task<string> DownloadTextFileAsync(string fileKey)
        {
            using var client = _amazonS3ClientFactory.CreateClient();

            GetObjectRequest getRequest = new GetObjectRequest()
            {
                BucketName = BucketName,
                Key = fileKey
            };
                
            var response = await client.GetObjectAsync(getRequest);
            using StreamReader r = new StreamReader(response.ResponseStream);

            return await r.ReadToEndAsync();
        }

        public async Task<string> DownloadLatestTextFileAsync(string prefix)
        {
            var latestFileKey = await GetLatestModifiedFileKey(prefix);
            return await DownloadTextFileAsync(latestFileKey);
        }

        public async Task<IEnumerable<string>> GetFileKeys(string prefix)
        {
            using var client = _amazonS3ClientFactory.CreateClient();

            Log.Information("Ranked merchant sync is looking up s3 files in bucket {BucketName} path {StoragePath}{prefix}", BucketName, StoragePath, prefix);
            ListObjectsV2Request request = new ListObjectsV2Request()
            {
                BucketName = BucketName,
                Prefix = $"{StoragePath}{prefix}"
            };

            var result = await client.ListObjectsV2Async(request);
            return result.S3Objects.Select(o => o.Key);
        }

        public async Task<string> GetLatestModifiedFileKey(string prefix)
        {
            using var client = _amazonS3ClientFactory.CreateClient();

            ListObjectsV2Request request = new ListObjectsV2Request()
            {
                BucketName = BucketName,
                Prefix = $"{StoragePath}{prefix}"
            };

            var result = await client.ListObjectsV2Async(request);
            return result.S3Objects.OrderByDescending(obj => obj.LastModified).FirstOrDefault()?.Key;
        }
    }
}
