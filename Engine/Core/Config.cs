using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SE.Core.Extensions;

namespace SE.Core
{
    public static class Config
    {
        private const string _CONFIG_FILE_NAME = "SE_CONFIG.json";

        public static bool RequireSave { get; private set; }
        public static bool RequireRestart { get; private set; }
        internal static bool Initialized { get; private set; }

        private static ConfigData configData;
        private static JsonSerializerSettings jsonSettings;

        public static void Initialize()
        {
            jsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented
            };

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
            if(!Initialized)
                return;

            RequireRestart = requireRestart;
            RequireSave = true;
        }

        public static void Save() 
            => FileIO.SaveFile(configData.Serialize(false, jsonSettings), _CONFIG_FILE_NAME);

        public static void Load()
            => configData = FileIO.ReadFile(_CONFIG_FILE_NAME).Deserialize<ConfigData>(false, jsonSettings);

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

            internal static void Setup()
            {
                useArrayPoolCoreVal = configData.Performance.UseArrayPoolCore;
                useArrayPoolParticlesVal = configData.Performance.UseArrayPoolParticles;
            }
        }
    }

    [JsonObject(MemberSerialization.OptOut)]
    internal class ConfigData
    {
        public PerformanceConfigData Performance = new PerformanceConfigData();

        [JsonObject(MemberSerialization.OptOut)]
        public class PerformanceConfigData
        {
            public bool UseArrayPoolCore = true;
            public bool UseArrayPoolParticles = true;
        }
    }
}
