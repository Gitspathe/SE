using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SE.Engine.Utility;
using SE.Serialization;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace DeeZ.Editor.GUI
{
    public static class EditorGUIHelper
    {
        public static Dictionary<Type, Func<int, dynamic, dynamic>> GUITable = new Dictionary<Type, Func<int, dynamic, dynamic>> {
            {
                typeof(bool), (i, inVar) => {
                    bool outVar = inVar;
                    GUI.Checkbox("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(byte), (i, inVar) => {
                    byte outVar = inVar;
                    GUI.InputByte("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(sbyte), (i, inVar) => {
                    sbyte outVar = inVar;
                    GUI.InputSByte("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(short), (i, inVar) => {
                    short outVar = inVar;
                    GUI.InputShort("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(ushort), (i, inVar) => {
                    ushort outVar = inVar;
                    GUI.InputUShort("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(int), (i, inVar) => {
                    int outVar = inVar;
                    GUI.InputInt("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(uint), (i, inVar) => {
                    uint outVar = inVar;
                    GUI.InputUInt("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(long), (i, inVar) => {
                    long outVar = inVar;
                    GUI.InputLong("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(ulong), (i, inVar) => {
                    ulong outVar = inVar;
                    GUI.InputULong("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(float), (i, inVar) => {
                    float outVar = inVar;
                    GUI.InputFloat("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(double), (i, inVar) => {
                    double outVar = inVar;
                    GUI.InputDouble("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(Color), (i, inVar) => {
                    Color outVar = inVar;
                    GUI.InputColor4("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(Vector2), (i, inVar) => {
                    Vector2 outVar = inVar;
                    GUI.InputVector2("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(Rectangle), (i, inVar) => {
                    Rectangle outVar = inVar;
                    GUI.InputRectangle("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(RectangleF), (i, inVar) => {
                    RectangleF outVar = inVar;
                    GUI.InputRectangleF("##" + i, ref outVar);
                    return outVar;
                }
            },
            {
                typeof(string), (i, inVar) => {
                    string outVar = inVar;
                    GUI.InputText("##" + i, ref outVar);
                    return outVar;
                }
            }
        };


        public static Dictionary<Type, Action<int, Type, SerializedValue>> GenericGUITable = new Dictionary<Type, Action<int, Type, SerializedValue>> {
            {
                typeof(List<>), (i, innerType, valueBase) => {
                    if (ImGui.TreeNode("List<" + innerType.Name + ">###")) {
                        for (int y = 0; y < valueBase.Value.Count; y++) {
                            dynamic item = valueBase.Value[y];
                            if (GUITable.TryGetValue(item.GetType(), out Func<int, dynamic, dynamic> func)) {
                                valueBase.Value[y] = func.Invoke(y, item);
                            }
                        }
                        ImGui.TreePop();
                    }
                }
            },
        };

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
                    if (GUITable.TryGetValue(value.ReflectionInfo.Type, out Func<int, dynamic, dynamic> func)) {
                        value.Value = func.Invoke(labelIndex, value.Value);
                    }
                    return true;
                }

                case SerializedValue.VarType.GenericInterface:
                case SerializedValue.VarType.GenericClass: {
                    Action<int, Type, SerializedValue> func = GenericGUITable[value.ReflectionInfo.genericType];
                    func.Invoke(labelIndex, value.ReflectionInfo.innerType, value);
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
