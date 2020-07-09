using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using FastMember;
using FastStream;
using SE.Engine.Serialization.Attributes;
using SE.Serialization.Resolvers;
using SE.Utility;

namespace SE.Serialization.Converters
{
    public sealed class GeneratedConverter : Converter
    {
        public override Type Type { get; }

        private object defaultInstance;
        private Func<object> objCtor;
        private bool isValueType;

        private Dictionary<string, Node> nodesDictionary = new Dictionary<string, Node>();
        private Node[] nodesArr;

        private const int _NODE_HEADER_SIZE = sizeof(char) + sizeof(uint);

        private GeneratedConverter(Type type, ConverterResolver resolver)
        {
            Type = type;
            isValueType = type.IsValueType;
            TypeAccessor accessor = TypeAccessor.Create(type, true);

            // Generate compiled constructors for reference types.
            if(!isValueType)
                GenerateCtor();

            // Try and create a default instance.
            try {
                defaultInstance = Activator.CreateInstance(Type);
            } catch (Exception) {
                throw new Exception($"Could not create an instance of type {Type}. Ensure a parameterless constructor is present.");
            }

            // Get the default serialization mode.
            ObjectSerialization defaultSerialization = ObjectSerialization.OptOut;
            SerializeObjectAttribute serializeObjAttribute = type.GetCustomAttribute<SerializeObjectAttribute>();
            if (serializeObjAttribute != null) {
                defaultSerialization = serializeObjAttribute.ObjectSerialization;
            }

            QuickList<Node> tmpNodes = new QuickList<Node>();
            MemberSet set = accessor.GetMembers();
            uint curIndex = 0;

            foreach (Member member in set) {
                // Skip member if it has an ignore attribute.
                SerializeIgnoreAttribute ignoreAttribute = member.Info.GetCustomAttribute<SerializeIgnoreAttribute>();
                if(ignoreAttribute != null)
                    continue;

                // Set some default info.
                Converter converter = resolver.GetConverter(member.Type);
                string realMemberName = member.Name;
                string memberName = member.Name + ':';
                uint index = curIndex;

                // Process other attributes.
                SerializeAttribute serializeAttribute = member.Info.GetCustomAttribute<SerializeAttribute>();
                if (serializeAttribute != null) {
                    memberName = serializeAttribute.Name ?? member.Name + ':';
                    index = serializeAttribute.Order ?? curIndex;
                }

                // Determine whether or not to serialize.
                if (converter == null)
                    continue;
                if (defaultSerialization == ObjectSerialization.OptIn && serializeAttribute == null)
                    continue;
                if (defaultSerialization == ObjectSerialization.OptOut && (member.IsField || !member.IsPublic))
                    continue;

                Node node = new Node(converter, accessor, memberName, realMemberName, index);
                nodesDictionary.Add(memberName, node);
                tmpNodes.Add(node);

                curIndex++;
            }
            nodesArr = tmpNodes.OrderBy(node => node.Index).ToArray();
        }

        internal static GeneratedConverter Generate(Type type, ConverterResolver resolver)
        {
            GeneratedConverter gen = new GeneratedConverter(type, resolver);
            return gen.nodesDictionary.Count > 0 ? gen : null;
        }

        private void GenerateCtor() 
            => objCtor = Expression.Lambda<Func<object>>(Expression.New(Type)).Compile();

        public override bool IsDefault(object obj) 
            => obj.Equals(defaultInstance);

        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings)
        {
            bool writeName = settings.ConvertBehaviour == ConvertBehaviour.NameAndOrder;

            for (int i = 0; i < nodesArr.Length; i++) {
                writer.Write('|');
                nodesArr[i].Write(obj, writer, settings, writeName);
            }
        }

        public override object Deserialize(FastReader reader, SerializerSettings settings)
        {
            // TODO: Error handling.
            // - Will need to throw an error when creating an instance fails.
            object obj = isValueType ? Activator.CreateInstance(Type) : objCtor.Invoke();

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
            for (int i = 0; i < nodesArr.Length; i++) {
                long startIndex = baseStream.Position + _NODE_HEADER_SIZE;
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
            for (int i = 0; i < nodesArr.Length; i++) {
                long startIndex = baseStream.Position + _NODE_HEADER_SIZE;
                uint readIndex = 0;

                // Step 1: Attempt to deserialize based on name.
                try {
                    SkipDelimiter(reader);
                    readIndex = reader.ReadUInt32();
                    if(!TryReadString(reader, out string name))
                        goto Step2;
                    if (!nodesDictionary.TryGetValue(name, out Node node))
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
                baseStream.Position -= sizeof(int);
                if (baseStream.Position + size > baseStream.Length)
                    return false;

                str = reader.ReadString();
                return true;
            } catch(Exception) { return false; }
        }

        private sealed class Node 
        {
            public string Name;
            public string RealName;
            public uint Index;

            private Converter converter;
            private TypeAccessor accessor;

            // Faster access for delegate accessors.
            private TypeAccessor.DelegateAccessor delegateAccessor;
            private int accessorIndex;

            public Node(Converter converter, TypeAccessor accessor, string name, string realName, uint index)
            {
                this.converter = converter;
                this.accessor = accessor;
                Index = index;
                Name = name;
                RealName = realName;
                if (accessor is TypeAccessor.DelegateAccessor delAccessor) {
                    delegateAccessor = delAccessor;
                    accessorIndex = delAccessor.GetIndex(RealName) ?? throw new IndexOutOfRangeException();
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
                writer.Write(Index);
                if (writeName) {
                    writer.Write(Name);
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
                    accessor[target, RealName] = value;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public object GetValue(object target)
                => delegateAccessor != null
                    ? delegateAccessor.Get(target, accessorIndex) 
                    : accessor[target, RealName];
        }
    }
}
