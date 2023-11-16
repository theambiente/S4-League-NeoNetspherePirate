﻿using System;
using System.Net;
using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace ProudNetSrc.Serialization.Messages
{
  [BlubContract]
  internal class ReliablePongMessage : IMessage
  {
  }

  [BlubContract]
  internal class NotifyUdpToTcpFallbackByServerMessage : IMessage
  {
  }

  [BlubContract]
  internal class ShutdownTcpAckMessage : IMessage
  {
  }

  [BlubContract]
  internal class P2PGroup_MemberJoinMessage : IMessage
  {
    public P2PGroup_MemberJoinMessage()
    {
      UserData = Array.Empty<byte>();
      SecureKey = Array.Empty<byte>();
      ConnectionMagicNumber = Guid.Empty;
    }

    public P2PGroup_MemberJoinMessage(uint groupId, uint memberId, uint eventId, byte[] secureKey,
        bool enableDirectP2P)
        : this()
    {
      GroupId = groupId;
      MemberId = memberId;
      EventId = eventId;
      EnableDirectP2P = enableDirectP2P;
      SecureKey = secureKey;
    }

    [BlubMember(0)] public uint GroupId { get; set; }

    [BlubMember(1)] public uint MemberId { get; set; }

    [BlubMember(2, typeof(ArrayWithScalarSerializer))]
    public byte[] UserData { get; set; }

    [BlubMember(3)] public uint EventId { get; set; }

    [BlubMember(4, typeof(ArrayWithScalarSerializer))]
    public byte[] SecureKey { get; set; }

    [BlubMember(5)] public uint P2PFirstFrameNumber { get; set; }

    [BlubMember(6)] public Guid ConnectionMagicNumber { get; set; }

    [BlubMember(7)] public bool EnableDirectP2P { get; set; }

    [BlubMember(8)] public ushort BindPort { get; set; }
  }

  [BlubContract]
  internal class P2PGroup_MemberJoin_UnencryptedMessage : IMessage
  {
    public P2PGroup_MemberJoin_UnencryptedMessage()
    {
      UserData = Array.Empty<byte>();
      ConnectionMagicNumber = Guid.Empty;
    }

    public P2PGroup_MemberJoin_UnencryptedMessage(uint groupId, uint memberId, uint eventId, bool enableDirectP2P)
        : this()
    {
      GroupId = groupId;
      MemberId = memberId;
      EventId = eventId;
      EnableDirectP2P = enableDirectP2P;
    }

    [BlubMember(0)] public uint GroupId { get; set; }

    [BlubMember(1)] public uint MemberId { get; set; }

    [BlubMember(2, typeof(ArrayWithScalarSerializer))]
    public byte[] UserData { get; set; }

    [BlubMember(3)] public uint EventId { get; set; }

    [BlubMember(4)] public uint P2PFirstFrameNumber { get; set; }

    [BlubMember(5)] public Guid ConnectionMagicNumber { get; set; }

    [BlubMember(6)] public bool EnableDirectP2P { get; set; }

    [BlubMember(7)] public ushort BindPort { get; set; }
  }

  [BlubContract]
  internal class P2PRecycleCompleteMessage : IMessage
  {
    public P2PRecycleCompleteMessage()
    {
      InternalAddress = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 65535);
      ExternalAddress = InternalAddress;
      SendAddress = InternalAddress;
      RecvAddress = InternalAddress;
    }

    public P2PRecycleCompleteMessage(uint hostId)
        : this()
    {
      HostId = hostId;
    }

    [BlubMember(0)] public uint HostId { get; set; }

    [BlubMember(1)] public bool Recycled { get; set; }

    [BlubMember(2, typeof(IPEndPointSerializer))]
    public IPEndPoint InternalAddress { get; set; }

    [BlubMember(3, typeof(IPEndPointSerializer))]
    public IPEndPoint ExternalAddress { get; set; }

    [BlubMember(4, typeof(IPEndPointSerializer))]
    public IPEndPoint SendAddress { get; set; }

    [BlubMember(5, typeof(IPEndPointSerializer))]
    public IPEndPoint RecvAddress { get; set; }
  }

  [BlubContract]
  internal class RequestP2PHolepunchMessage : IMessage
  {
    public RequestP2PHolepunchMessage()
    {
      LocalEndPoint = new IPEndPoint(0, 0);
      EndPoint = new IPEndPoint(0, 0);
    }

    public RequestP2PHolepunchMessage(uint hostId, IPEndPoint localEndPoint, IPEndPoint endPoint)
    {
      HostId = hostId;
      LocalEndPoint = localEndPoint;
      EndPoint = endPoint;
    }

    [BlubMember(0)] public uint HostId { get; set; }

    [BlubMember(1, typeof(IPEndPointSerializer))]
    public IPEndPoint LocalEndPoint { get; set; }

    [BlubMember(2, typeof(IPEndPointSerializer))]
    public IPEndPoint EndPoint { get; set; }
  }

  [BlubContract]
  internal class P2P_NotifyDirectP2PDisconnected2Message : IMessage
  {
    public P2P_NotifyDirectP2PDisconnected2Message()
    {
    }

    public P2P_NotifyDirectP2PDisconnected2Message(uint remotePeerHostId, uint reason)
    {
      RemotePeerHostId = remotePeerHostId;
      Reason = reason;
    }

    [BlubMember(0)] public uint RemotePeerHostId { get; set; }

    [BlubMember(1)] public uint Reason { get; set; }
  }

  [BlubContract]
  internal class P2PGroup_MemberLeaveMessage : IMessage
  {
    public P2PGroup_MemberLeaveMessage()
    {
    }

    public P2PGroup_MemberLeaveMessage(uint memberId, uint groupId)
    {
      MemberId = memberId;
      GroupId = groupId;
    }

    [BlubMember(0)] public uint MemberId { get; set; }

    [BlubMember(1)] public uint GroupId { get; set; }
  }

  [BlubContract]
  internal class NotifyDirectP2PEstablishMessage : IMessage
  {
    public NotifyDirectP2PEstablishMessage()
    {
      ABSendAddr = new IPEndPoint(0, 0);
      ABRecvAddr = ABSendAddr;
      BASendAddr = ABSendAddr;
      BARecvAddr = ABSendAddr;
    }

    public NotifyDirectP2PEstablishMessage(uint a, uint b, IPEndPoint abSendAddr, IPEndPoint abRecvAddr,
        IPEndPoint baSendAddr, IPEndPoint baRecvAddr)
    {
      A = a;
      B = b;
      ABSendAddr = abSendAddr;
      ABRecvAddr = abRecvAddr;
      BASendAddr = baSendAddr;
      BARecvAddr = baRecvAddr;
    }

    [BlubMember(0)] public uint A { get; set; }

    [BlubMember(1)] public uint B { get; set; }

    [BlubMember(2, typeof(IPEndPointSerializer))]
    public IPEndPoint ABSendAddr { get; set; }

    [BlubMember(3, typeof(IPEndPointSerializer))]
    public IPEndPoint ABRecvAddr { get; set; }

    [BlubMember(4, typeof(IPEndPointSerializer))]
    public IPEndPoint BASendAddr { get; set; }

    [BlubMember(5, typeof(IPEndPointSerializer))]
    public IPEndPoint BARecvAddr { get; set; }
  }

  [BlubContract]
  internal class RenewP2PConnectionStateMessage : IMessage
  {
    public RenewP2PConnectionStateMessage()
    {
    }

    public RenewP2PConnectionStateMessage(uint hostId)
    {
      HostId = hostId;
    }

    [BlubMember(0)] public uint HostId { get; set; }
  }

  [BlubContract]
  internal class NewDirectP2PConnectionMessage : IMessage
  {
    public NewDirectP2PConnectionMessage()
    {
    }

    public NewDirectP2PConnectionMessage(uint hostId)
    {
      HostId = hostId;
    }

    [BlubMember(0)] public uint HostId { get; set; }
  }

  [BlubContract]
  internal class S2C_RequestCreateUdpSocketMessage : IMessage
  {
    public S2C_RequestCreateUdpSocketMessage()
    {
    }

    public S2C_RequestCreateUdpSocketMessage(IPEndPoint endPoint)
    {
      EndPoint = endPoint;
    }

    [BlubMember(0, typeof(IPEndPointAddressStringSerializer))]
    public IPEndPoint EndPoint { get; set; }
  }

  [BlubContract]
  internal class S2C_CreateUdpSocketAckMessage : IMessage
  {
    public S2C_CreateUdpSocketAckMessage()
    {
    }

    public S2C_CreateUdpSocketAckMessage(bool success, IPEndPoint endPoint)
    {
      Success = success;
      EndPoint = endPoint;
    }

    [BlubMember(0)] public bool Success { get; set; }

    [BlubMember(1, typeof(IPEndPointAddressStringSerializer))]
    public IPEndPoint EndPoint { get; set; }
  }
}
