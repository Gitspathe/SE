using FastMember;

namespace SE.Serialization
{
    public abstract class EngineSerializer<T> : EngineSerializerBase
    {
        public T Obj { get; }

        protected EngineSerializer(T obj)
        {
            Obj = obj;
            Accessor = TypeAccessor.Create(typeof(T), true);
        }
    }
}
