using System;
using System.IO;

namespace PollingClass
{
    /// <summary>Validates and composes paths from configured application directories.</summary>
    public static class ConfiguredFilePaths
    {
        public static string Combine(string directory, string fileName)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("A configured directory is required.", nameof(directory));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("A file name is required.", nameof(fileName));
            if (Path.IsPathRooted(fileName) || fileName.IndexOf(Path.DirectorySeparatorChar) >= 0 || fileName.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
                throw new ArgumentException("A file name must not contain a path.", nameof(fileName));
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException("Configured directory does not exist.");
            return Path.Combine(directory, fileName);
        }
    }
}
