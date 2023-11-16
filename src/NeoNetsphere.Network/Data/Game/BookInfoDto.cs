using BlubLib.Serialization.Serializers;
using BlubLib.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class BookInfoDto
    {
        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }


        [BlubMember(3, typeof(StringSerializer), 6)]
        public int[] Unk4 { get; set; }

        public BookInfoDto()
        {
            Unk4 = new int[6];
        }
    }
}
