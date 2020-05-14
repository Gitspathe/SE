using System.Collections.Generic;
using SE.AssetManagement;
using SE.Attributes;
using SE.Core;
using SE.Serialization;
using SE.World.Partitioning;
using SE.Core.Extensions;

namespace SE.Common
{
    /// <summary>
    /// Containers for logic which are added to GameObjects.
    /// </summary>
    [ExecuteInEditor]
    public class Component : DeeZObject, IAssetConsumer
    {
        public bool Serialized => !NeverSerialize && InstantiatedFromAttribute;

        /// <summary>If true, Component will be serialized into it's owner's data.</summary>
        public virtual bool NeverSerialize => false;

        /// <summary>If true, Component will be able to be serialized.</summary>
        internal bool InstantiatedFromAttribute = false;

        /// <summary>The component's order in which it is processed. Lower = earlier update loop and initialize.</summary>
        public virtual int Queue { get; } = 0;

        HashSet<IAsset> IAssetConsumer.ReferencedAssets { get; set; }

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

        public EngineSerializerBase Serializer { get; private set; }

        /// <summary>
        /// Runs before initialize.
        /// </summary>
        internal virtual void OnInitializeInternal() { }

        internal void InitializeInternal(GameObject owner)
        {
            Owner = owner;
            Serializer = SerializerReflection.GetEngineSerializer(GetType(), this);
            if (Serializer != null) {
                // Custom
                Serializer.Initialize();
            } else {
                // Default
                Serializer = new EngineSerializerDefault(this);
                Serializer.Initialize();
            }

            OnInitializeInternal();
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
            if (this is IPartitionObject pObj) {
                SpatialPartitionManager.Insert(pObj);
            }
            OnEnable();
        }

        /// <summary>
        /// Disables the component, preventing the Update loop.
        /// </summary>
        protected virtual void OnDisable() { }

        internal void Disable()
        {
            if (this is IPartitionObject pObj) {
                SpatialPartitionManager.Remove(pObj);
            }
            OnDisable();
        }

        /// <summary>
        /// Destroys the component.
        /// </summary>
        protected virtual void OnDestroy() { }

        public void Destroy()
        {
            OnDestroy();
        }

        /// <summary>
        /// Force the component to call it's Initialize function again.
        /// </summary>
        public void ForceReinitialize()
        {
            Initialized = false;
            OnInitialize();
        }

        public ComponentData Serialize()
        {
            ComponentData data = new ComponentData {
                Type = GetType()
            };
            if (Serializer != null) {
                data.AdditionalData = new EngineSerializerData(Serializer.ValueWrappers);
            }
            return data;
        }

        public void Deserialize(string jsonData) 
            => Deserialize(jsonData.Deserialize<ComponentData>());

        public void Deserialize(ComponentData data)
        {
            if (data.AdditionalData != null) {
                Serializer.Restore(data.AdditionalData);
            }
        }
    }
}
