namespace SE
{
    internal static class EngineUtility
    {
        internal static bool TransformHierarchyDirty;
    }

    /// <summary>
    /// Describes the high-level display mode of the game.
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>An SDL window with full graphics will be displayed.</summary>
        Normal,

        /// <summary>An SDL window with only the console will be displayed.</summary>
        Headless,

        /// <summary>No SDL window will be displayed. An OS console/terminal window will handle the standard I/O streams.</summary>
        Decapitated
    }
}
