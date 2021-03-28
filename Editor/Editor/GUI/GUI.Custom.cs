using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace SE.Editor.GUI
{
    public static partial class GUI
    {
        #region INPUTS

        public static bool SetDragDropPayload(string type, string str)
        {
            IntPtr ptr = Marshal.StringToHGlobalUni(str);
            bool b = ImGui.SetDragDropPayload(type, ptr, (uint)str.Length * sizeof(char) + sizeof(int));
            Marshal.FreeHGlobal(ptr);
            return b;
        }

        public static string GetDragDropPayload()
        {
            ImGuiPayloadPtr ptr = ImGui.GetDragDropPayload();
            string str = Marshal.PtrToStringUni(ptr.Data);
            ptr.Clear();
            return str;
        }

        public static unsafe string AcceptDragDropPayload(string type)
        {
            ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload(type);
            if (payload.NativePtr != null) {
                string str = Marshal.PtrToStringUni(payload.Data);
                payload.Clear();
                return str;
            }
            return null;
        }

        public static void Image(IntPtr textureID, Vector2 size, Vector2 uv0, Vector2 uv1, Vector4 tintColor, Vector4 borderColor) 
            => ImGui.Image(textureID, size, uv0, uv1, tintColor, borderColor);
        public static void Image(IntPtr textureID, Vector2 size, Vector2 uv0, Vector2 uv1, Vector4 tintColor) 
            => ImGui.Image(textureID, size, uv0, uv1, tintColor);
        public static void Image(IntPtr textureID, Vector2 size, Vector2 uv0, Vector2 uv1) 
            => ImGui.Image(textureID, size, uv0, uv1);
        public static void Image(IntPtr textureID, Vector2 size, Vector2 uv0) 
            => ImGui.Image(textureID, size, uv0);
        public static void Image(IntPtr textureID, Vector2 size) 
            => ImGui.Image(textureID, size);

        public static bool CollapsingHeader(string label)
            => ImGui.CollapsingHeader(label);
        public static bool CollapsingHeader(string label, ref bool pOpen, GUITreeNodeFlags flags) 
            => ImGui.CollapsingHeader(label, ref pOpen, (ImGuiTreeNodeFlags)flags);
        public static bool CollapsingHeader(string label, ref bool pOpen) 
            => ImGui.CollapsingHeader(label, ref pOpen, ImGuiTreeNodeFlags.None);
        public static bool CollapsingHeader(string label, GUITreeNodeFlags flags) 
            => ImGui.CollapsingHeader(label, (ImGuiTreeNodeFlags)flags);

        public static bool Checkbox(string label, ref bool val, string fmt)
        {
            GUIUtility.TextIndentFunction(fmt);
            return ImGui.Checkbox(label, ref val);
        }
        public static bool Checkbox(string label, ref bool val)
            => Checkbox(label, ref val, null);

        public static bool InputText(string label, ref string val, uint maxLength, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(1, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            SetNextItemWidth(itemSize);
            return ImGui.InputText(label, ref val, maxLength, (ImGuiInputTextFlags)flags);
        }
        public static bool InputText(string label, ref string val, string fmt = null)
            => InputText(label, ref val, 128, GUIInputTextFlags.None, fmt);

        public static bool InputColor3(string label, ref Color color, GUIColorEditFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(1, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            Vector3 tmp = new Vector3(color.R, color.G, color.B) / 255;
            SetNextItemWidth(itemSize);
            bool col = ImGui.ColorEdit3(label + "color", ref tmp, (ImGuiColorEditFlags)flags);
            color = new Color(tmp.X, tmp.Y, tmp.Z);
            return col;
        }
        public static bool InputColor3(string label, ref Color color, string fmt = null)
            => InputColor3(label, ref color, GUIColorEditFlags.None, fmt);

        public static bool InputColor4(string label, ref Color color, GUIColorEditFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(1, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            Vector4 tmp = new Vector4(color.R, color.G, color.B, color.A) / 255;
            SetNextItemWidth(itemSize);
            bool col = ImGui.ColorEdit4(label + "color", ref tmp, (ImGuiColorEditFlags)flags);
            color = new Color(tmp.X, tmp.Y, tmp.Z, tmp.W);
            return col;
        }
        public static bool InputColor4(string label, ref Color color, string fmt = null)
            => InputColor4(label, ref color, GUIColorEditFlags.None, fmt);

        public static bool InputRectangle(string label, ref Rectangle val, int step, int stepFast, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(2, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            Text("X");
            SameLine();
            SetNextItemWidth(itemSize);
            bool x = ImGui.InputInt(label + "X", ref val.X, step, stepFast, (ImGuiInputTextFlags)flags);

            TextInlined("Y");
            SetNextItemWidth(itemSize);
            bool y = ImGui.InputInt(label + "Y", ref val.Y, step, stepFast, (ImGuiInputTextFlags)flags);

            ImGui.NewLine();
            Indent();
            Text("W");
            SameLine();
            SetNextItemWidth(itemSize);
            bool w = ImGui.InputInt(label + "W", ref val.Width, step, stepFast, (ImGuiInputTextFlags)flags);

            TextInlined("H");
            SetNextItemWidth(itemSize);
            bool h = ImGui.InputInt(label + "H", ref val.Height, step, stepFast, (ImGuiInputTextFlags)flags);

            return x || y || w || h;
        }
        public static bool InputRectangle(string label, ref Rectangle val, string fmt = null)
            => InputRectangle(label, ref val, 0, 0, GUIInputTextFlags.None, fmt);

        public static bool InputRectangleF(string label, ref RectangleF val, float step, float stepFast, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(2, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            Text("X");
            SameLine();
            SetNextItemWidth(itemSize);
            bool x = ImGui.InputFloat(label + "X", ref val.X, step, stepFast, null, (ImGuiInputTextFlags)flags);

            TextInlined("Y");
            SetNextItemWidth(itemSize);
            bool y = ImGui.InputFloat(label + "Y", ref val.Y, step, stepFast, null, (ImGuiInputTextFlags)flags);

            ImGui.NewLine();
            Indent();
            Text("W");
            SameLine();
            SetNextItemWidth(itemSize);
            bool w = ImGui.InputFloat(label + "W", ref val.Width, step, stepFast, null, (ImGuiInputTextFlags)flags);

            TextInlined("H");
            SetNextItemWidth(itemSize);
            bool h = ImGui.InputFloat(label + "H", ref val.Height, step, stepFast, null, (ImGuiInputTextFlags)flags);

            return x || y || w || h;
        }
        public static bool InputRectangleF(string label, ref RectangleF val, string fmt = null)
            => InputRectangleF(label, ref val, 0, 0, GUIInputTextFlags.None, fmt);

        public static bool InputVector2(string label, ref Vector2 val, float step, float stepFast, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(2, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            Text("X");
            SameLine();
            SetNextItemWidth(itemSize);
            bool x = ImGui.InputFloat(label + "X", ref val.X, step, stepFast, null, (ImGuiInputTextFlags)flags);

            TextInlined("Y");
            SetNextItemWidth(itemSize);
            bool y = ImGui.InputFloat(label + "Y", ref val.Y, step, stepFast, null, (ImGuiInputTextFlags)flags);

            return x || y;
        }
        public static bool InputVector2(string label, ref Vector2 val, string fmt = null)
            => InputVector2(label, ref val, 0.0f, 0.0f, GUIInputTextFlags.None, fmt);

        public static bool InputVector3(string label, ref Vector3 val, float step, float stepFast, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(3, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            Text("X");
            SameLine();
            SetNextItemWidth(itemSize);
            bool x = ImGui.InputFloat(label + "X", ref val.X, step, stepFast, null, (ImGuiInputTextFlags)flags);

            TextInlined("Y");
            SetNextItemWidth(itemSize);
            bool y = ImGui.InputFloat(label + "Y", ref val.Y, step, stepFast, null, (ImGuiInputTextFlags)flags);

            TextInlined("Z");
            SetNextItemWidth(itemSize);
            bool z = ImGui.InputFloat(label + "Z", ref val.Z, step, stepFast, null, (ImGuiInputTextFlags)flags);

            return x || y || z;
        }
        public static bool InputVector3(string label, ref Vector3 val, string fmt = null)
            => InputVector3(label, ref val, 0.0f, 0.0f, GUIInputTextFlags.None, fmt);

        public static bool InputVector4(string label, ref Vector4 val, float step, float stepFast, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(3, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            Text("X");
            SameLine();
            SetNextItemWidth(itemSize);
            bool x = ImGui.InputFloat(label + "X", ref val.X, step, stepFast, null, (ImGuiInputTextFlags)flags);

            TextInlined("Y");
            SetNextItemWidth(itemSize);
            bool y = ImGui.InputFloat(label + "Y", ref val.Y, step, stepFast, null, (ImGuiInputTextFlags)flags);

            TextInlined("Y");
            SetNextItemWidth(itemSize);
            bool z = ImGui.InputFloat(label + "Z", ref val.Z, step, stepFast, null, (ImGuiInputTextFlags)flags);

            TextInlined("Y");
            SetNextItemWidth(itemSize);
            bool w = ImGui.InputFloat(label + "W", ref val.W, step, stepFast, null, (ImGuiInputTextFlags)flags);

            return x || y || z || w;
        }
        public static bool InputVector4(string label, ref Vector4 val, string fmt = null)
            => InputVector4(label, ref val, 0.0f, 0.0f, GUIInputTextFlags.None, fmt);

        // TODO: InputVector4Drag etc...

        public static bool InputFloat(string label, ref float val, float step, float stepFast, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(1, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            SetNextItemWidth(itemSize);
            return ImGui.InputFloat(label, ref val, step, stepFast, null, (ImGuiInputTextFlags)flags);
        }
        public static bool InputFloat(string label, ref float val, string fmt = null)
            => InputFloat(label, ref val, 0.0f, 0.0f, GUIInputTextFlags.None, fmt);

        public static bool InputDouble(string label, ref double val, float step, float stepFast, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(1, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            SetNextItemWidth(itemSize);
            return ImGui.InputDouble(label, ref val, step, stepFast, null, (ImGuiInputTextFlags)flags);
        }
        public static bool InputDouble(string label, ref double val, string fmt = null)
            => InputDouble(label, ref val, 0.0f, 0.0f, GUIInputTextFlags.None, fmt);

        public static bool InputInt(string label, ref int val, int step, int stepFast, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(1, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            SetNextItemWidth(itemSize);
            return ImGui.InputInt(label, ref val, step, stepFast, (ImGuiInputTextFlags)flags);
        }
        public static bool InputInt(string label, ref int val, string fmt = null)
            => InputInt(label, ref val, 0, 0, GUIInputTextFlags.None, fmt);

        private static unsafe bool InputScalar<T>(string label, ImGuiDataType dataType, ref T val, T? step, T? stepFast, GUIInputTextFlags flags, string fmt) where T : unmanaged
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(1, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            IntPtr valPtr;
            IntPtr stepPtr = IntPtr.Zero;
            IntPtr stepFastPtr = IntPtr.Zero;

            fixed (T* ptr = &val) {
                valPtr = (IntPtr)ptr;
            }
            if (step.HasValue) {
                T temp = step.Value;
                stepPtr = (IntPtr)(&temp);
            }
            if (stepFast.HasValue) {
                T temp = stepFast.Value;
                stepFastPtr = (IntPtr)(&temp);
            }

            SetNextItemWidth(itemSize);
            return ImGui.InputScalar(label, dataType, valPtr, stepPtr, stepFastPtr, fmt, (ImGuiInputTextFlags)flags);
        }
        // TODO DragScalar (Line 3986 in ImGui.cs).

        public static bool InputLong(string label, ref long val, long? step, long? stepFast, GUIInputTextFlags flags, string fmt)
            => InputScalar(label, ImGuiDataType.S64, ref val, step, stepFast, flags, fmt);
        public static bool InputLong(string label, ref long val, string fmt = null)
            => InputLong(label, ref val, null, null, GUIInputTextFlags.None, fmt);

        public static bool InputULong(string label, ref ulong val, ulong? step, ulong? stepFast, GUIInputTextFlags flags, string fmt)
            => InputScalar(label, ImGuiDataType.U64, ref val, step, stepFast, flags, fmt);
        public static bool InputULong(string label, ref ulong val, string fmt = null)
            => InputULong(label, ref val, null, null, GUIInputTextFlags.None, fmt);

        public static bool InputUInt(string label, ref uint val, uint? step, uint? stepFast, GUIInputTextFlags flags, string fmt)
            => InputScalar(label, ImGuiDataType.U32, ref val, step, stepFast, flags, fmt);
        public static bool InputUInt(string label, ref uint val, string fmt = null)
            => InputUInt(label, ref val, null, null, GUIInputTextFlags.None, fmt);

        public static bool InputShort(string label, ref short val, short? step, short? stepFast, GUIInputTextFlags flags, string fmt)
            => InputScalar(label, ImGuiDataType.S16, ref val, step, stepFast, flags, fmt);
        public static bool InputShort(string label, ref short val, string fmt = null)
            => InputShort(label, ref val, null, null, GUIInputTextFlags.None, fmt);

        public static bool InputUShort(string label, ref ushort val, ushort? step, ushort? stepFast, GUIInputTextFlags flags, string fmt)
            => InputScalar(label, ImGuiDataType.U16, ref val, step, stepFast, flags, fmt);
        public static bool InputUShort(string label, ref ushort val, string fmt = null)
            => InputUShort(label, ref val, null, null, GUIInputTextFlags.None, fmt);

        public static bool InputByte(string label, ref byte val, byte? step, byte? stepFast, GUIInputTextFlags flags, string fmt)
            => InputScalar(label, ImGuiDataType.U8, ref val, step, stepFast, flags, fmt);
        public static bool InputByte(string label, ref byte val, string fmt = null)
            => InputByte(label, ref val, null, null, GUIInputTextFlags.None, fmt);

        public static bool InputSByte(string label, ref sbyte val, sbyte? step, sbyte? stepFast, GUIInputTextFlags flags, string fmt)
            => InputScalar(label, ImGuiDataType.S8, ref val, step, stepFast, flags, fmt);
        public static bool InputSByte(string label, ref sbyte val, string fmt = null)
            => InputSByte(label, ref val, null, null, GUIInputTextFlags.None, fmt);

        public static void Text(string fmt)
        {
            AlignTextToFramePadding();
            ImGui.Text(fmt);
        }

        public static void TextInlined(string fmt)
        {
            SameLine();
            AlignTextToFramePadding();
            ImGui.Text(fmt);
            SameLine();
        }

        #endregion
    }
}
