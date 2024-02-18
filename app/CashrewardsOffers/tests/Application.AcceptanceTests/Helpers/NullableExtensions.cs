using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CashrewardsOffers.Application.AcceptanceTests.Helpers
{
    public static class NullableExtensions
    {
        public static void ShouldNotBeNull<T>([NotNull] this T? o, string? because = null)
        {
            o.Should().NotBeNull(because);

            // This is actually unreachable as the above will throw an assertion exception.
            // We can use the above directly when they fix FluentAssertions nullable checks for static code analysis.
            if (o == null) throw new Exception($"{typeof(T)} shoud not be null but was");
        }
    }
}
