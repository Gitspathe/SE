using System;
using System.Collections.Generic;
using System.IO;

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
        /// Reads a file from the Data directory.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>String data from the file.</returns>
        public static string ReadFile(string path)
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
        /// Checks if a file exists in the Data directory.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if the file is found.</returns>
        public static bool FileExists(string path)
        {
            string finalPath = Path.Combine(DataDirectory, path);
            return File.Exists(finalPath);
        }

        static FileIO()
        {
            //folders.Add("Levels");
            BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DataDirectory = Path.Combine(BaseDirectory, "Data");
            if(!Directory.Exists(DataDirectory)) {
                Directory.CreateDirectory(DataDirectory);
            }
            foreach(string folder in folders) {
                if(!Directory.Exists(Path.Combine(DataDirectory, folder))) {
                    Directory.CreateDirectory(Path.Combine(DataDirectory, folder));
                }
            }
        }

    }

}
