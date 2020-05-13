using System;

namespace DeeZ.Engine.Networking.Attributes
{
    /// <summary>
    /// Attribute applied to functions. Tags the desired function as being a Remote Procedure Call from the client to the server.
    /// Ensure that code inside will not create security vulnerabilities, as it will be called on the server, possibly by cheaters or hackers.
    /// NOTE: After a ClientRPC is sent, the server will also call a ClientRPC function of the same name, ending with _CLIENT, on all other clients.
    /// For example: if a client calls UpdatePosition() on the server, the server will then call UpdatePosition_CLIENT() on all other clients except the sender.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServerRPCAttribute : Attribute
    {
        internal readonly bool CallClientRPC;
        internal readonly bool Frequent;

        public ServerRPCAttribute(bool callClientRPC = true, bool frequent = false)
        {
            CallClientRPC = callClientRPC;
            Frequent = frequent;
        }
    }
}
