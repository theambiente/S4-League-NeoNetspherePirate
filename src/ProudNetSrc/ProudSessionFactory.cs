﻿using DotNetty.Transport.Channels;

namespace ProudNetSrc
{
  public class ProudSessionFactory : ISessionFactory
  {
    public ProudSession Create(uint hostId, IChannel channel, ProudServer server)
    {
      return new ProudSession(hostId, channel, server);
    }
  }
}
