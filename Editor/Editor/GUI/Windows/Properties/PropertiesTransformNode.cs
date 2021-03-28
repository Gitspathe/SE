using System;
using Microsoft.Xna.Framework;
using SE.Common;
using Vector2 = System.Numerics.Vector2;

namespace SE.Editor.GUI.Windows.Properties
{
    public class PropertiesTransformNode : PropertiesNode<Transform>
    {
        public override Type NodeType => typeof(Transform);

        public override void OnPaint()
        {
            Vector2 position = Internal.Position;
            Vector2 scale = Internal.Scale;
            float rotation = MathHelper.ToDegrees(Internal.Rotation);

            GUI.PushMargin(0.3f);
            GUI.InputVector2("##pos", ref position, "Position");
            GUI.InputVector2("##scale", ref scale, "Scale");
            GUI.InputFloat("##rotation", ref rotation, "Rotation");
            GUI.PopMargin();

            Internal.Position = position;
            Internal.Scale = scale;
            Internal.Rotation = MathHelper.ToRadians(rotation);
        }

        public PropertiesTransformNode(Transform obj) : base(obj) { }
    }
}
