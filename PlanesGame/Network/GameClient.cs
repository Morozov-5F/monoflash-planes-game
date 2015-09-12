using System;
using Lidgren.Network;
using System.Diagnostics;
using System.Threading;

namespace PlanesGame.Network
{
    public class GameClient
    {
        public const int NET_PORT = 14242;
        public const string NET_ADRESS = "188.226.195.54";

        public const byte APPROVAL_BYTE = 0x5F;
        public const byte JOIN_BYTE = 0x00;
        public const byte HOST_BYTE = 0x01;

        public enum DataMessageTypes : byte
        {
            HostsListRequest = 0x01,
            HostsListResponse = 0x11,
            LocalCoordinates = 0x02,
            RemoteCoordinates = 0x12,
            HostConnectionRequest = 0x03, 
            HostConnectionResponse = 0x13,
            ClientJoined = 0x04, 
            LocalShooting = 0x05,
            RemoteShooting = 0x15
        }
    }
}

