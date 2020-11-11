using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SE.Rendering
{
    public class MaterialEffect : Effect, IEquatable<MaterialEffect>
    {
        private Texture2D albedoMap;
        private Texture2D roughnessMap;
        private Texture2D mask;
        private Texture2D normalMap;
        private Texture2D metallicMap;
        private Texture2D displacementMap;

        public bool RenderCClockwise = false;
        public bool IsTransparent = false;
        public bool HasShadow = true;
        public bool HasDiffuse;
        public bool HasRoughnessMap;
        public bool HasMask;
        public bool HasNormalMap;
        public bool HasMetallic;
        public bool HasDisplacement;

        private MaterialTypes type = MaterialTypes.Basic;
        public int MaterialTypeNumber;

        public Vector3 DiffuseColor = Color.Gray.ToVector3();

        private float roughness = 0.5f;

        public float Metallic;
        public float EmissiveStrength;

        public float Roughness { 
            get => roughness;
            set => roughness = Math.Max(value, 0.001f);
        }

        public Texture2D AlbedoMap {
            get => albedoMap;
            set {
                if (value == null) 
                    return;

                albedoMap = value;
                HasDiffuse = true;
            }
        }

        public Texture2D RoughnessMap {
            get => roughnessMap;
            set {
                if (value == null) 
                    return;

                roughnessMap = value;
                HasRoughnessMap = true;
            }
        }

        public Texture2D MetallicMap {
            get => metallicMap;
            set {
                if (value == null) 
                    return;

                metallicMap = value;
                HasMetallic = true;
            }
        }

        public Texture2D NormalMap {
            get => normalMap;
            set {
                if (value == null) 
                    return;

                normalMap = value;
                HasNormalMap = true;
            }
        }

        public Texture2D DisplacementMap {
            get => displacementMap;
            set {
                if (value == null) 
                    return;

                displacementMap = value;
                HasDisplacement = true;
            }
        }

        public Texture2D Mask {
            get => mask;
            set {
                if (value == null)
                    return;

                mask = value;
                HasMask = true;
            }
        }

        public MaterialTypes Type {
            get => type;
            set {
                type = value;
                MaterialTypeNumber = (int)value;
            }
        }


        public void Initialize(Color diffuseColor, float roughness, float metalness, Texture2D albedoMap = null, Texture2D normalMap = null, Texture2D roughnessMap = null, Texture2D metallicMap = null, Texture2D mask = null, Texture2D displacementMap = null, MaterialTypes type = MaterialTypes.Basic, float emissiveStrength = 0)
        {
            DiffuseColor = diffuseColor.ToVector3();
            Roughness = roughness;
            Metallic = metalness;

            AlbedoMap = albedoMap;
            NormalMap = normalMap;
            RoughnessMap = roughnessMap;
            MetallicMap = metallicMap;
            DisplacementMap = displacementMap;
            Mask = mask;

            Type = type;

#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif

            if (emissiveStrength > 0)
            {
                //Type = MaterialTypes.Emissive;
                EmissiveStrength = emissiveStrength;
            }
        }

        public MaterialEffect(Effect cloneSource) : base(cloneSource)
        {
#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif
        }

        public MaterialEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
        }

        public MaterialEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
        }


        public bool Equals(MaterialEffect b)
        {
            if (b == null) return false;

            if (HasDiffuse != b.HasDiffuse) return false;

            if (HasRoughnessMap != b.HasRoughnessMap) return false;

            if (IsTransparent != b.IsTransparent) return false;

            if (HasMask != b.HasMask) return false;

            if (HasNormalMap != b.HasNormalMap) return false;

            if (HasShadow != b.HasShadow) return false;

            if (HasDisplacement != b.HasDisplacement) return false;

            if (Vector3.DistanceSquared(DiffuseColor, b.DiffuseColor) > 0.01f) return false;

            if (AlbedoMap != b.AlbedoMap) return false;

            if (Type != b.Type) return false;

            if (Math.Abs(Roughness - b.Roughness) > 0.01f) return false;

            if (Math.Abs(Metallic - b.Metallic) > 0.01f) return false;

            if (AlbedoMap != b.AlbedoMap) return false;

            if (NormalMap != b.NormalMap) return false;

            return true;
        }

        public MaterialEffect Clone()
        {
            return new MaterialEffect(this);
        }

        public enum MaterialTypes
        {
            Basic = 0,
            Emissive = 3,
            Hologram = 1,
            ProjectHologram = 2,
            SubsurfaceScattering = 4,
            ForwardShaded = 5,
        }
    }
}
