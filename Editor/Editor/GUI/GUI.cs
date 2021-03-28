using System;
using System.Runtime.InteropServices;
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

        public static bool ArrowButton(string strID, ImGuiDir dir) => ImGui.ArrowButton(strID, dir);
        public static bool BeginChild(string strID) => ImGui.BeginChild(strID);
        public static bool BeginChild(string strID, Vector2 size) => ImGui.BeginChild(strID, size);
        public static bool BeginChild(string strID, Vector2 size, bool border, GUIWindowFlags flags) => ImGui.BeginChild(strID, size, border, (ImGuiWindowFlags)flags);
        public static bool BeginChild(uint id) => ImGui.BeginChild(id);
        public static bool BeginChild(uint id, Vector2 size) => ImGui.BeginChild(id, size);
        public static bool BeginChild(uint id, Vector2 size, bool border, GUIWindowFlags flags) => ImGui.BeginChild(id, size, border, (ImGuiWindowFlags)flags);
        public static bool BeginChildFrame(uint id, Vector2 size) => ImGui.BeginChildFrame(id, size);
        public static bool BeginChildFrame(uint id, Vector2 size, GUIWindowFlags flags) => ImGui.BeginChildFrame(id, size, (ImGuiWindowFlags)flags);
        public static bool BeginCombo(string label, string previewValue) => ImGui.BeginCombo(label, previewValue);
        public static bool BeginCombo(string label, string previewValue, GUIComboFlags flags) => ImGui.BeginCombo(label, previewValue, (ImGuiComboFlags)flags);
        public static void BeginGroup() => ImGui.BeginGroup();
        public static bool BeginPopupContextItem() => ImGui.BeginPopupContextItem();
        public static bool BeginPopupContextItem(string id) => ImGui.BeginPopupContextItem(id);
        public static bool BeginPopupContextItem(string id, GUIPopupFlags flags) => ImGui.BeginPopupContextItem(id, (ImGuiPopupFlags)flags);
        public static bool BeginPopupContextVoid() => ImGui.BeginPopupContextVoid();
        public static bool BeginPopupContextVoid(string id) => ImGui.BeginPopupContextVoid(id);
        public static bool BeginPopupContextVoid(string id, GUIPopupFlags flags) => ImGui.BeginPopupContextVoid(id, (ImGuiPopupFlags)flags);
        public static bool BeginPopupContextWindow() => ImGui.BeginPopupContextWindow();
        public static bool BeginPopupContextWindow(string id) => ImGui.BeginPopupContextWindow(id);
        public static bool BeginPopupContextWindow(string id, GUIPopupFlags flags) => ImGui.BeginPopupContextWindow(id, (ImGuiPopupFlags)flags);
        public static bool BeginPopupModal(string name) => ImGui.BeginPopupModal(name);
        public static bool BeginPopupModal(string name, ref bool pOpen) => ImGui.BeginPopupModal(name, ref pOpen);
        public static bool BeginPopupModal(string name, ref bool pOpen, GUIWindowFlags flags) => ImGui.BeginPopupModal(name, ref pOpen, (ImGuiWindowFlags)flags);
        public static bool BeginTabBar(string id) => ImGui.BeginTabBar(id);
        public static bool BeginTabBar(string id, GUITabBarFlags flags) => ImGui.BeginTabBar(id, (ImGuiTabBarFlags)flags);
        public static bool BeginTabItem(string label) => ImGui.BeginTabItem(label);
        public static bool BeginTabItem(string label, ref bool pOpen) => ImGui.BeginTabItem(label, ref pOpen);
        public static bool BeginTabItem(string label, ref bool pOpen, GUITabItemFlags flags) => ImGui.BeginTabItem(label, ref pOpen, (ImGuiTabItemFlags)flags);
        public static void BeginTooltip() => ImGui.BeginTooltip();
        public static void Bullet() => ImGui.Bullet();
        public static void BulletText(string fmt) => ImGui.BulletText(fmt);
        public static bool Button(string label) => ImGui.Button(label);
        public static bool Button(string label, Vector2 size) => ImGui.Button(label, size);
        public static float CalcItemWidth() => ImGui.CalcItemWidth();
        public static Vector2 CalcTextSize(string text) => ImGui.CalcTextSize(text);
        public static void CaptureKeyboardFromApp() => ImGui.CaptureKeyboardFromApp();
        public static void CaptureKeyboardFromApp(bool wantCaptureKeyboardValue) => ImGui.CaptureKeyboardFromApp(wantCaptureKeyboardValue);
        public static void CaptureMouseFromApp() => ImGui.CaptureMouseFromApp();
        public static void CaptureMouseFromApp(bool wantCaptureMouseValue) => ImGui.CaptureMouseFromApp(wantCaptureMouseValue);
        public static bool CheckboxFlags(string label, ref uint flags, uint flagsVal) => ImGui.CheckboxFlags(label, ref flags, flagsVal);
        public static void CloseCurrentPopup() => ImGui.CloseCurrentPopup();
        public static bool ColorButton(string descID, Vector4 col) => ImGui.ColorButton(descID, col);
        public static bool ColorButton(string descID, Vector4 col, GUIColorEditFlags flags) => ImGui.ColorButton(descID, col, (ImGuiColorEditFlags)flags);
        public static bool ColorButton(string descID, Vector4 col, GUIColorEditFlags flags, Vector2 size) => ImGui.ColorButton(descID, col, (ImGuiColorEditFlags)flags, size);
        public static uint ColorConvertFloat4ToU32(Vector4 col) => ImGui.ColorConvertFloat4ToU32(col);
        public static void ColorConvertHsVtoRgb(float h, float s, float v, out float outR, out float outG, out float outB) => ImGui.ColorConvertHSVtoRGB(h, s, v, out outR, out outG, out outB);
        public static void ColorConvertRgBtoHsv(float r, float g, float b, out float outH, out float outS, out float outV) => ImGui.ColorConvertRGBtoHSV(r, g, b, out outH, out outS, out outV);
        public static Vector4 ColorConvertU32ToFloat4(uint col) => ImGui.ColorConvertU32ToFloat4(col);
        public static bool ColorEdit3(string label, ref Vector3 col) => ImGui.ColorEdit3(label, ref col);
        public static bool ColorEdit3(string label, ref Vector3 col, GUIColorEditFlags flags) => ImGui.ColorEdit3(label, ref col, (ImGuiColorEditFlags)flags);
        public static bool ColorEdit4(string label, ref Vector4 col) => ImGui.ColorEdit4(label, ref col);
        public static bool ColorEdit4(string label, ref Vector4 col, GUIColorEditFlags flags) => ImGui.ColorEdit4(label, ref col, (ImGuiColorEditFlags)flags);
        public static bool ColorPicker3(string label, ref Vector3 col) => ImGui.ColorPicker3(label, ref col);
        public static bool ColorPicker3(string label, ref Vector3 col, GUIColorEditFlags flags) => ImGui.ColorPicker3(label, ref col, (ImGuiColorEditFlags)flags);
        public static bool ColorPicker4(string label, ref Vector4 col) => ImGui.ColorPicker4(label, ref col);
        public static bool ColorPicker4(string label, ref Vector4 col, GUIColorEditFlags flags) => ImGui.ColorPicker4(label, ref col, (ImGuiColorEditFlags)flags);
        public static bool ColorPicker4(string label, ref Vector4 col, GUIColorEditFlags flags, ref float refCol) => ImGui.ColorPicker4(label, ref col, (ImGuiColorEditFlags)flags, ref refCol);
        public static void Columns() => ImGui.Columns();
        public static void Columns(int count) => ImGui.Columns(count);
        public static void Columns(int count, string id) => ImGui.Columns(count, id);
        public static void Columns(int count, string id, bool border) => ImGui.Columns(count, id, border);
        public static bool Combo(string label, ref int currentItem, string[] items, int itemsCount) 
            => ImGui.Combo(label, ref currentItem, items, itemsCount);
        public static bool Combo(string label, ref int currentItem, string[] items, int itemsCount, int popupMaxHeightInItems) 
            => ImGui.Combo(label, ref currentItem, items, itemsCount, popupMaxHeightInItems);
        public static bool Combo(string label, ref int currentItem, string itemsSeparatedByZeros)
            => ImGui.Combo(label, ref currentItem, itemsSeparatedByZeros);
        public static bool Combo(string label, ref int currentItem, string itemsSeparatedByZeros, int popupMaxHeightInItems) 
            => ImGui.Combo(label, ref currentItem, itemsSeparatedByZeros, popupMaxHeightInItems);
        public static IntPtr CreateContext() => ImGui.CreateContext();
        public static IntPtr CreateContext(ImFontAtlasPtr sharedFontAtlas) => ImGui.CreateContext(sharedFontAtlas);
        public static void DockSpace(uint id) 
            => ImGui.DockSpace(id);
        public static void DockSpace(uint id, Vector2 size) 
            => ImGui.DockSpace(id, size);
        public static void DockSpace(uint id, Vector2 size, GUIDockNodeFlags flags) 
            => ImGui.DockSpace(id, size, (ImGuiDockNodeFlags)flags);
        public static void DockSpace(uint id, Vector2 size, GUIDockNodeFlags flags, ImGuiWindowClassPtr windowClass) 
            => ImGui.DockSpace(id, size, (ImGuiDockNodeFlags) flags, windowClass);


        public static uint DockSpaceOverViewport() 
            => ImGui.DockSpaceOverViewport();

        public static uint DockSpaceOverViewport(ImGuiViewportPtr viewport) 
            => ImGui.DockSpaceOverViewport(viewport);

        public static uint DockSpaceOverViewport(ImGuiViewportPtr viewport, GUIDockNodeFlags flags)
            => ImGui.DockSpaceOverViewport(viewport, (ImGuiDockNodeFlags) flags);

        public static uint DockSpaceOverViewport(ImGuiViewportPtr viewport, GUIDockNodeFlags flags, ImGuiWindowClassPtr windowClass) 
            => ImGui.DockSpaceOverViewport(viewport, (ImGuiDockNodeFlags) flags, windowClass);

        //public static bool DragFloat(string label, ref float v)
        //    => ImGui.DragFloat(label, ref v);

        //public static bool DragFloat(string label, ref float v, float vSpeed)
        //    => ImGui.DragFloat(label, ref v, vSpeed);

        //public static bool DragFloat(string label, ref float v, float vSpeed, float vMin) 
        //    => ImGui.DragFloat(label, ref v, vSpeed, vMin);

        //public static bool DragFloat(string label, ref float v, float vSpeed, float vMin, float vMax)
        //    => ImGui.DragFloat(label, ref v, vSpeed, vMin, vMax);

        //public static bool DragFloat(string label, ref float v, float vSpeed, float vMin, float vMax, string format)
        //    => ImGui.DragFloat(label, ref v, vSpeed, vMin, vMax, format);

        //public static bool DragFloat(string label, ref float v, float vSpeed, float vMin, float vMax, string format, GUISliderFlags flags)
        //    => ImGui.DragFloat(label, ref v, vSpeed, vMin, vMax, format, (ImGuiSliderFlags) flags);

        //public static bool DragFloat2(string label, ref Vector2 v)
        //    => ImGui.DragFloat2(label, ref v);
        //public static bool DragFloat2(string label, ref Vector2 v, float vSpeed)
        //    => ImGui.DragFloat2(label, ref v, vSpeed);
        //public static bool DragFloat2(string label, ref Vector2 v, float vSpeed, float vMin)
        //    => ImGui.DragFloat2(label, ref v, vSpeed, vMin);
        //public static bool DragFloat2(string label, ref Vector2 v, float vSpeed, float vMin, float vMax)
        //    => ImGui.DragFloat2(label, ref v, vSpeed, vMin, vMax);
        //public static bool DragFloat2(string label, ref Vector2 v, float vSpeed, float vMin, float vMax, string format)
        //    => ImGui.DragFloat2(label, ref v, vSpeed, vMin, vMax, format);
        //public static bool DragFloat2(string label, ref Vector2 v, float vSpeed, float vMin, float vMax, string format, GUISliderFlags flags) 
        //    => ImGui.DragFloat2(label, ref v, vSpeed, vMin, vMax, format, (ImGuiSliderFlags)flags);

        //public static bool DragFloat3(string label, ref Vector3 v)
        //    => ImGui.DragFloat3(label, ref v);
        //public static bool DragFloat3(string label, ref Vector3 v, float vSpeed)
        //    => ImGui.DragFloat3(label, ref v, vSpeed);
        //public static bool DragFloat3(string label, ref Vector3 v, float vSpeed, float vMin)
        //    => ImGui.DragFloat3(label, ref v, vSpeed, vMin);
        //public static bool DragFloat3(string label, ref Vector3 v, float vSpeed, float vMin, float vMax)
        //    => ImGui.DragFloat3(label, ref v, vSpeed, vMin, vMax);
        //public static bool DragFloat3(string label, ref Vector3 v, float vSpeed, float vMin, float vMax, string format)
        //    => ImGui.DragFloat3(label, ref v, vSpeed, vMin, vMax, format);
        //public static bool DragFloat3(string label, ref Vector3 v, float vSpeed, float vMin, float vMax, string format, GUISliderFlags flags)
        //    => ImGui.DragFloat3(label, ref v, vSpeed, vMin, vMax, format, (ImGuiSliderFlags)flags);

        //public static bool DragFloat4(string label, ref Vector4 v)
        //    => ImGui.DragFloat4(label, ref v);
        //public static bool DragFloat4(string label, ref Vector4 v, float vSpeed)
        //    => ImGui.DragFloat4(label, ref v, vSpeed);
        //public static bool DragFloat4(string label, ref Vector4 v, float vSpeed, float vMin)
        //    => ImGui.DragFloat4(label, ref v, vSpeed, vMin);
        //public static bool DragFloat4(string label, ref Vector4 v, float vSpeed, float vMin, float vMax)
        //    => ImGui.DragFloat4(label, ref v, vSpeed, vMin, vMax);
        //public static bool DragFloat4(string label, ref Vector4 v, float vSpeed, float vMin, float vMax, string format)
        //    => ImGui.DragFloat4(label, ref v, vSpeed, vMin, vMax, format);
        //public static bool DragFloat4(string label, ref Vector4 v, float vSpeed, float vMin, float vMax, string format, GUISliderFlags flags)
        //    => ImGui.DragFloat4(label, ref v, vSpeed, vMin, vMax, format, (ImGuiSliderFlags)flags);

        //public static bool DragFloatRange2(string label, ref float vCurrentMin, ref float vCurrentMax)
        //    => ImGui.DragFloatRange2(label, ref vCurrentMin, ref vCurrentMax);
        //public static bool DragFloatRange2(string label, ref float vCurrentMin, ref float vCurrentMax, float vSpeed)
        //    => ImGui.DragFloatRange2(label, ref vCurrentMin, ref vCurrentMax, vSpeed);
        //public static bool DragFloatRange2(string label, ref float vCurrentMin, ref float vCurrentMax, float vSpeed, float vMin)
        //    => ImGui.DragFloatRange2(label, ref vCurrentMin, ref vCurrentMax, vSpeed, vMin);
        //public static bool DragFloatRange2(string label, ref float vCurrentMin, ref float vCurrentMax, float vSpeed, float vMin, float vMax)
        //    => ImGui.DragFloatRange2(label, ref vCurrentMin, ref vCurrentMax, vSpeed, vMin, vMax);
        //public static bool DragFloatRange2(string label, ref float vCurrentMin, ref float vCurrentMax, float vSpeed, float vMin, float vMax, string format)
        //    => ImGui.DragFloatRange2(label, ref vCurrentMin, ref vCurrentMax, vSpeed, vMin, vMax, format);
        //public static bool DragFloatRange2(string label, ref float vCurrentMin, ref float vCurrentMax, float vSpeed, float vMin, float vMax, string format, string formatMax)
        //    => ImGui.DragFloatRange2(label, ref vCurrentMin, ref vCurrentMax, vSpeed, vMin, vMax, format, formatMax);
        //public static bool DragFloatRange2(string label, ref float vCurrentMin, ref float vCurrentMax, float vSpeed, float vMin, float vMax, string format, string formatMax, GUISliderFlags flags)
        //    => ImGui.DragFloatRange2(label, ref vCurrentMin, ref vCurrentMax, vSpeed, vMin, vMax, format, formatMax, (ImGuiSliderFlags) flags);

        //public static bool DragInt(string label, ref int v)
        //    => ImGui.DragInt(label, ref v);
        //public static bool DragInt(string label, ref int v, float vSpeed)
        //    => ImGui.DragInt(label, ref v, vSpeed);
        //public static bool DragInt(string label, ref int v, float vSpeed, int vMin)
        //    => ImGui.DragInt(label, ref v, vSpeed, vMin);
        //public static bool DragInt(string label, ref int v, float vSpeed, int vMin, int vMax)
        //    => ImGui.DragInt(label, ref v, vSpeed, vMin, vMax);
        //public static bool DragInt(string label, ref int v, float vSpeed, int vMin, int vMax, string format)
        //    => ImGui.DragInt(label, ref v, vSpeed, vMin, vMax, format);
        //public static bool DragInt(string label, ref int v, float vSpeed, int vMin, int vMax, string format, GUISliderFlags flags)
        //    => ImGui.DragInt(label, ref v, vSpeed, vMin, vMax, format, (ImGuiSliderFlags) flags);
        
        // TODO: From line 3044 in ImGui.cs is DragInt2..4, but wtf does it do??

        public static void Dummy(Vector2 size) => ImGui.Dummy(size);
        public static void EndCombo() => ImGui.EndCombo();
        public static void EndFrame() => ImGui.EndFrame();
        public static void EndGroup() => ImGui.EndGroup();
        public static void EndMenu() => ImGui.EndMenu();
        public static void EndMenuBar() => ImGui.EndMenuBar();
        public static void EndTabBar() => ImGui.EndTabBar();
        public static void EndTabItem() => ImGui.EndTabItem();
        public static void EndTooltip() => ImGui.EndTooltip();

        public static ImGuiViewportPtr FindViewportByID(uint id) => ImGui.FindViewportByID(id);
        public static ImGuiViewportPtr FindViewportByPlatformHandle(IntPtr platformHandle) => ImGui.FindViewportByPlatformHandle(platformHandle);
        public static ImDrawListPtr GetBackgroundDrawList() => ImGui.GetBackgroundDrawList();
        public static ImDrawListPtr GetBackgroundDrawList(ImGuiViewportPtr viewport) => ImGui.GetBackgroundDrawList(viewport);
        public static string GetClipboardText() => ImGui.GetClipboardText();
        public static uint GetColorU32(ImGuiCol idx) => ImGui.GetColorU32(idx);
        public static uint GetColorU32(ImGuiCol idx, float alphaMul) => ImGui.GetColorU32(idx, alphaMul);
        public static uint GetColorU32(Vector4 col) => ImGui.GetColorU32(col);
        public static uint GetColorU32(uint col) => ImGui.GetColorU32(col);

        public static int GetColumnIndex() => ImGui.GetColumnIndex();
        public static float GetColumnOffset() => ImGui.GetColumnOffset();
        public static float GetColumnOffset(int columnIndex) => ImGui.GetColumnOffset(columnIndex);
        public static int GetColumnsCount() => ImGui.GetColumnsCount();
        public static float GetColumnWidth() => ImGui.GetColumnWidth();
        public static float GetColumnWidth(int columnIndex) => ImGui.GetColumnWidth(columnIndex);
        public static Vector2 GetContentRegionAvail() => ImGui.GetContentRegionAvail();
        public static IntPtr GetCurrentContext() => ImGui.GetCurrentContext();
        public static Vector2 GetCursorPos() => ImGui.GetCursorPos();
        public static float GetCursorPosX() => ImGui.GetCursorPosX();
        public static float GetCursorPosY() => ImGui.GetCursorPosY();
        public static Vector2 GetCursorScreenPos() => ImGui.GetCursorScreenPos();
        public static Vector2 GetCursorStartPos() => ImGui.GetCursorStartPos();
        public static ImDrawDataPtr GetDrawData() => ImGui.GetDrawData();
        public static IntPtr GetDrawListSharedData() => ImGui.GetDrawListSharedData();
        public static ImFontPtr GetFont() => ImGui.GetFont();
        public static float GetFontSize() => ImGui.GetFontSize();
        public static Vector2 GetFontTexUvWhitePixel() => ImGui.GetFontTexUvWhitePixel();
        public static ImDrawListPtr GetForegroundDrawList() => ImGui.GetForegroundDrawList();
        public static ImDrawListPtr GetForegroundDrawList(ImGuiViewportPtr viewport) => ImGui.GetForegroundDrawList(viewport);
        public static int GetFrameCount() => ImGui.GetFrameCount();
        public static float GetFrameHeight() => ImGui.GetFrameHeight();
        public static float GetFrameHeightWithSpacing() => ImGui.GetFrameHeightWithSpacing();
        public static uint GetID(string strID) => ImGui.GetID(strID);
        public static uint GetID(IntPtr ptrID) => ImGui.GetID(ptrID);
        public static ImGuiIOPtr GetIO() => ImGui.GetIO();
        public static Vector2 GetItemRectMax() => ImGui.GetItemRectMax();
        public static Vector2 GetItemRectMin() => ImGui.GetItemRectMin();
        public static Vector2 GetItemRectSize() => ImGui.GetItemRectSize();
        public static int GetKeyIndex(ImGuiKey imguiKey) => ImGui.GetKeyIndex(imguiKey);
        public static int GetKeyPressedAmount(int keyIndex, float repeatDelay, float rate) => ImGui.GetKeyPressedAmount(keyIndex, repeatDelay, rate);
        public static ImGuiViewportPtr GetMainViewport() => ImGui.GetMainViewport();
        public static ImGuiMouseCursor GetMouseCursor() => ImGui.GetMouseCursor();
        public static Vector2 GetMouseDragDelta() => ImGui.GetMouseDragDelta();
        public static Vector2 GetMouseDragDelta(ImGuiMouseButton button) => ImGui.GetMouseDragDelta(button);
        public static Vector2 GetMouseDragDelta(ImGuiMouseButton button, float lockThreshold) => ImGui.GetMouseDragDelta(button, lockThreshold);
        public static Vector2 GetMousePos() => ImGui.GetMousePos();
        public static Vector2 GetMousePosOnOpeningCurrentPopup() => ImGui.GetMousePosOnOpeningCurrentPopup();
        public static ImGuiPlatformIOPtr GetPlatformIO() => ImGui.GetPlatformIO();
        public static float GetScrollMaxX() => ImGui.GetScrollMaxX();
        public static float GetScrollMaxY() => ImGui.GetScrollMaxY();
        public static float GetScrollX() => ImGui.GetScrollX();
        public static float GetScrollY() => ImGui.GetScrollY();
        public static ImGuiStoragePtr GetStateStorage() => ImGui.GetStateStorage();
        public static ImGuiStylePtr GetStyle() => ImGui.GetStyle();
        public static string GetStyleColorName(ImGuiCol idx) => ImGui.GetStyleColorName(idx);
        public static unsafe Vector4* GetStyleColorVec4(ImGuiCol idx) => ImGui.GetStyleColorVec4(idx);
        public static float GetTextLineHeight() => ImGui.GetTextLineHeight();
        public static float GetTextLineHeightWithSpacing() => ImGui.GetTextLineHeightWithSpacing();
        public static double GetTime() => ImGui.GetTime();
        public static float GetTreeNodeToLabelSpacing() => ImGui.GetTreeNodeToLabelSpacing();
        public static string GetVersion() => ImGui.GetVersion();
        public static Vector2 GetWindowContentRegionMax() => ImGui.GetWindowContentRegionMax();
        public static Vector2 GetWindowContentRegionMin() => ImGui.GetWindowContentRegionMin();
        public static float GetWindowContentRegionWidth() => ImGui.GetWindowContentRegionWidth();
        public static uint GetWindowDockID() => ImGui.GetWindowDockID();
        public static float GetWindowDpiScale() => ImGui.GetWindowDpiScale();
        public static ImDrawListPtr GetWindowDrawList() => ImGui.GetWindowDrawList();
        public static Vector2 GetWindowSize() => ImGui.GetWindowSize();
        public static ImGuiViewportPtr GetWindowViewport() => ImGui.GetWindowViewport();

        public static void SetNextWindowBgAlpha(float alpha) => ImGui.SetNextWindowBgAlpha(alpha);
        public static void SetNextWindowClass(ImGuiWindowClassPtr windowClass) => ImGui.SetNextWindowClass(windowClass);
        public static void SetNextWindowCollapsed(bool collapsed) => ImGui.SetNextWindowCollapsed(collapsed);
        public static void SetNextWindowCollapsed(bool collapsed, ImGuiCond cond) => ImGui.SetNextWindowCollapsed(collapsed, cond);
        public static void SetNextWindowContentSize(Vector2 size) => ImGui.SetNextWindowContentSize(size);
        public static void SetNextWindowDockID(uint dockID) => ImGui.SetNextWindowDockID(dockID);
        public static void SetNextWindowDockID(uint dockID, ImGuiCond cond) => ImGui.SetNextWindowDockID(dockID, cond);
        public static void SetNextWindowFocus() => ImGui.SetNextWindowFocus();
        public static void SetNextWindowPos(Vector2 pos) => ImGui.SetNextWindowPos(pos);
        public static void SetNextWindowPos(Vector2 pos, ImGuiCond cond) => ImGui.SetNextWindowPos(pos, cond);
        public static void SetNextWindowPos(Vector2 pos, ImGuiCond cond, Vector2 pivot) => ImGui.SetNextWindowPos(pos, cond, pivot);
        public static void SetNextWindowSize(Vector2 size) => ImGui.SetNextWindowSize(size);
        public static void SetNextWindowSize(Vector2 size, ImGuiCond cond) 
            => ImGui.SetNextWindowSize(size, cond);
        public static void SetNextWindowSizeConstraints(Vector2 sizeMin, Vector2 sizeMax)
            => ImGui.SetNextWindowSizeConstraints(sizeMin, sizeMax);
        public static void SetNextWindowSizeConstraints(Vector2 sizeMin, Vector2 sizeMax, ImGuiSizeCallback customCallback) 
            => ImGui.SetNextWindowSizeConstraints(sizeMin, sizeMax, customCallback);
        public static void SetNextWindowSizeConstraints(Vector2 sizeMin, Vector2 sizeMax, ImGuiSizeCallback customCallback, IntPtr customCallbackData) 
            => ImGui.SetNextWindowSizeConstraints(sizeMin, sizeMax, customCallback, customCallbackData);
        public static void SetNextWindowViewport(uint viewportID) => ImGui.SetNextWindowViewport(viewportID);
        public static void EndChild() => ImGui.EndChild();
        public static void EndChildFrame() => ImGui.EndChildFrame();

        // TODO: Continue from line 4845 in ImGui.cs.

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

        public static void SetWindowSize(Vector2 size) => ImGui.SetWindowSize(size);
        public static void SetWindowSize(string windowName, Vector2 size) => ImGui.SetWindowSize(windowName, size);
        public static void SetWindowSize(string windowName, Vector2 size, ImGuiCond cond) => ImGui.SetWindowSize(windowName, size, cond);
        public static void SetWindowSize(Vector2 size, ImGuiCond cond) => ImGui.SetWindowSize(size, cond);

        public static void AlignTextToFramePadding() => ImGui.AlignTextToFramePadding();
        public static void SetNextItemWidth(float width) => ImGui.SetNextItemWidth(width);

        public static void Indent() => SameLine(GUIUtility.LabelIndent);
        public static void SameLine() => ImGui.SameLine();
        public static void SameLine(float offsetX) => ImGui.SameLine(offsetX);
        public static void SameLine(float offsetX, float spacing) => ImGui.SameLine(offsetX, spacing);

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
    }
}
