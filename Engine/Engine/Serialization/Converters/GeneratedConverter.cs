using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using FastMember;
using FastStream;
using SE.Engine.Serialization.Attributes;
using SE.Serialization.Attributes;
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
        private Node[] nodesArray;

        private const int _NODE_HEADER_SIZE = sizeof(byte) + sizeof(ushort);
        private const byte _NAME_DELIMITER  = (byte) ':';
        private const byte _DELIMITER       = (byte) '|';
        private const byte _BREAK_DELIMITER = (byte) '^';

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
                defaultInstance = isValueType ? Activator.CreateInstance(Type) : null;
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
            HashSet<ushort> indexes = new HashSet<ushort>();
            List<Member> members = accessor.Members;
            ushort curIndex = 0;

            foreach (Member member in members) {
                // Skip member if it has an ignore attribute.
                SerializeIgnoreAttribute ignoreAttribute = member.Info.GetCustomAttribute<SerializeIgnoreAttribute>();
                if(ignoreAttribute != null)
                    continue;

                // Set some default info.
                Converter converter = resolver.GetConverter(member.Type);
                string realMemberName = member.Name;
                string memberName = member.Name;
                ushort index = curIndex;

                // Process other attributes.
                SerializeAttribute serializeAttribute = member.Info.GetCustomAttribute<SerializeAttribute>();
                if (serializeAttribute != null) {
                    memberName = serializeAttribute.Name ?? member.Name;
                    index = serializeAttribute.Order ?? curIndex;
                }

                // Determine whether or not to serialize.
                if (converter == null)
                    continue;
                if (defaultSerialization == ObjectSerialization.OptIn && serializeAttribute == null)
                    continue;
                if (defaultSerialization == ObjectSerialization.OptOut && (member.IsField || !member.IsPublic))
                    continue;

                // Resolve duplicates.
                memberName = ResolveName(memberName);
                index = ResolveIndex(indexes, index);

                // Create the node, and add it to the generated converter.
                Node node = new Node(converter, accessor, memberName, realMemberName, index);
                nodesDictionary.Add(memberName, node);
                tmpNodes.Add(node);
                indexes.Add(index);

                // Increment index.
                curIndex++;
                if (index > curIndex) {
                    curIndex = (ushort)(index + 1);
                }
            }
            nodesArray = new Node[tmpNodes.Count];
            Array.Copy(tmpNodes.Array, nodesArray, tmpNodes.Count);
            Array.Sort(nodesArray, new NodeComparer());
        }

        private struct NodeComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y) => x.Index.CompareTo(y.Index);
        }

        private string ResolveName(string memberName)
        {
            if (!nodesDictionary.ContainsKey(memberName)) 
                return memberName;

            int tmpInt = 0;
            string name = memberName + tmpInt;
            while (nodesDictionary.ContainsKey(name)) {
                tmpInt++;
                name = memberName + tmpInt;
            }
            return name;
        }

        private ushort ResolveIndex(HashSet<ushort> indexes, ushort index)
        {
            ushort i = index;
            while (indexes.Contains(i)) {
                i++;
            }
            return i;
        }

        internal static GeneratedConverter Create(Type type, ConverterResolver resolver)
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
            for (int i = 0; i < nodesArray.Length; i++) {
                nodesArray[i].Write(obj, writer, settings, writeName);
            }
            writer.Write(_BREAK_DELIMITER);
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
            Stream stream = reader.BaseStream;
            while (stream.Position + 1 < stream.Length && reader.ReadByte() == _DELIMITER) {
                long startIndex = (stream.Position + _NODE_HEADER_SIZE) - 1;
                try {
                    nodesArray[reader.ReadUInt16()].Read(obj, reader, settings);
                    continue;
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

                // Failed. Skip to next node.
                try {
                    stream.Position = startIndex;
                    if(!SkipToNextDelimiter(reader))
                        break;

                } catch(Exception) { break; }
            }
        }

        private void DeserializeNameAndOrder(ref object obj, FastReader reader, SerializerSettings settings)
        {
            Stream stream = reader.BaseStream;
            while (stream.Position + 1 < stream.Length && reader.ReadByte() == _DELIMITER) {
                long startIndex = (stream.Position + _NODE_HEADER_SIZE) - 1;
                ushort readIndex = 0;

                // Step 1: Attempt to deserialize based on name.
                try {
                    readIndex = reader.ReadUInt16();
                    if(!TryReadString(reader, out string name))
                        goto Step2;
                    if (!nodesDictionary.TryGetValue(name, out Node node))
                        goto Step2;

                    SkipDelimiter(reader);
                    node.Read(obj, reader, settings);
                    continue;
                } catch(Exception) { /* ignored */ }

                // Step 2: Attempt to deserialize based on declaration order.
                Step2:
                try {
                    stream.Position = startIndex;
                    if(!SkipToEndOfName(reader))
                        break;

                    nodesArray[readIndex].Read(obj, reader, settings);
                    continue;
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

                // Step 3: Failed. Skip to next node.
                try {
                    stream.Position = startIndex;
                    if(!SkipToNextDelimiter(reader))
                        break;
                } catch (Exception) {
                    break;
                }
            }
        }

        public T Deserialize<T>(FastReader reader, SerializerSettings settings) 
            => (T) Deserialize(reader, settings);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SkipDelimiter(FastReader reader) 
            => reader.BaseStream.Position += sizeof(byte);

        internal static bool SkipToNextDelimiter(FastReader reader)
        {
            Stream baseStream = reader.BaseStream;
            while (baseStream.Position + 1 < baseStream.Length) {
                byte b = reader.ReadByte();
                if (b == _DELIMITER) {
                    reader.BaseStream.Position -= 1;
                    return true;
                }
                if (b == _BREAK_DELIMITER) {
                    reader.BaseStream.Position -= 1;
                    return false;
                }
            }
            return false;
        }

        internal static bool SkipToEndOfName(FastReader reader)
        {
            Stream baseStream = reader.BaseStream;
            while (baseStream.Position + 1 < baseStream.Length) {
                byte b = reader.ReadByte();
                if (b == _NAME_DELIMITER) {
                    SkipDelimiter(reader);
                    return true;
                } 
                if (b == _BREAK_DELIMITER) {
                    SkipDelimiter(reader);
                    return false;
                }
            }
            return false;
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
            public ushort Index;

            private Converter converter;
            private TypeAccessor accessor;

            // Faster access for delegate accessors.
            private TypeAccessor.DelegateAccessor delegateAccessor;
            private int accessorIndex;

            public Node(Converter converter, TypeAccessor accessor, string name, string realName, ushort index)
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
                bool shouldReadBool = settings.NullValueHandling == NullValueHandling.DefaultValue
                                      || settings.DefaultValueHandling == DefaultValueHandling.Serialize;

                bool shouldSetValue = !shouldReadBool || reader.ReadBoolean();
                if (shouldSetValue) {
                    SetValue(target, converter.Deserialize(reader, settings));
                    return;
                }

                // Set to default.
                SetValue(target, default);
            }

            public void Write(object target, FastMemoryWriter writer, SerializerSettings settings, bool writeName)
            {
                object val = GetValue(target);
                bool writeNull = settings.NullValueHandling == NullValueHandling.DefaultValue;
                bool writeDefault = settings.DefaultValueHandling == DefaultValueHandling.Serialize;
                bool isDefault = converter.IsDefault(val);
                if(!writeNull && val == null)
                    return;
                if(!writeDefault && isDefault)
                    return;

                writer.Write(_DELIMITER);
                writer.Write(Index);
                if (writeName) {
                    writer.Write(Name);
                    writer.Write(_NAME_DELIMITER);
                }

                if(writeNull || writeDefault) {
                    bool shouldWrite = writeNull && val != null || writeDefault && !isDefault;
                    writer.Write(shouldWrite);
                    if (!shouldWrite)
                        return;
                }

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
