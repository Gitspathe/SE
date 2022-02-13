using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Attributes;
using SE.Common;
using SE.Core;
using SE.Particles;
using SE.Particles.Modules;
using SE.Particles.Shapes;
using Curve = SE.Utility.Curve;
using Vector2 = System.Numerics.Vector2;

namespace SE.Components
{
    [HeadlessMode(HeadlessSupportMode.NoHeadless)]
    public class ParticleSystem : Component
    {
        public Texture2D Texture;
        public Rectangle SourceRect;

        public Emitter Emitter;

        protected override void OnInitialize()
        {
            //Emitter = new Emitter(shape: new CircleEmitterShape(64.0f, EmissionDirection.Out, true, true, 0.5f));

            CircleEmitterShape circleShape = new CircleEmitterShape(32.0f, EmissionDirection.Out, false, false);
            RectangleEmitterShape rectangleShape = new RectangleEmitterShape(
                new Vector2(128.0f, 128.0f),
                EmissionDirection.Out,
                true,
                true);

            Emitter = new Emitter(4096, shape: circleShape);
            //Emitter.Space = Space.Local;

            Curve emission = new Curve();
            emission.Keys.Add(0.0f, 0.0f);
            emission.Keys.Add(0.1f, 1000.0f);
            emission.Keys.Add(0.667f, 500.0f);
            emission.Keys.Add(1.0f, 0.0f);

            Emitter.Config.Emission.SetConstant(4096);
            Emitter.Config.Emission.Loop = true;
            Emitter.Config.Texture.SetSheet(Texture, 5, 5);
            Emitter.Config.Color.SetRandomBetween(
                new ParticleColor(0, 100, 25, 1),
                new ParticleColor(360, 100, 50, 1));

            Emitter.Config.Scale.SetRandomBetween(0.0333f, 0.0667f);
            Emitter.Config.Life.SetRandomBetween(0.2f, 1.0f);
            Emitter.Config.Speed.SetRandomBetween(32.0f, 128.0f);

            Curve alphaCurve = new Curve();
            alphaCurve.Keys.Add(0.0f, 0.0f);
            alphaCurve.Keys.Add(0.1f, 1.0f);
            alphaCurve.Keys.Add(0.667f, 1.0f);
            alphaCurve.Keys.Add(1.0f, 0.0f);

            //Emitter.AddModule(HueModule.RandomLerp(0, 360.0f));
            //Emitter.AddModule(LightnessModule.Lerp(0.0f));
            Emitter.AddModule(ScaleModule.Lerp(0.5f, 2.0f));
            //Emitter.AddModule(AlphaModule.Curve(alphaCurve));
            Emitter.AddModule(SpriteRotationModule.RandomConstant(2.0f, 10.0f));
            Emitter.AddModule(TextureAnimationModule.OverLifetime(5, 5));

            Emitter.Play();
        }

        protected override void OnEnable()
        {
            Emitter.Enabled = true;
        }

        protected override void OnDestroy()
        {
            Emitter?.DisposeAfter();
            Emitter = null; // Fix weird memory leak.
        }

        protected override void OnDisable()
        {
            if (!PendingDestroy)
                Emitter.Enabled = false;
        }

        protected override void OnUpdate()
        {
            Emitter.Position = Owner.Transform.GlobalPositionInternal;
            Emitter.Rotation += MathHelper.TwoPi * Time.DeltaTime;
        }

        protected override void Dispose(bool disposing = true)
        {
            base.Dispose(disposing);
            if (!PendingDestroy)
                Emitter?.Dispose();
        }
    }
}
