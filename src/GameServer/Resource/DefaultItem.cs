﻿namespace NeoNetsphere.Resource
{
  public class DefaultItem
  {
    public ItemNumber ItemNumber { get; set; }

    public CharacterGender Gender { get; set; }

    //public byte Slot { get; set; }
    public byte Variation { get; set; }

    public override string ToString()
    {
      return ItemNumber.ToString();
    }
  }
}