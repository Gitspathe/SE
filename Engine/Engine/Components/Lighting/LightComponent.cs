﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Attributes;
using SE.Common;
using SE.Lighting;
using Vector2 = System.Numerics.Vector2;
using PenumbraLight = Penumbra.Light;

namespace SE.Components.Lighting
{
    // TODO: DRUNK: Fix bug relating to SpatialPartition<T>, where performance is low when the origin point (0,0) is on-screen.
    // TODO: Seems to be caused by Components Enabled state automatically being set to true.
    // TODO: FIXED THIS WITH (ENABLED) ON INITIALIZE();
    [ExecuteInEditor]
    public class LightComponent : Component
    {
        internal PenumbraLight PenumbraLight => Light?.PenumbraLight;

        private Light light;
        public Light Light { 
            get => light;
            set {
                if (light != null) {
                    Core.Lighting.RemoveLight(light);
                }
                light = value;
                if(Enabled) 
                    Core.Lighting.AddLight(light);
            }
        }
        
        public LightType LightType {
            get => light.LightType;
            set {
                if(value == LightType)
                    return;

                Light = new Light(value, Texture) {
                    Position = Position,
                    Offset = Offset,
                    Size = Size,
                    Rotation = Rotation,
                    CastsShadows = CastsShadows,
                    Color = Color,
                    Intensity = Intensity,
                    ConeDecay = ConeDecay,
                    Texture = Texture
                };
            }
        }

        public Vector2 Position {
            get => Light.Position;
            set => Light.Position = value;
        }

        public Vector2 Offset {
            get => Light.Offset;
            set => Light.Offset = value;
        }

        public Vector2 Size {
            get => Light.Size;
            set => Light.Size = value;
        }

        public float Rotation {
            get => Light.Rotation;
            set => Light.Rotation = value;
        }

        public bool CastsShadows
        {
            get => Light.CastsShadows;
            set => Light.CastsShadows = value;
        }

        public Color Color
        {
            get => Light.Color;
            set => Light.Color = value;
        }

        public float Intensity
        {
            get => Light.Intensity;
            set => Light.Intensity = value;
        }

        public float ConeDecay {
            get => light.ConeDecay;
            set => light.ConeDecay = value;
        }

        public Texture2D Texture {
            get => light.Texture;
            set => light.Texture = value;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if(Light == null)
                return;

            Light.Position = Owner.Transform.GlobalPositionInternal;
            Light.Rotation = Owner.Transform.GlobalRotationInternal;
        }

        public LightComponent(Vector2 size, Color color, float intensity = 1.0f, LightType lightType = LightType.Point)
        {
            Light = new Light(lightType) {
                Size = size
            };
            PenumbraLight.Color = color;
        }

        public LightComponent(Vector2 size, Vector2 offset, Color color, float intensity = 1.0f, LightType lightType = LightType.Point)
        {
            Light = new Light(lightType) {
                Size = size,
                Offset = offset
            };
            PenumbraLight.Color = color;
        }

        public LightComponent()
        {
            Light = new Light {
                Size = new Vector2(200, 200)
            };
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Light == null)
                return;

            if(Enabled)
                Core.Lighting.AddLight(Light);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Light == null)
                return;

            Core.Lighting.RemoveLight(Light);
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (Light == null)
                return;

            Core.Lighting.RemoveLight(Light);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Light == null)
                return;

            Core.Lighting.AddLight(Light);
        }

    }

}
