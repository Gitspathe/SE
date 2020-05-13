using System;
using System.Collections.Generic;
using SE.Common;

namespace SE.Attributes
{
    /// <summary>
    /// When a GameObject is initialized, this attribute controls which components the GameObject contains. Components included within this attribute
    /// will be serialized to the scene, unless they have 'NeverSerialize' set to true, or the GameObject has 'SerializeToScene' set to false.
    /// Components may also be added to the GameObject manually during run-time, however these components WILL NOT be serialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentsAttribute : Attribute
    {
        public List<Type> Components = new List<Type>();

        public ComponentsAttribute(params Type[] componentTypes)
        {
            for (int i = 0; i < componentTypes.Length; i++) {
                Type t = componentTypes[i];
                if (!t.IsSubclassOf(typeof(Component)))
                    throw new Exception(nameof(t) + " is not of type Component.");

                Components.Add(t);
            }
        }
    }
}