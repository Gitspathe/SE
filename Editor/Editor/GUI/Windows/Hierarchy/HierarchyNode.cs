using ImGuiNET;
using SE.Common;
using SE.Core.Extensions;
using SE.Editor.GUI.Windows.Properties.Views;
using SE.Editor.GUI.Windows.Properties;
using SE.Utility;

namespace SE.Editor.GUI.Windows.Hierarchy
{
    public class HierarchyNode : GUIObject
    {
        public HierarchyWindow Hierarchy;
        public GameObject GameObject;
        public int NodeIndex;
        private QuickList<HierarchyNode> children = new QuickList<HierarchyNode>();

        public string GameObjectID { get; }

        public bool IsSelected {
            get => isSelected;
            private set {
                if(value == isSelected)
                    return;

                foreach (PropertiesWindow propWindow in EditorGUI.GetGUIObjects<PropertiesWindow>()) {
                    propWindow.View = value ? new GameObjectProperties(GameObject) : null;
                }
            }
        }
        private bool isSelected;

        public bool DisplayDisabled { get; private set; }

        public override void OnPaint()
        {
            Hierarchy.currentNodeIndex++;
            IsSelected = Hierarchy.selectedNode == Hierarchy.currentNodeIndex;

            bool disabled = !GameObject.Enabled || DisplayDisabled;
            ImGuiTreeNodeFlags flags = children.Count > 0 ? TreeNodeFlags : TreeLeafFlags;
            if (disabled) {
                GUI.PushStyleColor(GUIColor.Text, EditorGUI.Colors.TextDisabled);
            }
            if (IsSelected) {
                flags |= ImGuiTreeNodeFlags.Selected;
            }

            bool opened;
            if (children.Count > 0) {
                opened = ImGui.TreeNodeEx(GameObjectID, flags, GameObject.EngineName);
                if (ImGui.IsItemClicked()) {
                    Hierarchy.selectedNode = Hierarchy.currentNodeIndex;
                }
                DragDropSource();
                DragDropTarget();
                if (opened) {
                    UpdateChildren(disabled);
                }
            } else {
                opened = true;
                ImGui.TreeNodeEx(GameObjectID, flags, GameObject.EngineName);
                if (GUI.IsItemClicked()) {
                    Hierarchy.selectedNode = Hierarchy.currentNodeIndex;
                }
                DragDropSource();
                DragDropTarget();
            }

            if (opened) {
                GUI.TreePop();
            }

            if (disabled) {
                GUI.PopStyleColor();
            }
        }

        public struct Data
        {
            public int index;
        }

        private void DragDropSource()
        {
            if (GUI.BeginDragDropSource()) {
                Data d = new Data {
                    index = NodeIndex
                };

                string data = d.Serialize();
                GUI.SetDragDropPayload("Hierarchy", data);
                GUI.EndDragDropSource();
            }
        }

        private void DragDropTarget()
        {
            if (GUI.BeginDragDropTarget()) {
                string payload = GUI.AcceptDragDropPayload("Hierarchy");

                if (payload != null && GUI.IsMouseReleased(0)) {
                    Data data = payload.Deserialize<Data>();

                    GameObject go = Hierarchy.allGameObjects.Array[data.index];

                    go.Transform.SetParent(GameObject.Transform);
                    Hierarchy.ResetNodes();
                }
                GUI.EndDragDropTarget();
            }
        }

        private void UpdateChildren(bool childrenDisabled)
        {
            foreach (HierarchyNode child in children) {
                child.DisplayDisabled = childrenDisabled;
                child.OnPaint();
            }
        }

        public HierarchyNode(HierarchyWindow hierarchy, GameObject go)
        {
            Hierarchy = hierarchy;
            GameObject = go;
            GameObjectID = go.GetHashCode().ToString();
            NodeIndex = hierarchy.allGameObjects.Count;
            hierarchy.allGameObjects.Add(GameObject);
            foreach (Transform child in go.Transform.Children) {
                children.Add(new HierarchyNode(Hierarchy, child.GameObject));
            }
        }

        private ImGuiTreeNodeFlags TreeNodeFlags => ImGuiTreeNodeFlags.None 
                                                    | ImGuiTreeNodeFlags.OpenOnArrow 
                                                    | ImGuiTreeNodeFlags.OpenOnDoubleClick;

        private ImGuiTreeNodeFlags TreeLeafFlags => ImGuiTreeNodeFlags.None 
                                                    | ImGuiTreeNodeFlags.Leaf;
    }
}
