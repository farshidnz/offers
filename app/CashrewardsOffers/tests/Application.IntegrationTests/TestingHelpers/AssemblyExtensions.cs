using System.IO;
using System.Reflection;

namespace CashrewardsOffers.Application.IntegrationTests.TestingHelpers
{
    public static class AssemblyExtensions
    {
        public static string Folder(this Assembly assembly) => Path.GetDirectoryName(assembly.Location);
    }
}
