namespace SE.World
{
    /// <summary>
    /// IMPORTANT: SceneScripts must contain the default constructor (or no constructor).
    /// 
    /// Allows for custom logic to be executed on scene load, unload and update. Override the LevelName and LevelNamespace properties
    /// to the namespace and name of the level you wish to apply custom logic. Reflection is used to automatically instantiate these
    /// classes at runtime.
    ///
    /// By default, the class methods are not executed in the editor. Apply a [ExecuteInEditor] attribute
    /// to the class to enable execution in the editor.
    /// </summary>
    public abstract class SceneScript
    {
        public abstract string LevelName { get; }
        public abstract string LevelNamespace { get; }

        public virtual void BeforeSceneLoad() { }
        public virtual void AfterSceneLoad() { }
        public virtual void BeforeSceneUnload() { }
        public virtual void AfterSceneUnload() { }
        public virtual void SceneUpdate() { }
    }
}
