using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using Vector2 = System.Numerics.Vector2;

namespace SE.Lighting
{
    public class Light
    {
        public Vector2 Position;
        public Vector2 Offset;
        public Vector2 Size;
        public float Rotation;

        internal Penumbra.Light PenumbraLight;
        internal bool AddedToLighting = false;

        public LightType LightType { get; }

        public bool CastsShadows {
            get => PenumbraLight.CastsShadows;
            set => PenumbraLight.CastsShadows = value;
        }

        public Color Color {
            get => PenumbraLight.Color;
            set => PenumbraLight.Color = value;
        }

        public float Intensity {
            get => PenumbraLight.Intensity;
            set => PenumbraLight.Intensity = value;
        }

        /// <summary> Gets or sets the rate of cone attenuation to the sides. A higher value means softer edges.
        ///           Default is 1.5. Only valid for Spotlights, returns -1 if the light isn't a Spotlight.</summary>
        public float ConeDecay {
            get {
                if (PenumbraLight is Spotlight spotlight)
                    return spotlight.ConeDecay;

                return -1;
            }
            set {
                if(PenumbraLight is Spotlight spotlight)
                    spotlight.ConeDecay = value;
            }
        }

        /// <summary>Gets or sets the texture for the light. Only valid for TexturedLights. Returns null
        ///          if the light isn't a TexturedLight, or if the TexturedLight doesn't have a texture.</summary>
        public Texture2D Texture {
            get {
                if (PenumbraLight is TexturedLight texturedLight)
                    return texturedLight.Texture;

                return null;
            }
            set {
                if(PenumbraLight is TexturedLight texturedLight)
                    texturedLight.Texture = value;
            }
        }

        /// <summary>
        /// Creates a new Light instance.
        /// </summary>
        /// <param name="lightType">Type of light to create.</param>
        /// <param name="texture">Texture passed to a TexturedLight, if the lightType is set to Textured.</param>
        public Light(LightType lightType = LightType.Point, Texture2D texture = null)
        {
            switch (lightType) {
                case LightType.Point:
                    PenumbraLight = new PointLight();
                    break;
                case LightType.Spot:
                    PenumbraLight = new Spotlight();
                    break;
                case LightType.Textured:
                    PenumbraLight = new TexturedLight(texture);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lightType), lightType, null);
            }
            LightType = lightType;
        }

        /// <summary>
        /// Creates a new textured Light instance.
        /// </summary>
        /// <param name="texture">Texture passed to the TexturedLight.</param>
        public Light(Texture2D texture)
        {
            PenumbraLight = new TexturedLight(texture);
            LightType = LightType.Textured;
        }

    }

}
