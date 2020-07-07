using FastMember;
using FastStream;
using SE.Engine.Networking.Internal;
using SE.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SE.Serialization
{
    internal class GeneratedSerializer : TypeSerializer
    {
        public override Type Type { get; }

        private QuickList<Node> nodeList = new QuickList<Node>();
        private Dictionary<string, Node> nodes = new Dictionary<string, Node>();

        private delegate object ObjectActivator();
        private ObjectActivator activator;
        private TypeAccessor accessor;
        private bool isValueType;

        private GeneratedSerializer(Type type)
        {
            Type = type;
            isValueType = type.IsValueType;
            accessor = TypeAccessor.Create(type, true);

            // Generate compiled constructors for reference types.
            if(!isValueType)
                GenerateCtor();

            foreach (Member member in accessor.GetMembers()) {
                
                // Example code of skipping non-public members.
                if(!member.IsPublic)
                    continue;

                TypeSerializer serializer = Serializer.GetSerializer(member.Type);

                // Skip this member if a valid serializer was not found.
                if (serializer == null) 
                    continue;

                //Node val = null;
                //if (member.Type.IsGenericType) {
                //    Type innerTYpe = member.Type.getgen
                //}

                Node val = new Node(serializer, member.Type, accessor, member.Name);
                nodes.Add(member.Name, val);
                nodeList.Add(val);
            }
        }

        public static GeneratedSerializer Generate(Type type)
        {
            GeneratedSerializer gen = new GeneratedSerializer(type);
            return gen.nodes.Count > 0 ? gen : null;
        }

        private void GenerateCtor()
        {
            ConstructorInfo emptyConstructor = Type.GetConstructor(Type.EmptyTypes);
            var dynamicMethod = new DynamicMethod("CreateInstance", Type, Type.EmptyTypes, true);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
            ilGenerator.Emit(OpCodes.Ret);
            activator = (ObjectActivator) dynamicMethod.CreateDelegate(typeof(ObjectActivator));
        }

        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings)
        {
            Node[] arr = nodeList.Array;
            for (int i = 0; i < nodeList.Count; i++) {
                try {
                    arr[i].Write(obj, writer, settings);
                } catch (Exception) { /* ignored */ }
            }
            writer.Write('|');
        }

        public override object Deserialize(FastReader reader, SerializerSettings settings)
        {
            object obj = isValueType ? Activator.CreateInstance(Type) : activator.Invoke();

            while (reader.PeekChar() != '|') {
                try {
                    string nextVarName = reader.ReadString();
                    nodes[nextVarName].Read(obj, reader, settings);
                } catch (Exception) { /* ignored */ }
            }

            // TODO: Why the fuck do I have to move 2 chars forward !?!?!?!?
            reader.BaseStream.Position += 2;

            return obj;
        }

        public T Deserialize<T>(FastReader reader, SerializerSettings settings) 
            => (T) Deserialize(reader, settings);

        private class Node
        {
            private TypeSerializer valueTypeSerializer;
            private TypeAccessor accessor;
            private string name;
            private Type type;

            // Generic type serializer variables.
            private Type[] innerTypes;
            private bool isGeneric;

            public Node(TypeSerializer serializer, Type type, TypeAccessor accessor, string name)
            {
                valueTypeSerializer = serializer;
                this.type = type;
                this.accessor = accessor;
                this.name = name;
                if (valueTypeSerializer.IsGeneric) {
                    innerTypes = Serializer.GetGenericInnerTypes(type);
                    isGeneric = true;
                } else {
                    isGeneric = false;
                }
            }

            public void Read(object target, FastReader reader, SerializerSettings settings)
            {
                // If the next value exists / isn't null, deserialize it's data.
                if (reader.ReadBoolean()) {
                    SetValue(target, isGeneric 
                        ? ((GenericTypeSerializer)valueTypeSerializer).DeserializeGeneric(innerTypes, reader, settings) 
                        : valueTypeSerializer.Deserialize(reader, settings));
                    return;
                } 
                
                // If the next value is null, set it to default, or ignore, depending on settings.
                if (settings.NullValueHandling == NullValueHandling.DefaultValue) {
                    SetValue(target, default);
                }
            }

            public void Write(object target, FastMemoryWriter writer, SerializerSettings settings)
            {
                object val = GetValue(target);
                writer.Write(name);
                writer.Write(val != null);
                if (val == null) 
                    return;

                if (isGeneric) {
                    ((GenericTypeSerializer)valueTypeSerializer).SerializeGeneric(val, innerTypes, writer, settings);
                } else {
                    valueTypeSerializer.Serialize(val, writer, settings);
                }
            }

            public void SetValue(object target, object value)
                => accessor[target, name] = value;

            public object GetValue(object target)
                => accessor[target, name];
        }
    }
}
