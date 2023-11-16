using BlubLib.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class AlchemyCombinationDto
    {
        [BlubMember(0)]
        public ItemNumber ItemNumber { get; set; }

        [BlubMember(1)]
        public int UseValue { get; set; }
    }
}
