using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Core;
using SE.Rendering;
using SEParticles;
using SEParticles.Modules;
using Curve = SE.Utility.Curve;

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
            Emitter = new Emitter();
            Emitter.AddModule(new ColorModule(
                new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f), 
                new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 0.0f)));

            Curve angleCurve = new Curve();
            angleCurve.Keys.Add(new Utility.CurveKey(0.0f, 0.0f));
            angleCurve.Keys.Add(new Utility.CurveKey(0.25f, 0.1f));
            angleCurve.Keys.Add(new Utility.CurveKey(0.5f, 1.0f));
            angleCurve.Keys.Add(new Utility.CurveKey(1.0f, 10.0f));

            Curve forwardVelocityCurve = new Curve();
            forwardVelocityCurve.Keys.Add(new Utility.CurveKey(0.0f, 0.0f));
            forwardVelocityCurve.Keys.Add(new Utility.CurveKey(0.20f, 128.0f));
            forwardVelocityCurve.Keys.Add(new Utility.CurveKey(0.5f, 512.0f));
            forwardVelocityCurve.Keys.Add(new Utility.CurveKey(1.0f, 3000.0f));

            Emitter.AddModule(AngleModule.RandomCurve(angleCurve));
            Emitter.AddModule(new ForwardVelocityModule(forwardVelocityCurve));

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
