﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.AssetManagement;
using SE.Attributes;
using SE.Components;
using SE.Components.Network;
using SE.Core;
using SE.Core.Internal;
using SE.Pooling;
using SE.Serialization;
using SE.UI;
using SE.Core.Extensions;
using SE.Engine.Networking;
using SE.Networking.Internal;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.Common
{
    /// <summary>
    /// GameObjects are containers for logic and components.
    /// </summary>
    public class GameObject : SEObject, INetLogicProxy, IDisposable
    {
        public string EngineName { get; set; }

        /// <summary>If true, GameObject will be serialized to the scene.</summary>
        public virtual bool SerializeToScene { get; set; } = true;
        /// <summary>If true, GameObject is attached to the level it spawns in, and is destroyed when a new level is loaded.</summary>
        public virtual bool DestroyOnLoad { get; set; } = true;
        /// <summary>True if Initialize has been called on the GameObject.</summary>
        public bool Initialized { get; internal set; }
        /// <summary>True if Awake has been called on the GameObject.</summary>
        public bool AwakeCalled { get; internal set; }
        /// <summary>The enabled state.</summary>
        public bool Enabled { get; protected set; }
        /// <summary>True if the GameObject is being destroyed.</summary>
        public bool PendingDestroy { get; private set; }
        /// <summary>True if Destroy() was called on the GameObject this frame.</summary>
        public bool Destroyed { get; private set; }
        /// <summary>Internal reflection information based on GameObject type.</summary>
        internal Reflection.GameObjectInfo ReflectionInfo { get; }

        /// <summary>Dynamic state of a GameObject. Dynamic GameObjects have their Update methods called while static
        ///          GameObjects do not.</summary>
        public virtual bool IsDynamic {
            get => dynamic;
            protected set {
                EnsureValidAccess();
                dynamic = value;
                GameEngine.UpdateGameObjectState(this);
            }
        }
        private bool dynamic;

        /// <summary>The GameObject's transform.</summary>
        public Transform Transform {
            get => TransformProp;
            protected set => TransformProp = value;
        }
        protected virtual Transform TransformProp { get; set; }

        public INetLogic NetLogic => NetIdentity;

        public NetworkIdentity NetIdentity { get; internal set; }

        internal PooledList<SpriteBase> Sprites = new PooledList<SpriteBase>(Config.Performance.UseArrayPoolCore, true, 1);
        internal PooledList<Component> Components = new PooledList<Component>(Config.Performance.UseArrayPoolCore, true, 1);
        internal PhysicsObject PhysicsObject = null;

        /// <summary>
        /// Creates a new GameObject instance.
        /// </summary>
        /// <param name="pos">Position.</param>
        /// <param name="rot">Rotation.</param>
        /// <param name="scale">Scale.</param>
        public GameObject(Vector2 pos, float rot, Vector2 scale)
        {
            ReflectionInfo = Reflection.GetGameObjectInfo(GetType());

            // Skip this constructor if the GameObject is a UIObject.
            if (this is UIObject) 
                return;

            Transform = new Transform(pos, scale, rot, this);
            GameEngine.GameObjectConstructorCallback(this);
        }

        public GameObject() : this(Vector2.Zero, 0f, Vector2.One) { }

        /// <summary>
        /// Create a new GameObject instance from a type.
        /// </summary>
        /// <param name="gameObjectType">GameObject type.</param>
        /// <param name="position">2D position.</param>
        /// <param name="rotation">Rotation in degrees.</param>
        /// <param name="scale">Scale.</param>
        /// <returns>GameObject which was instantiated.</returns>
        public static GameObject Instantiate(Type gameObjectType, Vector2 position, float rotation, Vector2 scale)
            => (GameObject)Activator.CreateInstance(gameObjectType, position, rotation, scale);

        /// <summary>
        /// Throws exceptions if the GameObject cannot be accessed.
        /// </summary>
        private protected void EnsureValidAccess()
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");
        }

        public bool ExecuteIsValid(Component component)
            => ExecuteIsValid() && component.ReflectionInfo.Execute;

        public bool ExecuteIsValid()
            => ReflectionInfo.Execute;

        internal void AddSprite(SpriteBase s) 
            => Sprites.Add(s);

        internal void RemoveSprite(SpriteBase s) 
            => Sprites.Remove(s);

        internal void UpdateSpriteBounds()
        {
            SpriteBase[] array = Sprites.Array;
            for (int i = 0; i < Sprites.Count; i++) {
                array[i].RecalculateBounds();
            }
        }

        internal virtual void RecalculateBounds() { }

        internal void RecalculateBoundsInternal()
        {
            UpdateSpriteBounds();
            RecalculateBounds();
            Component[] componentArr = Components.Array;
            for (int i = 0; i < Components.Count; i++) {
                componentArr[i].OwnerBoundsUpdated();
            }
        }

        internal virtual void OnInitializeInternal()
        {
            if (string.IsNullOrEmpty(EngineName)) {
                EngineName = GetType().Name;
            }

            // If the GameObject has a Components attribute, add the components required, from the reflection cache.
            // Components added in this step are not initialized until Initialize() is called.
            foreach (Type type in Reflection.GetGameObjectInfo(GetType()).Components) {
                Component component = (Component) Activator.CreateInstance(type);
                component.InstantiatedFromAttribute = true;
                AddComponentInternal(component, false);
            }
            SortComponents();

            Enabled = true;         // TODO: What about GameObjects that are instantiated disabled?

            // Register GameObject to the core engine, and call OnInitializeInternal() on children.
            GameEngine.AddGameObject(this);

            Transform[] children = Transform.Children.Array;
            for (int i = 0; i < Transform.Children.Count; i++) {
                Transform child = children[i];
                child.GameObject?.OnInitializeInternal();
            }
        }

        public void Initialize()
        {
            OnInitializeInternal();
            OnAwakeInternal();
            if(ExecuteIsValid())
                OnInitialize();

            // Initialize any components from the EngineInitialize() method.
            for (int i = 0; i < Components.Count; i++) {
                if (!Components.Array[i].Initialized && ExecuteIsValid(Components.Array[i]))
                    Components.Array[i].Initialize();
            }

            // Initialize children.
            for (int i = 0; i < Transform.Children.Count; i++) {
                Transform t = Transform.Children.Array[i];
                if (t.GameObject != null && !t.GameObject.Initialized) {
                    t.GameObject?.Initialize();
                }
            }
            Initialized = true;
            RecalculateBoundsInternal();
        }

        public void Update()
        {
            OnUpdateInternal();
            if(ExecuteIsValid())
                OnUpdate();
        }

        public override void Destroy()
        {
            if(Destroyed)
                return;

            base.Destroy();

            PendingDestroy = true;
            bool wasPooled = false;

            // Process IPoolableGameObject logic if the GameObject is poolable.
            if (this is IPoolableGameObject poolable && poolable.MyPool != null) {
                if (poolable.ReturnOnDestroy) {
                    poolable.MyPool.Return(this);
                    wasPooled = true;
                } else {
                    poolable.MyPool.DestroyedCallback(this);
                }
            }

            // If the GameObject was pooled, return.
            if (wasPooled)
                return;

            // Call destroy if valid.
            if(ExecuteIsValid())
                OnDestroy();

            // If the GameObject is networked, tell the NetHelper to clean it up and then call OnDestroyInternal.
            // Otherwise, just call OnDestroyInternal().
            if (NetIdentity != null) {
                if (Network.IsServer) {
                    NetHelper.Destroy(NetLogic.ID);
                }
            } else {
                OnDestroyInternal();
            }
        }

        public void Enable(bool isRoot = true)
        {
            if (!Enabled) 
                OnEnableInternal(isRoot);
        }

        public void Disable(bool isRoot = true)
        {
            if(Enabled)
                OnDisableInternal(isRoot);
        }

        /// <summary>
        /// Enables or disabled the GameObject.
        /// </summary>
        /// <param name="value">Enabled/disabled state.</param>
        public void SetActive(bool value)
        {
            if (value)
                Enable();
            else
                Disable();
        }

        protected virtual void OnAwake() { }

        private protected void OnAwakeInternal()
        {
            EnsureValidAccess();

            // Awaken any components from the EngineInitialize() method.
            for (int i = 0; i < Components.Count; i++) {
                Component component = Components.Array[i];
                if (!component.AwakeCalled && ExecuteIsValid(component)) {
                    component.Awake();
                }
            }

            // Awaken children.
            for (int i = 0; i < Transform.Children.Count; i++) {
                Transform t = Transform.Children.Array[i];
                if (t.GameObject != null && !t.GameObject.AwakeCalled) {
                    t.GameObject?.OnAwake();
                }
            }
            AwakeCalled = true;

            OnAwake();
        }

        /// <summary>
        /// Initializes the GameObject.
        /// </summary>
        protected virtual void OnInitialize() { }

        internal virtual void OnUpdateInternal()
        {
            EnsureValidAccess();

            Component[] components = Components.Array;
            for (int i = 0; i < Components.Count; i++) {
                components[i].UpdateInternal();
            }

            for (int i = 0; i < Components.Count; i++) {
                Component c = components[i];
                if (!c.Enabled || !ExecuteIsValid(c))
                    continue;

                c.Update();

                // Iterate over the fixed time steps for this frame.
                int curStep = 0;
                while (curStep < Time.FixedTimeStepIterations) {
                    c.OnFixedUpdate();
                    curStep++;
                }
            }
            //RecalculateBounds(); // TODO: Get rid of this. Bounds should be updated as needed!
        }

        /// <summary>
        /// Update loop.
        /// </summary>
        protected virtual void OnUpdate() { }

        internal void OnDestroyInternal()
        {
            for (int i = Components.Count - 1; i >= 0; i--) {
                Components.Array[i].Destroy();
            }

            Components.Clear();
            GameEngine.RemoveGameObject(this, true);
            Disable(false);
            Transform.SetParent(null);

            Transform[] children = Transform.Children.Array;
            for (int i = Transform.Children.Count - 1; i >= 0; i--) {
                children[i].GameObject?.Destroy();
            }

            AssetConsumerInternal?.DereferenceAssets();
            Dispose();
            Transform = null;
            Destroyed = true;
        }

        private void OnDestroyEditor()
        {
            if (Destroyed)
                return;

            OnDestroyInternal();
        }

        /// <summary>
        /// Destroys a GameObject and all of its components. The GameObject is not completely destroyed immediately, but after all dynamic GameObjects are updated.
        /// Beware that references to the GameObject will prevent it from being destroyed properly. For example, an enemy referenced in a custom 'enemy manager' will
        /// continue to exist even after Destroy() is called. In this example, the GameObject reference must be manually removed from the 'enemy manager'.
        ///
        /// If the GameObject is an IPoolableGameObject, currently in a pool, and has ReturnOnDestroy set to true, it will instead be disabled and returned to it's
        /// pool when this function is called.
        /// </summary>
        protected virtual void OnDestroy() { }

        /// <summary>
        /// Enables, allowing the Update loop.
        /// </summary>
        protected virtual void OnEnable(bool isRoot = true) { }

        private protected void OnEnableInternal(bool isRoot = true)
        {
            EnsureValidAccess();
            bool execute = ExecuteIsValid();
            if (isRoot && Enabled && execute) {
                OnEnable();
                return;
            }

            GameEngine.AddGameObject(this);
            for (int i = 0; i < Components.Count; i++) {
                Component component = Components.Array[i];
                if (ExecuteIsValid(component)) { 
                    component.Enabled = true;
                }
            }

            Enabled = true;
            if (isRoot) {
                Transform.ChildStateTree.Apply();
            }

            if(execute)
                OnEnable(isRoot);
        }

        /// <summary>
        /// Disables, preventing the Update loop.
        /// </summary>
        protected virtual void OnDisable(bool isRoot = true) { }

        private protected void OnDisableInternal(bool isRoot = true)
        {
            EnsureValidAccess();
            bool execute = ExecuteIsValid();
            if (isRoot && !Enabled && ExecuteIsValid()) {
                OnDisable();
                return;
            }

            if (isRoot) {
                Transform.ChildStateTree.Regenerate();
            }
            GameEngine.RemoveGameObject(this);
            for (int i = 0; i < Components.Count; i++) {
                Component component = Components.Array[i];
                if (ExecuteIsValid(component)) {
                    component.Enabled = false;
                }
            }

            Enabled = false;
            Transform[] children = Transform.Children.Array;
            for (int i = 0; i < Transform.Children.Count; i++) {
                children[i].GameObject?.OnDisableInternal(false);
            }

            if(execute)
                OnDisable(isRoot);
        }

        internal void OnTransformChildrenChangedInternal()
        {
            Component[] components = Components.Array;
            for (int i = 0; i < Components.Count; i++) {
                components[i].OnTransformChildrenChanged();
            }
            OnTransformChildrenChanged();
        }

        protected virtual void OnTransformChildrenChanged() { }

        /// <summary>
        /// Checks if the GameObject contains a component of a specified type.
        /// </summary>
        /// <typeparam name="T">Component Type.</typeparam>
        /// <returns>True if the GameObject has a component of the specified Type.</returns>
        public bool HasComponent<T>() where T : Component 
            => HasComponent(typeof(T));

        public bool HasComponent(Type type)
        {
            EnsureValidAccess();
            for (int i = 0; i < Components.Count; i++) {
                Component component = Components.Array[i];
                if (component.GetType() == type || component.GetType().IsSubclassOf(type)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get a Component from a GameObject.
        /// </summary>
        /// <typeparam name="T">Component Type.</typeparam>
        /// <returns>A Component.</returns>
        public T GetComponent<T>() where T : Component 
            => (T) GetComponent(typeof(T));

        public Component GetComponent(Type type)
        {
            EnsureValidAccess();
            for (int i = 0; i < Components.Count; i++) {
                Component component = Components.Array[i];
                if (component.GetType() == type || component.GetType().IsSubclassOf(type)) {
                    return component;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all the components of a certain type from a GameObject.
        /// </summary>
        /// <typeparam name="T">Component Type.</typeparam>
        /// <returns>List of components.</returns>
        public List<T> GetComponents<T>() where T : Component
        {
            EnsureValidAccess();
            List<T> cList = new List<T>();
            for (int i = 0; i < Components.Count; i++) {
                Component component = Components.Array[i];
                if (component.GetType() == typeof(T) || component.GetType().IsSubclassOf(typeof(T))) {
                    cList.Add((T)component);
                }
            }
            return cList;
        }

        /// <summary>
        /// Gets all the components of a certain type from a GameObject. Non-allocating.
        /// </summary>
        /// <typeparam name="T">Component Type.</typeparam>
        /// <returns>List of components.</returns>
        public void GetComponents<T>(List<T> existingList) where T : Component
        {
            EnsureValidAccess();
            for (int i = 0; i < Components.Count; i++) {
                Component component = Components.Array[i];
                if (component.GetType() == typeof(T) || component.GetType().IsSubclassOf(typeof(T))) {
                    existingList.Add((T)component);
                }
            }
        }

        /// <summary>
        /// Gets all the components of a certain type from a GameObject and all of it's children.
        /// </summary>
        /// <typeparam name="T">Component Type.</typeparam>
        /// <returns>List of components.</returns>
        public List<T> GetAllComponents<T>() where T : Component
        {
            EnsureValidAccess();
            List<T> cList = new List<T>();
            Transform[] children = Transform.Children.Array;
            for (int i = 0; i < Transform.Children.Count; i++) {
                cList.AddRange(children[i].GameObject.GetAllComponents<T>());
            }
            cList.AddRange(GetComponents<T>());
            return cList;
        }

        /// <summary>
        /// Gets all the components of a certain type from a GameObject and all of it's children. Non-allocating.
        /// </summary>
        /// <typeparam name="T">Component Type.</typeparam>
        /// <returns>List of components.</returns>
        public void GetAllComponents<T>(List<T> existingList) where T : Component
        {
            EnsureValidAccess();
            Transform[] children = Transform.Children.Array;
            for (int i = 0; i < Transform.Children.Count; i++) {
                children[i].GameObject.GetAllComponents(existingList);
            }
            GetComponents(existingList);
        }

        /// <summary>
        /// Adds a Component to a GameObject.
        /// </summary>
        /// <param name="component">Component to add.</param>
        /// <param name="doInitialize">If true, the component will be initialized after being added.</param>
        internal Component AddComponentInternal(Component component, bool doInitialize = true)
        {
            EnsureValidAccess();
            if (!Components.Contains(component)) {
                Components.Add(component);
                component.InitializeInternal(this);
                if (doInitialize && ExecuteIsValid(component)) {
                    component.Initialize();
                }
            }
            SortComponents();
            return component;
        }

        public Component AddComponent(Component component) 
            => AddComponentInternal(component);

        /// <summary>
        /// Adds components to a GameObject.
        /// </summary>
        /// <param name="component">Components to add.</param>
        public void AddComponents(params Component[] component)
        {
            for (int i = 0; i < component.Length; i++) {
                AddComponent(component[i]);
            }
        }

        /// <summary>
        /// Adds components to a GameObject.
        /// </summary>
        /// <param name="componentList">Components to add.</param>
        public void AddComponents(List<Component> componentList)
        {
            for (int i = 0; i < componentList.Count; i++) {
                AddComponent(componentList[i]);
            }
        }

        /// <summary>
        /// Removes a component from a GameObject.
        /// </summary>
        /// <param name="component">Component to remove.</param>
        /// <returns>True if the component existed on the GameObject, and was successfully removed..</returns>
        public void RemoveComponent(Component component)
        {
            EnsureValidAccess();
            Components.Remove(component);
            component.Destroy();
            SortComponents();
        }

        /// <summary>
        /// Removes components from a GameObject.
        /// </summary>
        /// <typeparam name="T">Type of component to remove.</typeparam>
        public void RemoveComponentsOfType<T>() where T : Component
        {
            for (int i = 0; i < Components.Count; i++) {
                if (Components.Array[i].GetType() == typeof(T)) {
                    RemoveComponent(Components.Array[i]);
                }
            }
        }

        protected internal void SortComponents() 
            => Components.Sort(new ComponentQueueComparer());

        internal string SerializeJson()
        {
            GameObjectData data = Serialize();
            string str = data.Serialize();
            data.Dispose();
            return str;
        }

        internal GameObjectData Serialize()
        {
            GameObjectData data = new GameObjectData {
                EngineName = EngineName,
                Type = GetType(),
                Position = Transform.Position,
                Rotation = Transform.Rotation,
                Scale = Transform.Scale,
                componentData = new QuickList<ComponentData>()
            };
            for (int i = 0; i < Components.Count; i++) {
                Component component = Components.Array[i];
                if(!component.Serialized)
                    continue;

                ComponentData componentData = component.Serialize();
                componentData.ComponentIndex = (ulong) i;
                data.componentData.Add(componentData);
            }
            return data;
        }

        internal void Deserialize(string jsonData) 
            => Deserialize(jsonData.Deserialize<GameObjectData>());

        internal void Deserialize(GameObjectData data)
        {
            // Deserialize primary data...
            EngineName = data.EngineName;
            Transform.Position = data.Position;
            Transform.Rotation = data.Rotation;
            Transform.Scale = data.Scale;

            // Deserialize any valid components...
            foreach (ComponentData serializedComponent in data.componentData) {
                for (int i = 0; i < Components.Count; i++) {
                    if(!Components.Array[i].Serialized)
                        continue;
                    if(serializedComponent.ComponentIndex != (ulong) i)
                        continue;

                    Component component = Components.Array[i];
                    component.Deserialize(serializedComponent);
                }
            }
            data.Dispose();
        }

        protected virtual object SerializeAdditionalData() => null;
        protected virtual void DeserializeAdditionalData(object data) { }

        private struct ComponentQueueComparer : IComparer<Component>
        {
            int IComparer<Component>.Compare(Component x, Component y) => y.Queue.CompareTo(x.Queue);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                if (Config.Performance.UseArrayPoolCore) {
                    Components.Dispose();
                    Sprites.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}