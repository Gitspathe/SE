using System;
using System.Collections.Generic;
using ImGuiNET;
using SE.Serialization;

namespace SE.Editor.GUI.ValueDrawers
{
    public interface IGenericGUIValueDrawer
    {
        Type ValueType { get; }
        void Display(int index, Type innerType, SerializedValue valueBase);
    }

    public class GUIIListDrawer : IGenericGUIValueDrawer
    {
        public Type ValueType => typeof(IList<>);

        public void Display(int index, Type innerType, SerializedValue valueBase)
        {
            bool isArray = valueBase.Value.GetType().IsArray;
            if (ImGui.TreeNode((isArray ? "Array<" : "List<") + innerType.Name + ">###")) {
                if (isArray) {
                    for (int y = 0; y < valueBase.Value.Length; y++) {
                        DisplayItem(valueBase.Value[y], y);
                    }
                } else {
                    for (int y = 0; y < valueBase.Value.Count; y++) {
                        DisplayItem(valueBase.Value[y], y);
                    }
                }

                ImGui.TreePop();
            }

            void DisplayItem(dynamic item, int y)
            {
                if (EditorGUIHelper.GUITable.TryGetValue(item.GetType(), out IGUIValueDrawer drawer)) {
                    valueBase.Value[y] = drawer.Display(y, item);
                }
            }
        }
    }
}
