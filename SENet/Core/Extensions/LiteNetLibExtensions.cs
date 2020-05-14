using LiteNetLib;

namespace SE.Core.Extensions
{
    public static class LiteNetLibExtensions
    {
        public static string GetUniqueID(this NetPeer netPeer) 
            => netPeer.EndPoint.ToString();
    }
}
