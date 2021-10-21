using Newtonsoft.Json;

namespace SE.Core
{
    // TODO: Use ini instead of json for config.
    public static class Config
    {
        private const string _CONFIG_FILE_NAME = "SE_CONFIG.json";

        public static bool RequireSave { get; private set; }
        public static bool RequireRestart { get; private set; }
        internal static bool Initialized { get; private set; }

        private static ConfigData configData;
        private static JsonSerializerSettings settings = new JsonSerializerSettings {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            Formatting = Formatting.Indented
        };

        public static void Initialize()
        {
            if (FileIO.FileExists(_CONFIG_FILE_NAME)) {
                Load();
            } else {
                configData = new ConfigData();
                Save();
            }
            // Set up values that cannot be changed after initialization.
            // These use dummy variables after initialization.
            Performance.Setup();

            Initialized = true;
        }

        public static void Update()
        {
            if (!RequireSave)
                return;

            Save();
            RequireSave = false;
        }

        private static void SettingChanged(bool requireRestart = false)
        {
            if (!Initialized)
                return;

            RequireRestart = requireRestart;
            RequireSave = true;
        }

        public static void Save()
            => FileIO.SaveFile(configData.Serialize(options: settings), _CONFIG_FILE_NAME);

        public static void Load()
            => configData = FileIO.ReadFileString(_CONFIG_FILE_NAME).Deserialize<ConfigData>(options: settings);

        public static class Performance
        {
            public static bool UseArrayPoolCore {
                get => useArrayPoolCoreVal;
                set {
                    configData.Performance.UseArrayPoolCore = value;
                    SettingChanged(Initialized);
                }
            }
            private static bool useArrayPoolCoreVal;

            public static bool UseArrayPoolParticles {
                get => useArrayPoolParticlesVal;
                set {
                    configData.Performance.UseArrayPoolParticles = value;
                    SettingChanged(Initialized);
                }
            }
            private static bool useArrayPoolParticlesVal;

            public static bool UseParticleInstancing {
                get => useParticleInstancing;
                set {
                    configData.Performance.UseParticleInstancing = value;
                    SettingChanged(Initialized);
                }
            }
            private static bool useParticleInstancing;

            internal static void Setup()
            {
                useArrayPoolCoreVal = configData.Performance.UseArrayPoolCore;
                useArrayPoolParticlesVal = configData.Performance.UseArrayPoolParticles;
                useParticleInstancing = configData.Performance.UseParticleInstancing;
            }
        }
    }

    internal class ConfigData
    {
        public PerformanceConfigData Performance { get; set; } = new PerformanceConfigData();

        public class PerformanceConfigData
        {
            public bool UseArrayPoolCore { get; set; } = true;
            public bool UseArrayPoolParticles { get; set; } = true;
            public bool UseParticleInstancing { get; set; } = true;
        }
    }
}
