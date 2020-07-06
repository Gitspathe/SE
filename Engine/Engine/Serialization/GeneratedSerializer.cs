using FastMember;
using FastStream;
using SE.Engine.Networking.Internal;
using SE.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;

namespace SE.Serialization
{
    internal class GeneratedSerializer : ISerializer
    {
        public Type Type { get; set; }

        private QuickList<Value> valList = new QuickList<Value>();
        private Dictionary<string, Value> values = new Dictionary<string, Value>();

        private delegate object ObjectActivator();
        private ObjectActivator activator;
        private TypeAccessor accessor;
        private bool isValueType;

        public GeneratedSerializer(Type type)
        {
            Type = type;
            isValueType = type.IsValueType;
            accessor = TypeAccessor.Create(type, true);

            // Generate compiled constructors for reference types.
            if(!isValueType)
                GenerateCtor();

            foreach (Member member in accessor.GetMembers()) {
                ISerializer serializer;
                
                if (Serializer.ValueSerializersType.TryGetValue(member.Type, out IValueSerializer valueSerializer)) {
                    serializer = valueSerializer;
                } else {
                    // Try and get nested value serializer.
                    // TODO: Stack overflow / infinite recursion detection.
                    serializer = Serializer.GetSerializer(member.Type);
                }

                if(serializer != null) {
                    Value val = new Value(serializer, accessor, member.Name);
                    values.Add(member.Name, val);
                    valList.Add(val);
                }
            }
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

        public void Serialize(FastMemoryWriter writer, object obj)
        {
            Value[] arr = valList.Array;
            for (int i = 0; i < valList.Count; i++) {
                try {
                    arr[i].Write(obj, writer);
                } catch (Exception) { /* ignored */ }
            }
            writer.Write('|');
        }

        public object Deserialize(FastReader reader)
        {
            object obj = !isValueType ? activator.Invoke() : Activator.CreateInstance(Type);

            while (reader.PeekChar() != '|') {
                try {
                    string nextVarName = reader.ReadString();
                    values[nextVarName].Read(obj, reader);
                } catch (Exception) { /* ignored */ }
            }

            // TODO: Why the fuck do I have to move 2 chars forward !??!?!?!!?
            reader.BaseStream.Position += 2;

            //reader.ReadChar();
            //reader.ReadChar();

            return obj;
        }

        public T Deserialize<T>(FastReader reader)
        {
            return (T) Deserialize(reader);
        }

        private class Value
        {
            private ISerializer valueSerializer;
            private TypeAccessor accessor;
            private string name;

            public void Read(object target, FastReader reader)
            {
                bool nextExists = reader.ReadBoolean();
                SetValue(target, nextExists ? valueSerializer.Deserialize(reader) : default);
            }

            public void Write(object target, FastMemoryWriter writer)
            {
                object val = GetValue(target);
                writer.Write(name);
                writer.Write(val != null);
                if(val != null)
                    valueSerializer.Serialize(writer, val);
            }

            public void SetValue(object target, object value)
            {
                accessor[target, name] = value;
            }

            public object GetValue(object target)
            {
                return accessor[target, name];
            }

            public Value(ISerializer serializer, TypeAccessor accessor, string name)
            {
                valueSerializer = serializer;
                this.accessor = accessor;
                this.name = name;
            }
        }
    }
}
