using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SE.Engine.Utility;
using SE.Utility;

namespace SEParticles
{
    public class EmitterConfig
    {
        public ColorConfig Color = new ColorConfig();
        public ScaleConfig Scale = new ScaleConfig();
        public LifeConfig Life = new LifeConfig();
        public SpeedConfig Speed = new SpeedConfig();

        public class ColorConfig
        {
            public StartingValue StartValueType;
            public Vector4 Min, Max;
            public Curve4 Curve;

            public ColorConfig()
            {
                Min = new Vector4(0, 1.0f, 1.0f, 1.0f);
                StartValueType = StartingValue.Normal;
            }

            public void SetNormal(Vector4 value)
            {
                Min = value;
                StartValueType = StartingValue.Normal;
            }

            public void SetRandomBetween(Vector4 min, Vector4 max)
            {
                Min = min;
                Max = max;
                StartValueType = StartingValue.Random;
            }

            public void SetRandomCurve(Curve4 curve)
            {
                Curve = curve;
                StartValueType = StartingValue.RandomCurve;
            }

            public ColorConfig DeepCopy()
                => new ColorConfig {
                    StartValueType = StartValueType,
                    Min = Min,
                    Max = Max,
                    Curve = Curve
                };
        }

        public class ScaleConfig
        {
            public StartingValue StartValueType;
            public Vector2 Min, Max;
            public Curve2 Curve;
            public bool TwoDimensions;

            public ScaleConfig()
            {
                StartValueType = StartingValue.Normal;
                Min = new Vector2(1.0f, 1.0f);
            }

            public void SetNormal(Vector2 value)
            {
                Min = value;
                TwoDimensions = true;
                StartValueType = StartingValue.Normal;
            }

            public void SetNormal(float value)
            {
                Min = new Vector2(value, value);
                TwoDimensions = false;
                StartValueType = StartingValue.Normal;
            }

            public void SetRandomBetween(Vector2 min, Vector2 max)
            {
                Min = min;
                Max = max;
                TwoDimensions = true;
                StartValueType = StartingValue.Random;
            }

            public void SetRandomBetween(float min, float max)
            {
                Min = new Vector2(min, min);
                Max = new Vector2(max, max);
                TwoDimensions = false;
                StartValueType = StartingValue.Random;
            }

            public void SetRandomCurve(Curve2 curve)
            {
                Curve = curve;
                TwoDimensions = true;
                StartValueType = StartingValue.RandomCurve;
            }

            public void SetRandomCurve(Curve curve)
            {
                Curve = new Curve2(curve, curve);
                TwoDimensions = false;
                StartValueType = StartingValue.RandomCurve;
            }

            public ScaleConfig DeepCopy() 
                => new ScaleConfig {
                    StartValueType = StartValueType,
                    Min = Min,
                    Max = Max,
                    Curve = Curve
                };
        }

        public class LifeConfig
        {
            public StartingValue StartValueType;
            public float Min, Max;
            public Curve Curve;

            public void SetNormal(float value)
            {
                Min = value;
                StartValueType = StartingValue.Normal;
            }

            public void SetRandomBetween(float min, float max)
            {
                Min = min;
                Max = max;
                StartValueType = StartingValue.Random;
            }

            public void SetRandomCurve(Curve curve)
            {
                Curve = curve;
                StartValueType = StartingValue.RandomCurve;
            }

            public LifeConfig DeepCopy() 
                => new LifeConfig {
                    StartValueType = StartValueType,
                    Min = Min,
                    Max = Max,
                    Curve = Curve
                };
        }

        public class SpeedConfig
        {
            public StartingValue StartValueType;
            public float Min, Max;
            public Curve Curve;

            public void SetNormal(float value)
            {
                Min = value;
                StartValueType = StartingValue.Normal;
            }

            public void SetRandomBetween(float min, float max)
            {
                Min = min;
                Max = max;
                StartValueType = StartingValue.Random;
            }

            public void SetRandomCurve(Curve curve)
            {
                Curve = curve;
                StartValueType = StartingValue.RandomCurve;
            }

            public SpeedConfig DeepCopy() 
                => new SpeedConfig {
                    StartValueType = StartValueType,
                    Min = Min,
                    Max = Max,
                    Curve = Curve
                };
        }

        public enum StartingValue
        {
            Normal,
            Random,
            RandomCurve
        }

        public EmitterConfig DeepCopy() 
            => new EmitterConfig {
                Color = Color.DeepCopy(),
                Life = Life.DeepCopy(),
                Scale = Scale.DeepCopy(),
                Speed = Speed.DeepCopy()
            };
    }

}
