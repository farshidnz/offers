using System.IO;
using System.Reflection;

namespace CashrewardsOffers.Application.UnitTests.Common.TestHelpers
{
    public static class AssemblyExtensions
    {
        public static string Folder(this Assembly assembly) => Path.GetDirectoryName(assembly.Location);
    }
}
