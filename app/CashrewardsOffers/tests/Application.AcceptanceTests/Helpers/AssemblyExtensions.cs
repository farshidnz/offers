using System.IO;
using System.Reflection;

namespace Application.AcceptanceTests.Helpers
{
    public static class AssemblyExtensions
    {
        public static string Folder(this Assembly assembly) => Path.GetDirectoryName(assembly.Location) ?? string.Empty;
    }
}
