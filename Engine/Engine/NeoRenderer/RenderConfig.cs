using System;
using System.Collections.Generic;
using System.Text;

namespace SE.NeoRenderer
{
    public static class RenderConfig
    {
        public static int MaxMaterialSlots {
            get => maxMaterialSlots;
            set {
                if (initialized)
                    throw new Exception();

                if (value < 512)
                    value = 512;

                maxMaterialSlots = value;
            }
        }
        private static int maxMaterialSlots = 8192;

        private static bool initialized;

        internal static void Done()
        {
            initialized = true;
            SpriteMaterialHandler.Initialize();
        }
    }
}
