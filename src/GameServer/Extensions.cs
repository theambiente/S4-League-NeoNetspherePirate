﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlubLib.IO;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Network;
using NeoNetsphere.Shop;
using ProudNetSrc;
using Serilog;

namespace NeoNetsphere
{
  internal static class EnumerableExtentions
  {
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
      var rng = new SecureRandom();
      var elements = source.ToArray();
      for (var i = elements.Length - 1; i >= 0; i--)
      {
        var swapIndex = rng.Next(i + 1);
        yield return elements[swapIndex];
        elements[swapIndex] = elements[i];
      }
    }

    public static T AnyOrDefault<T>(this IList<T> e, Func<T, double> weightSelector)
    {
      if (e.Count < 1)
        return default(T);
      if (e.Count == 1)
        return e[0];
      var weights = e.Select(o => Math.Max(weightSelector(o), 0)).ToArray();
      var sum = weights.Sum(d => d);

      var rnd = new SecureRandom().NextDouble();
      for (int i = 0; i < weights.Length; i++)
      {
        var w = sum == 0
            ? 1 / (double)e.Count
            : weights[i] / sum;
        if (rnd < w)
          return e[i];
        rnd -= w;
      }

      throw new Exception("Should not happen");
    }
  }

  internal static class Extensions
  {
    public static ILogger ForAccount(this ILogger logger, ulong id, string user,
        SecurityLevel securityLevel = SecurityLevel.User)
    {
      if (user == string.Empty)
        user = "n/A";

      if (user.Length > 8)
      {
        user = user.Substring(0, 8) + "..";
      }

      var accid = $"({id})";
      var accuser = $"{user}{accid}";
      var max = 11;

      if (accid.Length < max)
      {
        if (accuser.Length > max)
        {
          var l = accuser.Length - accid.Length - (accuser.Length - max);
          if (l < 0)
            user = "";
          else
            user = user.Substring(0, l);
          accuser = $"{user}{accid}";
        }
      }
      else
      {
        var l = accid.Length - (accid.Length - max);
        accid = accid.Substring(0, l);
        accuser = accid;
      }

      return logger
          .ForContext("Accid", id)
          .ForContext("Accuser", accuser)
          .ForContext("Acclevel", securityLevel);
    }

    public static ILogger ForAccount(this ILogger logger, AccountDto account)
    {
      var name = account.Nickname;
      if (string.IsNullOrEmpty(account.Nickname))
        name = account.Username;

      return logger.ForAccount((ulong)account.Id, name, (SecurityLevel)account.SecurityLevel);
    }

    public static ILogger ForAccount(this ILogger logger, Account account)
    {
      var name = account.Nickname;
      if (string.IsNullOrEmpty(account.Nickname))
        name = account.Username;

      return logger.ForAccount(account.Id, name, account.SecurityLevel);
    }

    public static ILogger ForAccount(this ILogger logger, Player player)
    {
      return logger.ForAccount(player.Account);
    }

    public static ILogger ForAccount(this ILogger logger, GameSession session)
    {
      return session.IsLoggedIn() ? logger.ForAccount(session.Player) : logger;
    }

    public static ILogger ForAccount(this ILogger logger, ChatSession session)
    {
      return session.IsLoggedIn() ? logger.ForAccount(session.GameSession.Player) : logger;
    }

    public static ILogger ForAccount(this ILogger logger, RelaySession session)
    {
      return session.IsLoggedIn() ? logger.ForAccount(session.GameSession.Player) : logger;
    }

    public static bool IsLoggedIn(this GameSession session)
    {
      return !string.IsNullOrWhiteSpace(session?.Player?.Account?.Nickname) && session.IsConnected;
    }

    public static bool IsLoggedIn(this ChatSession session)
    {
      return !string.IsNullOrWhiteSpace(session?.GameSession?.Player?.Account?.Nickname) && session.IsConnected;
    }

    public static bool IsLoggedIn(this RelaySession session)
    {
      return !string.IsNullOrWhiteSpace(session?.GameSession?.Player?.Account?.Nickname) && session.IsConnected;
    }

    public static bool IsLoggedIn(this Player plr)
    {
      return (plr?.Session?.IsLoggedIn() ?? false) && (plr?.ChatSession?.IsLoggedIn() ?? false);
    }

    public static void Serialize(this BinaryWriter w, ICollection<ShopPriceGroup> value)
    {
      w.Write(value.Count);
      foreach (var group in value)
      {
        w.WriteProudString($"{group.Name}_{group.Id}");
        w.WriteEnum(group.PriceType);

        w.Write(group.Prices.Count);
        foreach (var price in group.Prices)
        {
          w.WriteEnum(price.PeriodType);
          w.Write(price.Period);
          w.Write(price.Price);
          w.Write(price.CanRefund);
          w.Write(price.Durability);
          w.Write(price.IsEnabled);
        }
      }
    }

    public static void Serialize(this BinaryWriter w, ICollection<ShopEffectGroup> value)
    {
      w.Write(value.Count);
      foreach (var group in value)
      {
        w.Write(group.MainEffect);
        w.Write(group.Effects.Count);
        foreach (var effect in group.Effects)
          w.Write(effect.Effect);
      }
    }

    public static void Serialize(this BinaryWriter w, ICollection<ShopItem> value)
    {
      w.Write(value.Count);
      foreach (var item in value)
      {
        w.Write(item.ItemNumber);

        switch (item.Gender)
        {
          case Gender.Female:
            w.Write((uint)1);
            break;

          case Gender.Male:
            w.Write((uint)0);
            break;

          case Gender.None:
            w.Write((uint)2);
            break;
        }

        w.Write((ushort)item.License);
        w.Write((ushort)item.ColorGroup);
        w.Write((ushort)item.UniqueColorGroup); //unknown
        w.Write((ushort)item.MinLevel);
        w.Write((ushort)item.MaxLevel);
        w.Write((ushort)item.MasterLevel);
        w.Write(0); // RepairCost
        w.Write(item.IsOneTimeUse);
        w.Write(!item.IsDestroyable);
        w.Write((ushort)item.MainTab);
        w.Write((ushort)item.SubTab);
        w.Write((ushort)1); // shop_order

        w.Write(item.ItemInfos.Count);
        foreach (var info in item.ItemInfos)
        {
          w.WriteProudString(((ShopInfoTypeEnum)info.ShopInfoType).ToString().Replace("@", ""));
          w.WriteEnum(info.PriceGroup.PriceType);
          w.Write((ushort)info.Discount);
          w.WriteProudString($"{info.PriceGroup.Name}_{info.PriceGroup.Id}");
          w.Write(info.EffectGroup.MainEffect);
        }
      }
    }

    // ToDo
    //public static void Serialize(this BinaryWriter w, ICollection<ShopUniqueItemDto> value)
    //{
    //    w.Write(value.Count);
    //    foreach (var item in value)
    //    {
    //        w.Write((uint)item.item_number);
    //        w.Write((uint)item.price_type);
    //        w.Write((ushort)item.discount);
    //        w.Write((uint)item.period_type);
    //        w.Write((ushort)item.period);
    //        w.Write((byte)item.color);
    //        w.WriteProudString(item.is_enabled > 0 ? "on": "off");
    //        w.Write(item.can_refund);
    //        w.Write((int)item.reward);
    //        w.WriteProudString(""); // ToDo StartDate
    //        w.WriteProudString(""); // ToDo EndDate
    //    }
    //}
  }
}