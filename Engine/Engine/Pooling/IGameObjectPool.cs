using SE.Common;

namespace SE.Pooling
{
    public interface IGameObjectPool
    {
        void Return(GameObject go);
        void DestroyedCallback(GameObject go);
    }
}
