using FastMember;
using SE.Core;
using SE.Serialization.Attributes;
using SE.Serialization.Exceptions;
using SE.Serialization.Resolvers;
using SE.Utility;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static SE.Serialization.Constants;
using static SE.Serialization.SerializerUtil;

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

        private GeneratedConverter(Type type, ConverterResolver resolver)
        {
            // Throw error if this generated converter is for a type which isn't whitelisted.
            // This is performed to mitigate possibly dangerous exploits (remote code execution).
            if (!Serializer.Whitelist.TypeIsWhiteListed(type))
                throw new SerializerWhitelistException(Type);

            Type = type;
            isValueType = type.IsValueType;
            TypeAccessor accessor = TypeAccessor.Create(type, true);

            // Try and create a default instance for the THIS generator's type.
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
                if (ignoreAttribute != null)
                    continue;

                // Set some default info.
                bool recursiveMember = member.Type == type;
                Converter converter = recursiveMember ? this : resolver.GetConverter(member.Type);
                string realMemberName = member.Name;
                string memberName = member.Name;
                ushort index = curIndex;

                // Skip property backing fields. // TODO: These might be needed to deserialize read-only properties.
                if (realMemberName.Contains("k__BackingField"))
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
            if (!isValueType)
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
            //gen.PostConstructor();
            return gen.nodesDictionary.Count > 0 ? gen : null;
        }

        private void GenerateCtor()
            => objCtor = Expression.Lambda<Func<object>>(Expression.New(Type)).Compile();

        public override bool IsDefault(object obj)
            => obj == null || obj.Equals(defaultTypeInstance);

        public override void SerializeBinary(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            ConvertBehaviour behaviour = task.Settings.ConvertBehaviour;
            if (behaviour == ConvertBehaviour.Configuration)
                throw new NotSupportedException("Binary serialization is not supported with the 'Configuration' convert behaviour.");

            bool writeClassDelimiters = task.CurrentDepth > 1;
            if (writeClassDelimiters) {
                writer.Write(_BEGIN_CLASS);
            }

            if (task.CurrentDepth < task.Settings.MaxDepth) {
                bool writeName = behaviour == ConvertBehaviour.NameAndOrder;
                for (int i = 0; i < nodesArray.Length; i++) {
                    nodesArray[i].WriteBinary(obj, writer, writeName, ref task);
                }
            }

            if (writeClassDelimiters) {
                writer.Write(_END_CLASS);
            }
        }

        public override void SerializeText(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            task.CurrentParameterIndex = 0;
            if (task.CurrentDepth >= task.Settings.MaxDepth)
                return;

            // Starting delimiters.
            bool writeClassDelimiters = task.CurrentDepth > 1;
            if (writeClassDelimiters) {
                writer.Write(_BEGIN_CLASS);
            }

            // Write value.
            for (int i = 0; i < nodesArray.Length; i++) {
                if (nodesArray[i].WriteText(obj, writer, ref task)) {
                    task.CurrentParameterIndex += 1;
                    writer.Write(_NEW_LINE);
                }
            }

            // End delimiters.
            if (writeClassDelimiters) {
                if (task.CurrentParameterIndex > 0) {
                    writer.WriteIndent(task.CurrentDepth - 1);
                }
                writer.Write(_END_CLASS);
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
                }
                break;
                case ConvertBehaviour.NameAndOrder: {
                    DeserializeBinaryNameAndOrder(ref obj, reader, ref task);
                }
                break;
                case ConvertBehaviour.Configuration:
                    throw new NotSupportedException("Binary deserialization is not supported with the 'Configuration' convert behaviour.");
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return obj;
        }

        private void DeserializeBinaryOrder(ref object obj, Utf8Reader reader, ref DeserializeTask task)
        {
            Stream stream = reader.BaseStream;
            while (true) {
                long startIndex = stream.Position;
                if (stream.Position + 1 > stream.Length)
                    return;

                switch (reader.ReadByte()) {
                    case _BEGIN_CLASS:
                        continue;
                    case _END_CLASS:
                        return;
                    case _BEGIN_META:
                        break;
                    default:
                        goto failed;
                }

                try {
                    ushort val = reader.ReadUInt16();
                    stream.Position += 1;
                    nodesArray[val].ReadBinary(obj, reader, ref task);
                    continue;
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

            // Failed. Skip to next node.
            // Note that binary error handling is unstable due to limitations.
            failed:
                try {
                    stream.Position = startIndex + 1;
                    if (!BinarySkipToNextNode(reader))
                        break;

                } catch (Exception) { break; }
            }
        }

        private void DeserializeBinaryNameAndOrder(ref object obj, Utf8Reader reader, ref DeserializeTask task)
        {
            Stream stream = reader.BaseStream;
            while (true) {
                if (stream.Position + 1 > stream.Length)
                    return;

                long startIndex = stream.Position;
                switch (reader.ReadByte()) {
                    case _BEGIN_CLASS:
                        continue;
                    case _END_CLASS:
                        return;
                    case _BEGIN_META:
                        break;
                    default:
                        goto failed;
                }

                // Get the index first.
                ushort readIndex = reader.ReadUInt16();
                stream.Position += 1;

                // Step 1: Attempt to deserialize based on name.
                try {
                    if (!TryReadString(reader, out string name))
                        goto Step2;
                    if (!nodesDictionary.TryGetValue(name, out Node node))
                        goto Step2;

                    stream.Position += 1;
                    node.ReadBinary(obj, reader, ref task);
                    continue;
                } catch (Exception) { /* ignored */ }

            // Step 2: Attempt to deserialize based on declaration order.
            Step2:
                try {
                    stream.Position = startIndex;
                    if (!SkipToNextSymbol(reader, _BEGIN_VALUE))
                        break;

                    nodesArray[readIndex].ReadBinary(obj, reader, ref task);
                    continue;
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

            // Step 3: Failed. Skip to next node.
            // Note that binary error handling is unstable due to limitations.
            failed:
                try {
                    stream.Position = startIndex;
                    if (!BinarySkipToNextNode(reader))
                        break;
                } catch (Exception) {
                    break;
                }
            }
        }

        public T Deserialize<T>(Utf8Reader reader, ref DeserializeTask task)
            => (T)DeserializeBinary(reader, ref task);

        public override object DeserializeText(Utf8Reader reader, ref DeserializeTask task)
        {
            object obj = isValueType ? Activator.CreateInstance(Type) : objCtor.Invoke();

            switch (task.Settings.ConvertBehaviour) {
                case ConvertBehaviour.Order: {
                    DeserializeTextOrder(ref obj, reader, ref task);
                }
                break;
                case ConvertBehaviour.Configuration: {
                    DeserializeTextName(ref obj, reader, ref task);
                }
                break;
                case ConvertBehaviour.NameAndOrder: {
                    DeserializeTextNameAndOrder(ref obj, reader, ref task);
                }
                break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return obj;
        }

        private void DeserializeTextOrder(ref object obj, Utf8Reader reader, ref DeserializeTask task)
        {
            Stream stream = reader.BaseStream;
            while (true) {
                if (stream.Position + 1 > stream.Length)
                    return;

                long startIndex = stream.Position;
                try {
                    // Look for next variable.
                    // Skip _TAB, _NEW_LINE and _BEGIN_CLASS.
                    // Break if _END_CLASS is encountered.
                    switch (reader.ReadByte()) {
                        case _TAB:
                        case _NEW_LINE:
                        case _BEGIN_CLASS:
                            continue;
                        case _END_CLASS:
                            return;
                    }
                    stream.Position -= 1;

                    // Index meta parsing.
                    SkipToNextSymbol(reader, _BEGIN_META);
                    uint index = uint.Parse(reader.ReadTo(_END_META));

                    // Read node value.
                    SkipToNextSymbol(reader, _BEGIN_VALUE);
                    reader.SkipWhiteSpace();
                    nodesArray[index].ReadText(obj, reader, ref task);

                    continue;
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

                // Failed, skip to next node.
                try {
                    stream.Position = startIndex + 1;
                    if (!TextSkipToNextNode(reader, ref task))
                        break;
                } catch (Exception) {
                    break;
                }
            }
        }

        private void DeserializeTextName(ref object obj, Utf8Reader reader, ref DeserializeTask task)
        {
            Stream stream = reader.BaseStream;
            while (true) {
                if (stream.Position + 1 > stream.Length)
                    return;

                long startIndex = stream.Position;
                try {
                    switch (reader.ReadByte()) {
                        case _TAB:
                        case _NEW_LINE:
                        case _BEGIN_CLASS:
                            continue;
                        case _END_CLASS:
                            return;
                    }
                    stream.Position -= 1;

                    // Grab name, and attempt to get the node based on it. If no node is found, goto Failed.
                    reader.SkipWhiteSpace();
                    string name = reader.ReadQuotedString();
                    reader.BaseStream.Position += 1;
                    reader.SkipWhiteSpace();
                    if (!nodesDictionary.TryGetValue(name, out Node node))
                        goto Failed;

                    node.ReadText(obj, reader, ref task);
                    continue;
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

            Failed:
                try {
                    stream.Position = startIndex;
                    if (!TextSkipToNextNode(reader, ref task))
                        break;
                } catch (Exception) {
                    break;
                }
            }
        }

        private void DeserializeTextNameAndOrder(ref object obj, Utf8Reader reader, ref DeserializeTask task)
        {
            Stream stream = reader.BaseStream;
            while (true) {
                if (stream.Position + 1 > stream.Length)
                    return;

                long startIndex = stream.Position;
                uint? readIndex = null;

                // Step 1: Parse by name.
                try {
                    // Look for next variable.
                    // Skip _TAB, _NEW_LINE and _BEGIN_CLASS.
                    // Break if _END_CLASS is encountered.
                    switch (reader.ReadByte()) {
                        case _TAB:
                        case _NEW_LINE:
                        case _BEGIN_CLASS:
                            continue;
                        case _END_CLASS:
                            return;
                    }
                    stream.Position -= 1;

                    // Find the index.
                    SkipToNextSymbol(reader, _BEGIN_META);
                    readIndex = uint.Parse(reader.ReadTo(_END_META));

                    // Grab name, and attempt to get the node based on it. If no node is found, goto Failed.
                    reader.SkipWhiteSpace();
                    string name = reader.ReadQuotedString();
                    reader.BaseStream.Position += 1;
                    if (!nodesDictionary.TryGetValue(name, out Node node))
                        goto Step2;

                    // Parse the node.
                    reader.SkipWhiteSpace();
                    node.ReadText(obj, reader, ref task);
                    continue;
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

            // Step 2: Failed, try to parse via index.
            Step2:
                if (readIndex == null)
                    goto Failed;

                try {
                    // Go to the original position, and skip to the value.
                    stream.Position = startIndex;
                    SkipToNextSymbol(reader, _BEGIN_META, false);
                    SkipMeta(reader);
                    SkipToNextSymbol(reader, _BEGIN_VALUE);
                    reader.SkipWhiteSpace();

                    // Finally, read the value.
                    nodesArray[readIndex.Value].ReadText(obj, reader, ref task);
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception) { /* ignored */ }

            // Failed. Go to next node.
            Failed:
                try {
                    stream.Position = startIndex;
                    if (!TextSkipToNextNode(reader, ref task))
                        break;
                } catch (Exception) {
                    break;
                }
            }
        }

        internal bool BinarySkipToNextNode(Utf8Reader reader)
        {
            Stream stream = reader.BaseStream;
            while (true) {
                if (stream.Position + 1 > stream.Length)
                    return false;

                // Attempt to skip to the next node, usually as a result of an error.
                switch (reader.ReadByte()) {
                    case _BEGIN_ARRAY:
                        stream.Position -= 1;
                        SkipArray(reader);
                        break;
                    case _BEGIN_CLASS:
                        stream.Position -= 1;
                        SkipClass(reader);
                        break;
                    case _END_CLASS:
                        return false;
                    case _STRING_IDENTIFIER:
                        stream.Position -= 1;
                        SkipQuotedString(reader);
                        break;
                    case _BEGIN_META:
                        ushort index = reader.ReadUInt16();
                        if (index > nodesArray.Length) {
                            stream.Position -= 1 + sizeof(ushort);
                            return true;
                        } else {
                            stream.Position -= 1 + sizeof(ushort);
                            SkipMeta(reader);
                        }
                        break;
                    case _ESCAPE:
                        if (stream.Position + 1 > stream.Length) {
                            return false;
                        }
                        stream.Position += 1;
                        break;
                    default:
                        continue;

                }
            }
        }

        internal bool TextSkipToNextNode(Utf8Reader reader, ref DeserializeTask task)
        {
            Stream stream = reader.BaseStream;
            ConvertBehaviour convertType = task.Settings.ConvertBehaviour;

            if (convertType == ConvertBehaviour.NameAndOrder || convertType == ConvertBehaviour.Order) {
                // Meta tokens are before each node. Therefore, the reader needs to read until a begin meta token,
                // and then check if the next byte is a UTF8 number character. If both conditions are true,
                // the next node has been found.
                while (true) {
                    if (stream.Position + 1 > stream.Length)
                        return false;

                    switch (reader.ReadByte()) {
                        case _BEGIN_ARRAY:
                            stream.Position -= 1;
                            SkipArray(reader);
                            break;
                        case _BEGIN_CLASS:
                            stream.Position -= 1;
                            SkipClass(reader);
                            break;
                        case _END_CLASS:
                            return false;
                        case _STRING_IDENTIFIER:
                            stream.Position -= 1;
                            SkipQuotedString(reader);
                            break;
                        case _BEGIN_META:
                            byte numTest = reader.ReadByte();
                            if (IsNumber(numTest)) {
                                stream.Position -= 2;
                                return true;
                            } else {
                                stream.Position -= 2;
                                SkipMeta(reader);
                            }
                            break;
                        case _ESCAPE:
                            if (stream.Position + 1 > stream.Length) {
                                return false;
                            }
                            stream.Position += 1;
                            break;
                        default:
                            continue;

                    }
                }
            }

            // There is NO meta token used to identify new nodes. Therefore, a begin value token must be found,
            // and then the reader needs to seek backwards to the beginning of the node's name.
            // The first begin token is ignored, as that would be the current node.
            bool firstBeginValueTokenFound = false;
            while (true) {
                if (stream.Position + 1 > stream.Length)
                    return false;

                switch (reader.ReadByte()) {
                    case _BEGIN_ARRAY:
                        stream.Position -= 1;
                        SkipArray(reader);
                        break;
                    case _BEGIN_CLASS:
                        stream.Position -= 1;
                        SkipClass(reader);
                        break;
                    case _END_CLASS:
                        return false;
                    case _STRING_IDENTIFIER:
                        stream.Position -= 1;
                        SkipQuotedString(reader);
                        break;
                    case _BEGIN_META:
                        stream.Position -= 1;
                        SkipMeta(reader);
                        break;
                    case _BEGIN_VALUE:
                        if (firstBeginValueTokenFound) {
                            return SeekBackToStartOfNodeName(reader);
                        } else {
                            firstBeginValueTokenFound = true;
                        }
                        continue;
                    case _ESCAPE:
                        if (stream.Position + 1 > stream.Length) {
                            return false;
                        }
                        stream.Position += 1;
                        break;
                    default:
                        continue;

                }
            }
        }

        internal static bool SeekBackToStartOfNodeName(Utf8Reader reader)
        {
            Stream stream = reader.BaseStream;
            while (true) {
                if (stream.Position - 1 < 0)
                    return false;

                stream.Position -= 1;
                byte b = reader.ReadByte();
                stream.Position -= 1;

                if (IsControlCharOrWhitespace(b)) {
                    stream.Position += 1;
                    return true;
                }
                continue;
            }
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
            } catch (Exception) { return false; }
        }

        private sealed class Node
        {
            public SerializableType Info;
            public string Name;
            public string RealName;
            public ushort Index;
            public object NodeDefault;

            private Converter converter;
            private TypeAccessor accessor;
            private bool recursive;

            // Faster access for delegate accessors.
            private TypeAccessor.DelegateAccessor delegateAccessor;
            private int accessorIndex;

            // Cached stuff.
            private byte[] precompiledName;
            private byte[] precompiledNameWithIndex;

            public Node(Converter converter, TypeAccessor accessor, Type type, object nodeDefault, string name, string realName, ushort index, bool recursive)
            {
                this.converter = converter;
                this.accessor = accessor;
                this.recursive = recursive;
                Index = index;
                Name = name;
                RealName = realName;
                Info = GetSerializerTypeInfo(type);
                NodeDefault = nodeDefault;
                precompiledName = Serializer.UTF8.GetBytes((char)_STRING_IDENTIFIER + Name + (char)_STRING_IDENTIFIER + (char)_BEGIN_VALUE + ' ');

                // Precompiled names with meta index.
                byte[] tmpByteArr = ArrayPool<byte>.Shared.Rent(5);
                Span<byte> indexSpan = new Span<byte>(tmpByteArr);
                Utf8Formatter.TryFormat(Index, indexSpan, out int bytesWritten);
                string indexStr = Serializer.UTF8.GetString(indexSpan.Slice(0, bytesWritten));
                
                precompiledNameWithIndex = Serializer.UTF8.GetBytes(
                    (char)_BEGIN_META
                    + indexStr
                    + (char)_END_META
                    + (char)_STRING_IDENTIFIER
                    + Name
                    + (char)_STRING_IDENTIFIER
                    + (char)_BEGIN_VALUE + ' ');

                ArrayPool<byte>.Shared.Return(tmpByteArr);

                if (accessor is TypeAccessor.DelegateAccessor delAccessor) {
                    delegateAccessor = delAccessor;
                    accessorIndex = delAccessor.GetIndex(RealName) ?? throw new IndexOutOfRangeException();
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
                // TODO: ^^^ How do I handle NULL vs default??
                if (!shouldSetValue) {
                    if (NodeDefault == null)
                        return; // Fix dumb FastMember bug where setting null causes exception.

                    SetValue(target, NodeDefault);
                    return;
                }

                // Otherwise, set the value.

                // Handle meta-data.
                // Type.
                Converter typeConverter = converter;
                Serializer.TryReadMetaBinary(reader, task.Settings, out string valueType, out int? id, out int? reference);
                if (Info.PossiblePolymorphism) {
                    if (valueType != null) {
                        typeConverter = Serializer.GetConverterFromTypeString(valueType, task.Settings);
                    }
                }

                // Reference.
                // If 'id' is set, the object is not a reference, so it's added to a
                // temporary lookup table in the case that another object references it.
                // Otherwise, if 'ref' is set, the object is a reference.
                bool pendingRecordReference = false;
                if (task.Settings.ReferenceHandling == ReferenceHandling.Preserve) {
                    if (id != null) {
                        pendingRecordReference = true;
                    } else if (reference != null) {
                        if (task.TryGetObjectRefByKey(reference.Value, out ObjectRef objRef)) {
                            SetValue(target, objRef.Obj);
                            return;
                        }
                    }
                }

                // Set the value.
                SetValue(target, Serializer.DeserializeReader(reader, typeConverter, ref task));

                // If the object isn't a reference, record it for possible future use.
                if (pendingRecordReference) {
                    task.AddReference(target, new ObjectRef(target, id.Value));
                }
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
                bool serializeType = Info.PossiblePolymorphism && settings.TypeHandling != TypeHandling.Ignore;
                bool checkReference = !Info.IsValueType && settings.ReferenceHandling == ReferenceHandling.Preserve;

                // Resolve true type converter (in case of polymorphism).
                if (serializeType && val != null) {
                    valType = val.GetType();
                    if (valType != Info) {
                        typeConverter = settings.Resolver.GetConverter(valType);
                    }
                }

                // Null and default value handling.
                bool isDefault = IsDefault(val);
                bool writeNull = settings.NullValueHandling == NullValueHandling.DefaultValue;
                bool writeDefault = settings.DefaultValueHandling == DefaultValueHandling.Serialize;
                if (!writeNull && val == null || !writeDefault && isDefault)
                    return;

                // Name & index.
                writer.Write(_BEGIN_META);
                writer.Write(Index);
                writer.Write(_END_META);
                if (writeName) {
                    writer.Write(Name);
                    writer.Write(_BEGIN_VALUE);
                }

                // Write bool for null or default values.
                if (writeNull || writeDefault) {
                    bool shouldWrite = writeNull && val != null || writeDefault && !isDefault;
                    writer.Write(shouldWrite);
                    if (!shouldWrite)
                        return;
                }

                // Prepare meta info.
                // Type.
                bool shouldWriteType = Serializer.ShouldWriteConverterType(valType, Info, settings);
                string metaType = null;
                if (shouldWriteType && valType != null) {
                    metaType = GetQualifiedTypeName(valType, settings);
                }

                // Reference.
                int? id = null;
                int? reference = null;
                if (checkReference) {
                    if (task.TryGetObjectRef(val, out ObjectRef existingRef)) {
                        reference = existingRef.Reference;
                        existingRef.RefCount++;
                    } else {
                        id = task.curReference++;
                        task.AddReference(val, new ObjectRef(target, id.Value));
                    }
                }

                // Write meta-data.
                Serializer.WriteMetaBinary(writer, settings, metaType, id, reference);

                // Serialize the value if it isn't a reference.
                if (!reference.HasValue) {
                    Serializer.SerializeWriter(writer, val, typeConverter, ref task);
                }
            }

            public bool WriteText(object target, Utf8Writer writer, ref SerializeTask task)
            {
                if (recursive && task.Settings.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                    throw new ReferenceLoopException();

                object val = GetValue(target);
                Type valType = null;
                Converter typeConverter = converter;
                SerializerSettings settings = task.Settings;
                bool serializeType = Info.PossiblePolymorphism && settings.TypeHandling != TypeHandling.Ignore;
                bool checkReference = !Info.IsValueType && settings.ReferenceHandling == ReferenceHandling.Preserve;

                byte[] nameToWrite = settings.ConvertBehaviour == ConvertBehaviour.Configuration
                    ? precompiledName
                    : precompiledNameWithIndex;

                // Resolve true type converter (in case of polymorphism).
                if (serializeType && val != null) {
                    valType = val.GetType();
                    if (valType != Info) {
                        typeConverter = settings.Resolver.GetConverter(valType);
                    }
                }

                // Null and default value handling.
                bool writeNull = settings.NullValueHandling == NullValueHandling.DefaultValue;
                bool writeDefault = settings.DefaultValueHandling == DefaultValueHandling.Serialize;
                if (!writeNull && val == null || !writeDefault && IsDefault(val))
                    return false;

                // If this is the first parameter, go to new line.
                if (task.CurrentParameterIndex == 0 && task.CurrentDepth > 1) {
                    writer.Write(_NEW_LINE);
                }

                // Write name.
                writer.WriteIndent(task.CurrentDepth);
                writer.Write(nameToWrite);

                // Prepare the meta-data.
                // Type name.
                bool shouldWriteType = Serializer.ShouldWriteConverterType(valType, Info, settings);
                string metaType = null;
                if (shouldWriteType && valType != null) {
                    metaType = GetQualifiedTypeName(valType, settings);
                }

                // Reference.
                int? id = null;
                int? reference = null;
                if (checkReference) {
                    if(task.TryGetObjectRef(val, out ObjectRef existingRef)) {
                        reference = existingRef.Reference;
                        existingRef.RefCount++;
                    } else {
                        id = task.curReference++;
                        task.AddReference(val, new ObjectRef(target, id.Value));
                    }
                }

                // Write meta-data.
                Serializer.WriteMetaText(writer, settings, metaType, id, reference);

                // Serialize the value if it isn't a reference.
                if (!reference.HasValue) {
                    Serializer.SerializeWriter(writer, val, typeConverter, ref task);
                }
                return true;
            }

            public void ReadText(object target, Utf8Reader reader, ref DeserializeTask task)
            {
                if (recursive && task.Settings.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                    throw new ReferenceLoopException();

                // Read the meta data.
                // Type.
                Converter typeConverter = converter;
                Serializer.TryReadMetaText(reader, task.Settings, out string valueType, out int? id, out int? reference);
                if (Info.PossiblePolymorphism) {
                    if (valueType != null) {
                        typeConverter = Serializer.GetConverterFromTypeString(valueType, task.Settings);
                    }
                }

                // Reference.
                // If 'id' is set, the object is not a reference, so it's added to a
                // temporary lookup table in the case that another object references it.
                // Otherwise, if 'ref' is set, the object is a reference.
                bool pendingRecordReference = false;
                if(task.Settings.ReferenceHandling == ReferenceHandling.Preserve) {
                    if(id != null) {
                        pendingRecordReference = true;
                    } else if (reference != null) {
                        if (task.TryGetObjectRefByKey(reference.Value, out ObjectRef objRef)) {
                            SetValue(target, objRef.Obj);
                            return;
                        }
                    }
                }

                // Set the value.
                SetValue(target, Serializer.DeserializeReader(reader, typeConverter, ref task));

                // If the object isn't a reference, record it for possible future use.
                if (pendingRecordReference) {
                    task.AddReference(target, new ObjectRef(target, id.Value));
                }
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
                if (NodeDefault == null && value == null)
                    return true;

                return value.Equals(NodeDefault);
            }
        }

        private struct NodeIndexComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y) => x.Index.CompareTo(y.Index);
        }
    }
}
