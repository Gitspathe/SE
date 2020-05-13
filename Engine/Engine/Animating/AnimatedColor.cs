using System;
using Microsoft.Xna.Framework;

namespace SE.Animating
{
    public class AnimatedColor : AnimatedValue<Color>
    {
        private Curve curveR, curveG, curveB, curveA;

        protected override void UpdateValue()
        {
            base.UpdateValue();
            switch (TransitionType) {
                case TransitionType.Lerp:
                    InnerValue = Color.Lerp(From, To, Location / Duration);
                    break;
                case TransitionType.Curve:
                    InnerValue = new Color(curveR.Evaluate(Location), curveG.Evaluate(Location), curveB.Evaluate(Location), curveA.Evaluate(Location));
                    break;
                case TransitionType.Inactive:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetCurve(Curve r, Curve g, Curve b, Curve a)
        {
            curveR = r;
            curveG = g;
            curveB = b;
            curveA = a;
            TransitionType = TransitionType.Curve;
            float highest = 0;
            float cur = curveR.Keys[curveR.Keys.Count-1].Position;
            if (cur > highest)
                highest = cur;

            cur = curveG.Keys[curveG.Keys.Count-1].Position;
            if (cur > highest)
                highest = cur;

            cur = curveB.Keys[curveB.Keys.Count-1].Position;
            if (cur > highest)
                highest = cur;

            cur = curveA.Keys[curveA.Keys.Count-1].Position;
            if (cur > highest)
                highest = cur;

            Duration = highest;
        }
    }
}