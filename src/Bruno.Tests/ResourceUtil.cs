namespace Bruno.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    internal static class ResourceUtil
    {
        private static Assembly _assembly;

        private static Assembly Assembly => _assembly ??= typeof(ResourceUtil).Assembly;

        internal static IEnumerable<string> FindByPrefix(string prefix)
            => Assembly
               .GetManifestResourceNames()
               .Where(x => x.StartsWith(prefix));

        internal static IEnumerable<string> FindByRegex(string pattern)
            => Assembly
               .GetManifestResourceNames()
               .Where(x => Regex.IsMatch(x, pattern));

        internal static string GetContent(string resourcePath)
        {
            if (Assembly.GetManifestResourceInfo(resourcePath) == null) return null;

            using Stream stream = Assembly.GetManifestResourceStream(resourcePath)
                                  ?? throw new InvalidOperationException($"Resource not found ({resourcePath})");
            return new StreamReader(stream).ReadToEnd();
        }
    }
}