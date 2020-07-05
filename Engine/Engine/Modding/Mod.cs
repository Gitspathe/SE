using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SE.Core;

namespace SE.Modding
{
    /// <summary>
    /// Represents a modification which was loaded during runtime. Parameters are assigned when the mod's manifest and assembly are loaded.
    /// </summary>
    public class Mod
    {
        /// <summary>Modifications primary assembly, contains the mod's entry point and Main() method.</summary>
        public Assembly ModAssembly { get; private set; }

        /// <summary>Name which the user sees.</summary>
        public string Name { get; private set; }

        /// <summary>Internal name used by the engine. Should be the same as the .dll file name.</summary>
        public string InternalName { get; private set; }

        /// <summary>At which order the mod will be loaded, assuming additional mods have the same or no dependencies.
        ///          Higher load order means the first to load.</summary>
        public int LoadOrder { get; private set; }

        /// <summary>True if the mod's assembly has been loaded.</summary>
        public bool IsLoaded { get; private set; }

        /// <summary>Dependencies for the modification.</summary>
        public List<Mod> Dependencies { get; private set; } = new List<Mod>();

        /// <summary>Internal names of the dependencies for the modification.</summary>
        public List<string> DependencyNames { get; private set; } = new List<string>();

        /// <summary>Returns true if the mod has any dependencies.</summary>
        public bool HasDependencies => Dependencies != null && Dependencies.Count >= 1;

        internal void LoadAssembly()
        {
            string path = Path.Combine(FileIO.DataDirectory, InternalName, InternalName + ".dll");
            ModAssembly = Assembly.LoadFrom(path);
        }

        internal void Initialize()
        {
            Type mainType = ModAssembly.GetTypes().First(t => typeof(IModEntryPoint).IsAssignableFrom(t));
            object obj = Activator.CreateInstance(mainType);

            ((IModEntryPoint)obj).Main();
            IsLoaded = true;
        }

        internal void PostConstructor()
        {
            for (int i = 0; i < DependencyNames.Count; i++) {
                Dependencies.Add(ModLoader.GetMod(DependencyNames[i]));
            }
            Dependencies = Dependencies.OrderBy(x => x.LoadOrder).ToList();
        }

        public Mod(ModManifest manifest)
        {
            if (manifest.Dependencies != null) {
                DependencyNames.AddRange(manifest.Dependencies);
            }
            Name = manifest.Name;
            InternalName = manifest.InternalName;
            LoadOrder = manifest.LoadOrder;
        }

    }

}
