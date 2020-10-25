using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FastMember;
using FastStream;
using SE.Core;
using SE.Serialization.Attributes;
using SE.Serialization.Exceptions;
using SE.Serialization.Resolvers;
using SE.Utility;
using System.Text;

namespace SE.Serialization.Converters
{
    public sealed class GeneratedConverter : Converter
    {
        public override Type Type { get; }

        private object defaultInstance;
        private Func<object> objCtor;
        private bool isValueType;

        private Dictionary<string, Node> nodesDictionary;
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

            // Order members by declaration order.
            List<Member> members = accessor.Members.OrderBy(member => member.Info.MetadataToken).ToList();

            nodesDictionary = new Dictionary<string, Node>(members.Count);
            QuickList<Node> tmpNodes = new QuickList<Node>(members.Count);
            HashSet<ushort> indexes = new HashSet<ushort>(members.Count);
            ushort curIndex = 0;
            foreach (Member member in members) {
                // Skip member if it has an ignore attribute.
                SerializeIgnoreAttribute ignoreAttribute = member.Info.GetCustomAttribute<SerializeIgnoreAttribute>();
                if(ignoreAttribute != null)
                    continue;

                // Set some default info.
                bool recursiveMember = member.Type == type;
                Converter converter = recursiveMember ? this : resolver.GetConverter(member.Type);
                string realMemberName = member.Name;
                string memberName = member.Name;
                ushort index = curIndex;

                // Skip property backing fields. TODO: May want to support serializing these?
                if(realMemberName.Contains("k__BackingField"))
                    continue;

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

                // Resolve true type.
                Type memberType = member.Type;
                if (Nullable.GetUnderlyingType(member.Type) != null) {
                    memberType = Nullable.GetUnderlyingType(member.Type);
                }

                // Create the node, and add it to the generated converter.
                Node node = new Node(converter, accessor, memberType, memberName, realMemberName, index, recursiveMember);
                nodesDictionary.Add(memberName, node);
                tmpNodes.Add(node);
                indexes.Add(index);

                // Increment index.
                curIndex++;
                if (index > curIndex) {
                    curIndex = (ushort)(index + 1);
                }
            }

            // Create and sort nodes array based on order.
            nodesArray = new Node[tmpNodes.Count];
            Array.Copy(tmpNodes.Array, nodesArray, tmpNodes.Count);
            Array.Sort(nodesArray, new NodeIndexComparer());

            // Generate compiled constructors for reference types.
            if(!isValueType)
                GenerateCtor();
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
            => obj is null || obj.Equals(defaultInstance);

        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task)
        {
            if (task.CurrentDepth < task.Settings.MaxDepth) {
                bool writeName = task.Settings.ConvertBehaviour == ConvertBehaviour.NameAndOrder;
                for (int i = 0; i < nodesArray.Length; i++) {
                    nodesArray[i].WriteBinary(obj, writer, writeName, ref task);
                }
            }

            // Always write delimiter, even when MaxDepth has been reached.
            writer.Write(_BREAK_DELIMITER);
        }

        public override void SerializeText(object obj, FastMemoryWriter writer, ref SerializeTask task)
        {
            if (task.CurrentDepth >= task.Settings.MaxDepth) 
                return;

            bool writeClassDelimiters = task.CurrentDepth > 1;
            if (writeClassDelimiters) {
                writer.Write(Serializer._BEGIN_CLASS);
                writer.Write(Serializer._NEW_LINE);
            }
            for (int i = 0; i < nodesArray.Length; i++) {
                nodesArray[i].WriteText(obj, writer, ref task);
                writer.Write(Serializer._NEW_LINE);
            }
            if (writeClassDelimiters) {
                writer.WriteIndent(task.CurrentDepth-1);
                writer.Write(Serializer._END_CLASS);
            }
        }

        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task)
        {
            // TODO: Error handling.
            // - Will need to throw an error when creating an instance fails.
            object obj = isValueType ? Activator.CreateInstance(Type) : objCtor.Invoke();

            switch (task.Settings.ConvertBehaviour) {
                case ConvertBehaviour.Order: {
                    DeserializeOrder(ref obj, reader, ref task);
                } break;
                case ConvertBehaviour.NameAndOrder: {
                    DeserializeNameAndOrder(ref obj, reader, ref task);
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return obj;
        }

        private void DeserializeOrder(ref object obj, FastReader reader, ref DeserializeTask task)
        {
            Stream stream = reader.BaseStream;
            while (stream.Position + 1 < stream.Length && reader.ReadByte() == _DELIMITER) {
                long startIndex = (stream.Position + _NODE_HEADER_SIZE) - 1;
                try {
                    nodesArray[reader.ReadUInt16()].ReadBinary(obj, reader, ref task);
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

        private void DeserializeNameAndOrder(ref object obj, FastReader reader, ref DeserializeTask task)
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
                    node.ReadBinary(obj, reader, ref task);
                    continue;
                } catch(Exception) { /* ignored */ }

                // Step 2: Attempt to deserialize based on declaration order.
                Step2:
                try {
                    stream.Position = startIndex;
                    if(!SkipToEndOfName(reader))
                        break;

                    nodesArray[readIndex].ReadBinary(obj, reader, ref task);
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

        public T Deserialize<T>(FastReader reader, ref DeserializeTask task) 
            => (T) DeserializeBinary(reader, ref task);

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
            public Type Type;

            private Converter converter;
            private TypeAccessor accessor;
            private bool recursive;

            // Faster access for delegate accessors.
            private TypeAccessor.DelegateAccessor delegateAccessor;
            private int accessorIndex;

            // Cached stuff.
            private byte[] precompiledName;

            public Node(Converter converter, TypeAccessor accessor, Type type, string name, string realName, ushort index, bool recursive)
            {
                this.converter = converter;
                this.accessor = accessor;
                this.recursive = recursive;
                Index = index;
                Name = name;
                RealName = realName;
                Type = type;
                precompiledName = Encoding.Unicode.GetBytes(Name + (char)Serializer._BEGIN_VALUE + ' ');
                if (accessor is TypeAccessor.DelegateAccessor delAccessor) {
                    delegateAccessor = delAccessor;
                    accessorIndex = delAccessor.GetIndex(RealName) ?? throw new IndexOutOfRangeException();
                }
            }

            public void ReadBinary(object target, FastReader reader, ref DeserializeTask task)
            {
                if (recursive && task.Settings.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                    throw new ReferenceLoopException();

                bool shouldReadBool = task.Settings.NullValueHandling == NullValueHandling.DefaultValue
                                      || task.Settings.DefaultValueHandling == DefaultValueHandling.Serialize;

                bool shouldSetValue = !shouldReadBool || reader.ReadBoolean();
                if (shouldSetValue) {
                    Converter typeConverter = Serializer.GetConverterForType(reader, task.Settings) ?? converter;
                    SetValue(target, Serializer.DeserializeReader(reader, typeConverter, ref task));
                    return;
                }

                // Set to default.
                SetValue(target, default);
            }

            public void WriteBinary(object target, FastMemoryWriter writer, bool writeName, ref SerializeTask task)
            {
                if (recursive && task.Settings.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                    throw new ReferenceLoopException();

                Type valType = null;
                Converter typeConverter = converter;
                SerializerSettings settings = task.Settings;
                object val = GetValue(target);
                bool serializeType = settings.TypeHandling != TypeHandling.Ignore;

                // Resolve true type converter (in case of polymorphism).
                if (serializeType) {
                    if (val != null) {
                        valType = val.GetType();
                        if (valType != Type) {
                            typeConverter = settings.Resolver.GetConverter(valType);
                        }
                    }
                }

                // Null and default value handling.
                bool isDefault = typeConverter.IsDefault(val);
                bool writeNull = settings.NullValueHandling == NullValueHandling.DefaultValue;
                bool writeDefault = settings.DefaultValueHandling == DefaultValueHandling.Serialize;
                if(!writeNull && val == null || !writeDefault && isDefault)
                    return;

                // Name & index.
                writer.Write(_DELIMITER);
                writer.Write(Index);
                if (writeName) {
                    writer.Write(Name);
                    writer.Write(_NAME_DELIMITER);
                }

                // Write Type if needed.
                if (serializeType) {
                    Serializer.WriteConverterType(writer, valType, Type, settings);
                }

                // Write bool for null or default values.
                if (writeNull || writeDefault) {
                    bool shouldWrite = writeNull && val != null || writeDefault && !isDefault;
                    writer.Write(shouldWrite);
                    if (!shouldWrite)
                        return;
                }

                Serializer.SerializeWriter(writer, val, typeConverter, ref task);
            }

            public void WriteText(object target, FastMemoryWriter writer, ref SerializeTask task)
            {
                if (recursive && task.Settings.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                    throw new ReferenceLoopException();

                Type valType = null;
                Converter typeConverter = converter;
                SerializerSettings settings = task.Settings;
                object val = GetValue(target);
                bool serializeType = settings.TypeHandling != TypeHandling.Ignore;

                // Resolve true type converter (in case of polymorphism).
                if (serializeType) {
                    if (val != null) {
                        valType = val.GetType();
                        if (valType != Type) {
                            typeConverter = settings.Resolver.GetConverter(valType);
                        }
                    }
                }

                // Null and default value handling.
                bool isDefault = typeConverter.IsDefault(val);
                bool writeNull = settings.NullValueHandling == NullValueHandling.DefaultValue;
                bool writeDefault = settings.DefaultValueHandling == DefaultValueHandling.Serialize;
                if (!writeNull && val == null || !writeDefault && isDefault)
                    return;

                // Write name.
                writer.WriteIndent(task.CurrentDepth);
                writer.Write(precompiledName);

                // Write Type if needed.
                if (serializeType) {
                    Serializer.WriteConverterType(writer, valType, Type, settings);
                }

                // Serialize the actual value.
                Serializer.SerializeWriter(writer, val, typeConverter, ref task);
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

        private struct NodeIndexComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y) => x.Index.CompareTo(y.Index);
        }
    }
}
