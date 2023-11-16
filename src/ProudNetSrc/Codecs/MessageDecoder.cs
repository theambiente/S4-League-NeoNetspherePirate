﻿namespace ProudNetSrc.Codecs
{
  using System.Collections.Generic;
  using System.Linq;
  using BlubLib.IO;
  using DotNetty.Buffers;
  using DotNetty.Codecs;
  using DotNetty.Transport.Channels;
  using ProudNetSrc.Serialization;
  using ProudNetSrc.Serialization.Messages;
  using ReadOnlyByteBufferStream = BlubLib.DotNetty.ReadOnlyByteBufferStream;

  internal class MessageDecoder : MessageToMessageDecoder<RecvContext>
  {
    private readonly MessageFactory[] _userMessageFactories;

    public MessageDecoder(MessageFactory[] userMessageFactories)
    {
      _userMessageFactories = userMessageFactories;
    }

    protected override void Decode(IChannelHandlerContext context, RecvContext message, List<object> output)
    {
      var buffer = message.Message as IByteBuffer;
      try
      {
        // Drop core messages
        if (buffer == null)
          return;

        using (var r = new ReadOnlyByteBufferStream(buffer, false).ToBinaryReader(false))
        {
          var opCode = r.ReadUInt16();
          var isInternal = opCode >= 64000;
          var factory = isInternal
              ? RmiMessageFactory.Default
              : _userMessageFactories.FirstOrDefault(userFactory => userFactory.ContainsOpCode(opCode));

          if (factory == null)
          {
#if DEBUG
            throw new ProudBadOpCodeException(opCode, buffer.GetIoBuffer());
#else
                        throw new ProudException($"No {nameof(MessageFactory)} found for rmi {opCode}");
#endif
          }

          message.Message = factory.GetMessage(opCode, r);
          output.Add(message);
        }
      }
      finally
      {
        buffer?.Release();
      }
    }
  }
}
