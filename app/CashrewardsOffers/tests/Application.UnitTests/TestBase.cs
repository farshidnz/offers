using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests
{
    public class TestMessage { }

    public class TestBase
    {
        public Expression<Action<ILogger<T>>> CheckLogMesssageMatches<T>(LogLevel logLevel, string logMsg)
        {
            return x => x.Log(logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => string.Equals(logMsg, o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>());
        }
    }
}