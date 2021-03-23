using System;
using Microsoft.Xna.Framework;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace SE.Editor.GUI.ValueDrawers
{
    public interface IGUIValueDrawer
    {
        Type ValueType { get; }
        dynamic Display(int index, dynamic value);
    }

    public class GUIBooleanDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(bool);

        public dynamic Display(int index, dynamic value)
        {
            bool outVar = value;
            GUI.Checkbox("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIByteDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(byte);

        public dynamic Display(int index, dynamic value)
        {
            byte outVar = value;
            GUI.InputByte("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUISByteDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(sbyte);

        public dynamic Display(int index, dynamic value)
        {
            sbyte outVar = value;
            GUI.InputSByte("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIShortDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(short);

        public dynamic Display(int index, dynamic value)
        {
            short outVar = value;
            GUI.InputShort("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIUShortDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(ushort);

        public dynamic Display(int index, dynamic value)
        {
            ushort outVar = value;
            GUI.InputUShort("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIIntDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(int);

        public dynamic Display(int index, dynamic value)
        {
            int outVar = value;
            GUI.InputInt("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIUIntDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(uint);

        public dynamic Display(int index, dynamic value)
        {
            uint outVar = value;
            GUI.InputUInt("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUILongDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(long);

        public dynamic Display(int index, dynamic value)
        {
            long outVar = value;
            GUI.InputLong("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIULongDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(ulong);

        public dynamic Display(int index, dynamic value)
        {
            ulong outVar = value;
            GUI.InputULong("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIFloatDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(float);

        public dynamic Display(int index, dynamic value)
        {
            float outVar = value;
            GUI.InputFloat("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIDoubleDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(double);

        public dynamic Display(int index, dynamic value)
        {
            double outVar = value;
            GUI.InputDouble("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIColorDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(Color);

        public dynamic Display(int index, dynamic value)
        {
            Color outVar = value;
            GUI.InputColor4("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIVector2Drawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(Vector2);

        public dynamic Display(int index, dynamic value)
        {
            Vector2 outVar = value;
            GUI.InputVector2("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIVector3Drawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(Vector3);

        public dynamic Display(int index, dynamic value)
        {
            Vector3 outVar = value;
            GUI.InputVector3("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIVector4Drawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(Vector4);

        public dynamic Display(int index, dynamic value)
        {
            Vector4 outVar = value;
            GUI.InputVector4("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIRectangleDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(Rectangle);

        public dynamic Display(int index, dynamic value)
        {
            Rectangle outVar = value;
            GUI.InputRectangle("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIRectangleFDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(RectangleF);

        public dynamic Display(int index, dynamic value)
        {
            RectangleF outVar = value;
            GUI.InputRectangleF("##" + index, ref outVar);
            return outVar;
        }
    }

    public class GUIStringDrawer : IGUIValueDrawer
    {
        public Type ValueType => typeof(string);

        public dynamic Display(int index, dynamic value)
        {
            string outVar = value;
            GUI.InputText("##" + index, ref outVar);
            return outVar;
        }
    }
}
