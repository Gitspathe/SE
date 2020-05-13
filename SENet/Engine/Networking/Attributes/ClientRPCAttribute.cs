using System;

namespace DeeZ.Engine.Networking.Attributes
{

    /// <summary>
    /// Attribute applied to functions. Tags the desired function as being a Remote Procedure Call from the server to clients.
    /// NOTE: Server RPC functions MUST be appended with _CLIENT. For example: UpdatePosition() would become UpdatePosition_CLIENT().
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientRPCAttribute : Attribute
    {
        internal readonly bool Frequent;

        public ClientRPCAttribute(bool frequent = false)
        {
            Frequent = frequent;
        }
    }

}
