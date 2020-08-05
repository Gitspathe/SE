using System.Collections.Generic;
using SE.AssetManagement;
using SE.Attributes;
using SE.Core;
using SE.Serialization;
using SE.World.Partitioning;
using SE.Core.Extensions;
using System;
using SE.Core.Internal;
using Microsoft.Xna.Framework;

namespace SE.Common
{
    /// <summary>
    /// Containers for logic which are added to GameObjects.
    /// </summary>
    [ExecuteInEditor]
    public class Component : SEObject, IAssetConsumer, IDisposable
    {
        public bool Serialized => !NeverSerialize && InstantiatedFromAttribute;
        
        /// <summary>If true, Component will be able to be serialized.</summary>
        internal bool InstantiatedFromAttribute = false;

        /// <summary>If true, Component will be serialized into it's owner's data.</summary>
        public virtual bool NeverSerialize => false;
        /// <summary>Internal reflection information based on Component type.</summary>
        internal Reflection.ComponentInfo ReflectionInfo { get; }

        /// <summary>The component's order in which it is processed. Lower = earlier update loop and initialize.</summary>
        public virtual int Queue { get; } = 0;

        public AssetConsumer AssetConsumer { get; } = new AssetConsumer();

        /// <summary>The component's enabled state.</summary>
        [NoSerialize] public bool Enabled {
            get => enabled;
            set {
                if(enabled == value)
                    return;

                enabled = value;
                if (value) {
                    Enable();
                } else {
                    Disable();
                }
            }
        }
        private bool enabled = true;

        /// <summary>True if the component is being destroyed.</summary>
        public bool PendingDestroy { get; private set; }

        /// <summary>True if Initialize was called on the component.</summary>
        public bool Initialized { get; private set; }

        /// <summary>True if Awake has been called on the component.</summary>
        [NoSerialize] public bool AwakeCalled { get; internal set; }

        /// <summary>
        /// GameObject which the component belongs to.
        /// </summary>
        public GameObject Owner {
            get => OwnerProp;
            protected set => OwnerProp = value;
        }

        protected virtual GameObject OwnerProp { get; set; }

        /// <summary>Serializer for the component. Only valid when in the editor.</summary>
        internal EngineSerializerBase Serializer {
            get => serializer;
            private set {
                if (serializer != null || (value == null && serializer != null))
                    serializer?.Dispose();

                serializer = value;
            }
        }
        private EngineSerializerBase serializer;

        public Transform Transform => Owner.Transform;

        private IPartitionObject pObj;

        public Component()
        {
            ReflectionInfo = Reflection.GetComponentInfo(GetType());
            pObj = this as IPartitionObject;
        }

        /// <summary>
        /// Runs before initialize.
        /// </summary>
        internal virtual void OnInitializeInternal() { }

        internal void InitializeInternal(GameObject owner)
        {
            Owner = owner;
            if (enabled) {
                pObj?.InsertIntoPartition();
            }
            if (GameEngine.IsEditor) {
                GenerateSerializer();
            }
            OnInitializeInternal();
        }

        private void GenerateSerializer()
        {
            Serializer = SerializerReflection.GetEngineSerializer(GetType(), this);
            if (Serializer != null) {
                Serializer.Initialize(); // Custom
            } else {
                Serializer = new EngineSerializerDefault(this);
                Serializer.Initialize(); // Default
            }
        }

        protected virtual void OnAwake() { }

        internal void Awake()
        {
            if(AwakeCalled)
                return;

            OnAwake();
            AwakeCalled = true;
        }

        /// <summary>
        /// Initialize component instance.
        /// </summary>
        protected virtual void OnInitialize() { }

        internal void Initialize()
        {
            if (Initialized)
                return;

            OnInitialize();
            Initialized = true;
        }

        internal virtual void OnUpdateInternal() { }
        internal void UpdateInternal() => OnUpdateInternal();

        /// <summary>
        /// Update loop which is called at a fixed rate.
        /// May be called zero, or multiple times, within a single frame.
        /// </summary>
        public virtual void OnFixedUpdate() { }
        internal void FixedUpdate() => OnFixedUpdate();

        /// <summary>
        /// Update loop, this is called every frame.
        /// </summary>
        protected virtual void OnUpdate() { }
        internal void Update() => OnUpdate();

        /// <summary>
        /// Enables the component, allowing the Update loop.
        /// </summary>
        protected virtual void OnEnable() { }

        internal void Enable()
        {
            pObj?.InsertIntoPartition();
            OnEnable();
        }

        /// <summary>
        /// Disables the component, preventing the Update loop.
        /// </summary>
        protected virtual void OnDisable() { }

        internal void Disable()
        {
            pObj?.RemoveFromPartition();
            OnDisable();
        }

        /// <summary>
        /// Destroys the component.
        /// </summary>
        protected virtual void OnDestroy() { }

        public void Destroy()
        {
            PendingDestroy = true;
            pObj?.RemoveFromPartition();
            OnDestroy();
            Dispose(true);
        }

        internal void OwnerBoundsUpdated()
        {
            if (enabled) {
                pObj?.InsertIntoPartition();
            }
        }

        /// <summary>
        /// Force the component to call it's Initialize function again.
        /// </summary>
        public void ForceReinitialize()
        {
            Initialized = false;
            OnInitialize();
        }

        internal ComponentData Serialize()
        {
            ComponentData data = new ComponentData {
                Type = GetType()
            };
            
            // If serializer is null, initialize it.
            if (Serializer == null) {
                GenerateSerializer();
            }
            data.AdditionalData = new EngineSerializerData(Serializer.ValueWrappers);
            
            // Clear the serializer if the engine is not in editor mode.
            if (!GameEngine.IsEditor) {
                Serializer = null;
            }
            return data;
        }

        internal void Deserialize(string jsonData) 
            => Deserialize(jsonData.Deserialize<ComponentData>());

        internal void Deserialize(ComponentData data)
        {
            // If serializer is null, initialize it.
            if (Serializer == null) {
                GenerateSerializer();
            }
            if (data.AdditionalData != null) {
                Serializer.Restore(data.AdditionalData);
            }
            data.Dispose();
            
            // Clear the serializer if the engine is not in editor mode.
            if (!GameEngine.IsEditor) {
                Serializer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing = true)
        {
            Serializer = null;
        }
    }
}
