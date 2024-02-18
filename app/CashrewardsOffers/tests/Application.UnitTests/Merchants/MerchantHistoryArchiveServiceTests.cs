using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Merchants.Services;
using CashrewardsOffers.Domain.Entities;
using FluentAssertions;
using Mapster;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Merchants
{
    public class MerchantHistoryArchiveServiceTests
    {
        private class TestState
        {
            public MerchantHistoryArchiveService MerchantHistoryArchiveService { get; }

            public Mock<IMerchantHistoryPersistenceContext> MerchantHistoryPersistenceContext { get; } = new();
            public Mock<IS3MerchantHistoryArchive> S3MerchantHistoryArchive { get; } = new();
            public Mock<IDomainTime> DomainTime { get; } = new();

            public TestState()
            {
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));
                MerchantHistoryPersistenceContext
                    .Setup(c => c.GetByDateRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                    .ReturnsAsync(MerchantHistorySample);

                MerchantHistoryArchiveService = new MerchantHistoryArchiveService(MerchantHistoryPersistenceContext.Object, S3MerchantHistoryArchive.Object, new MerchantHistoryExcelService(), DomainTime.Object);
            }

            public static List<MerchantHistory> MerchantHistorySample =>
                new()
                {
                    new()
                    {
                        ChangeTime = new DateTimeOffset(2022, 8, 18, 13, 30, 0, TimeSpan.FromHours(10)),
                        MerchantId = 1001234,
                        Client = Domain.Enums.Client.Cashrewards,
                        Name = "Booking.com",
                        HyphenatedString = "booking-com",
                        ClientCommissionString = "10%"
                    }
                };
        }

        [Test]
        public async Task TryArchiveAsync_ShouldGetHistoryWithSydneysYesterdayDateRangeInUtc()
        {
            var state = new TestState();
            state.DomainTime.Setup(t => t.UtcNow).Returns(new DateTimeOffset(2022, 1, 2, 20, 0, 0, TimeSpan.Zero));
            DateTimeOffset? calledStartRange = null;
            DateTimeOffset? calledEndRange = null;
            state.MerchantHistoryPersistenceContext
                .Setup(c => c.GetByDateRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(TestState.MerchantHistorySample)
                .Callback((DateTimeOffset start, DateTimeOffset end) => { calledStartRange = start; calledEndRange = end; });

            await state.MerchantHistoryArchiveService.TryArchiveAsync();

            calledStartRange.Should().Be(new DateTimeOffset(2022, 1, 1, 13, 0, 0, TimeSpan.Zero));
            calledEndRange.Should().Be(new DateTimeOffset(2022, 1, 2, 13, 0, 0, TimeSpan.Zero));
        }

        [Test]
        public async Task TryArchiveAsync_ShouldPutJsonDataIntoS3()
        {
            var state = new TestState();
            state.DomainTime.Setup(t => t.UtcNow).Returns(new DateTimeOffset(2022, 1, 2, 20, 0, 0, TimeSpan.Zero));
            string calledData = null;
            state.S3MerchantHistoryArchive
                .Setup(s => s.Put(It.Is<int>(y => y == 2022), It.Is<int>(m => m == 1), It.Is<int>(d => d == 2), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((int y, int m, int d, string e, string t) => calledData = t);
            await state.MerchantHistoryArchiveService.TryArchiveAsync();

            state.S3MerchantHistoryArchive.Verify(s => s.Put(It.Is<int>(y => y == 2022), It.Is<int>(m => m == 1), It.Is<int>(d => d == 2), It.IsAny<string>(), It.IsAny<string>()));
            calledData.Should().Be(@"[{""ChangeInSydneyTime"":""2022-08-18T13:30:00+10:00"",""MerchantId"":1001234,""Client"":1000000,""Name"":""Booking.com"",""HyphenatedString"":""booking-com"",""ClientCommissionString"":""10%""}]");
        }

        [Test]
        public async Task TryArchiveAsync_ShouldPutExcelReportIntoS3()
        {
            var state = new TestState();
            state.DomainTime.Setup(t => t.UtcNow).Returns(new DateTimeOffset(2022, 1, 2, 20, 0, 0, TimeSpan.Zero));
            byte[] calledData = null;
            state.S3MerchantHistoryArchive
                .Setup(s => s.Put(It.Is<int>(y => y == 2022), It.Is<int>(m => m == 1), It.Is<int>(d => d == 2), It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback((int y, int m, int d, string e, Stream s) => { var t = new byte[s.Length]; s.Read(t); calledData = t; });
            await state.MerchantHistoryArchiveService.TryArchiveAsync();

            state.S3MerchantHistoryArchive.Verify(s => s.Put(It.Is<int>(y => y == 2022), It.Is<int>(m => m == 1), It.Is<int>(d => d == 2), It.IsAny<string>(), It.IsAny<Stream>()));
            calledData.Length.Should().BeGreaterThan(2000);
            //File.WriteAllBytes("C:\\Sandbox\\test.xlsx", calledData);
        }

        [Test]
        public async Task TryArchiveAsync_ShouldDeleteHistoryWithSydneysYesterdayDateRangeInUtc()
        {
            var state = new TestState();
            state.DomainTime.Setup(t => t.UtcNow).Returns(new DateTimeOffset(2022, 1, 2, 20, 0, 0, TimeSpan.Zero));
            DateTimeOffset? calledStartRange = null;
            DateTimeOffset? calledEndRange = null;
            state.MerchantHistoryPersistenceContext
                .Setup(c => c.DeleteByDateRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(0)
                .Callback((DateTimeOffset start, DateTimeOffset end) => { calledStartRange = start; calledEndRange = end; });

            await state.MerchantHistoryArchiveService.TryArchiveAsync();

            calledStartRange.Should().Be(new DateTimeOffset(2022, 1, 1, 13, 0, 0, TimeSpan.Zero));
            calledEndRange.Should().Be(new DateTimeOffset(2022, 1, 2, 13, 0, 0, TimeSpan.Zero));
        }
    }
}
