using System;

namespace SE.Editor.GUI.Properties
{
    public abstract class PropertiesNode<T> : GUIObject
    {
        public T Internal { get; }
        public abstract Type NodeType { get; }

        public override void OnPaint()
        {
            throw new NotImplementedException();
        }

        public PropertiesNode(T obj)
        {
            Internal = obj;
        }
    }
}
