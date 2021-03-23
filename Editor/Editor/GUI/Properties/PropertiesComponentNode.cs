using System;
using SE.Common;
using SE.Serialization;
using SE.Utility;

namespace SE.Editor.GUI.Properties
{
    public class PropertiesComponentNode : PropertiesNode<Component>
    {
        public override Type NodeType => typeof(Component);

        private QuickList<PropertiesValue> objects = new QuickList<PropertiesValue>();

        public override void OnPaint()
        {
            if (Internal.Serializer != null) {
                for (int i = 0; i < objects.Count; i++) {
                    objects.Array[i].OnPaint();
                }
            }
        }

        public void Reset()
        {
            objects.Clear();
            if(Internal.Serializer == null)
                return;

            for (int i = 0; i < Internal.Serializer.ValueWrappers.Count; i++) {
                SerializedValue value = Internal.Serializer.ValueWrappers.Array[i];
                if(value == null || value.Value == null)
                    continue;

                objects.Add(new PropertiesValue(i, this, value));
            }
        }

        public PropertiesComponentNode(Component component) : base(component)
        {
            Reset();
        }
    }
}
