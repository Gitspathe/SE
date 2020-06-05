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

        private static ConfigObj configObj;

        private static JsonSerializerSettings jsonSettings {
            get {
                JsonSerializerSettings settings = new JsonSerializerSettings {
                    Formatting = Formatting.Indented
                };
                return settings;
            }
        }

        public static bool UseArrayPoolCore {
            get => configObj.UseArrayPoolCore;
            set {
                if (Initialized) 
                    ThrowInitialized();

                configObj.UseArrayPoolCore = value;
            }
        }

        public static bool UseArrayPoolParticles {
            get => configObj.useArrayPoolParticles;
            set {
                if (Initialized) 
                    ThrowInitialized();

                configObj.useArrayPoolParticles = value;
            }
        }

        private static void ThrowInitialized()
            => throw new InvalidOperationException("Cannot modify engine config after Initialize() has been called.");

        public static void Initialize()
        {
            if (FileIO.FileExists("SE_CONFIG")) {
                Load();
            } else {
                configObj = new ConfigObj();
                Save();
            }
            Initialized = true;
        }

        public static void Save() 
            => FileIO.SaveFile(configObj.Serialize(false, jsonSettings), "SE_CONFIG");

        public static void Load()
            => configObj = FileIO.ReadFile("SE_CONFIG").Deserialize<ConfigObj>(false, jsonSettings);

        [JsonObject(MemberSerialization.OptOut)]
        private class ConfigObj
        {
            public bool UseArrayPoolCore = true;
            public bool useArrayPoolParticles = true;
        }
    }
}
