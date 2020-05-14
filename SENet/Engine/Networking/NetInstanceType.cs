namespace SE.Engine.Networking
{
    /// <summary>
    /// An instance's state in relation to the networking system.
    /// </summary>
    public enum NetInstanceType
    {
        /// <summary>Instance is not a client or a server.</summary>
        None,

        /// <summary>Instance is hosting a server.</summary>
        Server,

        /// <summary>Instance is a client connected to a server.</summary>
        Client
    }
}
