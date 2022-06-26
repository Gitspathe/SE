using System;
using System.Collections.Generic;

namespace SE.NeoRenderer
{
    public static class SpriteMaterialHandler
    {
        private static readonly HashSet<IMaterialObserver> materialObservers = new HashSet<IMaterialObserver>();
        
        private static readonly HashSet<SpriteMaterial> materials = new HashSet<SpriteMaterial>();
        private static readonly Dictionary<uint, SpriteMaterialInfo> materialInfos = new Dictionary<uint, SpriteMaterialInfo>();

        // TODO: Dictionary<uint, Material>

        internal static void RegisterMaterialObserver(IMaterialObserver observer)
        {
            materialObservers.Add(observer);
        }

        internal static void UnregisterMaterialObserver(IMaterialObserver observer)
        {
            materialObservers.Remove(observer);
        }

        internal static void MaterialCreated(SpriteMaterial material)
        {
            if (materials.Contains(material))
                throw new Exception();

            materials.Add(material);
            foreach (IMaterialObserver observer in materialObservers) {
                observer.MaterialCreated(material);
            }

            uint matID = material.MaterialID;
            materialInfos.Add(matID, new SpriteMaterialInfo(matID, material, SpriteBatchManager.GetBatcher(matID)));
        }

        // TODO: This is fairly inefficient for simple material property changes. Should look into making a less brute-forcey system.
        internal static void MaterialPropertyChanged(SpriteMaterial material)
        {
            foreach (IMaterialObserver observer in materialObservers) {
                observer.MaterialPropertyChanged(material);
            }
        }

        internal static void MaterialDispose(SpriteMaterial material)
        {
            materials.Remove(material);
            materialInfos.Remove(material.MaterialID);
            foreach (IMaterialObserver observer in materialObservers) {
                observer.MaterialDispose(material);
            }
        }

        internal static SpriteMaterialInfo GetSpriteMaterialInfo(uint materialID)
        {
            return materialInfos[materialID];
        }

        internal static void IncrementMaterialRefCount(SpriteMaterial material)
        {
            material.RefCount++;
        }

        internal static void DecrementMaterialRefCount(SpriteMaterial material)
        {
            material.RefCount--;
        }
    }

    internal class SpriteMaterialInfo
    {
        public uint MaterialID;
        public SpriteMaterial Material;
        public SpriteBatcher Batcher;

        public SpriteMaterialInfo(uint id, SpriteMaterial material, SpriteBatcher batcher)
        {
            MaterialID = id;
            Material = material;
            Batcher = batcher;
        }
    }

    internal interface IMaterialObserver
    {
        void MaterialDispose(SpriteMaterial material);
        void MaterialPropertyChanged(SpriteMaterial material);
        void MaterialCreated(SpriteMaterial material);
    }
}
