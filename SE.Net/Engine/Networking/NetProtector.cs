using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using static SE.Core.Network;

namespace SE.Engine.Networking
{
    /// <summary>
    /// Protects the server instance, and reports errors or warnings to the Console (for both clients and servers).
    /// </summary>
    public static class NetProtector
    {
        /// <summary>If true, server peers will be kicked if they cause too many exceptions.</summary>
        public static bool KickPeers { get; set; } = false;

        /// <summary>If true, all exceptions will be passed to the Console.</summary>
        public static bool LogExceptions { get; set; } = true;

        /// <summary>Peers who exceed this threshold of exceptions within 1 minute will be kicked from the server,
        ///          if KickPeers is set to true. Intended to mitigate exception-spamming DOS attacks.</summary>
        public static int KickExceptionThreshold { get; set; } = 512;

        private static Dictionary<NetPeer, PeerRecord> peerRecords = new Dictionary<NetPeer, PeerRecord>();

        public static void Update(float deltaTime)
        {
            if (!IsServer)
                return;

            foreach (PeerRecord peer in peerRecords.Values) {
                peer.Update(deltaTime);
            }
        }

        public static void AddPeer(NetPeer peer)
        {
            if (!IsServer)
                return;

            if (!peerRecords.ContainsKey(peer)) {
                peerRecords.Add(peer, new PeerRecord(peer));
            }
        }

        public static bool RemovePeer(NetPeer peer)
        {
            if (!IsServer)
                return false;

            return peerRecords.Remove(peer);
        }

        public static void ReportError(Exception exception, NetPeer peer = null)
        {
            if (LogExceptions) {
                LogError(peer != null
                    ? new Exception("NetException from " + peer.EndPoint + ": ", exception)
                    : new Exception("NetException: ", exception));
            }
            if (!IsServer || peer == null)
                return;

            PeerRecord record = peerRecords[peer];
            record.TotalErrors++;
            record.KickThreshold += 1.0f / KickExceptionThreshold;

            if (KickPeers && record.KickThreshold >= 1.0f) {
                Kick(peer, "Exception threshold reached.");
            }
        }

        public static void ReportWarning(Exception exception, NetPeer peer = null)
        {
            if (LogExceptions) {
                LogWarning(peer != null
                    ? new Exception("NetException from " + peer.EndPoint + ": ", exception)
                    : new Exception("NetException: ", exception));
            }
            if (!IsServer || peer == null)
                return;

            PeerRecord record = peerRecords[peer];
            record.TotalWarnings++;
        }

        public static void Kick(NetPeer peer, string reason)
        {
            if (!IsServer || peer == null)
                return;

            NetDataWriter writer = new NetDataWriter();
            writer.Put(reason);
            peer.Disconnect(writer);
            LogInfo("Kicked peer " + peer.EndPoint + ". Reason: '" + reason + "'");
        }

        internal class PeerRecord
        {
            public NetPeer Peer;
            public ulong TotalErrors;
            public ulong TotalWarnings;
            public float KickThreshold;

            public PeerRecord(NetPeer peer)
            {
                Peer = peer;
            }

            public void Update(float deltaTime)
            {
                KickThreshold -= deltaTime / 60.0f;
                if (KickThreshold < 0.0f)
                    KickThreshold = 0.0f;
            }
        }

    }
}
