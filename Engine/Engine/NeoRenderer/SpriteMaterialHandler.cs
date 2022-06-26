using System;
using System.Collections.Generic;

namespace SE.NeoRenderer
{
    public static class SpriteMaterialHandler
    {
        private static readonly HashSet<IMaterialObserver> materialObservers = new HashSet<IMaterialObserver>();
        
        private static HashSet<SpriteMaterial> materials;
        private static SpriteMaterialInfo[] materialInfos;

        private static int curMaterialID;
        private static Queue<int> avaliableIDs;

        internal static void Initialize()
        {
            materialInfos = new SpriteMaterialInfo[RenderConfig.MaxMaterialSlots];
            materials = new HashSet<SpriteMaterial>(RenderConfig.MaxMaterialSlots);
            avaliableIDs = new Queue<int>(RenderConfig.MaxMaterialSlots);
            for (int i = 0; i < RenderConfig.MaxMaterialSlots; i++) {
                avaliableIDs.Enqueue(i);
            }
        }

        internal static int GetNextMaterialID()
        {
            if (avaliableIDs.TryDequeue(out int i)) {
                return i;
            }
            throw new Exception("Exceeded material slots!");
        }

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

            int matID = material.MaterialID;
            materialInfos[matID] = new SpriteMaterialInfo(matID, material, SpriteBatchManager.GetBatcher(matID));
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
            avaliableIDs.Enqueue(material.MaterialID);
            materials.Remove(material);
            materialInfos[material.MaterialID] = null;
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
        public int MaterialID;
        public SpriteMaterial Material;
        public SpriteBatcher Batcher;

        public SpriteMaterialInfo(int id, SpriteMaterial material, SpriteBatcher batcher)
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
