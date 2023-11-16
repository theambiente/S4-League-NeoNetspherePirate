﻿namespace ProudNetSrc.Handlers
{
  using System.Collections.Generic;
  using DotNetty.Buffers;
  using DotNetty.Codecs;
  using DotNetty.Transport.Channels;

  internal class RecvContextDecoder : MessageToMessageDecoder<IByteBuffer>
  {
    protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
    {
      output.Add(new RecvContext
      {
        Message = message.Retain(),
        UdpEndPoint = null
      });
    }
  }
}
