﻿namespace ProudNetSrc.Codecs
{
  using System.Collections.Generic;
  using BlubLib.IO;
  using DotNetty.Buffers;
  using DotNetty.Codecs;
  using DotNetty.Transport.Channels;
  using ProudNetSrc.Serialization.Messages.Core;
  using ReadOnlyByteBufferStream = BlubLib.DotNetty.ReadOnlyByteBufferStream;

  internal class CoreMessageDecoder : MessageToMessageDecoder<RecvContext>
  {
    protected override void Decode(IChannelHandlerContext context, RecvContext message, List<object> output)
    {
      var buffer = message.Message as IByteBuffer;
      try
      {
        if (buffer == null)
          throw new ProudException($"{nameof(CoreMessageDecoder)} can only handle {nameof(IByteBuffer)}");

        message.Message = Decode(buffer);
        output.Add(message);
      }
      finally
      {
        buffer?.Release();
      }
    }

    public static ICoreMessage Decode(IByteBuffer buffer)
    {
      using (var r = new ReadOnlyByteBufferStream(buffer, false).ToBinaryReader(false))
      {
        if (r.IsEOF())
        {
          throw new ProudException("Empty packet");
        }

        var opCode = r.ReadEnum<ProudCoreOpCode>();
        return CoreMessageFactory.Default.GetMessage(opCode, r);
      }
    }
  }
}
