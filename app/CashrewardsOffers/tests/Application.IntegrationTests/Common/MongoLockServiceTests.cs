using CashrewardsOffers.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.IntegrationTests.Common
{
    using static Testing;

    public class MongoLockServiceTests : TestBase
    {
        [Test]
        public async Task LockAsync_ShouldReturnLockId()
        {
            using var scope = GivenScope();
            var lockService = scope.ServiceProvider.GetService<IMongoLockService>();

            var lockId = await lockService.LockAsync("TestLock");
            lockId.Should().NotBeNull();
        }

        [Test]
        public async Task RelockAsync_ShouldUpdateLockTime()
        {
            using var scope = GivenScope();
            var lockService = scope.ServiceProvider.GetService<IMongoLockService>();

            var lockId = await lockService.LockAsync("TestLock");
            lockId.Should().NotBeNull();
            await lockService.RelockAsync("TestLock", lockId.Value);
        }

        [Test]
        public async Task LockAsync_ShouldReturnNull_GivenLockIsAlreadyAquiredByAnother()
        {
            using var scope = GivenScope();
            var lockService = scope.ServiceProvider.GetService<IMongoLockService>();

            var lockId1 = await lockService.LockAsync("TestLock");
            lockId1.Should().NotBeNull();
            var lockId2 = await lockService.LockAsync("TestLock");
            lockId2.Should().BeNull();
        }

        [Test]
        public async Task ReleaseLockAsync_ShouldThrowException_GivenInvalidLockId()
        {
            using var scope = GivenScope();
            var lockService = scope.ServiceProvider.GetService<IMongoLockService>();

            var lockId1 = await lockService.LockAsync("TestLock");
            lockId1.Should().NotBeNull();
            Func<Task> act = () => lockService.ReleaseLockAsync("TestLock", Guid.NewGuid());
            await act.Should().ThrowAsync<LockException>().WithMessage("unable to release lock on item 'TestLock'");
        }

        [Test]
        public async Task RelockAsync_ShouldThrowException_GivenInvalidLockId()
        {
            using var scope = GivenScope();
            var lockService = scope.ServiceProvider.GetService<IMongoLockService>();

            var lockId1 = await lockService.LockAsync("TestLock");
            lockId1.Should().NotBeNull();
            Func<Task> act = () => lockService.RelockAsync("TestLock", Guid.NewGuid());
            await act.Should().ThrowAsync<LockException>().WithMessage("unable to relock item 'TestLock'");
        }

        [Test]
        public async Task ReleaseLockAsync_ShouldReleaseLock()
        {
            using var scope = GivenScope();
            var lockService = scope.ServiceProvider.GetService<IMongoLockService>();

            var lockId1 = await lockService.LockAsync("TestLock");
            lockId1.Should().NotBeNull();
            await lockService.ReleaseLockAsync("TestLock", lockId1.Value);
            var lockId2 = await lockService.LockAsync("TestLock");
            lockId2.Should().NotBeNull();
        }

        [Test]
        public async Task LockAsync_ShouldReturnLockId_GivenDifferentLockNames()
        {
            using var scope = GivenScope();
            var lockService = scope.ServiceProvider.GetService<IMongoLockService>();

            var lockId = await lockService.LockAsync("TestLock");
            lockId.Should().NotBeNull();
            var lockId2 = await lockService.LockAsync("TestLock2");
            lockId2.Should().NotBeNull();
        }

        [Test]
        public async Task ReleaseLockAsync_ShouldReleaseLockByName_GivenDifferentLockNames()
        {
            using var scope = GivenScope();
            var lockService = scope.ServiceProvider.GetService<IMongoLockService>();

            var lockId1 = await lockService.LockAsync("TestLock");
            lockId1.Should().NotBeNull();
            var lockId2 = await lockService.LockAsync("TestLock2");
            lockId2.Should().NotBeNull();

            await lockService.ReleaseLockAsync("TestLock", lockId1.Value);
            var lockId3 = await lockService.LockAsync("TestLock");
            lockId3.Should().NotBeNull();
            var lockId4 = await lockService.LockAsync("TestLock2");
            lockId4.Should().BeNull();
        }
    }
}
