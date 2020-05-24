using SE.Attributes;
using SE.Common;
using SE.Core;
using SE.Particles;

namespace SE.Components
{
    [ExecuteInEditor]
    public class ParticleEmitter : Component
    {
        private Particles.ParticleSystem particleSystem;
        public Particles.ParticleSystem ParticleSystem {
            get => particleSystem;
            set {
                if (particleSystem != null) {
                    particleSystem.Enabled = false;
                    particleSystem.Dispose();
                }

                particleSystem = value;
                particleSystem.Emitter = this;
            }
        }

        /// <summary>If true, emitted particles will linger after this component is disabled.
        ///          Otherwise, all particles are disabled.</summary>
        public bool KeepParticlesOnDisable = true;

        private float timer = 0.05f;

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if(particleSystem == null)
                return;

            particleSystem.Rotation = Owner.Transform.GlobalRotationInternal;
            particleSystem.Position = Owner.Transform.GlobalPositionInternal;
            timer -= Time.DeltaTime;
            if (timer <= 0.0f) {
                particleSystem.Emit(10);
                timer = 0.05f;
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (particleSystem == null)
                return;

            particleSystem.Enabled = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (particleSystem == null)
                return;

            particleSystem.Enabled = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (particleSystem == null || KeepParticlesOnDisable)
                return;

            particleSystem.Enabled = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (particleSystem == null)
                return;

            particleSystem.Enabled = false;
            particleSystem.Dispose();
            particleSystem = null;
        }

        public ParticleEmitter(Particles.ParticleSystem particleSystem)
        {
            ParticleSystem = particleSystem;
        }

        public ParticleEmitter() { }

    }

}
