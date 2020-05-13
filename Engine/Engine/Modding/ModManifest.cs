namespace SE.Modding
{
    /// <summary>
    /// Contains metadata for modifications.
    /// </summary>
    public class ModManifest
    {
        /// <summary>Name which the user sees.</summary>
        public string Name = "";

        /// <summary>Internal name used by the engine. Should be the same as the .dll file name.</summary>
        public string InternalName = "";

        /// <summary>At which order the mod will be loaded, assuming additional mods have the same or no dependencies.
        ///          Higher load order means the first to load.</summary>
        public int LoadOrder;

        /// <summary>Which mods this modification is dependent on. Dependencies will be loaded first, ensuring correct
        ///          load order. Dependencies are based on internal names.</summary>
        public string[] Dependencies;
    }
}
