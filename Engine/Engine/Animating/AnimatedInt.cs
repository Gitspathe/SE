using Microsoft.Xna.Framework;
using System;

namespace SE.Animating
{
    public class AnimatedInt : AnimatedValue<int>
    {
        private Curve curve;

        protected override void UpdateValue()
        {
            base.UpdateValue();
            switch (TransitionType) {
                case TransitionType.Lerp:
                    InnerValue = (int)MathHelper.Lerp(From, To, Location / Duration);
                    break;
                case TransitionType.Curve:
                    InnerValue = (int)curve.Evaluate(Location);
                    break;
                case TransitionType.Inactive:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetCurve(Curve curve)
        {
            this.curve = curve;
            TransitionType = TransitionType.Curve;
            Duration = curve.Keys[curve.Keys.Count - 1].Position;
        }
    }
}
