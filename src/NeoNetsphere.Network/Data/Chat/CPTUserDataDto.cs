﻿using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Chat
{
  [BlubContract]
  public class CPTUserDataDto
  {
    [BlubMember(0)] public uint Kills { get; set; }

    [BlubMember(1)] public uint Domination { get; set; }
  }
}
