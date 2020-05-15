using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.AssetManagement;
using SE.Components;
using SE.Components.Network;
using SE.Core;
using SE.Core.Internal;
using SE.Engine.Utility;
using SE.Pooling;
using SE.Serialization;
using SE.UI;
using SE.World.Partitioning;
using SE.Core.Extensions;
using SE.Engine.Networking;
using SE.Networking.Internal;
using Vector2 = System.Numerics.Vector2;

namespace SE.Common
{
    /// <summary>
    /// GameObjects are containers for logic and components.
    /// </summary>
    public class GameObject : INetLogicProxy, IAssetConsumer, IPartitionObjectExtended
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

        /// <summary>True if Destroy() was called on the GameObject this frame.</summary>
        public bool Destroyed { get; private set; }

        public Vector2 PartitionPosition => Transform.GlobalPositionInternal;
        public Type PartitionObjectType => typeof(GameObject);
        public PartitionTile CurrentPartitionTile { get; set; }

        HashSet<IAsset> IAssetConsumer.ReferencedAssets { get; set; }

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

        /// <summary>If this GameObject ignores the culling grid.</summary>
        public virtual bool IgnoreCulling {
            get => ignoreCulling;
            protected set {
                EnsureValidAccess();
                ignoreCulling = value;
                GameEngine.UpdateGameObjectState(this);
            }
        }
        private bool ignoreCulling;

        /// <summary>The GameObject's transform.</summary>
        public Transform Transform {
            get => TransformProp;
            protected set => TransformProp = value;
        }

        /// <summary>If true, the GameObject will have it's bounds automatically calculated from it's sprites' bounds.
        ///          Defaults to true for regular GameObjects, defaults to false for UIObjects.</summary>
        public virtual bool AutoBounds { get; set; } = true;

        /// <summary>The bounds of a GameObject, scaled to the object's transform scale.</summary>
        public RectangleF Bounds {
            get => new RectangleF(unscaledBounds.X, unscaledBounds.Y,
                (int) (unscaledBounds.Width * Transform.Scale.X),
                (int) (unscaledBounds.Height * Transform.Scale.Y));
            set => UnscaledBounds = new RectangleF(value.X, value.Y,
                (int) (value.Width * Transform.Scale.X),
                (int) (value.Height * Transform.Scale.Y));
        }

        private RectangleF unscaledBounds = RectangleF.Empty;
        public RectangleF UnscaledBounds {
            get => unscaledBounds;
            set {
                EnsureValidAccess();
                unscaledBounds = value;
                UpdateSpriteBounds();
            }
        }

        public INetLogic NetLogic => NetIdentity;

        public NetworkIdentity NetIdentity { get; internal set; }

        protected virtual Transform TransformProp { get; set; }

        internal bool AddedToGameManager;
        internal SpriteBase[] Sprites = new SpriteBase[0];
        internal QuickList<IPartitionObject> PartitionObjects = new QuickList<IPartitionObject>();
        internal PhysicsObject PhysicsObject = null;
        protected internal QuickList<Component> Components = new QuickList<Component>();
        protected internal QuickList<Component> SerializedComponents = new QuickList<Component>();

        /// <summary>
        /// Throws exceptions if the GameObject cannot be accessed.
        /// </summary>
        private void EnsureValidAccess()
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");
        }

        internal void AddSprite(SpriteBase s)
        {
            List<SpriteBase> tmpSprites = new List<SpriteBase>();
            tmpSprites.AddRange(Sprites);
            tmpSprites.Add(s);
            Sprites = tmpSprites.ToArray();
        }

        internal void RemoveSprite(SpriteBase s)
        {
            List<SpriteBase> tmpSprites = new List<SpriteBase>();
            tmpSprites.AddRange(Sprites);
            tmpSprites.Remove(s);
            Sprites = tmpSprites.ToArray();
        }

        internal void UpdateSpriteBounds()
        {
            for (int i = 0; i < Sprites.Length; i++) {
                Sprites[i].RecalculateBounds();
            }
        }

        internal void RecalculateBounds()
        {
            if (Transform == null)
                return;

            UpdateSpriteBounds();
            if (!AutoBounds || Sprites.Length < 1) {
                unscaledBounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y,
                    unscaledBounds.Width, unscaledBounds.Height);
                return;
            }

            float largestWidth = 0, largestHeight = 0, minX = int.MaxValue, minY = int.MaxValue;
            for (int i = 0; i < Sprites.Length; i++) {
                Rectangle bounds = Sprites[i].Bounds;
                if (bounds.X < minX)
                    minX = bounds.X;
                if (bounds.Y < minY)
                    minY = bounds.Y;
                if (bounds.Width + Sprites[i].Offset.X > largestWidth)
                    largestWidth = bounds.Width + Sprites[i].Offset.X;
                if (bounds.Height + Sprites[i].Offset.Y > largestHeight)
                    largestHeight = bounds.Height + Sprites[i].Offset.Y;
            }

            unscaledBounds = new RectangleF(minX, minY, largestWidth, largestHeight);
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

            for (int i = 0; i < Transform.Children.Count; i++) {
                Transform child = Transform.Children[i];
                child.GameObject?.OnInitializeInternal();
            }
        }

        public void Initialize()
        {
            OnInitializeInternal();
            OnAwakeInternal();
            OnInitialize();

            // Initialize any components from the EngineInitialize() method.
            for (int i = 0; i < Components.Count; i++) {
                if (!Components.Array[i].Initialized && ExecuteIsValid(Components.Array[i]))
                    Components.Array[i].Initialize();
            }

            // Initialize children.
            for (int i = 0; i < Transform.Children.Count; i++) {
                Transform t = Transform.Children[i];
                if (t.GameObject != null && !t.GameObject.Initialized) {
                    t.GameObject?.Initialize();
                }
            }
            Initialized = true;

            RecalculateBounds();
        }

        public void Update()
        {
            OnUpdateInternal();
            OnUpdate();
        }

        public void Destroy()
        {
            if(Destroyed)
                return;

            if (GameEngine.IsEditor) {
                OnDestroyEditor();
            } else {
                bool wasPooled = false;
                if (this is IPoolableGameObject poolable && poolable.MyPool != null) {
                    if (poolable.ReturnOnDestroy) {
                        poolable.MyPool.Return(this);
                        wasPooled = true;
                    } else {
                        poolable.MyPool.DestroyedCallback(this);
                    }
                }

                if (wasPooled)
                    return;

                OnDestroy();
                if (NetIdentity != null) {
                    if (Network.IsServer) {
                        NetHelper.Destroy(NetLogic.ID);
                    }
                } else {
                    OnDestroyInternal();
                }
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

        protected void OnAwakeInternal()
        {
            EnsureValidAccess();

            // Awaken any components from the EngineInitialize() method.
            for (int i = 0; i < Components.Count; i++) {
                if (!Components.Array[i].AwakeCalled && ExecuteIsValid(Components.Array[i])) {
                    Components.Array[i].Awake();
                }
            }

            // Awaken children.
            for (int i = 0; i < Transform.Children.Count; i++) {
                Transform t = Transform.Children[i];
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

            for (int i = 0; i < Components.Count; i++) {
                Component c = Components.Array[i];
                c.UpdateInternal();
            }

            for (int i = 0; i < Components.Count; i++) {
                Component c = Components.Array[i];
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
            RecalculateBounds();
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

            for (int i = Transform.Children.Count - 1; i >= 0; i--) {
                Transform.Children[i].GameObject?.Destroy();
            }

            ((IAssetConsumer)this).DereferenceAssets();
            Transform.Dispose();
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

        protected void OnEnableInternal(bool isRoot = true)
        {
            EnsureValidAccess();
            if (isRoot && Enabled) {
                OnEnable();
                return;
            }

            GameEngine.AddGameObject(this);
            for (int i = 0; i < Components.Count; i++) {
                Components.Array[i].Enable();
            }
            Enabled = true;
            if (isRoot) {
                Transform.ChildStateTree.Apply();
            }

            OnEnable(isRoot);
        }

        /// <summary>
        /// Disables, preventing the Update loop.
        /// </summary>
        protected virtual void OnDisable(bool isRoot = true) { }

        protected void OnDisableInternal(bool isRoot = true)
        {
            EnsureValidAccess();
            if (isRoot && !Enabled) {
                OnDisable();
                return;
            }

            if (isRoot) {
                Transform.ChildStateTree.Regenerate();
            }
            GameEngine.RemoveGameObject(this);
            for (int i = 0; i < Components.Count; i++) {
                Components.Array[i].Disable();
            }
            Enabled = false;
            for (int i = 0; i < Transform.Children.Count; i++) {
                Transform.Children[i].GameObject?.OnDisableInternal(false);
            }

            OnDisable(isRoot);
        }

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
            for (int i = 0; i < Transform.Children.Count; i++) {
                cList.AddRange(Transform.Children[i].GameObject.GetAllComponents<T>());
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
            for (int i = 0; i < Transform.Children.Count; i++) {
                Transform.Children[i].GameObject.GetAllComponents(existingList);
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
                if (component is IPartitionObject pObj) {
                    PartitionObjects.Add(pObj);
                    ResetPartition();
                }
                if (doInitialize && ExecuteIsValid(component)) {
                    component.Initialize();
                }
            }
            if (component.Serialized && !SerializedComponents.Contains(component)) {
                SerializedComponents.Add(component);
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
            if (component is IPartitionObject pObj) {
                SpatialPartitionManager.Remove(pObj);
                PartitionObjects.Remove(pObj);
            }
            ((IAssetConsumer)component).DereferenceAssets();
            Components.Remove(component);
            SerializedComponents.Remove(component);

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

        /// <summary>
        /// Called after the object is destroyed. Unmanaged resources and events should be dealt with here.
        /// </summary>
        protected virtual void Dispose()
        {
            // Unregister events here when I add them.
        }

        public void InsertedIntoPartition(PartitionTile tile)
        {
            IPartitionObject[] array = PartitionObjects.Array;
            for (int i = 0; i < PartitionObjects.Count; i++) {
                SpatialPartitionManager.Insert(array[i]);
            }
        }

        public void RemovedFromPartition(PartitionTile tile)
        {
            IPartitionObject[] array = PartitionObjects.Array;
            for (int i = 0; i < PartitionObjects.Count; i++) {
                SpatialPartitionManager.Remove(array[i]);
            }
        }

        internal void ResetPartition()
        {
            SpatialPartitionManager.Remove(this);
            SpatialPartitionManager.Insert(this);
        }

        protected internal void SortComponents() 
            => Components.Sort(new ComponentQueueComparer());

        public bool ExecuteIsValid(Component component) 
            => !GameEngine.IsEditor || GameEngine.IsEditor && Reflection.GetComponentInfo(component.GetType()).RunInEditor;

        /// <summary>
        /// Creates a new GameObject instance.
        /// </summary>
        /// <param name="pos">Position.</param>
        /// <param name="rot">Rotation.</param>
        /// <param name="scale">Scale.</param>
        public GameObject(Vector2 pos, float rot, Vector2 scale)
        {
            // Skip this constructor if the GameObject is a UIObject.
            if (!(this is UIObject)) {
                Transform = new Transform(pos, scale, rot, this);
                GameEngine.GameObjectConstructorCallback(this);
            }
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

        public string SerializeJson() 
            => Serialize().Serialize(); // wtf?

        public GameObjectData Serialize()
        {
            GameObjectData data = new GameObjectData {
                EngineName = EngineName,
                Type = GetType(),
                Position = Transform.Position,
                Rotation = Transform.Rotation,
                Scale = Transform.Scale,
                componentData = new List<ComponentData>()
            };
            for (int i = 0; i < SerializedComponents.Count; i++) {
                Component component = SerializedComponents.Array[i];
                ComponentData componentData = component.Serialize();
                componentData.ComponentIndex = (ulong) i;
                data.componentData.Add(componentData);
            }
            return data;
        }

        public void Deserialize(string jsonData) 
            => Deserialize(jsonData.Deserialize<GameObjectData>());

        public void Deserialize(GameObjectData data)
        {
            // Deserialize primary data...
            EngineName = data.EngineName;
            Transform.Position = data.Position;
            Transform.Rotation = data.Rotation;
            Transform.Scale = data.Scale;

            // Deserialize any valid components...
            foreach (ComponentData serializedComponent in data.componentData) {
                for (int i = 0; i < SerializedComponents.Count; i++) {
                    if(serializedComponent.ComponentIndex != (ulong) i)
                        continue;

                    Component component = SerializedComponents.Array[i];
                    component.Deserialize(serializedComponent);
                }
            }
        }

        protected virtual object SerializeAdditionalData() => null;
        protected virtual void DeserializeAdditionalData(object data) { }

        private struct ComponentQueueComparer : IComparer<Component>
        {
            int IComparer<Component>.Compare(Component x, Component y) => y.Queue.CompareTo(x.Queue);
        }
    }
}