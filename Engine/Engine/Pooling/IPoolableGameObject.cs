namespace SE.Pooling
{
    public interface IPoolableGameObject : IPoolable
    {
        /// <summary>If true, the GameObject will be disabled, and returned to it's pool on Destroy().</summary>
        bool ReturnOnDestroy { get; set; }
        IGameObjectPool MyPool { get; set; }
    }
}
