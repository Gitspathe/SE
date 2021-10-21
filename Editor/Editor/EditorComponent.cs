using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SE.Editor
{
    public sealed class EditorComponentHolder<T> : IEnumerable<T> where T : EditorComponent
    {
        private HashSet<T> components = new HashSet<T>();

        public void Add(T component)
        {
            if (components.Add(component)) {
                component.Initialize();
            }
        }

        public void Remove(T component)
        {
            components.Remove(component);
        }

        public EditorComponent Get<TSub>() where TSub : T
        {
            foreach (T component in components) {
                Type cType = component.GetType();
                if (cType == typeof(TSub) || cType.IsSubclassOf(typeof(TSub))) {
                    return component;
                }
            }
            return null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return components.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return components.GetEnumerator();
        }
    }

    /// <summary>
    /// Module-like objects used by the editor.
    /// </summary>
    public abstract class EditorComponent
    {
        /// <summary>
        /// Called when the component is registered to the editor.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called when the currently loaded project instanced changes.
        /// </summary>
        public abstract void InstanceChanged();
    }

    /// <summary>
    /// Represents an editor component which supports hot-reloading.
    /// </summary>
    public interface IReloadableComponent
    {
        /// <summary>
        /// Called when a reload is initiated. This function should save any relevant persistent state.
        /// It MUST also delete any references to the current project instance.
        /// </summary>
        void ReloadInitiated();

        /// <summary>
        /// Called when the instance is fully unloaded. This function should restore relevant persistent state.
        /// </summary>
        void ReloadComplete();
    }

    /// <summary>
    /// Represents an editor component which can be painted by ImGUI.
    /// </summary>
    public interface IPaintableComponent
    {
        /// <summary>
        /// Paints the editor component.
        /// </summary>
        void Paint();
    }

    /// <summary>
    /// Represents an editor component which is updated each frame.
    /// </summary>
    public interface IUpdatableComponent
    {
        /// <summary>
        /// Updates the editor component.
        /// </summary>
        void Update();
    }
}
