namespace SE.Pooling
{
    /// <summary>How the pool behaves when it's capacity is overflowed.</summary>
    public enum PoolBehavior
    {
        /// <summary>Pool will automatically create new instances as needed.</summary>
        Grow,

        /// <summary>NULL will be returned, and it will not grow.</summary>
        Fixed,

        /// <summary>Pool will not grow. However new instances will be returned if needed.</summary>
        FixedInstantiate
    }
}