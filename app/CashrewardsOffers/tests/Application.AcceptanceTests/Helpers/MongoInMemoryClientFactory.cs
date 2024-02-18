using CashrewardsOffers.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Mongo2Go;
using MongoDB.Driver;
using System;

namespace Application.AcceptanceTests.Helpers
{
    public class MongoInMemoryClientFactory : IMongoClientFactory
    {
        private MongoDbRunner _runner;

        public MongoInMemoryClientFactory()
        {
            _runner = MongoDbRunner.Start(logger: new NullLogger());
        }

        public IMongoClient CreateClient(string? username = null, string? password = null)
        {
            return new MongoClient(_runner.ConnectionString);
        }
    }

    public class NullLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => new EmptyDisposable();

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }

    public class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
