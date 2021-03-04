using System;
using System.Buffers;
using System.Buffers.Text;
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

        private object defaultTypeInstance;
        private Func<object> objCtor;
        private bool isValueType;

        private Dictionary<string, Node> nodesDictionary;
        private Node[] nodesArray;

        // Binary format delimiters.
        private const int _NODE_HEADER_SIZE = sizeof(byte) + sizeof(ushort);
        private const byte _NAME_DELIMITER  = (byte) ':';
        private const byte _DELIMITER       = (byte) '|';
        private const byte _BREAK_DELIMITER = (byte) '^';

        private GeneratedConverter(Type type, ConverterResolver resolver)
        {
            Type = type;
            isValueType = type.IsValueType;
            TypeAccessor accessor = TypeAccessor.Create(type, true);

            // Try and create a default instance for the type.
            // Objects/RefTypes default to null. Structs/ValueTypes default to their default instance.
            try {
                defaultTypeInstance = isValueType ? Activator.CreateInstance(Type) : null;
            } catch (Exception) {
                throw new Exception($"Could not create an instance of type {Type}. Ensure a parameterless constructor is present.");
            }

            // Create the 'actual' default instance. Used for Object serialization.
            object defaultInstance = ConstructInstance();

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

                // Resolve default node value.
                object defaultVal = accessor[defaultInstance, realMemberName];

                // Create the node, and add it to the generated converter.
                Node node = new Node(converter, accessor, memberType, defaultVal, memberName, realMemberName, index, recursiveMember);
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

        private object ConstructInstance()
        {
            // TODO: Support non-default constructors.
            return Activator.CreateInstance(Type);
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
            => obj == null || obj.Equals(defaultTypeInstance);

        public override void SerializeBinary(object obj, Utf8Writer writer, ref SerializeTask task)
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

        public override void SerializeText(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            task.CurrentParameterIndex = 0;
            if (task.CurrentDepth >= task.Settings.MaxDepth) 
                return;

            // Starting delimiters.
            bool writeClassDelimiters = task.CurrentDepth > 1;
            if (writeClassDelimiters) {
                writer.Write(Serializer._BEGIN_CLASS);
            }

            // Write value.
            for (int i = 0; i < nodesArray.Length; i++) {
                if (nodesArray[i].WriteText(obj, writer, ref task)) {
                    task.CurrentParameterIndex += 1;
                    writer.Write(Serializer._NEW_LINE);
                }
            }

            // End delimiters.
            if (writeClassDelimiters) {
                if (task.CurrentParameterIndex > 0) {
                    writer.WriteIndent(task.CurrentDepth - 1);
                }
                writer.Write(Serializer._END_CLASS);
            }
        }

        public override object DeserializeBinary(Utf8Reader reader, ref DeserializeTask task)
        {
            // TODO: Error handling.
            // - Will need to throw an error when creating an instance fails.
            object obj = isValueType ? Activator.CreateInstance(Type) : objCtor.Invoke();

            switch (task.Settings.ConvertBehaviour) {
                case ConvertBehaviour.Order: {
                    DeserializeBinaryOrder(ref obj, reader, ref task);
                } break;
                case ConvertBehaviour.NameAndOrder: {
                    DeserializeBinaryNameAndOrder(ref obj, reader, ref task);
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return obj;
        }

        private void DeserializeBinaryOrder(ref object obj, Utf8Reader reader, ref DeserializeTask task)
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

        private void DeserializeBinaryNameAndOrder(ref object obj, Utf8Reader reader, ref DeserializeTask task)
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

        public T Deserialize<T>(Utf8Reader reader, ref DeserializeTask task) 
            => (T) DeserializeBinary(reader, ref task);

        public override object DeserializeText(Utf8Reader reader, ref DeserializeTask task)
        {
            object obj = isValueType ? Activator.CreateInstance(Type) : objCtor.Invoke();

            switch (task.Settings.ConvertBehaviour) {
                case ConvertBehaviour.Order: {
                    DeserializeTextOrder(ref obj, reader, ref task);
                } break;
                case ConvertBehaviour.NameAndOrder: {
                    DeserializeTextNameAndOrder(ref obj, reader, ref task);
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return obj;
        }

        private void DeserializeTextOrder(ref object obj, Utf8Reader reader, ref DeserializeTask task)
        {
            Stream stream = reader.BaseStream;
            while (stream.Position + 1 < stream.Length) {
                
                // Look for next variable.
                try {
                    byte b = reader.ReadByte();

                    // Skip _TAB, _NEW_LINE and _BEGIN_CLASS.
                    // Break if _END_CLASS is encountered.
                    switch (b) {
                        case Serializer._TAB:
                        case Serializer._NEW_LINE:
                        case Serializer._BEGIN_CLASS:
                            continue;
                        case Serializer._END_CLASS:
                            return;
                    }

                    reader.BaseStream.Position -= 1;

                    // Index meta parsing.
                    SkipToNextSymbol(reader, Serializer._BEGIN_META);
                    uint index = uint.Parse(reader.ReadUntil(Serializer._END_META));

                    // Read node value.
                    SkipToNextSymbol(reader, Serializer._BEGIN_VALUE);
                    reader.SkipWhiteSpace();
                    nodesArray[index].ReadText(obj, reader, ref task);

                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

                // TODO: Error handling.
            }
        }

        private void DeserializeTextNameAndOrder(ref object obj, Utf8Reader reader, ref DeserializeTask task)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SkipDelimiter(Utf8Reader reader) 
            => reader.BaseStream.Position += sizeof(byte);

        internal static bool SkipToNextSymbol(Utf8Reader reader, byte symbol, bool skipPast = true)
        {
            Stream baseStream = reader.BaseStream;
            while (baseStream.Position + 1 < baseStream.Length) {
                byte b = reader.ReadByte();
                if (b != symbol) 
                    continue;

                if (!skipPast) {
                    reader.BaseStream.Position -= 1;
                }
                return true;
            }
            return false;
        }

        internal static bool SkipToNextDelimiter(Utf8Reader reader)
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

        internal static bool SkipToEndOfName(Utf8Reader reader)
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

        private static bool TryReadString(Utf8Reader reader, out string str)
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
            public object Default;

            private Converter converter;
            private TypeAccessor accessor;
            private bool recursive;

            private bool allowPolymorphism;

            // Faster access for delegate accessors.
            private TypeAccessor.DelegateAccessor delegateAccessor;
            private int accessorIndex;

            // Cached stuff.
            private byte[] precompiledName;
            private byte[] precompiledNameWithIndex;

            public Node(Converter converter, TypeAccessor accessor, Type type, object defaultVal, string name, string realName, ushort index, bool recursive)
            {
                this.converter = converter;
                this.accessor = accessor;
                this.recursive = recursive;
                Index = index;
                Name = name;
                RealName = realName;
                Type = type;
                Default = defaultVal;
                precompiledName = Encoding.UTF8.GetBytes(Name + (char)Serializer._BEGIN_VALUE + ' ');

                // Precompiled name with meta index.
                byte[] tmpByteArr = ArrayPool<byte>.Shared.Rent(5);
                Span<byte> indexSpan = new Span<byte>(tmpByteArr);
                Utf8Formatter.TryFormat(Index, indexSpan, out int bytesWritten);
                string indexStr = Encoding.UTF8.GetString(indexSpan.Slice(0, bytesWritten));
                precompiledNameWithIndex = Encoding.UTF8.GetBytes(
                    (char) Serializer._BEGIN_META 
                    + indexStr 
                    + (char) Serializer._END_META 
                    + Name 
                    + (char) Serializer._BEGIN_VALUE + ' ');
                ArrayPool<byte>.Shared.Return(tmpByteArr);

                if (accessor is TypeAccessor.DelegateAccessor delAccessor) {
                    delegateAccessor = delAccessor;
                    accessorIndex = delAccessor.GetIndex(RealName) ?? throw new IndexOutOfRangeException();
                }

                allowPolymorphism = true;
                if (Type.IsValueType || Nullable.GetUnderlyingType(Type) != null) {
                    allowPolymorphism = false;
                }
            }

            public void ReadBinary(object target, Utf8Reader reader, ref DeserializeTask task)
            {
                if (recursive && task.Settings.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                    throw new ReferenceLoopException();

                bool shouldReadBool = task.Settings.NullValueHandling == NullValueHandling.DefaultValue
                                      || task.Settings.DefaultValueHandling == DefaultValueHandling.Serialize;

                bool shouldSetValue = !shouldReadBool || reader.ReadBoolean();

                // If value shouldn't be set, set it to default.
                if (!shouldSetValue) {
                    SetValue(target, default);
                    return;
                }

                // Otherwise, set the value.
                Converter typeConverter = converter;
                if (allowPolymorphism) {
                    Serializer.TryReadMetaBinary(reader, task.Settings, out string valueType, out int? id);
                    if (valueType != null) {
                        typeConverter = Serializer.GetConverterForTypeString(valueType, task.Settings);
                    }
                }
                SetValue(target, Serializer.DeserializeReader(reader, typeConverter, ref task));
            }

            public void WriteBinary(object target, Utf8Writer writer, bool writeName, ref SerializeTask task)
            {
                if (recursive && task.Settings.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                    throw new ReferenceLoopException();

                // Locals initialization.
                object val = GetValue(target);
                Type valType = null;
                Converter typeConverter = converter;
                SerializerSettings settings = task.Settings;
                bool serializeType = settings.TypeHandling != TypeHandling.Ignore && allowPolymorphism;

                // Resolve true type converter (in case of polymorphism).
                if (serializeType && val != null) {
                    valType = val.GetType();
                    if (valType != Type) {
                        typeConverter = settings.Resolver.GetConverter(valType);
                    }
                }

                // Null and default value handling.
                bool isDefault = IsDefault(val);
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

                // Write bool for null or default values.
                if (writeNull || writeDefault) {
                    bool shouldWrite = writeNull && val != null || writeDefault && !isDefault;
                    writer.Write(shouldWrite);
                    if (!shouldWrite)
                        return;
                }

                // Write meta-info.
                bool shouldWriteType = Serializer.ShouldWriteConverterType(valType, Type, settings);
                string metaType = null;
                if (shouldWriteType && valType != null) {
                    metaType = valType.AssemblyQualifiedName;
                }
                Serializer.WriteMetaBinary(writer, settings, metaType, null);

                // Serialize value.
                Serializer.SerializeWriter(writer, val, typeConverter, ref task);
            }

            public bool WriteText(object target, Utf8Writer writer, ref SerializeTask task)
            {
                if (recursive && task.Settings.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                    throw new ReferenceLoopException();

                object val = GetValue(target);
                Type valType = null;
                Converter typeConverter = converter;
                SerializerSettings settings = task.Settings;
                bool serializeType = settings.TypeHandling != TypeHandling.Ignore && allowPolymorphism;

                // Resolve true type converter (in case of polymorphism).
                if (serializeType && val != null) {
                    valType = val.GetType();
                    if (valType != Type) {
                        typeConverter = settings.Resolver.GetConverter(valType);
                    }
                }

                // Null and default value handling.
                bool writeNull = settings.NullValueHandling == NullValueHandling.DefaultValue;
                bool writeDefault = settings.DefaultValueHandling == DefaultValueHandling.Serialize;
                if (!writeNull && val == null || !writeDefault && IsDefault(val))
                    return false;

                // If this is the first parameter, go to new line.
                if (task.CurrentParameterIndex == 0) {
                    writer.Write(Serializer._NEW_LINE);
                }

                // Write name.
                writer.WriteIndent(task.CurrentDepth);
                writer.Write(precompiledNameWithIndex);

                // Write meta-info.
                bool shouldWriteType = Serializer.ShouldWriteConverterType(valType, Type, settings);
                string metaType = null;
                if (shouldWriteType && valType != null) {
                    metaType = valType.AssemblyQualifiedName;
                }
                Serializer.WriteMetaText(writer, settings, metaType, null);

                // Serialize the actual value.
                Serializer.SerializeWriter(writer, val, typeConverter, ref task);

                return true;
            }

            public void ReadText(object target, Utf8Reader reader, ref DeserializeTask task)
            {
                if (recursive && task.Settings.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                    throw new ReferenceLoopException();

                Converter typeConverter = converter;
                if (allowPolymorphism) {
                    Serializer.TryReadMetaText(reader, task.Settings, out string valueType, out int? id);
                    if (valueType != null) {
                        typeConverter = Serializer.GetConverterForTypeString(valueType, task.Settings);
                    }
                }
                SetValue(target, Serializer.DeserializeReader(reader, typeConverter, ref task));
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

            private bool IsDefault(object value)
            {
                if (Default == null && value == null)
                    return true;

                return value.Equals(Default);
            }
        }

        private struct NodeIndexComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y) => x.Index.CompareTo(y.Index);
        }
    }
}
