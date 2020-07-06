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



        private TypeAccessor accessor;

        public GeneratedSerializer(Type type)
        {
            Type = type;
            accessor = TypeAccessor.Create(type, true);
            foreach (Member member in accessor.GetMembers()) {
                ISerializer serializer;
                
                if (Serializer.ValueSerializersType.TryGetValue(member.Type, out (IValueSerializer, int) tuple)) {
                    serializer = tuple.Item1;
                } else {
                    // Try and get nested value serializer. TODO: Stack overflow / infinite recursion detection.
                    serializer = Serializer.GetSerializer(member.Type);
                }

                if(serializer != null) {
                    Value val = new Value(serializer, accessor, member.Name);
                    values.Add(member.Name, val);
                    valList.Add(val);
                }
            }
        }

        public void Serialize(FastMemoryWriter writer, object obj)
        {
            Value[] arr = valList.Array;
            for (int i = 0; i < valList.Count; i++) {
                arr[i].Write(obj, writer);
            }
            writer.Write('|');
        }


        public object Deserialize(FastReader reader)
        {
            object obj = Activator.CreateInstance(Type);

            while (reader.PeekChar() != '|') {
                string nextVarName = reader.ReadString();
                values[nextVarName].Read(obj, reader);
            }

            // TODO: Why the fuck do I have to call this twice !??!?!?!!?
            reader.ReadChar();
            reader.ReadChar();

            return obj;
        }

        public T Deserialize<T>(FastReader reader)
        {
            return (T) Deserialize(reader);
        }

        public class Value
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
