using System;
using System.Collections.Generic;

namespace SE.Animating
{
    public class Animation
    {
        public bool AutomaticDuration = true;
        public float Duration;
        public float Location;

        private List<IAnimatedValue> animatedValues = new List<IAnimatedValue>();
        private bool reverse;

        private WrapMode wrapMode = WrapMode.Once;
        public WrapMode WrapMode {
            get => wrapMode;
            set {
                wrapMode = value;
                if (wrapMode != WrapMode.PingPong)
                    reverse = false;
            }
        }

        public int Count => animatedValues.Count;

        public void Reset()
        {
            for (int i = 0; i < animatedValues.Count; i++)
                animatedValues[i].Reset();
        }

        public void Update(float timePassed)
        {
            if (reverse) {
                Location -= timePassed;
                if (Location <= 0f) {
                    reverse = false;
                }
            } else {
                Location += timePassed;
                if (Location >= Duration) {
                    switch (wrapMode) {
                        case WrapMode.Once:
                            Location = Duration;
                            break;
                        case WrapMode.Wrap:
                            Location = 0f;
                            break;
                        case WrapMode.PingPong:
                            reverse = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            for (int i = 0; i < animatedValues.Count; i++) {
                animatedValues[i].Update(Location);
            }
        }

        public IAnimatedValue Get(int index)
        {
            if (index < 0 || index > animatedValues.Count)
                return null;

            return animatedValues[index];
        }

        public void Add(IAnimatedValue animation)
        {
            if (animatedValues.Contains(animation))
                return;

            animatedValues.Add(animation);
            CalculateDuration();
        }

        public bool Remove(IAnimatedValue animation)
        {
            bool b = animatedValues.Remove(animation);
            CalculateDuration();
            return b;
        }

        public bool Contains(IAnimatedValue animation)
        {
            return animatedValues.Contains(animation);
        }

        public void CalculateDuration()
        {
            if (!AutomaticDuration)
                return;

            float highest = 0;
            for (int i = 0; i < animatedValues.Count; i++) {
                if (animatedValues[i].Duration > highest) {
                    highest = animatedValues[i].Duration;
                }
            }
            Duration = highest;
        }

        public Animation(IAnimatedValue animation)
        {
            Add(animation);
        }

        public Animation(params IAnimatedValue[] animations)
        {
            for (int i = 0; i < animations.Length; i++)
                Add(animations[i]);
        }
    }

    /// <summary>
    /// Specifies how an animated value is updated.
    /// </summary>
    public enum TransitionType
    {
        /// <summary>Animated value will not change.</summary>
        Inactive,

        /// <summary>Animated value will interpolate linearly between provided start and end values.</summary>
        Lerp,

        /// <summary>Animated value will update according to evaluation of curves.</summary>
        Curve
    }

    /// <summary>
    /// How the animated value will behave once it reaches the end of it's duration.
    /// </summary>
    public enum WrapMode
    {
        /// <summary>Animated value will stay at it's end value forever.</summary>
        Once,

        /// <summary>Animated value will instantly shift to it's start point, and continue.</summary>
        Wrap,

        /// <summary>Animated value will reverse once it reaches the end of it's duration.</summary>
        PingPong
    }
}
