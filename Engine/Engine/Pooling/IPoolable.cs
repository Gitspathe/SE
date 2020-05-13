namespace SE.Pooling
{
    public interface IPoolable
    {
        void TakenFromPool() { }
        void ReturnedToPool() { }
        void PoolInitialize() { }
    }
}
