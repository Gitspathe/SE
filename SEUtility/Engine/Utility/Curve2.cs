using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SE.Utility;

namespace SE.Engine.Utility
{
    public class Curve2
    {
        public Curve X = new Curve();
        public Curve Y = new Curve();

        public Vector2 Evaluate(float position)
            => new Vector2(
                X.Evaluate(position), 
                Y.Evaluate(position));

        public void Add(float position, Vector2 value)
        {
            X.Keys.Add(position, value.X);
            Y.Keys.Add(position, value.Y);
        }
    }
}
