using System;
using LiteNetLib.Utils;

namespace DeeZ.Engine.Networking
{
    public interface INetLogicProxy
    {
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
        /// <param name="netState">Serialized state information created by the SerializeNetworkState function.</param>
        void Setup(uint id, bool isOwner, string netState = null);
    }

    public interface INetPersistable : INetLogic
    {
        /// <summary>
        /// Serializes custom network information into a Json string.
        /// Deserialized to restore object state when a new client joins.
        /// </summary>
        /// <returns>Json string containing the custom state information.</returns>
        string SerializeNetworkState();

        /// <summary>
        /// Deserializes a Json string sent from the SerializeNetworkState function.
        /// Restores custom state information when a new client joins.
        /// </summary>
        /// <param name="jsonString">Serialized state information.</param>
        void RestoreNetworkState(string jsonString);
    }

    public interface INetInstantiatable : INetLogic
    {
        object[] InstantiateParameters { get; }
        void OnNetworkInstantiatedServer(string type, string owner);
        void OnNetworkInstantiatedClient(string type, bool isOwner, byte[] data);
        byte[] GetBufferedData();
        void NetClean();
    }

    public interface IInstantiateSerializer
    {
        byte[] Serialize(NetDataWriter writer);
    }
}