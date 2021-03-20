using LiteNetLib.Utils;
using SE.Core;
using SE.Engine.Networking.Utility;

namespace SE.Engine.Networking
{
    /// <summary>
    /// Interface representing an entity that HAS an INetLogic interface present, but one not present
    /// on the object itself. This is typically used for something like GameObjects, which may or may
    /// not have an INetLogic component.
    /// </summary>
    public interface INetLogicProxy
    {
        /// <summary>INetLogic interface.</summary>
        INetLogic NetLogic { get; }
    }

    /// <summary>
    /// Interface responsible for enabling networking functionality for an object.
    /// </summary>
    public interface INetLogic
    {
        /// <summary>Networked ID for the NetLogic. Will be the same across all devices in the network.</summary>
        uint ID { get; }
        /// <summary>If the NetLogic has been set up, and given a unique ID by the network manager.</summary>
        bool IsSetup { get; }
        /// <summary>If the NetLogic is owned by the local client.</summary>
        bool IsOwner { get; }

        /// <summary>
        /// Called by the network manager, assigns the NetLogic a unique network ID.
        /// SHOULD NOT be called manually, except for special non GameObject or Component types.
        /// </summary>
        /// <param name="id">Network ID.</param>
        /// <param name="isOwner">If the local client owns the NetLogic.</param>
        void Setup(uint id, bool isOwner);
    }

    /// <summary>
    /// Represents an object whose state persists to new clients on connection.
    /// </summary>
    public interface INetPersistable : INetLogic
    {
        /// <summary>
        /// Serializes custom network information into a Json string.
        /// Intended to be used for <see cref="RestoreNetworkState"/>.
        /// </summary>
        /// <returns>Json string containing the custom state information.</returns>
        byte[] SerializeNetworkState(NetDataWriter writer);

        /// <summary>
        /// Deserializes a Json string sent from the SerializeNetworkState function.
        /// Intended to restore state retrieved from <see cref="SerializeNetworkState()"/>.
        /// </summary>
        /// <param name="reader">Serialized state information.</param>
        void RestoreNetworkState(NetDataReader reader);
    }

    public static class NetLogicHelper
    {
        public static byte[] SerializePersistable(INetPersistable persist)
        {
            lock (Network.NetworkLock) {
                NetDataWriter writer = ReaderWriterPool.GetWriter();
                byte[] bytes = persist.SerializeNetworkState(writer);
                ReaderWriterPool.ReturnWriter(writer);
                return bytes;
            }
        }

        public static void RestorePersistable(INetPersistable persist, byte[] bytes)
        {
            lock (Network.NetworkLock) {
                NetDataReader reader = ReaderWriterPool.GetReader(bytes);
                persist.RestoreNetworkState(reader);
                ReaderWriterPool.ReturnReader(reader);
            }
        }
    }

    /// <summary>
    /// Represents an object which can be Instantiated across the network.
    /// </summary>
    public interface INetInstantiatable : INetLogic
    {
        /// <summary>Parameters used for the constructor(s) of the object.</summary>
        object[] InstantiateParameters { get; }

        /// <summary>
        /// Called when the object is instantiated on the server.
        /// This is called before <see cref="OnNetworkInstantiatedClient"/>.
        /// </summary>
        /// <param name="type">Spawnable ID for the object.</param>
        /// <param name="owner">Unique ID of the peer who has authority over the object.</param>
        void OnNetworkInstantiatedServer(string type, string owner, NetDataWriter writer);

        /// <summary>
        /// Called when an object is instantiated on the local client.
        /// This is called after <see cref="OnNetworkInstantiatedClient"/>.
        /// </summary>
        /// <param name="type">Spawnable ID for the object.</param>
        /// <param name="isOwner">Whether or not the local client has authority over the object.</param>
        /// <param name="reader">Data passed in from the server instance of the object.
        ///                    Intended to be used for constructors.</param>
        void OnNetworkInstantiatedClient(string type, bool isOwner, NetDataReader reader);
        
        /// <summary>
        /// Used to obtain a byte array of custom data. Intended to be used for constructors.
        /// </summary>
        /// <returns>Byte array of data.</returns>
        byte[] GetBufferedData(NetDataWriter writer);

        /// <summary>
        /// Called when the networked object is destroyed.
        /// </summary>
        void NetClean();
    }

    /// <summary>
    /// Represents a networked serializer related to <see cref="INetPersistable"/>.
    /// </summary>
    public interface IInstantiateSerializer
    {
        /// <summary>
        /// Serializes data into a byte array.
        /// </summary>
        /// <param name="writer">Helper NetDataWriter.</param>
        /// <returns>Byte array of serialized data.</returns>
        byte[] Serialize(NetDataWriter writer);
    }
}