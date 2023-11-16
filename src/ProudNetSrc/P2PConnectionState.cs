﻿using System;
using System.Net;

namespace ProudNetSrc
{
  internal class P2PConnectionState
  {
    public P2PConnectionState(RemotePeer remotePeer)
    {
      RemotePeer = remotePeer;
      EventId = (uint)Guid.NewGuid().GetHashCode();
    }

    public RemotePeer RemotePeer { get; }
    public uint EventId { get; }
    public bool IsInitialized { get; set; }
    public bool IsJoined { get; set; }
    public bool JitTriggered { get; set; }
    public bool PeerUdpHolepunchSuccess { get; set; }
    public bool HolepunchSuccess { get; set; }
    public DateTimeOffset LastHolepunch { get; set; }

    public IPEndPoint EndPoint { get; set; }
    public IPEndPoint LocalEndPoint { get; set; }
  }
}
