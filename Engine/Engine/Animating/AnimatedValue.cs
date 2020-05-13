using Microsoft.Xna.Framework;

namespace SE.Animating
{
    public class AnimatedValue<T> : IAnimatedValue
    {
        public T From;
        public T To;

        protected TransitionType TransitionType = TransitionType.Inactive;

        private Ref<T> refValue;

        private T innerValue;
        public T InnerValue
        {
            get => innerValue;
            set {
                innerValue = value;
                refValue.Value = innerValue;
            }
        }

        public float Duration { get; protected set; }
        public float Location { get; protected set; }

        public void SetLerp(T from, T to, float duration)
        {
            From = from;
            To = to;
            Duration = duration;
            TransitionType = TransitionType.Lerp;
        }

        public void SetInactive()
        {
            TransitionType = TransitionType.Inactive;
        }

        public void Attach(Ref<T> reference)
        {
            refValue = reference;
        }

        public void Reset()
        {
            Location = 0.0f;
        }

        public void Update(float time)
        {
            if (TransitionType == TransitionType.Inactive)
                return;

            time = MathHelper.Clamp(time, 0, Duration);
            Location = time;
            UpdateValue();
        }

        protected virtual void UpdateValue() { }
    }
}
