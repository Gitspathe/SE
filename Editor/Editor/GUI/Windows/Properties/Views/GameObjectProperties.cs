using System.Numerics;
using ImGuiNET;
using SE.Common;
using SE.Utility;
using FileIO = SE.Core.FileIO;

namespace SE.Editor.GUI.Windows.Properties.Views
{
    public class GameObjectProperties : PropertiesView<GameObject>
    {
        public GameObject SelectedGameObject {
            get => selectedGameObject;
            set {
                if (value != selectedGameObject) {
                    selectedGameObject = value;
                    isDirty = true;
                }
            }
        }
        private GameObject selectedGameObject;

        private QuickList<PropertiesComponentNode> componentNodes = new QuickList<PropertiesComponentNode>();
        private PropertiesTransformNode transformNode;

        private bool isDirty = true;

        public GameObjectProperties() { }

        public GameObjectProperties(GameObject go)
        {
            SelectedGameObject = go;
        }

        public override void OnPaint()
        {
            if (isDirty) {
                Reset();
                isDirty = false;
            }

            // Sanity check: make sure the selected gameObject is in a correct state.
            if (SelectedGameObject == null || selectedGameObject.Destroyed) {
                selectedGameObject = null;
                return;
            }
            GUI.PushID(selectedGameObject.GetHashCode());

            string goName = selectedGameObject.EngineName;
            bool enabled = selectedGameObject.Enabled;
            GUI.Checkbox("##goProp##Enabled", ref enabled);
            GUI.SameLine();
            GUI.InputText("##goProp##EngineName", ref goName);

            if (ImGui.CollapsingHeader("Transform", TransformHeaderFlags)) {
                GUI.PushID("Transform");
                transformNode.OnPaint();
                GUI.PopID();
            }

            for (int i = 0; i < componentNodes.Count; i++) {
                PropertiesComponentNode componentNode = componentNodes.Array[i];
                Component component = componentNodes.Array[i].Internal;

                bool cEnabled = component.Enabled;
                GUI.Checkbox("##goComp##Enabled" + i, ref cEnabled);
                GUI.SameLine();
                component.Enabled = cEnabled;

                if (!component.Enabled) {
                    PushComponentHeaderDisabledStyle();
                }

                string label = componentNode.Internal.GetType().Name + "##" + i;
                bool open = ImGui.CollapsingHeader(label, ComponentHeaderNodeFlags);
                if (open) {
                    GUI.PushID(label);
                    componentNode.OnPaint();
                    GUI.PopID();
                }

                if (!component.Enabled) {
                    PopComponentHeaderDisabledStyle();
                }
            }

            // Temporary
            if (ImGui.Button("Save")) {
                FileIO.SaveFile(selectedGameObject.SerializeJson(), "OBJECT");
            }
            if (ImGui.Button("Load")) {
                selectedGameObject.Deserialize(FileIO.ReadFileString("OBJECT"));
            }

            GUI.PopID();

            selectedGameObject.EngineName = goName;
            selectedGameObject.SetActive(enabled);
        }

        public override void Reset()
        {
            componentNodes.Clear();
            if (selectedGameObject != null) {
                transformNode = new PropertiesTransformNode(selectedGameObject.Transform);
                for (int i = 0; i < selectedGameObject.Components.Count; i++) {
                    componentNodes.Add(new PropertiesComponentNode(selectedGameObject.Components.Array[i]));
                }
            } else {
                transformNode = null;
            }
        }

        private void PushComponentHeaderDisabledStyle()
        {
            GUI.PushStyleColor(GUIColor.Header, new Vector4(0.333f, 0.333f, 0.333f, 1.0f));
        }

        private void PopComponentHeaderDisabledStyle()
        {
            GUI.PopStyleColor();
        }

        public ImGuiTreeNodeFlags TransformHeaderFlags => ImGuiTreeNodeFlags.DefaultOpen;
        public ImGuiTreeNodeFlags ComponentHeaderNodeFlags => ImGuiTreeNodeFlags.DefaultOpen;
    }
}
