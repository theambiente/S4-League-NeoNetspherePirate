﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlubLib.Threading.Tasks;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Message.Game;
using NeoNetsphere.Network.Services;
using NeoNetsphere.Resource;

namespace NeoNetsphere.Commands
{
  internal class ReloadCommand : ICommand
  {
    public ReloadCommand()
    {
      Name = "reload";
      AllowConsole = true;
      Permission = SecurityLevel.Developer;
      SubCommands = new ICommand[]
      {
                new ShopCommand(), new ReqBoxCommand(), new RoomMassKickCommand(), new ServerMassKickCommand(),
                new ClubCommand(), new ConfigCommand(), new Enchantcommand(), new CapsuleCommand(), new Levelrewardcommand()
      };
    }

    public string Name { get; }
    public bool AllowConsole { get; }
    public SecurityLevel Permission { get; }
    public IReadOnlyList<ICommand> SubCommands { get; }

    public async Task<bool> Execute(GameServer server, Player plr, string[] args)
    {
      return true;
    }

    public string Help()
    {
      var sb = new StringBuilder();
      sb.AppendLine(Name);
      foreach (var cmd in SubCommands)
      {
        sb.Append("");
        sb.AppendLine(cmd.Help());
      }

      return sb.ToString();
    }

    private class ClubCommand : ICommand
    {
      public ClubCommand()
      {
        Name = "clubs";
        AllowConsole = true;
        Permission = SecurityLevel.Developer;
        SubCommands = new ICommand[] { };
      }

      public string Name { get; }
      public bool AllowConsole { get; }
      public SecurityLevel Permission { get; }
      public IReadOnlyList<ICommand> SubCommands { get; }

      public async Task<bool> Execute(GameServer server, Player plr, string[] args)
      {
        await Task.Run(async () =>
        {
          var message = "Reloading clubs, server may lag for a short period of time...";

          if (plr == null)
            CommandManager.Logger.Information(message);
          else
            plr.SendConsoleMessage(S4Color.Green + message);

          server.ResourceCache.Clear(ResourceCacheType.Clubs);
          server.ClubManager = new ClubManager(server.ResourceCache.GetClubs());
          await ClubService.Update(null, true);
          message = "Club reload completed";
          if (plr == null)
            CommandManager.Logger.Information(message);
          else
            plr.SendConsoleMessage(S4Color.Green + message);
        });
        return true;
      }

      public string Help()
      {
        return Name;
      }
    }




        private class CapsuleCommand : ICommand
        {
            public CapsuleCommand()
            {
                Name = "capsules";
                AllowConsole = true;
                Permission = SecurityLevel.Developer;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public async Task<bool> Execute(GameServer server, Player plr, string[] args)
            {
                await Task.Run(async () =>
                {
                    var message = "Reloading Capsules, server may lag for a short period of time...";

                    if (plr == null)
                        CommandManager.Logger.Information(message);
                    else
                        plr.SendConsoleMessage(S4Color.Green + message);

                    server.BroadcastNotice(message);
                    server.ResourceCache.Clear(ResourceCacheType.Capsules);
                    server.ResourceCache.Clear(ResourceCacheType.ItemRewards);
                    server.ResourceCache.GetCapsules();
                    server.ResourceCache.GetItemRewards();

                    message = "Capsulereload completed";
                    server.BroadcastNotice(message);
                    if (plr == null)
                        CommandManager.Logger.Information(message);
                    else
                        plr.SendConsoleMessage(S4Color.Green + message);
                });
                return true;
            }

            public string Help()
            {
                return Name;
            }
        }


        private class Levelrewardcommand : ICommand
        {
            public Levelrewardcommand()
            {
                Name = "levelreward";
                AllowConsole = true;
                Permission = SecurityLevel.Developer;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public async Task<bool> Execute(GameServer server, Player plr, string[] args)
            {
                await Task.Run(async () =>
                {
                    var message = "Reloading Levelrewards..";

                    if (plr == null)
                        CommandManager.Logger.Information(message);
                    else
                        plr.SendConsoleMessage(S4Color.Green + message);

                    server.BroadcastNotice(message);
                    server.ResourceCache.Clear(ResourceCacheType.LevelRewards);
                    server.ResourceCache.Clear(ResourceCacheType.ItemRewards);
                    server.ResourceCache.GetLevelRewards();
                    server.ResourceCache.GetItemRewards();

                    message = "Levelreward reload completed..";
                    server.BroadcastNotice(message);
                    if (plr == null)
                        CommandManager.Logger.Information(message);
                    else
                        plr.SendConsoleMessage(S4Color.Purple + message);
                });
                return true;
            }

            public string Help()
            {
                return Name;
            }
        }

        private class Enchantcommand : ICommand
        {
            public Enchantcommand()
            {
                Name = "enchant";
                AllowConsole = true;
                Permission = SecurityLevel.Developer;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public async Task<bool> Execute(GameServer server, Player plr, string[] args)
            {
                await Task.Run(async () =>
                {
                    var message = "Reloading EnchantEffects, server may lag for a short period of time...";

                    if (plr == null)
                        CommandManager.Logger.Information(message);
                    else
                        plr.SendConsoleMessage(S4Color.Green + message);

                    server.BroadcastNotice(message);
                    server.ResourceCache.Clear(ResourceCacheType.EnchantSystem);
                    server.ResourceCache.Clear(ResourceCacheType.Effects);
                    server.ResourceCache.GetEffects();
                    server.ResourceCache.GetEnchantSystem();

                    message = "Enchantreload completed";
                    server.BroadcastNotice(message);
                    if (plr == null)
                        CommandManager.Logger.Information(message);
                    else
                        plr.SendConsoleMessage(S4Color.Green + message);
                });
                return true;
            }

            public string Help()
            {
                return Name;
            }
        }

        private class ShopCommand : ICommand
    {
      public ShopCommand()
      {
        Name = "shop";
        AllowConsole = true;
        Permission = SecurityLevel.Developer;
        SubCommands = new ICommand[] { };
      }

      public string Name { get; }
      public bool AllowConsole { get; }
      public SecurityLevel Permission { get; }
      public IReadOnlyList<ICommand> SubCommands { get; }

      public async Task<bool> Execute(GameServer server, Player plr, string[] args)
      {
        await Task.Run(async () =>
        {
          var message = "Reloading shop, server may lag for a short period of time...";

          if (plr == null)
            CommandManager.Logger.Information(message);
          else
            plr.SendConsoleMessage(S4Color.Green + message);

          server.BroadcastNotice(message);
          server.ResourceCache.Clear(ResourceCacheType.Shop);
          await ShopService.ShopUpdateMsg(null, true);

          message = "Shop reload completed";
          server.BroadcastNotice(message);
          if (plr == null)
            CommandManager.Logger.Information(message);
          else
            plr.SendConsoleMessage(S4Color.Green + message);
        });
        return true;
      }

      public string Help()
      {
        return Name;
      }
    }

    private class ConfigCommand : ICommand
    {
      public ConfigCommand()
      {
        Name = "config";
        AllowConsole = true;
        Permission = SecurityLevel.Developer;
        SubCommands = new ICommand[] { };
      }

      public string Name { get; }
      public bool AllowConsole { get; }
      public SecurityLevel Permission { get; }
      public IReadOnlyList<ICommand> SubCommands { get; }

      public async Task<bool> Execute(GameServer server, Player plr, string[] args)
      {
        Config.Load();
        var message = "config reload completed";
        if (plr == null)
          CommandManager.Logger.Information(message);
        else
          plr.SendConsoleMessage(S4Color.Green + message);
        return true;
      }

      public string Help()
      {
        return Name;
      }
    }

    private class ReqBoxCommand : ICommand
    {
      public ReqBoxCommand()
      {
        Name = "reqs";
        AllowConsole = true;
        Permission = SecurityLevel.Developer;
        SubCommands = new ICommand[] { };
      }

      public string Name { get; }
      public bool AllowConsole { get; }
      public SecurityLevel Permission { get; }
      public IReadOnlyList<ICommand> SubCommands { get; }

      public async Task<bool> Execute(GameServer server, Player plr, string[] args)
      {
        var message = "Trying to fix stuck request boxes..";

        if (plr == null)
          CommandManager.Logger.Information(message);
        else
          plr.SendConsoleMessage(S4Color.Green + message);

        GameServer.Instance.Broadcast(new ServerResultAckMessage(ServerResult.ServerError));

        message = "Done trying to fix stuck request boxes.";
                server.BroadcastNotice(message);
                if (plr == null)
          CommandManager.Logger.Information(message);
        else
          plr.SendConsoleMessage(S4Color.Green + message);

        return true;
      }

      public string Help()
      {
        return Name;
      }
    }

    private class ServerMassKickCommand : ICommand
    {
      public ServerMassKickCommand()
      {
        Name = "playerlist";
        AllowConsole = true;
        Permission = SecurityLevel.Developer;
        SubCommands = new ICommand[] { };
      }

      public string Name { get; }
      public bool AllowConsole { get; }
      public SecurityLevel Permission { get; }
      public IReadOnlyList<ICommand> SubCommands { get; }

      public async Task<bool> Execute(GameServer server, Player plr, string[] args)
      {
        var message = "Kicking all players..";

        if (plr == null)
          CommandManager.Logger.Information(message);
        else
          plr.SendConsoleMessage(S4Color.Green + message);

        foreach (var session in GameServer.Instance.Sessions.Values.Cast<GameSession>())
          session.Player?.Room?.Leave(session.Player);

        await Task.Delay(1000);
        GameServer.Instance.Broadcast(new ItemUseChangeNickAckMessage { Result = 0 });
        GameServer.Instance.Broadcast(new ServerResultAckMessage(ServerResult.CreateNicknameSuccess));
        await Task.Delay(1000);

        foreach (var session in GameServer.Instance.Sessions.Values)
          session.CloseAsync();

        message = "Done with kickall";
        if (plr == null)
          CommandManager.Logger.Information(message);
        else
          plr.SendConsoleMessage(S4Color.Green + message);

        return true;
      }

      public string Help()
      {
        return Name;
      }
    }

    private class RoomMassKickCommand : ICommand
    {
      public RoomMassKickCommand()
      {
        Name = "rooms";
        AllowConsole = true;
        Permission = SecurityLevel.Developer;
        SubCommands = new ICommand[] { };
      }

      public string Name { get; }
      public bool AllowConsole { get; }
      public SecurityLevel Permission { get; }
      public IReadOnlyList<ICommand> SubCommands { get; }

      public async Task<bool> Execute(GameServer server, Player plr, string[] args)
      {
        var message = "Kicking all players..";

        if (plr == null)
          CommandManager.Logger.Information(message);
        else
          plr.SendConsoleMessage(S4Color.Green + message);

        foreach (var sess in GameServer.Instance.Sessions.Values)
        {
          var session = (GameSession)sess;
          session.Player?.Room?.Leave(session.Player);
        }

        message = "Done kicking all players from all rooms.";
        if (plr == null)
          CommandManager.Logger.Information(message);
        else
          plr.SendConsoleMessage(S4Color.Green + message);

        return true;
      }

      public string Help()
      {
        return Name;
      }
    }
  }
}