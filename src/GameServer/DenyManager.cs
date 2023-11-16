﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BlubLib.Collections.Concurrent;
using Dapper.FastCrud;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;

namespace NeoNetsphere
{
  internal class DenyManager : IReadOnlyCollection<Deny>
  {
    private readonly ConcurrentDictionary<ulong, Deny> _denies = new ConcurrentDictionary<ulong, Deny>();
    private readonly ConcurrentStack<Deny> _deniesToRemove = new ConcurrentStack<Deny>();

    public DenyManager(Player plr, PlayerDto dto)
    {
      Player = plr;

      foreach (var denyDto in dto.Ignores)
      {
        var deny = new Deny(denyDto);
        _denies.TryAdd(deny.DenyId, deny);
      }
    }

    public Player Player { get; }
    public Deny this[ulong accountId] => CollectionExtensions.GetValueOrDefault(_denies, accountId);
    public int Count => _denies.Count;

    public IEnumerator<Deny> GetEnumerator()
    {
      return _denies.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public Deny Add(Player plr)
    {
      var deny = new Deny(plr.Account.Id, plr.Account.Nickname);
      if (!_denies.TryAdd(deny.DenyId, deny))
        throw new ArgumentException("Player is already ignored", nameof(plr));
      return deny;
    }

    public bool Remove(Deny deny)
    {
      return Remove(deny.DenyId);
    }

    public bool Remove(ulong accountId)
    {
      var deny = this[accountId];
      if (deny == null)
        return false;

      _denies.Remove(accountId);
      if (deny.ExistsInDatabase)
        _deniesToRemove.Push(deny);
      return true;
    }

    internal void Save(IDbConnection db)
    {
      if (!_deniesToRemove.IsEmpty)
      {
        var idsToRemove = new StringBuilder();
        var firstRun = true;
        Deny denyToRemove;
        while (_deniesToRemove.TryPop(out denyToRemove))
        {
          if (firstRun)
            firstRun = false;
          else
            idsToRemove.Append(',');
          idsToRemove.Append(denyToRemove.Id);
        }

        DbUtil.BulkDelete<PlayerDenyDto>(db, statement => statement
            .Where($"{nameof(PlayerDenyDto.Id):C} IN ({idsToRemove})"));
      }

      foreach (var deny in _denies.Values.Where(deny => !deny.ExistsInDatabase))
      {
        DbUtil.Insert(db, new PlayerDenyDto
        {
          Id = deny.Id,
          PlayerId = (int)Player.Account.Id,
          DenyPlayerId = (int)deny.DenyId
        });
        deny.ExistsInDatabase = true;
      }
    }

    public bool Contains(ulong accountId)
    {
      return _denies.ContainsKey(accountId);
    }
  }

  internal class Deny
  {
    internal Deny(PlayerDenyDto dto)
    {
      ExistsInDatabase = true;
      Id = dto.Id;
      DenyId = (ulong)dto.DenyPlayerId;

      // Try a fast lookup first in case the player is currently online
      // otherwise get the name from the database
      Nickname = GameServer.Instance.PlayerManager[DenyId]?.Account.Nickname;
      if (Nickname == null)
        using (var db = AuthDatabase.Open())
        {
          Nickname = DbUtil.Get(db, new AccountDto { Id = (int)DenyId })?.Nickname ?? "<Player not found>";
        }
    }

    internal Deny(ulong accountId, string nickname)
    {
      Id = DenyIdGenerator.GetNextId();
      DenyId = accountId;
      Nickname = nickname;
    }

    internal bool ExistsInDatabase { get; set; }

    public int Id { get; }
    public ulong DenyId { get; }
    public string Nickname { get; internal set; }
  }
}