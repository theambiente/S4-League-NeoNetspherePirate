﻿using BlubLib.Serialization;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Chat
{
  [BlubContract]
  public class NoteDto
  {
    public NoteDto()
    {
      Sender = "";
      Title = "";
      OpenedGift = true;
    }

    [BlubMember(0)] public ulong Id { get; set; }

    [BlubMember(1, typeof(StringSerializer))]
    public string Sender { get; set; }

    [BlubMember(2, typeof(IntBooleanSerializer))]
    public bool IsGift { get; set; }

    [BlubMember(3)] public ulong Unk1 { get; set; }

    [BlubMember(4)] public ulong Receiver { get; set; }

    [BlubMember(5, typeof(StringSerializer))]
    public string Title { get; set; }

    [BlubMember(6)] public uint ReadCount { get; set; }

    [BlubMember(7)] public bool OpenedGift { get; set; }

    [BlubMember(8)] public byte DaysLeft { get; set; }
  }
}
