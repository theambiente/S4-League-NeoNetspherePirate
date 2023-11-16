﻿namespace ProudNetSrc
{
  using System;
  using System.Net;
  using System.Threading.Tasks;
  using BlubLib;
  using BlubLib.Threading.Tasks;
  using DotNetty.Transport.Bootstrapping;
  using DotNetty.Transport.Channels;
  using DotNetty.Transport.Channels.Sockets;
  using ProudNetSrc.Codecs;
  using ProudNetSrc.Handlers;
  using ProudNetSrc.Serialization.Messages.Core;

  internal class UdpSocket : IDisposable
  {
    private readonly ProudServer _owner;
    private bool _disposed;
    private IEventLoopGroup _eventLoopGroup;

    public UdpSocket(ProudServer owner)
    {
      _owner = owner;
    }

    public IChannel Channel { get; private set; }

    public void Dispose()
    {
      if (_disposed)
        return;

      _disposed = true;
      _eventLoopGroup?.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10)).WaitEx();
    }

    public void Listen(IPEndPoint endPoint, IEventLoopGroup eventLoopGroup)
    {
      ThrowIfDisposed();
      _eventLoopGroup = eventLoopGroup ?? throw new ArgumentNullException(nameof(eventLoopGroup));

      try
      {
        Channel = new Bootstrap()
            .Group(_eventLoopGroup ?? eventLoopGroup)
            .Channel<SocketDatagramChannel>()
            .Option(ChannelOption.SoSndbuf, 8192)
            .Handler(new ActionChannelInitializer<IChannel>(ch =>
            {
              ch.Pipeline
                          .AddLast(new UdpFrameDecoder((int)_owner.Configuration.MessageMaxLength))
                          .AddLast(new UdpFrameEncoder())
                          .AddLast(new UdpHandler(this, _owner))
                          .AddLast(new ErrorHandler(_owner));
            }))
            .BindAsync(endPoint).WaitEx();
      }
      catch (Exception ex)
      {
        _eventLoopGroup?.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10)).WaitEx();
        _eventLoopGroup = null;
        Channel = null;
        ex.Rethrow();
      }
    }

    public Task SendAsync(ICoreMessage message, IPEndPoint endPoint)
    {
      return Channel.WriteAndFlushAsync(new SendContext { Message = message, UdpEndPoint = endPoint });
    }

    private void ThrowIfDisposed()
    {
      if (_disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }
  }
}
