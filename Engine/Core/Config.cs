using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SE.Core.Extensions;

namespace SE.Core
{
    public static class Config
    {
        internal static bool Initialized { get; private set; }

        private static ConfigData configData;
        private const string _CONFIG_FILE_NAME = "SE_CONFIG.json";

        private static JsonSerializerSettings jsonSettings {
            get {
                JsonSerializerSettings settings = new JsonSerializerSettings {
                    Formatting = Formatting.Indented
                };
                return settings;
            }
        }

        private static void ThrowInitialized(string name)
            => throw new InvalidOperationException($"Cannot modify engine parameter '{name}' after Initialize() has been called.");

        public static void Initialize()
        {
            if (FileIO.FileExists(_CONFIG_FILE_NAME)) {
                Load();
            } else {
                configData = new ConfigData();
                Save();
            }
            Initialized = true;
        }

        public static void Save() 
            => FileIO.SaveFile(configData.Serialize(false, jsonSettings), _CONFIG_FILE_NAME);

        public static void Load()
            => configData = FileIO.ReadFile(_CONFIG_FILE_NAME).Deserialize<ConfigData>(false, jsonSettings);

        public static class Performance
        {
            public static bool UseArrayPoolCore {
                get => configData.Performance.UseArrayPoolCore;
                set {
                    if (Initialized) 
                        ThrowInitialized(nameof(UseArrayPoolParticles));

                    configData.Performance.UseArrayPoolCore = value;
                }
            }

            public static bool UseArrayPoolParticles {
                get => configData.Performance.UseArrayPoolParticles;
                set {
                    if (Initialized) 
                        ThrowInitialized(nameof(UseArrayPoolParticles));

                    configData.Performance.UseArrayPoolParticles = value;
                }
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
