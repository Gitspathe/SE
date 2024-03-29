using SE.Modding;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SE.Core
{
    internal static class ModLoader
    {
        private static List<Mod> mods = new List<Mod>();

        public static void Initialize()
        {
            string[] modStrings = Directory.GetFiles(FileIO.DataDirectory, "modmanifest", SearchOption.AllDirectories);
            foreach (string s in modStrings) {
                ModManifest manifest = FileIO.ReadFileString(s).Deserialize<ModManifest>();
                mods.Add(new Mod(manifest));
            }

            mods = mods.OrderBy(x => x.LoadOrder).ToList();

            // First, load all mod assemblies into memory.
            foreach (Mod mod in mods) {
                mod.LoadAssembly();
            }

            // Then call post constructor and initialize methods.
            foreach (Mod mod in mods) {
                mod.PostConstructor();
                ModLoadIteration(mod);
            }
        }

        private static void ModLoadIteration(Mod mod)
        {
            if (mod.IsLoaded)
                return;

            foreach (Mod dependency in mod.Dependencies) {
                ModLoadIteration(dependency);
            }
            mod.Initialize();
        }

        internal static Mod GetMod(string modName)
        {
            foreach (Mod mod in mods) {
                if (mod.InternalName == modName) {
                    return mod;
                }
            }
            return null;
        }

    }

}