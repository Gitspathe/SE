using Microsoft.Xna.Framework;
using System;
using Vector2 = System.Numerics.Vector2;

namespace SE.Animating
{
    public class AnimatedVector2 : AnimatedValue<Vector2>
    {
        Curve curveX, curveY;

        protected override void UpdateValue()
        {
            base.UpdateValue();
            switch (TransitionType) {
                case TransitionType.Lerp:
                    InnerValue = Vector2.Lerp(From, To, Location / Duration);
                    break;
                case TransitionType.Curve:
                    InnerValue = new Vector2(curveX.Evaluate(Location), curveY.Evaluate(Location));
                    break;
                case TransitionType.Inactive:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetCurve(Curve x, Curve y)
        {
            curveX = x;
            curveY = y;
            TransitionType = TransitionType.Curve;
            float highest = 0;
            float cur = curveX.Keys[curveX.Keys.Count - 1].Position;
            if (cur > highest)
                highest = cur;

            cur = curveY.Keys[curveY.Keys.Count - 1].Position;
            if (cur > highest)
                highest = cur;

            Duration = highest;
        }

        public void SetCurve(Curve x)
        {
            SetCurve(x, x);
        }
    }
}
