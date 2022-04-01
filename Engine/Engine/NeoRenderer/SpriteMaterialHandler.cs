using System;
using System.Collections.Generic;
using System.Text;

namespace SE.NeoRenderer
{
    public static class SpriteMaterialHandler
    {
        private static HashSet<IMaterialObserver> materialObservers = new HashSet<IMaterialObserver>();
        private static HashSet<SpriteMaterial> materials = new HashSet<SpriteMaterial>();

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
            foreach (IMaterialObserver observer in materialObservers) {
                observer.MaterialDispose(material);
            }
        }

        internal static void IncrementMaterialRefCount(SpriteMaterial material)
        {
            material.RefCount += 20;
        }

        internal static void DecrementMaterialRefCount(SpriteMaterial material)
        {
            material.RefCount -= 20;
        }
    }

    internal interface IMaterialObserver
    {
        void MaterialDispose(SpriteMaterial material);
        void MaterialPropertyChanged(SpriteMaterial material);
        void MaterialCreated(SpriteMaterial material);
    }
}
