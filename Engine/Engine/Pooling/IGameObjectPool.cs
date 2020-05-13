using SE.Common;

namespace SE.Pooling
{
    public interface IGameObjectPool
    {
        public void Return(GameObject go);
        public void DestroyedCallback(GameObject go);
    }
}
