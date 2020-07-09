using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FastMember;
using FastStream;
using SE.Serialization.Resolvers;

namespace SE.Serialization.Converters
{
    public sealed class GeneratedConverter : Converter
    {
        public override Type Type { get; }

        private object defaultInstance;

        private Node[] nodesArr;
        private int nodesLength;

        private Dictionary<string, Node> nodes = new Dictionary<string, Node>();
        private Func<object> ctor;
        private TypeAccessor accessor;
        private bool isValueType;

        private GeneratedConverter(Type type, ConverterResolver resolver)
        {
            Type = type;
            isValueType = type.IsValueType;
            accessor = TypeAccessor.Create(type, true);

            try {
                defaultInstance = Activator.CreateInstance(Type);
            } catch (Exception) {
                throw new Exception($"Could not create an instance of type {Type}. Ensure a parameterless constructor is present.");
            }

            // Generate compiled constructors for reference types.
            if(!isValueType)
                GenerateCtor();

            MemberSet set = accessor.GetMembers();
            nodesArr = new Node[set.Count];
            uint curIndex = 0;
            foreach (Member member in set) {
                
                // TODO: Replace this with attributes and stuff.
                // TODO: Attribute to override declaration order and/or name for variables.
                if(!member.IsPublic)
                    continue;

                Converter converter = resolver.GetConverter(member.Type);

                // Skip this member if a valid converter was not found.
                if (converter == null) 
                    continue;

                string memberName = member.Name + ':';
                Node val = new Node(converter, accessor, memberName, curIndex);
                nodes.Add(memberName, val);
                nodesArr[curIndex] = val;

                curIndex++;
            }
            nodesLength = (int) curIndex;
        }

        internal static GeneratedConverter Generate(Type type, ConverterResolver resolver)
        {
            GeneratedConverter gen = new GeneratedConverter(type, resolver);
            return gen.nodes.Count > 0 ? gen : null;
        }

        private void GenerateCtor()
        {
            ctor = Expression.Lambda<Func<object>>(Expression.New(Type)).Compile();
        }

        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings)
        {
            bool writeName = settings.ConvertBehaviour == ConvertBehaviour.NameAndOrder;

            for (int i = 0; i < nodesLength; i++) {
                writer.Write('|');
                nodesArr[i].Write(obj, writer, settings, writeName);
            }
        }

        public override bool IsDefault(object obj) 
            => obj.Equals(defaultInstance);

        public override object Deserialize(FastReader reader, SerializerSettings settings)
        {
            // TODO: Error handling.
            // - Will need to throw an error when creating an instance fails.
            object obj = isValueType ? Activator.CreateInstance(Type) : ctor.Invoke();

            switch (settings.ConvertBehaviour) {
                case ConvertBehaviour.Order: {
                    DeserializeOrder(ref obj, reader, settings);
                } break;
                case ConvertBehaviour.NameAndOrder: {
                    DeserializeNameAndOrder(ref obj, reader, settings);
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return obj;
        }

        private void DeserializeOrder(ref object obj, FastReader reader, SerializerSettings settings)
        {
            Stream baseStream = reader.BaseStream;
            for (int i = 0; i < nodesLength; i++) {
                long startIndex = baseStream.Position + 6;
                try {
                    SkipDelimiter(reader);
                    uint readIndex = reader.ReadUInt32();
                    nodesArr[readIndex].Read(obj, reader, settings);
                    continue;
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

                // Failed. Skip to next node.
                try {
                    i++;
                    baseStream.Position = startIndex;
                    SkipToNextDelimiter(reader);
                } catch(Exception) { break; }
            }
        }

        private void DeserializeNameAndOrder(ref object obj, FastReader reader, SerializerSettings settings)
        {
            Stream baseStream = reader.BaseStream;
            for (int i = 0; i < nodesLength; i++) {
                uint readIndex = 0;
                long startIndex = baseStream.Position + 6;
                        
                // Step 1: Attempt to deserialize based on name.
                try {
                    SkipDelimiter(reader);
                    readIndex = reader.ReadUInt32();
                    if(!TryReadString(reader, out string name))
                        goto Step2;
                    if (!nodes.TryGetValue(name, out Node node))
                        goto Step2;
                            
                    node.Read(obj, reader, settings);
                    continue;
                } catch(Exception) { /* ignored */ }

                // Step 2: Attempt to deserialize based on declaration order.
                Step2:
                try {
                    baseStream.Position = startIndex;
                    SkipToEndOfName(reader);
                    nodesArr[readIndex].Read(obj, reader, settings);
                    continue;
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

                // Step 3: Failed. Skip to next node.
                try {
                    i++;
                    baseStream.Position = startIndex;
                    SkipToNextDelimiter(reader);
                } catch (Exception) {
                    break;
                }
            }
        }

        public T Deserialize<T>(FastReader reader, SerializerSettings settings) 
            => (T) Deserialize(reader, settings);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SkipDelimiter(FastReader reader) 
            => reader.BaseStream.Position += 2;

        internal static void SkipToNextDelimiter(FastReader reader)
        {
            Stream baseStream = reader.BaseStream;
            while (baseStream.Position + sizeof(char) < baseStream.Length) {
                if (reader.PeekChar() == '|') {
                    return;
                }
                reader.ReadChar();
            }
        }

        internal static void SkipToEndOfName(FastReader reader)
        {
            Stream baseStream = reader.BaseStream;
            while (baseStream.Position + sizeof(char) < baseStream.Length) {
                if (reader.PeekChar() == ':') {
                    SkipDelimiter(reader);
                    return;
                }
                reader.ReadChar();
            }
        }

        private static bool TryReadString(FastReader reader, out string str)
        {
            str = null;
            Stream baseStream = reader.BaseStream;

            try {
                uint size = reader.ReadUInt32();
                reader.BaseStream.Position -= sizeof(int);
                if (baseStream.Position + size > baseStream.Length)
                    return false;

                str = reader.ReadString();
                return true;
            } catch(Exception) { return false; }
        }

        private sealed class Node 
        {
            private Converter converter;
            private TypeAccessor accessor;
            private string name;
            private string realName;
            private uint index;

            // Faster access for delegate accessors.
            private TypeAccessor.DelegateAccessor delegateAccessor;
            private int accessorIndex;

            public Node(Converter converter, TypeAccessor accessor, string name, uint index)
            {
                this.converter = converter;
                this.accessor = accessor;
                this.index = index;
                this.name = name;
                realName = name.Replace(":", null);
                if (accessor is TypeAccessor.DelegateAccessor delAccessor) {
                    delegateAccessor = delAccessor;
                    accessorIndex = delAccessor.GetIndex(realName) ?? throw new IndexOutOfRangeException();
                }
            }

            public void Read(object target, FastReader reader, SerializerSettings settings)
            {
                // If the next value exists / isn't null, deserialize it's data.
                if (reader.ReadBoolean()) {
                    SetValue(target, converter.Deserialize(reader, settings));
                    return;
                }
                
                // If the next value is null, set it to default, or ignore, depending on settings.
                if (settings.NullValueHandling == NullValueHandling.DefaultValue) {
                    SetValue(target, default);
                }
            }

            public void Write(object target, FastMemoryWriter writer, SerializerSettings settings, bool writeName)
            {
                object val = GetValue(target);
                writer.Write(index);
                if (writeName) {
                    writer.Write(name);
                }

                bool shouldWrite = val != null;
                if(settings.DefaultValueHandling == DefaultValueHandling.Ignore && converter.IsDefault(val))
                    shouldWrite = false;

                writer.Write(shouldWrite);
                if (!shouldWrite)
                    return;

                converter.Serialize(val, writer, settings);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetValue(object target, object value)
            {
                if (delegateAccessor != null) {
                    delegateAccessor.Set(target, accessorIndex, value);
                } else {
                    accessor[target, realName] = value;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public object GetValue(object target)
                => delegateAccessor != null
                    ? delegateAccessor.Get(target, accessorIndex) 
                    : accessor[target, realName];
        }
    }
}
