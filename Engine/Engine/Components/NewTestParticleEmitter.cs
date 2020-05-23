using System;
using System.Numerics;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Core;
using SE.Engine.Utility;
using SE.Rendering;
using SEParticles;
using SEParticles.Modules;
using SEParticles.Shapes;
using Curve = SE.Utility.Curve;
using CurveKey = SE.Utility.CurveKey;
using Vector4 = System.Numerics.Vector4;
using Vector2 = System.Numerics.Vector2;

namespace SE.Engine.Components
{
    public class NewTestParticleEmitter : Component
    {
        public Texture2D Texture;
        public Rectangle SourceRect;

        internal Emitter Emitter;

        public bool AddedToParticleEngine { get; internal set; } = false;
        public int ParticleEngineIndex { get; internal set; } = -1;

        protected override void OnInitialize()
        {
            Emitter = new Emitter(shape: new CircleShape(64.0f, EmissionDirection.In, true, true));

            Curve angleCurve = new Curve();
            angleCurve.Keys.Add(0.0f, 0.0f);
            angleCurve.Keys.Add(0.25f, 0.1f);
            angleCurve.Keys.Add(0.5f, 1.0f);
            angleCurve.Keys.Add(1.0f, 10.0f);

            Curve forwardVelocityCurve = new Curve();
            forwardVelocityCurve.Keys.Add(0.0f, 0.0f);
            forwardVelocityCurve.Keys.Add(0.20f, 128.0f);
            forwardVelocityCurve.Keys.Add(0.5f, 512.0f);
            forwardVelocityCurve.Keys.Add(1.0f, 3000.0f);

            Curve4 colorCurve = new Curve4();
            colorCurve.Add(0.0f, new Vector4(0.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(0.25f, new Vector4(30.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(0.5f, new Vector4(120.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(0.8f, new Vector4(240.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(1.0f, new Vector4(360.0f, 1.0f, 0.5f, 0.0f));

            //Emitter.AddModule(AngleModule.RandomCurve(angleCurve));
            Emitter.AddModule(ForwardVelocityModule.Constant(64.0f));
            Emitter.AddModule(ScaleModule.Constant(0.5f));

            ColorModule baseColorModule = ColorModule.Lerp(
                new Vector4(0f, 1.0f, 0.5f, 1.0f),
                new Vector4(360f, 1.0f, 0.5f, 0.0f));

            //Emitter.AddModule(baseColorModule);
            Emitter.AddModule(baseColorModule);

            NewParticleEngine.AddEmitter(this);
        }

        protected override void OnEnable()
        {
            NewParticleEngine.AddEmitter(this);
        }

        protected override void OnDestroy()
        {
            NewParticleEngine.RemoveEmitter(this);
        }

        protected override void OnDisable()
        {
            NewParticleEngine.RemoveEmitter(this);
        }

        private float time;
        protected override void OnUpdate()
        {
            Emitter.Position = Owner.Transform.GlobalPositionInternal;

            time -= Time.DeltaTime;
            if (time <= 0.0f) {
                Emitter.Emit(4);
                time = 0.01f;
            }
        }
    }
}
