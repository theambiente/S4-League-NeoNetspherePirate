using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.GameRule
{
  [BlubContract]
  public class NameTagDto
  {
    public NameTagDto()
    {
      AccountId = 0;
      NameTag = 5;
    }

    public NameTagDto(ulong accountId, uint nameTag)
    {
      AccountId = accountId;
      NameTag = nameTag;
    }

    [BlubMember(0)] public ulong AccountId { get; set; }

    [BlubMember(1)] public uint NameTag { get; set; }
  }
}
