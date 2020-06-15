using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement.FileProcessors;
using SE.Core;
using SE.Utility;
using IOFile = System.IO.File;

namespace SE.AssetManagement
{
    internal static class FileMarshal
    {
        private const string _DATA_EXTENSION = ".sdata";
        private const string _MG_EXTENSION = ".xnb";
        private static string[] excludedExtensions = { _MG_EXTENSION, ".semap" };

        private static Dictionary<string, File> files = new Dictionary<string, File>();
        private static Dictionary<Type, FileProcessor> fileProcessors = new Dictionary<Type, FileProcessor>();

        public static void AddProcessor(FileProcessor processor)
        {
            if(fileProcessors.ContainsKey(processor.Type))
                return;

            //processor.ContentBaseDirectory = rootDirectory;
            fileProcessors.Add(processor.Type, processor);
        }

        public static bool TryGet<T>(GraphicsDevice gfx, string file, out T obj)
        {
            // Try to return from a FileProcessor.
            if (fileProcessors.TryGetValue(typeof(T), out FileProcessor processor)) {
                if (processor.GetFile(file, out object retrieved)) {
                    obj = (T) retrieved;
                    return true;
                }

                // Load if unloaded.

                if (files.TryGetValue(file, out File f)) {
                    if (processor.LoadFileInternal(gfx, f, out object loaded)) {
                        obj = (T) loaded;
                        return true;
                    }
                }
            }
            obj = default;
            return false;
        }

        public static void Setup()
        {
            AddProcessor(new Texture2DFileProcessor());

            QuickList<string> needsProcessing = new QuickList<string>();
            QuickList<string> processed = new QuickList<string>();

            // Locate files which need processing, or are already processed.
            foreach (string dir in Directory.GetDirectories(FileIO.DataDirectory)) {
                foreach (string file in FileIO.GetAllFiles(dir, excludedExtensions: excludedExtensions)) {
                    string extension = Path.GetExtension(file);
                    if(string.IsNullOrEmpty(extension))
                        continue;

                    if (extension != _DATA_EXTENSION) {
                        needsProcessing.Add(file);
                    } else {
                        processed.Add(file);
                    }
                }
            }

            // Process un-processed files (e.g .png, .jpg, .wav, etc).
            foreach(string file in needsProcessing) {
                // Read the file, and store in byte array.
                string extension = Path.GetExtension(file);
                string pathNoExtension = Path.ChangeExtension(file, null);
                string newFile = pathNoExtension + _DATA_EXTENSION;
                byte[] bytes = IOFile.ReadAllBytes(file);

                // Create write stream.
                using FileStream stream = new FileStream(newFile, FileMode.OpenOrCreate, FileAccess.Write);
                using BinaryWriter writer = new BinaryWriter(stream);
                stream.SetLength(0);

                // Construct header.
                SEFileHeader header = new SEFileHeader(SEFileHeaderFlags.None, 1, extension, new byte[0], (uint) bytes.Length);

                // Write to the processed file, and delete the old file.
                header.WriteToStream(writer);
                writer.Write(bytes);
                IOFile.Delete(file);

                processed.Add(newFile);
            }

            // Load headers of processed files.
            foreach (string file in processed) {
                string relativeDirectory = FileIO.GetRelativePath(FileIO.BaseDirectory, file);
                string fileName = Path.ChangeExtension(FileIO.GetRelativePath(FileIO.DataDirectory, file), null);

                // Read and construct header.
                using Stream titleStream = TitleContainer.OpenStream(relativeDirectory);
                using BinaryReader reader = new BinaryReader(titleStream);
                SEFileHeader header = SEFileHeader.ReadFromStream(reader);

                files.Add(fileName, new File(file, header));
            }
        }

        internal class File
        {
            public string FileName { get; }
            public string AbsoluteDirectory { get; }
            public string AppRelativeDirectory { get; }
            public string DataRelativeDirectory { get; }
            public SEFileHeader Header;

            public byte[] Data {
                get {
                    // Load if unloaded.
                    if (!IsLoaded && curTaskType == TaskType.Load) { 
                        WaitTask();
                    } else {
                        Load();
                        WaitTask();
                    }

                    // TODO: Handle unloading, etc.

                    return data;
                }
            }
            private byte[] data;

            public bool IsLoaded => data != null;

            private Task currentTask;
            private TaskType curTaskType;

            private void WaitTask()
            {
                if (currentTask != null && !(currentTask.IsCompleted || currentTask.IsCanceled)) {
                    currentTask.Wait();
                    curTaskType = TaskType.None;
                }
            }

            public void Load()
            {
                WaitTask();
                currentTask = Task.Run(() => {
                    Stream stream = TitleContainer.OpenStream(AppRelativeDirectory);
                    BinaryReader reader = new BinaryReader(stream);
                    try {
                        stream.Position = Header.HeaderSize;
                        data = reader.ReadBytes((int) Header.FileSize);
                    } catch (Exception) {
                        reader.Close();
                        stream.Close();
                        throw;
                    }

                    curTaskType = TaskType.None;
                });
                curTaskType = TaskType.Load;
            }

            public void Unload()
            {
                WaitTask();
                currentTask = Task.Run(() => {
                    data = null;
                    curTaskType = TaskType.None;
                });
                curTaskType = TaskType.Unload;
            }

            public File(string absoluteDirectory, SEFileHeader header)
            {
                FileName = Path.ChangeExtension(FileIO.GetRelativePath(FileIO.DataDirectory, absoluteDirectory), null);
                AppRelativeDirectory = FileIO.GetRelativePath(FileIO.BaseDirectory, absoluteDirectory);
                DataRelativeDirectory = FileIO.GetRelativePath(FileIO.DataDirectory, absoluteDirectory);
                AbsoluteDirectory = absoluteDirectory;
                Header = header;
            }

            private enum TaskType
            {
                None,
                Load,
                Unload
            }
        }
    }
}
