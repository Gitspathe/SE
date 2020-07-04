using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DeeZ.Editor.GUI.ValueDrawers;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SE.Core;
using SE.Core.Internal;
using SE.Engine.Utility;
using SE.Serialization;
using SE.Utility;
using SE.World;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace DeeZ.Editor.GUI
{
    public static class EditorGUIHelper
    {
        public static Dictionary<Type, IGUIValueDrawer> GUITable = new Dictionary<Type, IGUIValueDrawer>();
        public static Dictionary<Type, IGenericGUIValueDrawer> GenericGUITable = new Dictionary<Type, IGenericGUIValueDrawer>();

        public static bool TryDisplayValue(int labelIndex, SerializedValue value)
        {
            if (value == null || value.Value == null)
                return false;

            if (value.ReflectionInfo == null)
                BuildReflectionInfo(value);

            switch (value.ReflectionInfo?.ValueType) {
                case SerializedValue.VarType.None:
                    return false;

                case SerializedValue.VarType.Interface:
                case SerializedValue.VarType.Class: {
                    if (GUITable.TryGetValue(value.ReflectionInfo.Type, out IGUIValueDrawer drawer)) {
                        value.Value = drawer.Display(labelIndex, value.Value);
                    }
                    return true;
                }

                case SerializedValue.VarType.GenericInterface:
                case SerializedValue.VarType.GenericClass: {
                    IGenericGUIValueDrawer drawer = GenericGUITable[value.ReflectionInfo.genericType];
                    drawer.Display(labelIndex, value.ReflectionInfo.innerType, value);
                    return true;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void BuildReflectionInfo(SerializedValue value)
        {
            Type type = value.Value.GetType();
            bool isGeneric = type.IsGenericType;
            bool found = false;
            value.ReflectionInfo = new SerializedValue.Cache {
                Type = type
            };

            // Step 1: Regular class.
            if (GUITable.ContainsKey(type)) {
                value.ReflectionInfo.ValueType = SerializedValue.VarType.Class;
                found = true;
            }

            // Step 2: Generic class.
            if (!found) {
                if (isGeneric) {
                    Type genericType = type.GetGenericTypeDefinition();
                    Type innerType = type.GetGenericArguments()[0];
                    if (GenericGUITable.ContainsKey(genericType)) {
                        value.ReflectionInfo.ValueType = SerializedValue.VarType.GenericClass;
                        value.ReflectionInfo.genericType = genericType;
                        value.ReflectionInfo.innerType = innerType;
                        found = true;
                    }
                }
            }

            // Step 3: Interfaces.
            if (!found) {
                foreach (Type interfaceType in type.GetInterfaces()) {
                    bool isInterfaceGeneric = interfaceType.IsGenericType;
                    if (isInterfaceGeneric) {
                        Type genericType = interfaceType.GetGenericTypeDefinition();
                        Type innerType = interfaceType.GetGenericArguments()[0];
                        if (GenericGUITable.ContainsKey(genericType)) {
                            value.ReflectionInfo.ValueType = SerializedValue.VarType.GenericInterface;
                            value.ReflectionInfo.genericType = genericType;
                            value.ReflectionInfo.innerType = innerType;
                            found = true;
                        }
                    } else {
                        if (GUITable.ContainsKey(interfaceType)) {
                            value.ReflectionInfo.ValueType = SerializedValue.VarType.Interface;
                            found = true;
                        }
                    }
                }
            }

            if (!found) {
                value.ReflectionInfo.ValueType = SerializedValue.VarType.None;
            }
        }
    }

}
