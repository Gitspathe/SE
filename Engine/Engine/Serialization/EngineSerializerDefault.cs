using FastMember;

namespace SE.Serialization
{
    public class EngineSerializerDefault : EngineSerializerBase
    {
        public ISerializedObject Obj { get; }

        protected void Hook(object obj, string name)
        {
            ValueWrappers.Add(new SerializedValue(Accessor, obj, name));
        }

        protected override void Setup()
        {
            SerializerInfo serializerInfo = SerializerReflection.GetObjectSerializer(Obj.GetType());
            Accessor = TypeAccessor.Create(Obj.GetType(), true);
            if (serializerInfo != null) {
                foreach (string varName in serializerInfo.SerializedVariables) {
                    Hook(Obj, varName);
                }
            }
        }

        public void Initialize()
        {
            Setup();
            initialized = true;
        }

        public EngineSerializerDefault(ISerializedObject obj)
        {
            Obj = obj;
            initialized = false;
        }
    }
}
