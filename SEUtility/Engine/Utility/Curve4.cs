using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SE.Utility;

namespace SE.Engine.Utility
{
    public class Curve4
    {
        public Curve X = new Curve();
        public Curve Y = new Curve();
        public Curve Z = new Curve();
        public Curve W = new Curve();

        public Vector4 Evaluate(float position)
            => new Vector4(
                X.Evaluate(position), 
                Y.Evaluate(position), 
                Z.Evaluate(position), 
                W.Evaluate(position));

        public void Add(float position, Vector4 value)
        {
            X.Keys.Add(position, value.X);
            Y.Keys.Add(position, value.Y);
            Z.Keys.Add(position, value.Z);
            W.Keys.Add(position, value.W);
        }
    }
}
