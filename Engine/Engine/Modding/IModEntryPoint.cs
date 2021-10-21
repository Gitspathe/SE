namespace SE.Modding
{
    /// <summary>
    /// Represents the entry point for a modification.
    /// </summary>
    public interface IModEntryPoint
    {
        /// <summary>
        /// Called when the mod is loaded during runtime.
        /// </summary>
        void Main();
    }
}
