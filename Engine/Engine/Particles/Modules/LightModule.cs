using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Lighting;
using Vector2 = System.Numerics.Vector2;

namespace SE.Particles.Modules
{

    /// <summary>
    /// Allows particles to have lights attached. Heavy performance penalty, should be used sparingly.
    /// </summary>
    public class LightModule : ParticleModule
    {
        public bool ScaleWithParticle;
        public bool InheritParticleColor;

        private int maxLights;
        private List<ParticleLight> lightPool;
        private List<ParticleLight> activeLights;
        private Dictionary<Particle, ParticleLight> lookupDictionary = new Dictionary<Particle, ParticleLight>();

        private IntensityInfo intensity;
        private ColorInfo color;
        private ScaleInfo scale;

        public IntensityInfo Intensity => intensity ??= new IntensityInfo();
        public ColorInfo Color => color ??= new ColorInfo();
        public ScaleInfo Scale => scale ??= new ScaleInfo();

        public class IntensityInfo
        {
            public ParticleTransitionType Transition = ParticleTransitionType.Constant;
            public float Value;
            public float Start;
            public float End;
            public Curve Curve;

            public void SetConstant(float val) 
            {
                Start = val;
                Transition = ParticleTransitionType.Constant;
            }

            public void SetLerp(float start, float end) 
            {
                Start = start;
                End = end;
                Transition = ParticleTransitionType.Lerp;
            }

            public void SetCurve(Curve curve)
            {
                Curve = curve;
                Transition = ParticleTransitionType.Curve;
            }

            public void Disable() 
            {
                Transition = ParticleTransitionType.None;
            }
        }

        public class ColorInfo
        {
            public ParticleTransitionType Transition = ParticleTransitionType.Constant;
            public Color Value;
            public Color Start;
            public Color End;
            public Curve CurveR, CurveB, CurveG, CurveA;

            public void SetConstant(Color val)
            {
                Start = val;
                Transition = ParticleTransitionType.Constant;
            }

            public void SetLerp(Color start, Color end)
            {
                Start = start;
                End = end;
                Transition = ParticleTransitionType.Lerp;
            }

            public void SetCurve(Curve curveR, Curve curveB, Curve curveG, Curve curveA)
            {
                CurveR = curveR;
                CurveG = curveG;
                CurveB = curveB;
                CurveA = curveA;
                Transition = ParticleTransitionType.Curve;
            }

            public void Disable()
            {
                Transition = ParticleTransitionType.None;
            }
        }

        public class ScaleInfo
        {
            public ParticleTransitionType Transition = ParticleTransitionType.Constant;
            public Vector2 Value;
            public Vector2 Start;
            public Vector2 End;
            public Curve CurveX, CurveY;

            public void SetConstant(Vector2 val)
            {
                Start = val;
                Transition = ParticleTransitionType.Constant;
            }

            public void SetLerp(Vector2 start, Vector2 end)
            {
                Start = start;
                End = end;
                Transition = ParticleTransitionType.Lerp;
            }

            public void SetCurve(Curve curveX, Curve curveY)
            {
                CurveX = curveX;
                CurveY = curveY;
                Transition = ParticleTransitionType.Curve;
            }

            public void Disable()
            {
                Transition = ParticleTransitionType.None;
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            if(intensity != null)
                intensity.Value = intensity.Start;
            if(scale != null)
                scale.Value = scale.Start;
            if(color != null)
                color.Value = color.Start;

            for (int i = 0; i < maxLights; i++) {
                lightPool.Add(new ParticleLight(new Light()));
            }
        }

        public override void ParticleHook(Particle particle)
        {
            base.ParticleHook(particle);
            if (lightPool.Count < 1)
                return;

            ParticleLight particleLight = lightPool[lightPool.Count-1];
            lightPool.RemoveAt(lightPool.Count-1);
            activeLights.Add(particleLight);
            Core.Lighting.AddLight(particleLight.Light);

            particleLight.Particle = particle;
            lookupDictionary.Add(particle, particleLight);
        }

        public override void ParticleUnhook(Particle particle)
        {
            base.ParticleUnhook(particle);
            lookupDictionary.TryGetValue(particle, out ParticleLight particleLight);
            if (particleLight == null)
                return;

            lightPool.Add(particleLight);
            activeLights.Remove(particleLight);
            Core.Lighting.RemoveLight(particleLight.Light);
            lookupDictionary.Remove(particle);
        }

        public override void UpdateThreaded(float deltaTime)
        {
            base.UpdateThreaded(deltaTime);
            for (int i = 0; i < activeLights.Count; i++) {
                Particle particle = activeLights[i].Particle;
                Light pLight = activeLights[i].Light;

                float timeRatio = particle.Time / particle.TimeToLive;
                if (intensity != null) {
                    switch (intensity.Transition) {
                        case ParticleTransitionType.None:
                            intensity = null;
                            break;
                        case ParticleTransitionType.Constant:
                            intensity.Value = intensity.Start;
                            break;
                        case ParticleTransitionType.Lerp:
                            intensity.Value = MathHelper.Lerp(intensity.Start, intensity.End, timeRatio);
                            break;
                        case ParticleTransitionType.Curve:
                            intensity.Value = intensity.Curve.Evaluate(timeRatio);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    pLight.PenumbraLight.Intensity = intensity.Value;
                }
                if (color != null) {
                    switch (color.Transition) {
                        case ParticleTransitionType.None:
                            color = null;
                            break;
                        case ParticleTransitionType.Constant:
                            color.Value = color.Start;
                            break;
                        case ParticleTransitionType.Lerp:
                            color.Value = Microsoft.Xna.Framework.Color.Lerp(color.Start, color.End, timeRatio);
                            break;
                        case ParticleTransitionType.Curve:
                            color.Value = new Color(color.CurveR.Evaluate(timeRatio), color.CurveG.Evaluate(timeRatio),
                                color.CurveB.Evaluate(timeRatio), color.CurveA.Evaluate(timeRatio));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (intensity != null && InheritParticleColor) {
                        color.Value = particle.CurrentColor;
                        pLight.PenumbraLight.Intensity = (float)particle.CurrentColor.A / 256;
                    }
                    pLight.PenumbraLight.Color = color.Value;
                }
                if (scale != null) {
                    switch (scale.Transition) {
                        case ParticleTransitionType.None:
                            scale = null;
                            break;
                        case ParticleTransitionType.Constant:
                            scale.Value = scale.Start;
                            break;
                        case ParticleTransitionType.Lerp:
                            scale.Value = Vector2.Lerp(scale.Start, scale.End, timeRatio);
                            break;
                        case ParticleTransitionType.Curve:
                            scale.Value.X = scale.CurveX.Evaluate(timeRatio);
                            scale.Value.Y = scale.CurveY.Evaluate(timeRatio);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (ScaleWithParticle) {
                        scale.Value = scale.Start * particle.CurrentScale;
                    }
                    pLight.Size = scale.Value;
                }

                pLight.Position = particle.GlobalPosition;
            }
        }

        /// <summary>
        /// Creates a new LightModule.
        /// </summary>
        /// <param name="maxLights">Maximum amount of lights the ParticleSystem will support.</param>
        public LightModule(ParticleSystem particleSystem, int maxLights = 32) : base(particleSystem)
        {
            this.maxLights = maxLights;
            lightPool = new List<ParticleLight>(maxLights);
            activeLights = new List<ParticleLight>(maxLights);
        }

        /// <summary>
        /// Particle+light container class.
        /// </summary>
        public class ParticleLight
        {
            public Light Light;
            public Particle Particle;

            public ParticleLight(Light pLight)
            {
                Light = pLight;
            }
        }

    }

}
