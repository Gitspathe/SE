using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Core;
using SE.Rendering;
using SEParticles;
using SEParticles.Processors;

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
            Emitter.AddProcessor(new TestProcessor(
                new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f), 
                new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 0.0f)));

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

        private float time = 0.0f;
        protected override void OnUpdate()
        {
            Emitter.Position = Owner.Transform.GlobalPositionInternal;

            time -= Time.DeltaTime;
            if (time <= 0.0f) {
                Emitter.Emit(5);
                time = 0.01f;
            }

        }
    }
}
