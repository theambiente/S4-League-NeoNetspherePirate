﻿namespace NeoNetsphere
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Data;
  using Dapper.FastCrud;
  using Database.Game;

  internal class PlayerSettingManager
  {
    private static readonly IDictionary<string, IPlayerSettingConverter> s_converter =
        new ConcurrentDictionary<string, IPlayerSettingConverter>();

    private readonly IDictionary<string, Setting> _settings = new ConcurrentDictionary<string, Setting>();

    static PlayerSettingManager()
    {
      var communtiySettingConverter = new CommunitySettingConverter();
      RegisterConverter(PlayerSetting.AllowCombiInvite, communtiySettingConverter);
      RegisterConverter(PlayerSetting.AllowFriendRequest, communtiySettingConverter);
      RegisterConverter(PlayerSetting.AllowRoomInvite, communtiySettingConverter);
      RegisterConverter(PlayerSetting.AllowInfoRequest, communtiySettingConverter);
    }

    public PlayerSettingManager(Player player, PlayerDto dto)
    {
      Player = player;

      foreach (var settingDto in dto.Settings)
      {
        _settings[settingDto.Setting] =
            new Setting(GetObject(settingDto.Setting, settingDto.Value)) { ExistsInDatabase = true };
      }

      if (_settings.Count <= 0)
      {
        AddOrUpdate("AllowCombiInvite", 0);
        AddOrUpdate("AllowFriendRequest", 0);
        AddOrUpdate("AllowRoomInvite", 0);
        AddOrUpdate("AllowInfoRequest", 0);
      }
    }

    public Player Player { get; }

    public bool Contains(string name)
    {
      return _settings.ContainsKey(name);
    }

    public CommunitySetting GetSetting(string name)
    {
      if (!_settings.TryGetValue(name, out var setting))
        throw new Exception($"Setting {name} not found");
      Enum.TryParse(setting.Data.ToString(), out CommunitySetting outsetting);
      return outsetting;
    }

    public string Get(string name)
    {
      if (!_settings.TryGetValue(name, out var setting))
        throw new Exception($"Setting {name} not found");

      return (string)setting.Data;
    }

    public void AddOrUpdate(string name, string value)
    {
      if (_settings.TryGetValue(name, out var setting))
        setting.Data = value;
      else
        _settings[name] = new Setting(value);
    }

    public void AddOrUpdate<T>(string name, T value)
    {
      if (_settings.TryGetValue(name, out var setting))
        setting.Data = value;
      else
        _settings[name] = new Setting(value);
    }

    internal void Save(IDbConnection db)
    {
      foreach (var pair in _settings)
      {
        var name = pair.Key;
        var setting = pair.Value;
        if (!setting.ExistsInDatabase)
        {
          DbUtil.Insert(db, new PlayerSettingDto
          {
            PlayerId = (int)Player.Account.Id,
            Setting = name,
            Value = GetString(name, setting.Data)
          });
          setting.ExistsInDatabase = true;
        }
        else
        {
          if (!setting.NeedsToSave)
            continue;

          var dto = new PlayerSettingDto
          {
            PlayerId = (int)Player.Account.Id,
            Setting = name,
            Value = GetString(name, setting.Data)
          };

          DbUtil.BulkUpdate(db,
              dto,
              statement => statement
                  .Where(
                      $"{nameof(PlayerSettingDto.PlayerId):C} = @PlayerId AND {nameof(PlayerSettingDto.Setting):C} = @Setting")
                  .WithParameters(new { PlayerId = (int)Player.Account.Id, Setting = name }));
          setting.NeedsToSave = false;
        }
      }
    }

    private class Setting
    {
      private object _data;

      public Setting(object data)
      {
        _data = data;
      }

      public bool ExistsInDatabase { get; set; }
      public bool NeedsToSave { get; set; }

      public object Data
      {
        get => _data;
        set
        {
          if (_data == value)
            return;
          _data = value;
          NeedsToSave = true;
        }
      }
    }

    #region Converter

    public static void RegisterConverter(string name, IPlayerSettingConverter converter)
    {
      if (!s_converter.TryAdd(name, converter))
        throw new Exception($"Converter for {name} already registered");
    }

    public static void RegisterConverter(PlayerSetting name, IPlayerSettingConverter converter)
    {
      RegisterConverter(name.ToString(), converter);
    }

    private static IPlayerSettingConverter GetConverter(string name)
    {
      IPlayerSettingConverter converter;
      s_converter.TryGetValue(name, out converter);
      return converter;
    }

    private static object GetObject(string name, string value)
    {
      var converter = GetConverter(name);
      return converter != null ? converter.GetObject(value) : value;
    }

    private static string GetString(string name, object value)
    {
      var converter = GetConverter(name);
      return converter != null ? converter.GetString(value) : (string)value;
    }

    #endregion
  }

  internal interface IPlayerSettingConverter
  {
    object GetObject(string value);
    string GetString(object value);
  }

  internal class CommunitySettingConverter : IPlayerSettingConverter
  {
    public object GetObject(string value)
    {
      if (!Enum.TryParse(value, out CommunitySetting setting))
        throw new Exception($"CommunitySetting {value} not found");
      return setting;
    }

    public string GetString(object value)
    {
      return value.ToString();
    }
  }
}
