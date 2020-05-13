using DeeZ.Engine.Common;
using DeeZ.Engine.Utility;
using System.Numerics;

namespace DeeZ.Editor.GUI.Hierarchy
{
    public class Hierarchy : GUIObject
    {
        internal int currentNodeIndex;
        internal int selectedNode = -1;

        internal QuickList<HierarchyNode> nodes = new QuickList<HierarchyNode>();
        internal QuickList<GameObject> allGameObjects = new QuickList<GameObject>();
        private bool isDirty = true;

        public override void OnPaint()
        {
            currentNodeIndex = 0;
            isDirty = EngineUtility.TransformHierarchyDirty;
            if (isDirty) {
                ResetNodes();
                isDirty = false;
            }

            GUI.Begin("Hierarchy");
            GUI.PushStyleVar(GUIStyleVar.ItemSpacing, new Vector2(3.0f, 3.0f));
            foreach (HierarchyNode node in nodes) {
                node.OnPaint();
            }
            GUI.PopStyleVar();
            GUI.End();
        }

        public void ResetNodes()
        {
            nodes.Clear();
            allGameObjects.Clear();
            foreach (GameObject go in GameEngine.AllGameObjects) {
                if (go.Transform.Parent == null) {
                    nodes.Add(new HierarchyNode(this, go));
                }
            }
        }
    }
}
