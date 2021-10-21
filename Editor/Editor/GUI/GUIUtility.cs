using System;
using System.Collections.Generic;
using ImGuiNET;

namespace SE.Editor.GUI
{
    public static class GUIUtility
    {
        // TODO: Push & pop margin (for indentation)

        internal static Stack<float> MarginStack = new Stack<float>();

        public static float LabelMargin => MarginStack.Count > 0 ? MarginStack.Peek() : 0.0f;
        public static float LabelIndent => ImGui.GetContentRegionAvail().X * LabelMargin;

        public static float GetPreferredElementWidthSize(int numElements, bool indent = true)
        {
            if (numElements < 1)
                throw new Exception("Invalid number of elements.");

            float labelIndent = 0f;
            if (indent)
                labelIndent = LabelIndent;

            float totalWidth = ImGui.GetContentRegionAvail().X;
            float safeAreaPerElement = 0.025f;
            float width = totalWidth - labelIndent;
            if (numElements == 1)
                return width;

            return width * ((1.0f / numElements) - (safeAreaPerElement * numElements));
        }

        public static void TextIndentFunction(string fmt)
        {
            bool hasFmt = !string.IsNullOrEmpty(fmt);
            if (hasFmt) {
                GUI.Text(fmt);
                GUI.SameLine(LabelIndent);
            }
        }
    }
}
