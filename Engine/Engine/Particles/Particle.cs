using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using SE.Rendering;
using SE.Core.Extensions;
using Vector2 = System.Numerics.Vector2;
using MonoGameColor = Microsoft.Xna.Framework.Color;

namespace SE.Particles
{
    /// <summary>
    /// Represents a single particle. Contains various public fields which control how the particle is rendered and updated.
    /// </summary>
    public class Particle
    {
        public Vector2 EmitterPosition;
        public float EmitterRotation;
        public SpriteTexture Sprite;
        public Vector2 GlobalPosition;
        public Vector2 LocalPosition;
        public Vector2 CurrentVelocity;
        public Vector2 AccelerationVector;
        public Vector2 CurrentScale = Vector2.One;
        public float CurrentRotation;
        public float LayerDepth;
        public MonoGameColor CurrentColor;
        public Effect Effect;
        public float TimeToLive;
        public float Time;
        public Rectangle sourceRect;

        internal ParticleSystem ParticleSystem;
        internal bool InParticleEngine;
        internal bool InPool;
        internal int DrawCall;
        internal int CurList;
        internal Texture2D Texture;

        private ForwardVelocityInfo forwardVelocity;
        private EmitterForwardVelocityInfo emitterForwardVelocity;
        private ForwardAccelerationInfo forwardAcceleration;
        private EmitterForwardAccelerationInfo emitterForwardAcceleration;
        private VelocityInfo velocity;
        private AccelerationInfo acceleration;
        private AngularVelocityInfo angularVelocity;
        private RotationInfo rotation;
        private ScaleInfo scale;
        private ColorInfo color;
        private bool doAccelerationCalculations;
        public Vector2 Origin;

        private bool enabled;
        /// <summary>Is the particle active?</summary>
        public bool Enabled {
            get => enabled;
            set {
                enabled = value;
                if (!enabled) {
                    ParticleEngine.PendingDisable.Add(this);
                } else {
                    ParticleEngine.AddParticle(this);
                }
            }
        }

        private ParticleRenderType particleRenderType;
        public ParticleRenderType ParticleRenderType {
            get => particleRenderType;
            set {
                if(value == particleRenderType)
                    return;

                if (InParticleEngine) {
                    ParticleEngine.RemoveParticle(this);
                    particleRenderType = value;
                    ParticleEngine.AddParticle(this);
                } else {
                    particleRenderType = value;
                }
            }
        }

        public ForwardVelocityInfo ForwardVelocity => forwardVelocity ??= new ForwardVelocityInfo();
        public EmitterForwardVelocityInfo EmitterForwardVelocity => emitterForwardVelocity ??= new EmitterForwardVelocityInfo();
        public ForwardAccelerationInfo ForwardAcceleration => forwardAcceleration ??= new ForwardAccelerationInfo();
        public EmitterForwardAccelerationInfo EmitterForwardAcceleration => emitterForwardAcceleration ??= new EmitterForwardAccelerationInfo();
        public VelocityInfo Velocity => velocity ??= new VelocityInfo();
        public AccelerationInfo Acceleration => acceleration ??= new AccelerationInfo();
        public AngularVelocityInfo AngularVelocity => angularVelocity ??= new AngularVelocityInfo();
        public RotationInfo Rotation => rotation ??= new RotationInfo();
        public ScaleInfo Scale => scale ??= new ScaleInfo();
        public ColorInfo Color => color ??= new ColorInfo();

        public class ForwardVelocityInfo
        {
            internal ParticleTransitionType Transition;
            internal float Value, Start, End;
            internal Vector2 DirectionVector;
            internal Curve Curve;

            public void SetConstant(float val) {
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

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        public class EmitterForwardVelocityInfo
        {
            internal ParticleTransitionType Transition;
            internal float Value, Start, End;
            internal Vector2 DirectionVector;
            internal Curve Curve;

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

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        public class ForwardAccelerationInfo
        {
            internal ParticleTransitionType Transition;
            internal float Value, Start, End;
            internal Vector2 DirectionVector;
            internal Curve Curve;

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

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        public class EmitterForwardAccelerationInfo
        {
            internal ParticleTransitionType Transition;
            internal float Value, Start, End;
            internal Vector2 DirectionVector;
            internal Curve Curve;

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

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        public class VelocityInfo
        {
            internal ParticleTransitionType Transition;
            internal Vector2 Start, End;
            internal Curve CurveX, CurveY;

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

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        public class AccelerationInfo
        {
            internal ParticleTransitionType Transition;
            internal Vector2 Value, Start, End;
            internal Curve CurveX, CurveY;

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

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        public class AngularVelocityInfo
        {
            internal ParticleTransitionType Transition;
            internal float Value, Start, End;
            internal Curve Curve;

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

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        public class RotationInfo
        {
            internal ParticleTransitionType Transition;
            internal float Value, Start, End;
            internal Curve Curve;

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

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        public class ScaleInfo
        {
            internal ParticleTransitionType Transition;
            internal Vector2 Start, End;
            internal Curve CurveX, CurveY;

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

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        public class ColorInfo
        {
            internal ParticleTransitionType Transition;
            internal MonoGameColor Start, End;
            internal Curve CurveR, CurveB, CurveG, CurveA;

            public void SetConstant(MonoGameColor val)
            {
                Start = val;
                Transition = ParticleTransitionType.Constant;
            }

            public void SetLerp(MonoGameColor start, MonoGameColor end)
            {
                Start = start;
                End = end;
                Transition = ParticleTransitionType.Lerp;
            }

            public void SetCurve(Curve curveR, Curve curveB, Curve curveG, Curve curveA)
            {
                CurveR = curveR;
                CurveB = curveB;
                CurveG = curveG;
                CurveA = curveA;
                Transition = ParticleTransitionType.Curve;
            }

            public void Disable() => Transition = ParticleTransitionType.None;
        }

        // Contains duplicate code for that extra 0.1% FPS!!
        internal void Update(float deltaTime)
        {
            Time += deltaTime;
            if (Time > TimeToLive) {
                Enabled = false;
                return;
            }

            doAccelerationCalculations = false;
            float timeRatio = Time / TimeToLive;
            switch (velocity.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    CurrentVelocity = velocity.Start;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    CurrentVelocity = Vector2.Lerp(velocity.Start, velocity.End, timeRatio);
                    break;
                }
                case ParticleTransitionType.Curve: {
                    CurrentVelocity.X = velocity.CurveX.Evaluate(timeRatio);
                    CurrentVelocity.Y = velocity.CurveY.Evaluate(timeRatio);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (acceleration.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    acceleration.Value = acceleration.Start;
                    AccelerationVector += acceleration.Value * deltaTime;
                    doAccelerationCalculations = true;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    acceleration.Value = Vector2.Lerp(acceleration.Start,  acceleration.End, timeRatio);
                    AccelerationVector += acceleration.Value * deltaTime;
                    doAccelerationCalculations = true;
                    break;
                }
                case ParticleTransitionType.Curve: {
                    acceleration.Value.X = acceleration.CurveX.Evaluate(timeRatio);
                    acceleration.Value.Y = acceleration.CurveY.Evaluate(timeRatio);
                    AccelerationVector += acceleration.Value * deltaTime;
                    doAccelerationCalculations = true;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (angularVelocity.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    angularVelocity.Value = angularVelocity.Start;
                    CurrentRotation += angularVelocity.Value * deltaTime;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    angularVelocity.Value = MathHelper.Lerp(angularVelocity.Start, angularVelocity.End, timeRatio);
                    CurrentRotation += angularVelocity.Value * deltaTime;
                    break;
                }
                case ParticleTransitionType.Curve: {
                    angularVelocity.Value = angularVelocity.Curve.Evaluate(timeRatio);
                    CurrentRotation += angularVelocity.Value * deltaTime;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (rotation.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    rotation.Value = rotation.Start; 
                    CurrentRotation = rotation.Value;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    rotation.Value = MathHelper.Lerp(rotation.Start, rotation.End, timeRatio);
                    CurrentRotation = rotation.Value;
                    break;
                }
                case ParticleTransitionType.Curve: {
                    rotation.Value = rotation.Curve.Evaluate(timeRatio);
                    CurrentRotation = rotation.Value;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (forwardVelocity.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    forwardVelocity.Value = forwardVelocity.Start;
                    CurrentRotation.ToDirectionVector(out forwardVelocity.DirectionVector);
                    CurrentVelocity += forwardVelocity.DirectionVector * forwardVelocity.Value;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    forwardVelocity.Value = MathHelper.Lerp(forwardVelocity.Start, forwardVelocity.End, timeRatio);
                    CurrentRotation.ToDirectionVector(out forwardVelocity.DirectionVector);
                    CurrentVelocity += forwardVelocity.DirectionVector * forwardVelocity.Value;
                    break;
                }
                case ParticleTransitionType.Curve: {
                    forwardVelocity.Value = forwardVelocity.Curve.Evaluate(timeRatio);
                    CurrentRotation.ToDirectionVector(out forwardVelocity.DirectionVector);
                    CurrentVelocity += forwardVelocity.DirectionVector * forwardVelocity.Value;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (emitterForwardVelocity.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    emitterForwardVelocity.Value = emitterForwardVelocity.Start;
                    EmitterRotation.ToDirectionVector(out emitterForwardVelocity.DirectionVector);
                    CurrentVelocity += emitterForwardVelocity.DirectionVector * emitterForwardVelocity.Value;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    emitterForwardVelocity.Value = MathHelper.Lerp(emitterForwardVelocity.Start, emitterForwardVelocity.End, timeRatio);
                    EmitterRotation.ToDirectionVector(out emitterForwardVelocity.DirectionVector);
                    CurrentVelocity += emitterForwardVelocity.DirectionVector * emitterForwardVelocity.Value;
                    break;
                }
                case ParticleTransitionType.Curve: {
                    emitterForwardVelocity.Value = emitterForwardVelocity.Curve.Evaluate(timeRatio);
                    EmitterRotation.ToDirectionVector(out emitterForwardVelocity.DirectionVector);
                    CurrentVelocity += emitterForwardVelocity.DirectionVector * emitterForwardVelocity.Value;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (forwardAcceleration.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    forwardAcceleration.Value = forwardAcceleration.Start;
                    CurrentRotation.ToDirectionVector(out forwardAcceleration.DirectionVector);
                    AccelerationVector += forwardAcceleration.DirectionVector * (forwardAcceleration.Value * deltaTime);
                    doAccelerationCalculations = true;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    forwardAcceleration.Value = MathHelper.Lerp(forwardAcceleration.Start, forwardAcceleration.End, timeRatio);
                    CurrentRotation.ToDirectionVector(out forwardAcceleration.DirectionVector);
                    AccelerationVector += forwardAcceleration.DirectionVector * (forwardAcceleration.Value * deltaTime);
                    doAccelerationCalculations = true;
                    break;
                }
                case ParticleTransitionType.Curve: {
                    forwardAcceleration.Value = forwardAcceleration.Curve.Evaluate(timeRatio);
                    CurrentRotation.ToDirectionVector(out forwardAcceleration.DirectionVector);
                    AccelerationVector += forwardAcceleration.DirectionVector * (forwardAcceleration.Value * deltaTime);
                    doAccelerationCalculations = true;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (emitterForwardAcceleration.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    emitterForwardAcceleration.Value = emitterForwardAcceleration.Start;
                    EmitterRotation.ToDirectionVector(out emitterForwardAcceleration.DirectionVector);
                    AccelerationVector += emitterForwardAcceleration.DirectionVector * emitterForwardAcceleration.Value * deltaTime;
                    doAccelerationCalculations = true;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    emitterForwardAcceleration.Value = MathHelper.Lerp(emitterForwardAcceleration.Start, emitterForwardAcceleration.End, timeRatio);
                    EmitterRotation.ToDirectionVector(out emitterForwardAcceleration.DirectionVector);
                    AccelerationVector += emitterForwardAcceleration.DirectionVector * emitterForwardAcceleration.Value * deltaTime;
                    doAccelerationCalculations = true;
                    break;
                }
                case ParticleTransitionType.Curve: {
                    emitterForwardAcceleration.Value = emitterForwardAcceleration.Curve.Evaluate(timeRatio);
                    EmitterRotation.ToDirectionVector(out emitterForwardAcceleration.DirectionVector);
                    AccelerationVector += emitterForwardAcceleration.DirectionVector * emitterForwardAcceleration.Value * deltaTime;
                    doAccelerationCalculations = true;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (scale.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    CurrentScale = scale.Start;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    CurrentScale = Vector2.Lerp(scale.Start, scale.End, timeRatio);
                    break;
                }
                case ParticleTransitionType.Curve: {
                    CurrentScale.X = scale.CurveX.Evaluate(timeRatio);
                    CurrentScale.Y = scale.CurveY.Evaluate(timeRatio);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (color.Transition) {
                case ParticleTransitionType.None: {
                    break;
                }
                case ParticleTransitionType.Constant: {
                    CurrentColor = color.Start;
                    break;
                }
                case ParticleTransitionType.Lerp: {
                    CurrentColor = MonoGameColor.Lerp(color.Start, color.End, timeRatio);
                    break;
                }
                case ParticleTransitionType.Curve: {
                    CurrentColor = new MonoGameColor(color.CurveR.Evaluate(timeRatio), 
                        color.CurveG.Evaluate(timeRatio), 
                        color.CurveB.Evaluate(timeRatio), 
                        color.CurveA.Evaluate(timeRatio));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            CurrentVelocity *= deltaTime;

            // If the particle has acceleration, do more complex computations. Otherwise, just add the velocity to the current position.
            if (doAccelerationCalculations) {
                Vector2 finalVelocity = AccelerationVector + CurrentVelocity;
                LocalPosition += finalVelocity;
            } else {
                LocalPosition += CurrentVelocity;
            }
            GlobalPosition = LocalPosition + EmitterPosition;
        }

        public void Initialize()
        {
            Origin = new Vector2((float)Sprite.SourceRectangle.Width / 2, (float)Sprite.SourceRectangle.Height / 2);
            Texture = Sprite.Texture;
            sourceRect = Sprite.SourceRectangle;
            LocalPosition = Vector2.Zero;
            AccelerationVector = Vector2.Zero;
            CurrentRotation = 0f;
            Time = 0f;
            RegenerateDrawCall();
            Enabled = true;
        }

        public void Reset()
        {
            velocity.Disable();
            forwardVelocity.Disable();
            emitterForwardVelocity.Disable();
            acceleration.Disable();
            forwardAcceleration.Disable();
            emitterForwardAcceleration.Disable();
            angularVelocity.Disable();
            rotation.Disable();
            scale.Disable();
            color.Disable();
        }

        public void RegenerateDrawCall()
        {
            DrawCall = DrawCallDatabase.TryGetID(new DrawCall(Sprite.Texture, Effect));
        }

        public void Preallocate()
        {
            velocity = new VelocityInfo();
            forwardVelocity = new ForwardVelocityInfo();
            emitterForwardVelocity = new EmitterForwardVelocityInfo();
            acceleration = new AccelerationInfo();
            forwardAcceleration = new ForwardAccelerationInfo();
            emitterForwardAcceleration = new EmitterForwardAccelerationInfo();
            angularVelocity = new AngularVelocityInfo();
            rotation = new RotationInfo();
            scale = new ScaleInfo();
            color = new ColorInfo();
        }

        /// <summary>
        /// Creates a new Particle instance. Should only be used by ParticleSystems.
        /// </summary>
        internal Particle()
        {
            enabled = false;
            Preallocate();
        }
    }

    /// <summary>
    /// How the particle is rendered.
    /// </summary>
    public enum ParticleRenderType
    {
        /// <summary>Transparent, affected by lighting.</summary>
        Alpha,

        /// <summary>Transparent, ignores lighting.</summary>
        AlphaUnlit,

        /// <summary>Additive blending, in the same 'layer' as lighting.</summary>
        Additive,
    }

    /// <summary>
    /// Which transition or animation a particle's field undergoes over it's life.
    /// </summary>
    public enum ParticleTransitionType
    {
        /// <summary>Remains uninitialized or null for the entire life-time of the particle.</summary>
        None,

        /// <summary>Remains the same as it's starting value for the particle's life-time.</summary>
        Constant,

        /// <summary>Linearly transitions from 'start' and 'end' values over the particle's life.</summary>
        Lerp,

        /// <summary>Transitions via a curve. Has a moderate performance impact, and thus should be used sparingly.</summary>
        Curve
    }
}