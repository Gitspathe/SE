using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;

namespace DeeZ.Core.Extensions
{
    public static class LiteNetLibExtensions
    {
        public static string GetUniqueID(this NetPeer netPeer) 
            => netPeer.EndPoint.ToString();
    }
}
