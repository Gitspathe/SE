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
    internal class GeneratedSerializer : ISerializer
    {
        public Type Type { get; }

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
                ISerializer serializer = Serializer.GetSerializer(member.Type);

                // Skip this member if a valid serializer was not found.
                if (serializer == null) 
                    continue;

                Node val = new Node(serializer, accessor, member.Name);
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

        public void Serialize(FastMemoryWriter writer, object obj, SerializerSettings settings)
        {
            Node[] arr = nodeList.Array;
            for (int i = 0; i < nodeList.Count; i++) {
                try {
                    arr[i].Write(obj, writer, settings);
                } catch (Exception) { /* ignored */ }
            }
            writer.Write('|');
        }

        public object Deserialize(FastReader reader, SerializerSettings settings)
        {
            object obj = !isValueType ? activator.Invoke() : Activator.CreateInstance(Type);

            while (reader.PeekChar() != '|') {
                try {
                    string nextVarName = reader.ReadString();
                    nodes[nextVarName].Read(obj, reader, settings);
                } catch (Exception) { /* ignored */ }
            }

            // TODO: Why the fuck do I have to move 2 chars forward !?!?!?!?
            reader.BaseStream.Position += 2;

            //reader.ReadChar();
            //reader.ReadChar();

            return obj;
        }

        public T Deserialize<T>(FastReader reader, SerializerSettings settings) 
            => (T) Deserialize(reader, settings);

        private class Node
        {
            private ISerializer valueSerializer;
            private TypeAccessor accessor;
            private string name;

            public void Read(object target, FastReader reader, SerializerSettings settings)
            {
                // If the next value exists / isn't null, deserialize it's data.
                if (reader.ReadBoolean()) {
                    SetValue(target, valueSerializer.Deserialize(reader, settings));
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
                if(val != null)
                    valueSerializer.Serialize(writer, val, settings);
            }

            public void SetValue(object target, object value)
                => accessor[target, name] = value;

            public object GetValue(object target)
                => accessor[target, name];

            public Node(ISerializer serializer, TypeAccessor accessor, string name)
            {
                valueSerializer = serializer;
                this.accessor = accessor;
                this.name = name;
            }
        }
    }
}
