using CashrewardsOffers.Application.ANZ.Queries.GetAnzItems.v1;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Events;
using FluentAssertions;
using Mapster;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CashrewardsOffers.Application.UnitTests.ANZ
{
    public class AnzItemQueryTests
    {
        [Test]
        public void CalcTotalPageCount_ShouldNeverBeNegative_GivenAnyParametersPassed()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            int[] counts = { 0, 1, 2, 1000000, -1, -2, -1000000 };
            int?[] pageSizes = { null, 0, 1, 2, 1000000, -1, -2, -1000000 };

            foreach(var count in counts)
            {
                GetAnzItemsQueryConsumer.CalcTotalPageCount(count).Should().BeGreaterThanOrEqualTo(0);
                foreach (var pageSize in pageSizes)
                {
                    GetAnzItemsQueryConsumer.CalcTotalPageCount(count, pageSize).Should().BeGreaterThanOrEqualTo(0);
                }
            }
        }

        [Test]
        public void CalcTotalPageCount_ShouldBePositiveForPositiveCountValues_GivenAnyParametersPassed()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            int[] counts = { 1, 2, 1000000 };
            int?[] pageSizes = { null, 0, 1, 2, 1000000, -1, -2, -1000000 };

            foreach (var count in counts)
            {
                GetAnzItemsQueryConsumer.CalcTotalPageCount(count).Should().BeGreaterThan(0);
                foreach (var pageSize in pageSizes)
                {
                    GetAnzItemsQueryConsumer.CalcTotalPageCount(count, pageSize).Should().BeGreaterThan(0);
                }
            }
        }

        [Test]
        public void CalcTotalPageCount_MoreThanOneWhenPageSizePositiveIntAndLessThanCount_GivenAnyParametersPassed()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            int[] counts = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 13, 17, 49, 101, 111, 9999, 1000000 };
            int?[] pageSizes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 17, 23, 1000000 };

            foreach (var count in counts)
            {
                count.Should().BeGreaterThan(0);
                GetAnzItemsQueryConsumer.CalcTotalPageCount(count).Should().BeGreaterThan(0);
                foreach (var pageSize in pageSizes)
                {
                    if(pageSize < count)
                    {
                        pageSize.Should().BeLessThan(count);
                        GetAnzItemsQueryConsumer.CalcTotalPageCount(count, pageSize).Should().BeGreaterThan(1);
                    }
                        
                }
            }
        }

        [Test]
        public void CalcTotalPageCount_ShouldNeverBeGreaterThanPositiveCount_GivenAnyParametersPassed()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            int[] counts = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 13, 17, 49, 101, 111, 9999, 1000000 };
            int?[] pageSizes = { null, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 17, 23, 1000000, -1, -2, -3, -4, -5, -6, -7, 11, -13, -17, -23, -100, -1000000 };

            foreach (var count in counts)
            {
                count.Should().BeGreaterThan(0);
                GetAnzItemsQueryConsumer.CalcTotalPageCount(count).Should().BeLessThanOrEqualTo(count);
                foreach (var pageSize in pageSizes)
                {
                    if (pageSize < count)
                        GetAnzItemsQueryConsumer.CalcTotalPageCount(count, pageSize).Should().BeLessThanOrEqualTo(count);
                }
            }
        }

        [Test]
        public void CalcTotalPageCount_SomePreCalcResults_GivenAnyParametersPassed()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            GetAnzItemsQueryConsumer.CalcTotalPageCount(4, 2).Should().Be(2);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(5, 2).Should().Be(3);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(6, 2).Should().Be(3);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(7, 2).Should().Be(4);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(101, 100).Should().Be(2);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(201, 100).Should().Be(3);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(299, 100).Should().Be(3);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(300, 100).Should().Be(3);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(350, 100).Should().Be(4);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(350, 0).Should().Be(1);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(350, null).Should().Be(1);
            // Should this be 0 or 1? We return 1 empty page for 0 count.
            int PageCountThatShouldReturnFor0 = 0;
            GetAnzItemsQueryConsumer.CalcTotalPageCount(0, null).Should().Be(PageCountThatShouldReturnFor0);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(0, 0).Should().Be(PageCountThatShouldReturnFor0);
            GetAnzItemsQueryConsumer.CalcTotalPageCount(0).Should().Be(PageCountThatShouldReturnFor0);


        }
    }
}
