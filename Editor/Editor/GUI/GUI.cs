using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SE.Engine.Utility;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace DeeZ.Editor.GUI
{
    public static class GUI
    {
        #region FUNCTIONS

        public static void PushMargin(float margin)
        {
            if (margin < 0.0f || margin > 1.0f)
                throw new Exception("Margin must be between 0.0 and 1.0.");

            GUIUtility.MarginStack.Push(margin);
        }

        public static void PopMargin(int amount = 1)
        {
            while (amount > 0) {
                if(GUIUtility.MarginStack.Count < 1)
                    break;

                GUIUtility.MarginStack.Pop();
                amount--;
            }
        }

        #endregion

        #region IMGUI FUNCTIONS

        public static bool SetDragDropPayload(string type, string str)
        {
            IntPtr ptr = Marshal.StringToHGlobalUni(str);
            bool b = ImGui.SetDragDropPayload(type, ptr, (uint) str.Length * sizeof(char) + sizeof(int));
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

        public static void TreePop() => ImGui.TreePop();
        public static void TreePush() => ImGui.TreePush();
        public static void TreePush(string strID) => ImGui.TreePush(strID);
        public static void TreePush(IntPtr ptr) => ImGui.TreePush(ptr);

        public static bool IsAnyItemActive() => ImGui.IsAnyItemActive();
        public static bool IsAnyItemFocused() => ImGui.IsAnyItemFocused();
        public static bool IsAnyItemHovered() => ImGui.IsAnyItemHovered();
        public static bool IsItemClicked() => ImGui.IsItemClicked();
        public static bool IsItemClicked(ImGuiMouseButton button) => ImGui.IsItemClicked(button);
        public static bool IsItemActivated() => ImGui.IsItemActivated();
        public static bool IsItemActive() => ImGui.IsItemActive();
        public static bool IsItemDeactivated() => ImGui.IsItemDeactivated();
        public static bool IsItemFocused() => ImGui.IsItemFocused();
        public static bool IsItemHovered() => ImGui.IsItemHovered();
        public static bool IsItemActive(GUIHoveredFlags flags) => ImGui.IsItemHovered((ImGuiHoveredFlags) flags);
        public static bool IsItemVisible() => ImGui.IsItemVisible();

        public static bool IsAnyMouseDown() => ImGui.IsAnyMouseDown();
        public static bool IsMouseReleased(ImGuiMouseButton button) => ImGui.IsMouseReleased(button);
        public static bool IsMouseDoubleClicked(ImGuiMouseButton button) => ImGui.IsMouseDoubleClicked(button);
        public static bool IsMouseDown() => ImGui.IsMouseDragging(0);
        public static bool IsMouseDown(ImGuiMouseButton button) => ImGui.IsMouseDragging(button);
        public static bool IsMouseDown(ImGuiMouseButton button, float lockThreshold) => ImGui.IsMouseDragging(button, lockThreshold);
        public static bool IsMouseDoubleClicked(Vector2 min, Vector2 max) => ImGui.IsMouseHoveringRect(min, max);
        public static bool IsMouseDoubleClicked(Vector2 min, Vector2 max, bool clip) => ImGui.IsMouseHoveringRect(min, max, clip);
        public static bool IsMouseReleased() => ImGui.IsMousePosValid();
        public static bool IsMouseReleased(ref Vector2 mousePos) => ImGui.IsMousePosValid(ref mousePos);
        public static bool IsMouseClicked(ImGuiMouseButton button) => ImGui.IsMouseClicked(button);
        public static bool IsMouseClicked(ImGuiMouseButton button, bool repeat) => ImGui.IsMouseClicked(button, repeat);

        public static bool BeginDragDropSource() => ImGui.BeginDragDropSource();
        public static void BeginDragDropSource(GUIDragDropFlags flags) => ImGui.BeginDragDropSource((ImGuiDragDropFlags) flags);
        public static void EndDragDropSource() => ImGui.EndDragDropSource();

        public static bool BeginDragDropTarget() => ImGui.BeginDragDropTarget();
        public static void EndDragDropTarget() => ImGui.EndDragDropTarget();

        public static Vector2 GetWindowPos() => ImGui.GetWindowPos();
        public static float GetWindowWidth() => ImGui.GetWindowWidth();
        public static float GetWindowHeight() => ImGui.GetWindowHeight();

        public static Vector2 GetContentRegionAvailable() => ImGui.GetContentRegionAvail();
        public static Vector2 GetContentRegionMax() => ImGui.GetContentRegionMax();

        public static void Begin(string name) => ImGui.Begin(name);
        public static void Begin(string name, ref bool pOpen) => ImGui.Begin(name, ref pOpen);
        public static void Begin(string name, GUIWindowFlags flags) => ImGui.Begin(name, (ImGuiWindowFlags) flags);
        public static void Begin(string name, ref bool pOpen, GUIWindowFlags flags) => ImGui.Begin(name, ref pOpen, (ImGuiWindowFlags) flags);
        public static void End() => ImGui.End();

        public static void AlignTextToFramePadding() => ImGui.AlignTextToFramePadding();
        public static void SetNextItemWidth(float width) => ImGui.SetNextItemWidth(width);

        public static void Indent() => SameLine(GUIUtility.LabelIndent);
        public static void SameLine() => ImGui.SameLine();
        public static void SameLine(float offsetX) => ImGui.SameLine(offsetX);
        public static void SameLine(float offsetX, float spacing) => ImGui.SameLine(offsetX, spacing);

        public static void OpenPopupOnItemClick(string strID, ImGuiMouseButton mouseButton) => ImGui.OpenPopupOnItemClick(strID, mouseButton);
        public static void OpenPopupOnItemClick(string strID) => ImGui.OpenPopupOnItemClick(strID);
        public static void OpenPopupOnItemClick() => ImGui.OpenPopupOnItemClick();

        public static bool BeginPopup(string strID, GUIWindowFlags flags) => ImGui.BeginPopup(strID, (ImGuiWindowFlags) flags);
        public static bool BeginPopup(string strID) => ImGui.BeginPopup(strID);
        public static void EndPopup() => ImGui.EndPopup();

        public static bool BeginMainMenuBar() => ImGui.BeginMainMenuBar();
        public static void EndMainMenuBar() => ImGui.EndMainMenuBar();

        public static bool MenuItem(string label, string shortcut, ref bool pSelected, bool enabled) => ImGui.MenuItem(label, shortcut, ref pSelected, enabled);
        public static bool MenuItem(string label, string shortcut, ref bool pSelected) => ImGui.MenuItem(label, shortcut, ref pSelected);
        public static bool MenuItem(string label, string shortcut, bool selected) => ImGui.MenuItem(label, shortcut, ref selected);
        public static bool MenuItem(string label, string shortcut) => ImGui.MenuItem(label, shortcut);
        public static bool MenuItem(string label) => ImGui.MenuItem(label);
        public static bool MenuItem(string label, bool enabled) => ImGui.MenuItem(label, enabled);

        public static void PushStyleVar(GUIStyleVar styleVar, float var) => ImGui.PushStyleVar((ImGuiStyleVar)styleVar, var);
        public static void PushStyleVar(GUIStyleVar styleVar, Vector2 var) => ImGui.PushStyleVar((ImGuiStyleVar)styleVar, var);

        public static void PushStyleColor(GUIColor styleColor, uint color) => ImGui.PushStyleColor((ImGuiCol) styleColor, color);
        public static void PushStyleColor(GUIColor styleColor, Vector4 color) => ImGui.PushStyleColor((ImGuiCol)styleColor, color);

        public static void PopStyleVar() => ImGui.PopStyleVar();
        public static void PopStyleVar(int count) => ImGui.PopStyleVar(count);

        public static void PopStyleColor() => ImGui.PopStyleColor();
        public static void PopStyleColor(int count) => ImGui.PopStyleColor(count);

        public static void PushID(int id) => ImGui.PushID(id);
        public static void PushID(string id) => ImGui.PushID(id);
        public static void PushID(IntPtr id) => ImGui.PushID(id);
        public static void PopID() => ImGui.PopID();

        #endregion

        #region INPUTS

        public static void Image(IntPtr textureID, Vector2 size, Vector2 uv0, Vector2 uv1, Vector4 tintColor, Vector4 borderColor) =>
            ImGui.Image(textureID, size, uv0, uv1, tintColor, borderColor);
        public static void Image(IntPtr textureID, Vector2 size, Vector2 uv0, Vector2 uv1, Vector4 tintColor) =>
            ImGui.Image(textureID, size, uv0, uv1, tintColor);
        public static void Image(IntPtr textureID, Vector2 size, Vector2 uv0, Vector2 uv1) =>
            ImGui.Image(textureID, size, uv0, uv1);
        public static void Image(IntPtr textureID, Vector2 size, Vector2 uv0) =>
            ImGui.Image(textureID, size, uv0);
        public static void Image(IntPtr textureID, Vector2 size) =>
            ImGui.Image(textureID, size);

        public static bool CollapsingHeader(string label, ref bool pOpen, GUITreeNodeFlags flags) =>
            ImGui.CollapsingHeader(label, ref pOpen, (ImGuiTreeNodeFlags) flags);
        public static bool CollapsingHeader(string label, ref bool pOpen) =>
            ImGui.CollapsingHeader(label, ref pOpen, ImGuiTreeNodeFlags.None);
        public static bool CollapsingHeader(string label, GUITreeNodeFlags flags) =>
            ImGui.CollapsingHeader(label, (ImGuiTreeNodeFlags)flags);

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
            return ImGui.InputText(label, ref val, maxLength, (ImGuiInputTextFlags) flags);
        }
        public static bool InputText(string label, ref string val, string fmt = null)
            => InputText(label, ref val, 128, GUIInputTextFlags.None, fmt);

        public static bool InputColor3(string label, ref Color color, GUIColorEditFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(1, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            Vector3 tmp = new Vector3(color.R, color.G, color.B) / 255;
            SetNextItemWidth(itemSize);
            bool col = ImGui.ColorEdit3(label + "color", ref tmp, (ImGuiColorEditFlags) flags);
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
            bool col = ImGui.ColorEdit4(label + "color", ref tmp, (ImGuiColorEditFlags) flags);
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
            bool x = ImGui.InputFloat(label+"X", ref val.X, step, stepFast, null, (ImGuiInputTextFlags) flags);
            
            TextInlined("Y");
            SetNextItemWidth(itemSize);
            bool y = ImGui.InputFloat(label+"Y", ref val.Y, step, stepFast, null, (ImGuiInputTextFlags) flags);
            
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

        public static bool InputFloat(string label, ref float val, float step, float stepFast, GUIInputTextFlags flags, string fmt)
        {
            float itemSize = GUIUtility.GetPreferredElementWidthSize(1, !string.IsNullOrEmpty(fmt));
            GUIUtility.TextIndentFunction(fmt);

            SetNextItemWidth(itemSize);
            return ImGui.InputFloat(label, ref val, step, stepFast, null, (ImGuiInputTextFlags) flags);
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
            return ImGui.InputInt(label, ref val, step, stepFast, (ImGuiInputTextFlags) flags);
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
                valPtr = (IntPtr) ptr;
            }
            if (step.HasValue) {
                T temp = step.Value;
                stepPtr = (IntPtr) (&temp);
            }
            if (stepFast.HasValue) {
                T temp = stepFast.Value;
                stepFastPtr = (IntPtr) (&temp);
            }

            SetNextItemWidth(itemSize);
            return ImGui.InputScalar(label, dataType, valPtr, stepPtr, stepFastPtr, fmt, (ImGuiInputTextFlags)flags);
        }

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
