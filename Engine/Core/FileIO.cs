using SE.Particles.AreaModules;
using SE.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SE.Core
{
    /// <summary>
    /// Handles serializing and file I/O.
    /// </summary>
    public static class FileIO
    {
        /// <summary>Path to the Data directory.</summary>
        public static string DataDirectory { get; private set; }

        /// <summary>Path to where the game was executed from.</summary>
        public static string BaseDirectory { get; private set; }

        private static List<string> folders = new List<string>();

        static FileIO()
        {
            //folders.Add("Levels");
            BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DataDirectory = Path.Combine(BaseDirectory, "Data" + Path.DirectorySeparatorChar);
            if (!Directory.Exists(DataDirectory)) {
                Directory.CreateDirectory(DataDirectory);
            }
            foreach (string folder in folders) {
                if (!Directory.Exists(Path.Combine(DataDirectory, folder))) {
                    Directory.CreateDirectory(Path.Combine(DataDirectory, folder));
                }
            }
        }

        public static IEnumerable<string> GetAllFiles(string path, string[] extensions = null, string[] excludedExtensions = null)
        {
            QuickList<string> files = new QuickList<string>();
            files.AddRange(Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories));

            if (extensions != null && extensions.Length > 0) {
                for (int i = files.Count; i >= 0; --i) {
                    string file = files.Array[i];
                    if (!extensions.Contains(Path.GetExtension(file))) {
                        files.Remove(file);
                    }
                }
            }

            if (excludedExtensions != null && excludedExtensions.Length > 0) {
                for (int i = files.Count; i >= 0; --i) {
                    string file = files.Array[i];
                    if (excludedExtensions.Contains(Path.GetExtension(file))) {
                        files.Remove(file);
                    }
                }
            }
            return files;
        }

        public static string GetRelativePath(string from, string to)
        {
            Uri fromUri = new Uri(from);
            Uri toUri = new Uri(to);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Saves a file to the Data directory.
        /// </summary>
        /// <param name="data">String data to save.</param>
        /// <param name="path">Which folder in the Data directory to write the file.</param>
        public static void SaveFile(string data, string path)
        {
            string finalPath = Path.Combine(DataDirectory, path);
            using(StreamWriter sw = new StreamWriter(finalPath, false)) {
                sw.Write(data);
            }
        }

        /// <summary>
        /// Saves a file to the Data directory.
        /// </summary>
        /// <param name="data">String data to save.</param>
        /// <param name="path">Which folder in the Data directory to write the file.</param>
        public static void SaveFile(byte[] data, string path)
        {
            string finalPath = Path.Combine(DataDirectory, path);
            File.WriteAllBytes(finalPath, data);
        }

        /// <summary>
        /// Reads a file from the Data directory.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>String data from the file.</returns>
        public static string ReadFileString(string path)
        {
            string finalPath = Path.Combine(DataDirectory, path);
            try {
                using (StreamReader sr = new StreamReader(finalPath)) {
                    return sr.ReadToEnd();
                }
            } catch (FileNotFoundException) {
                return null;
            }
        }

        
        /// <summary>
        /// Reads a file from the Data directory.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>String data from the file.</returns>
        public static byte[] ReadFileBytes(string path)
        {
            string finalPath = Path.Combine(DataDirectory, path);
            try {
                return File.ReadAllBytes(finalPath);
            } catch (FileNotFoundException) {
                return null;
            }
        }

        /// <summary>
        /// Checks if a file exists in the Data directory.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if the file is found.</returns>
        public static bool FileExists(string path)
        {
            string finalPath = Path.Combine(DataDirectory, path);
            return File.Exists(finalPath);
        }

        public static class SEFileExtensions
        {
            /// <summary>Extension for human-readable SE configuration files.</summary>
            public const string _CONFIG = ".seconf"; // SpaghettiEngine CONFig.
            /// <summary>Extension for mostly non-readable SE data files.</summary>
            public const string _DATA = ".sedf";     // SpaghettiEngine Data Format.
        }
    }

}
