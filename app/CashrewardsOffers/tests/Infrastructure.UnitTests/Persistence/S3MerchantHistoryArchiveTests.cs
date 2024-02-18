using Amazon.S3;
using Amazon.S3.Model;
using CashrewardsOffers.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Persistence
{
    public class S3MerchantHistoryArchiveTests
    {
        private class TestState
        {
            public S3MerchantHistoryArchive S3MerchantHistoryArchive { get; }

            public PutObjectRequest PutObjectRequest { get; private set; }

            public TestState()
            {
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["MerchantHistoryArchiveBucket"] = "bucket",
                        ["MerchantHistoryArchivePath"] = "path"
                    })
                    .Build();

                var client = new Mock<IAmazonS3>();
                client.Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>())).Callback((PutObjectRequest r, CancellationToken c) => PutObjectRequest = r);
                var amazonS3ClientFactory = new Mock<IAmazonS3ClientFactory>();
                amazonS3ClientFactory.Setup(a => a.CreateClient()).Returns(client.Object);

                S3MerchantHistoryArchive = new S3MerchantHistoryArchive(configuration, amazonS3ClientFactory.Object);
            }
        }

        [Test]
        public async Task Put_ShouldCallS3Client()
        {
            var state = new TestState();

            await state.S3MerchantHistoryArchive.Put(2022, 1, 2, "txt", "testdata");

            state.PutObjectRequest.BucketName.Should().Be("bucket");
            state.PutObjectRequest.Key.Should().Be("path/history/2022/01/2022-01-02-merchant-changes.txt");
            var buffer = new byte[8];
            state.PutObjectRequest.InputStream.Read(buffer);
            Encoding.UTF8.GetString(buffer.ToArray()).Should().Be("testdata");
        }
    }
}
