﻿using System;
using BlubLib.Serialization;
using NeoNetsphere.Network.Serializers;

namespace NeoNetsphere.Network.Data.GameRule
{
  [BlubContract]
  public class ChangeItemsUnkDto
  {
    public ChangeItemsUnkDto()
    {
      Skills = Array.Empty<ItemNumber>();
      Weapons = Array.Empty<ItemNumber>();
      Unk4 = Array.Empty<int>();
      Unk5 = Array.Empty<int>();
    }

    [BlubMember(0)]
    public ulong AccountId { get; set; }

    [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
    public ItemNumber[] Skills { get; set; }

    [BlubMember(2, typeof(ArrayWithIntPrefixSerializer))]
    public ItemNumber[] Weapons { get; set; }

    [BlubMember(3, typeof(ArrayWithIntPrefixSerializer))]
    public int[] Unk4 { get; set; }

    [BlubMember(4, typeof(ArrayWithIntPrefixSerializer))]
    public int[] Unk5 { get; set; }

    [BlubMember(5)]
    public int Unk6 { get; set; }

    [BlubMember(6)]
    public float HP { get; set; }
  }
}
