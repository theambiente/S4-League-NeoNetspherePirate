﻿using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.GameRule
{
  [BlubContract]
  public class MixedTeamBriefingDto
  {
    [BlubMember(0)] public ulong AccountId { get; set; }

    [BlubMember(1)] public byte Unk { get; set; }
  }
}
