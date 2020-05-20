using Microsoft.Xna.Framework;
using SE.Core;
using SE.Engine.Particles.Modules;
using SE.Particles;
using SE.Particles.Modules;
using SE.Physics;
using SE.Rendering;
using Random = SE.Utility.Random;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.Particles
{
    public class TestParticleSystem : ParticleSystem
    {
        public override ParticleRenderType ParticleRenderType => ParticleRenderType.AlphaUnlit;

        private SpriteTexture s;
        private SpriteTexture s2;

        private Curve forwardVelocityCurve;
        private Curve rotationCurve;
        private Curve reverseRotCurve;

        public override Particle GenerateParticle()
        {
            Particle p = base.GenerateParticle();
            if (p != null) {
                Color col = Color.Blue;
                int i = Random.Next(0, 5);
                col = i switch {
                    0 => Color.Blue,
                    1 => Color.Green,
                    2 => Color.Red,
                    3 => Color.Yellow,
                    4 => Color.Purple,
                    5 => Color.Orange,
                    _ => col
                };

                SpriteTexture sprite;
                i = Random.Next(2);
                if (i < 1)
                    sprite = s2;
                else
                    sprite = s;

                p.Sprite = sprite;
                
                p.ForwardVelocity.SetLerp(Random.Next(-400.0f, 400.0f), Random.Next(-400.0f, 400.0f));

                //p.EmitterForwardVelocity.SetCurve(forwardVelocityCurve);

                p.Velocity.SetLerp(new Vector2(Random.Next(-50, 50), Random.Next(-100, 100)), Vector2.Zero);

                if (sprite == s) {
                    p.Scale.SetLerp(new Vector2(0.4f, 0.4f), Vector2.Zero);
                } else {
                    p.Scale.SetLerp(new Vector2(0.25f, 0.25f), Vector2.Zero);
                }

                //p.AngularVelocity.SetLerp(Random.Next(-10, 10), Random.Next(-10, 10));

                p.Color.SetLerp(col, new Color(col, 0.2f));

                int z = Random.Next(2);
                if (z < 1) {
                    p.Rotation.SetCurve(rotationCurve);
                } else {
                    p.Rotation.SetCurve(reverseRotCurve);
                }

                p.ParticleRenderType = ParticleRenderType.AlphaUnlit;

                p.TimeToLive = 1.5f;
                p.LayerDepth = 0.04f;
                p.Initialize();
            }
            return p;
        }

        public TestParticleSystem(bool enabledAtStart = true) : base(enabledAtStart, new Point(2048, 2048), 2048)
        {
            s = AssetManager.Get<SpriteTexture>(this, "circle");
            s2 = AssetManager.Get<SpriteTexture>(this, "floor");
            LightModule lm = new LightModule(this, 32);

            Curve scaleCurveX = new Curve();
            scaleCurveX.Keys.Add(new CurveKey(0, 150f));
            scaleCurveX.Keys.Add(new CurveKey(0.667f, 150f));
            scaleCurveX.Keys.Add(new CurveKey(1, 0));

            Curve scaleCurveY = new Curve();
            scaleCurveY.Keys.Add(new CurveKey(0, 100f));
            scaleCurveY.Keys.Add(new CurveKey(0.667f, 100f));
            scaleCurveY.Keys.Add(new CurveKey(1, 0));

            //lm.Intensity.SetConstant(1.0f);
            lm.Intensity.SetConstant(0.667f);
            lm.Color.SetConstant(Color.White);
            lm.Scale.SetLerp(new Vector2(100, 100), Vector2.Zero);
            //lm.Scale.SetCurve(scaleCurveX, scaleCurveY);
            lm.ScaleWithParticle = false;
            lm.InheritParticleColor = true;
            //AddModule(lm);

            rotationCurve = new Curve();
            rotationCurve.Keys.Add(new CurveKey(0, 0f));
            rotationCurve.Keys.Add(new CurveKey(0.2f, 5.0f));
            rotationCurve.Keys.Add(new CurveKey(0.4f, -5.0f));
            rotationCurve.Keys.Add(new CurveKey(0.6f, 2.5f));
            rotationCurve.Keys.Add(new CurveKey(0.6f, 0.0f));

            reverseRotCurve = new Curve();
            reverseRotCurve.Keys.Add(new CurveKey(0, 0f));
            reverseRotCurve.Keys.Add(new CurveKey(0.2f, -5.0f));
            reverseRotCurve.Keys.Add(new CurveKey(0.4f, 5.0f));
            reverseRotCurve.Keys.Add(new CurveKey(0.5f, -2.5f));
            reverseRotCurve.Keys.Add(new CurveKey(0.6f, 0.0f));

            CollisionModule cm = new CollisionModule(this, new Point(32, 32), new[] {LayerType.MapTile, LayerType.Player}) {
                Stick = true,
                OnCollision = particle => {
                    //particle.Color.SetConstant(Color.Blue);
                    particle.Velocity.Disable();
                    particle.ForwardVelocity.Disable();
                    particle.AngularVelocity.Disable();
                    particle.TimeToLive = 5.0f;
                }
            };
            //AddModule(cm);

            forwardVelocityCurve = new Curve();
            forwardVelocityCurve.Keys.Add(new CurveKey(0, -40f));
            forwardVelocityCurve.Keys.Add(new CurveKey(0.2f, -80f));
            forwardVelocityCurve.Keys.Add(new CurveKey(0.5f, -100f));
            forwardVelocityCurve.Keys.Add(new CurveKey(0.667f, -400f));
        }

    }

}
