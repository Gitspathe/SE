using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private static Dictionary<string, FileProcessor> fileProcessorExtensions = new Dictionary<string, FileProcessor>();

        private static QuickList<File> bgLoadFiles = new QuickList<File>();
        private static QuickList<File> bgUnloadFiles = new QuickList<File>();
        private static HashSet<File> pendingLoad = new HashSet<File>();
        private static HashSet<File> pendingUnload = new HashSet<File>();
        private static object loadLock = new object();

        private static int loadBatch = 2;

        static FileMarshal()
        {
            AddProcessor(new Texture2DFileProcessor());
        }

        public static void Setup()
        {
            HashSet<string> needsProcessing = new HashSet<string>();
            HashSet<string> processed = new HashSet<string>();
            QuickList<ValueTuple<Task, string>> processing = new QuickList<(Task, string)>();

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
            foreach (string file in needsProcessing) {
                byte[] bytes = IOFile.ReadAllBytes(file);
                string extension = Path.GetExtension(file);
                string pathNoExtension = Path.ChangeExtension(file, null);
                string newFile = pathNoExtension + _DATA_EXTENSION;

                // Create write stream.
                FileStream stream = new FileStream(newFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bytes.Length, FileOptions.Asynchronous);
                BinaryWriter writer = new BinaryWriter(stream);
                stream.SetLength(0);

                // Construct header.
                SEFileHeader header = new SEFileHeader(SEFileHeaderFlags.None, 1, extension, new byte[0], (uint)bytes.Length);

                // Write to the processed file.
                header.WriteToStream(writer);
                Task writeTask = stream.WriteAsync(bytes).AsTask().ContinueWith(_ => stream.Dispose());
                processing.Add((writeTask, newFile));

                // Delete the old file
                IOFile.Delete(file);
            }

            // Await pending process tasks.
            foreach ((Task task, string newFileName) in processing) {
                task.Wait();
                processed.Add(newFileName);
            }

            // Load headers of processed files.
            foreach (string file in processed) {
                string fileName = Path.ChangeExtension(FileIO.GetRelativePath(FileIO.DataDirectory, file), null);
                files.Add(fileName, new File(file));
            }
        }

        internal static void Update(float deltaTime)
        {
            // Process background loaded files.
            for (int i = bgLoadFiles.Count - 1; i > 0; --i) {
                File file = bgLoadFiles.Array[i];
                if (file.BackgroundState == BackgroundState.Load && file.LoadState < LoadState.LoadedBytes)
                    continue;
                
                file.BackgroundState = BackgroundState.None;
                bgLoadFiles.Remove(file);
            }

            // Process background unloaded files.
            for (int i = bgUnloadFiles.Count - 1; i > 0; --i) {
                File file = bgUnloadFiles.Array[i];
                if (file.BackgroundState == BackgroundState.Unload && file.LoadState >= LoadState.LoadedBytes)
                    continue;

                file.BackgroundState = BackgroundState.None;
                bgUnloadFiles.Remove(file);
            }

            lock (loadLock) {
                // Find new files to allocate as background loading tasks.
                foreach (File file in pendingLoad) {
                    if (bgLoadFiles.Count > loadBatch)
                        break;
                    if (file.BackgroundState != BackgroundState.Load || bgLoadFiles.Contains(file)) 
                        continue;

                    bgLoadFiles.Add(file);
                }

                // Find new files to allocate as background unloading tasks.
                foreach (File file in pendingUnload) {
                    if (bgUnloadFiles.Count > loadBatch)
                        break;
                    if (file.BackgroundState != BackgroundState.Unload || bgUnloadFiles.Contains(file)) 
                        continue;

                    bgUnloadFiles.Add(file);
                }
            }
        }

        internal static void FlagBackgroundLoad(File file)
        {
            lock (loadLock) {
                pendingLoad.Add(file);
                pendingUnload.Remove(file);
                file.Load();
            }
            file.BackgroundState = BackgroundState.Load;
        }

        internal static void FlagBackgroundUnload(File file)
        {
            lock (loadLock) {
                pendingUnload.Add(file);
                pendingLoad.Remove(file);
                file.Unload();
            }
            file.BackgroundState = BackgroundState.Unload;
        }

        internal static void ClearFlag(File file)
        {
            lock (loadLock) {
                pendingUnload.Remove(file);
                pendingLoad.Remove(file);
            }
        }

        internal static void Unload(File file)
        {
            file.Unload();
        }

        public static void AddProcessor(FileProcessor processor)
        {
            if(fileProcessors.ContainsKey(processor.Type))
                return;

            foreach (string ext in processor.FileExtensions) {
                fileProcessorExtensions.Add(ext, processor);
            }
            fileProcessors.Add(processor.Type, processor);
        }

        public static bool TryLoad<T>(string file, out T obj)
        {
            // Try to return from a FileProcessor.
            if(TryGet(file, out File f)) {
                obj = (T) f.Data;
                return true;
            }

            obj = default;
            return false;
        }

        public static bool TryGet(string file, out File f) 
            => files.TryGetValue(file, out f);

        internal class File
        {
            public string FileName { get; }
            public string AbsoluteDirectory { get; }
            public string AppRelativeDirectory { get; }
            public string DataRelativeDirectory { get; }
            public LoadState LoadState { get; private set; }

            public SEFileHeader Header;
            public BackgroundState BackgroundState;

            private Task currentTask;
            private TaskType curTaskType;

            public object Data {
                get {
                    if(data != null)
                        return data;

                    // Load into memory if unloaded.
                    if (!SyncTask(TaskType.Load)) {
                        Load();
                        WaitTask();
                    }

                    // Load the file using a file processor.
                    Stream stream = new MemoryStream(bytes);
                    BinaryReader reader = new BinaryReader(stream);
                    if (!fileProcessorExtensions.TryGetValue(Header.OriginalExtension, out FileProcessor processor))
                        return null;
                    if(processor.LoadFileInternal(GameEngine.Engine.GraphicsDevice, reader, ref Header, out object o))
                        data = o;

                    bytes = null;
                    return data;
                }
            }
            private object data;
            private byte[] bytes;

            public File(string absoluteDirectory)
            {
                FileName = Path.ChangeExtension(FileIO.GetRelativePath(FileIO.DataDirectory, absoluteDirectory), null);
                AppRelativeDirectory = FileIO.GetRelativePath(FileIO.BaseDirectory, absoluteDirectory);
                DataRelativeDirectory = FileIO.GetRelativePath(FileIO.DataDirectory, absoluteDirectory);
                AbsoluteDirectory = absoluteDirectory;
                LoadHeader();
            }

            /// <summary>
            /// Waits for the pending task to complete.
            /// </summary>
            private void WaitTask()
            {
                if (currentTask != null && !currentTask.IsCompleted) {
                    currentTask.Wait();
                }
            }

            private void UpdateState()
            {
                if (data != null) {
                    LoadState = LoadState.LoadedData;
                } else if (bytes != null) {
                    LoadState = LoadState.LoadedBytes;
                } else if (Header.Loaded) {
                    LoadState = LoadState.LoadedHeader;
                } else {
                    LoadState = LoadState.Unloaded;
                }

                ClearFlag(this);
                BackgroundState = BackgroundState.None;
            }

            /// <summary>
            /// Waits for pending tasks, and returns true if the provided task type was the type which was pending.
            /// </summary>
            /// <param name="type">Task type to check.</param>
            /// <returns>True if the pending task was of the type passed.</returns>
            private bool SyncTask(TaskType type)
            {
                if (currentTask == null || currentTask.IsCompleted)
                    return false;

                currentTask.Wait();
                return curTaskType == type;
            }

            public void LoadHeader()
            {
                if(SyncTask(TaskType.LoadHeader))
                    return;

                // Read and construct header.
                currentTask = Task.Run(() => {
                    Stream stream = TitleContainer.OpenStream(AppRelativeDirectory);
                    BinaryReader reader = new BinaryReader(stream);
                    try {
                        Header = SEFileHeader.FromStream(reader);
                    } finally {
                        reader.Close();
                        UpdateState();
                        curTaskType = TaskType.None;
                    }
                });
                curTaskType = TaskType.LoadHeader;
            }

            public void Load()
            {
                if(SyncTask(TaskType.Load))
                    return;

                // Load header if it's unloaded.
                if (!Header.Loaded) {
                    LoadHeader();
                    WaitTask();
                }

                currentTask = Task.Run(() => {
                    Stream stream = TitleContainer.OpenStream(AppRelativeDirectory);
                    BinaryReader reader = new BinaryReader(stream);
                    try {
                        stream.Position = Header.HeaderSize;
                        bytes = reader.ReadBytes((int) Header.FileSize);
                    } finally {
                        reader.Close();
                        UpdateState();
                        curTaskType = TaskType.None;
                    }
                });
                curTaskType = TaskType.Load;
            }

            public void Unload()
            {
                WaitTask();
                if (data is IDisposable disposable) {
                    disposable.Dispose();
                }
                bytes = null;
                data = null;
                UpdateState();
            }

            private enum TaskType
            {
                None,
                LoadHeader,
                Load
            }
        }

        internal enum LoadState
        {
            Unloaded,
            LoadedHeader,
            LoadedBytes,
            LoadedData
        }

        internal enum BackgroundState
        {
            None,
            Unload,
            Load
        }
    }
}
