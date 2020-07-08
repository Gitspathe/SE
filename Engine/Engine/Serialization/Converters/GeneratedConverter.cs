using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FastMember;
using FastStream;
using SE.Serialization.Resolvers;

namespace SE.Serialization.Converters
{
    public class GeneratedConverter : Converter
    {
        public override Type Type { get; }

        private Node[] nodesArr;
        private int nodesLength;

        private Dictionary<string, Node> nodes = new Dictionary<string, Node>();

        private delegate object ObjectActivator();
        private ObjectActivator activator;
        private TypeAccessor accessor;
        private bool isValueType;

        private GeneratedConverter(Type type, ConverterResolver resolver)
        {
            Type = type;
            isValueType = type.IsValueType;
            accessor = TypeAccessor.Create(type, true);

            // Generate compiled constructors for reference types.
            if(!isValueType)
                GenerateCtor();

            MemberSet set = accessor.GetMembers();
            nodesArr = new Node[set.Count];
            int curIndex = 0;
            foreach (Member member in set) {
                
                // TODO: Replace this with attributes and stuff.
                // TODO: Attribute to override declaration order and/or name for variables.
                if(!member.IsPublic)
                    continue;

                Converter converter = resolver.GetConverter(member.Type);

                // Skip this member if a valid converter was not found.
                if (converter == null) 
                    continue;

                Node val = new Node(converter, accessor, member.Name);
                nodes.Add(member.Name, val);
                nodesArr[curIndex] = val;

                curIndex++;
            }
            nodesLength = curIndex;
        }

        internal static GeneratedConverter Generate(Type type, ConverterResolver resolver)
        {
            GeneratedConverter gen = new GeneratedConverter(type, resolver);
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
            bool writeName = settings.ConvertBehaviour == ConvertBehaviour.Name
                             || settings.ConvertBehaviour == ConvertBehaviour.NameAndOrder;

            for (int i = 0; i < nodesLength; i++) {
                writer.Write('|');
                nodesArr[i].Write(obj, writer, settings, writeName);
            }
        }

        public override object Deserialize(FastReader reader, SerializerSettings settings)
        {
            object obj = isValueType ? Activator.CreateInstance(Type) : activator.Invoke();

            switch (settings.ConvertBehaviour) {
                
                case ConvertBehaviour.Name: {
                    for (int i = 0; i < nodesLength; i++) {
                        SkipDelimiter(reader);
                        if (nodes.TryGetValue(reader.ReadString(), out Node node)) {
                            try {
                                node.Read(obj, reader, settings);
                                continue;
                            } catch (Exception) { }
                        }

                        i++;
                        if(!SkipToNextDelimiter(reader))
                            break;
                    }
                } break;
                
                case ConvertBehaviour.Order: {
                    for (int i = 0; i < nodesLength; i++) {
                        SkipDelimiter(reader);
                        try {
                            nodesArr[i].Read(obj, reader, settings);
                            continue;
                        } catch (Exception) { /* ignored */ }

                        i++;
                        if(!SkipToNextDelimiter(reader))
                            break;
                    }
                } break;
                
                case ConvertBehaviour.NameAndOrder: {
                    for (int i = 0; i < nodesLength; i++) {
                        SkipDelimiter(reader);
                        long startIndex = reader.BaseStream.Position;
                        
                        // Step 1: Attempt to deserialize based on name.
                        try {
                            if (nodes.TryGetValue(reader.ReadString(), out Node node)) {
                                node.Read(obj, reader, settings);
                                continue;
                            }
                        } catch(Exception) { /* ignored */ }

                        // Step 2: Attempt to deserialize based on declaration order.
                        try {
                            reader.BaseStream.Position = startIndex;
                            nodesArr[i].Read(obj, reader, settings);
                        } catch (Exception) { /* ignored */ }

                        // Step 3: Skip to next element.
                        i++;
                        if(!SkipToNextDelimiter(reader))
                            break;
                    }
                } break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO: Error handling.
            // - What if the name of a variable changes? It should try and deserialize what it can.
            //   > Could use a '|' separator for this, and find the next separator if deserialization fails.
            // - Will need to throw an error when creating an instance fails.

            return obj;
        }

        public T Deserialize<T>(FastReader reader, SerializerSettings settings) 
            => (T) Deserialize(reader, settings);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipDelimiter(FastReader reader) 
            => reader.BaseStream.Position += 2;

        private bool SkipToNextDelimiter(FastReader reader)
        {
            Stream baseStream = reader.BaseStream;
            while (baseStream.Position + sizeof(char) < baseStream.Length) {
                if (reader.PeekChar() == '|') {
                    return true;
                }
                reader.ReadChar();
            }
            return false;
        }

        private sealed class Node 
        {
            private Converter converter;
            private TypeAccessor accessor;
            private string name;

            public Node(Converter converter, TypeAccessor accessor, string name)
            {
                this.converter = converter;
                this.accessor = accessor;
                this.name = name;
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
                if(writeName)
                    writer.Write(name);

                writer.Write(val != null);
                if (val == null) 
                    return;

                converter.Serialize(val, writer, settings);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetValue(object target, object value)
                => accessor[target, name] = value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public object GetValue(object target)
                => accessor[target, name];
        }
    }
}
