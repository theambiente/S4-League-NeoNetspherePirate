namespace NeoNetsphere
{
  using System;
  using System.Collections;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Data;
  using System.Linq;
  using System.Text;
  using BlubLib.Threading.Tasks;
  using Dapper.FastCrud;
  using ExpressMapper.Extensions;
  using NeoNetsphere.Database.Game;
  using NeoNetsphere.Network;
  using NeoNetsphere.Network.Data.Game;
  using NeoNetsphere.Network.Message.Game;
  using NeoNetsphere.Resource;
  using Serilog;
  using Serilog.Core;

  internal class CharacterManager : IReadOnlyCollection<Character>
  {
    // ReSharper disable once InconsistentNaming
    private static readonly ILogger Logger =
        Log.ForContext(Constants.SourceContextPropertyName, nameof(CharacterManager));

    private readonly ConcurrentDictionary<byte, Character>
        _characters = new ConcurrentDictionary<byte, Character>();

    private readonly ConcurrentStack<Character> _charactersToDelete = new ConcurrentStack<Character>();
    private readonly AsyncLock _sync = new AsyncLock();

    internal CharacterManager(Player plr, PlayerDto dto)
    {
      Player = plr;
      CurrentSlot = dto.CurrentCharacterSlot;

      foreach (var @char in dto.Characters.Select(@char => new Character(this, @char)))
      {
        if (!_characters.TryAdd(@char.Slot, @char))
        {
          Logger.ForAccount(Player).Warning("Multiple characters on slot {slot}", @char.Slot);
        }
      }
    }

    public Player Player { get; }
    public Character CurrentCharacter => GetCharacter(CurrentSlot);
    public byte CurrentSlot { get; private set; }

    /// <summary>
    ///     Returns the character on the given slot.
    ///     Returns null if the character does not exist
    /// </summary>
    public Character this[byte slot] => GetCharacter(slot);

    public int Count => _characters.Count;

        public void DecreaseDurability(int loss)
        {
            try
            {
                var items = new List<ItemDurabilityInfoDto>();
                if (CurrentCharacter == null)
                    return;

                foreach (var item in CurrentCharacter.Weapons.GetItems()
                    .Where(item => item != null && item.Durability != -1))
                {
                    items.Add(item.LoseDurability(loss));
                }
                //Disabled Duraability loss for costumes

                foreach (var item in CurrentCharacter.Costumes.GetItems()
                           .Where(item => item != null && item.Durability != -1))
                {
                    items.Add(item.LoseDurability(loss));
                }

                foreach (var item in CurrentCharacter.Skills.GetItems()
                    .Where(item => item != null && item.Durability != -1))
                {
                    items.Add(item.LoseDurability(loss));
                }

                Player?.SendAsync(new ItemDurabilityItemAckMessage(items.ToArray()));
            }
            catch (Exception)
            {
                // ignored
            }
        }

    public IEnumerator<Character> GetEnumerator()
    {
      return _characters.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    ///     Returns the character on the given slot.
    ///     Returns null if the character does not exist
    /// </summary>
    public Character GetCharacter(byte slot)
    {
    //  using (_sync.Lock())
      {
        return _characters.GetValueOrDefault(slot);
      }
    }

    /// <summary>
    ///     Creates a new character
    /// </summary>
    /// <exception cref="CharacterException"></exception>
    public Character Create(byte slot, CharacterGender gender)
    {
      if (Count >= 3)
        throw new CharacterException("Character limit reached");

      if (_characters.ContainsKey(slot))
        throw new CharacterException($"Slot {slot} is already in use");

      var @char = new Character(this, slot, gender);
      _characters.TryAdd(slot, @char);

      var charStyle = new CharacterStyle(@char.Gender, @char.Slot);
      Player.Session.SendAsync(new CSuccessCreateCharacterAckMessage(@char.Slot, charStyle));

      return @char;
    }

    public Character CreateFirst(byte slot, CharacterGender gender)
    {
      if (Count >= 3)
        throw new CharacterException("Character limit reached");

      if (_characters.ContainsKey(slot))
        throw new CharacterException($"Slot {slot} is already in use");

      var @char = new Character(this, slot, gender);
      _characters.TryAdd(slot, @char);

      return @char;
    }

    /// <summary>
    ///     Selects the character on the given slot
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public void Select(byte slot, bool silent = false)
    {
      if (!Contains(slot))
        throw new CharacterException($"Slot {slot} does not exist");

      if (CurrentSlot != slot)
        Player.NeedsToSave = true;

      CurrentSlot = slot;
      if (!silent) Player.Session.SendAsync(new CharacterSelectAckMessage(CurrentSlot));
    }

    public bool CheckChars()
    {
      var works = false;
      for (var i = 0; i <= 3; i++)
      {
        if (Contains((byte)i))
        {
          works = true;
        }
      }

      return works;
    }

    /// <summary>
    ///     Removes the character
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public void Remove(Character @char)
    {
      Remove(@char.Slot);
    }

    /// <summary>
    ///     Removes the character on the given slot
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public void Remove(byte slot)
    {
      if (Count == 1)
        throw new ArgumentException($"Slot {slot} is the last char", nameof(slot));

      var @char = GetCharacter(slot);
      if (@char == null)
        throw new ArgumentException($"Slot {slot} does not exist", nameof(slot));

      _characters.TryRemove(slot, out _);
      if (@char.ExistsInDatabase)
        _charactersToDelete.Push(@char);
      Player.Session.SendAsync(new CharacterDeleteAckMessage(slot));
    }

    internal void Save(IDbConnection db)
    {
      if (!_charactersToDelete.IsEmpty)
      {
        var idsToRemove = new StringBuilder();
        var firstRun = true;
        while (_charactersToDelete.TryPop(out var charToDelete))
        {
          if (firstRun)
            firstRun = false;
          else
            idsToRemove.Append(',');
          idsToRemove.Append(charToDelete.Id);
        }

        DbUtil.BulkDelete<PlayerCharacterDto>(db, statement => statement
            .Where($"{nameof(PlayerCharacterDto.Id):C} IN ({idsToRemove})"));
      }

      foreach (var @char in _characters.Values)
      {
        if (!@char.ExistsInDatabase)
        {
          var charDto = new PlayerCharacterDto
          {
            Id = @char.Id,
            PlayerId = (int)Player.Account.Id,
            Slot = @char.Slot,
            Gender = (byte)@char.Gender,
          };
          SetDtoItems(@char, charDto);
          DbUtil.Insert(db, charDto);
          @char.ExistsInDatabase = true;
        }
        else
        {
          if (!@char.NeedsToSave)
            continue;

          var charDto = new PlayerCharacterDto
          {
            Id = @char.Id,
            PlayerId = (int)Player.Account.Id,
            Slot = @char.Slot,
            Gender = (byte)@char.Gender,
          };
          SetDtoItems(@char, charDto);
          DbUtil.Update(db, charDto);
          @char.NeedsToSave = false;
        }
      }
    }

    public bool Contains(byte slot)
    {
      return _characters.ContainsKey(slot);
    }

    private void SetDtoItems(Character @char, PlayerCharacterDto charDto)
    {
      PlayerItem item;

      // Weapons
      for (var slot = WeaponSlot.Weapon1; slot <= WeaponSlot.Weapon3; slot++)
      {
        item = @char.Weapons.GetItem(slot);
        var itemId = item != null ? (int?)item.Id : null;

        switch (slot)
        {
          case WeaponSlot.Weapon1:
            charDto.Weapon1Id = itemId;
            break;

          case WeaponSlot.Weapon2:
            charDto.Weapon2Id = itemId;
            break;

          case WeaponSlot.Weapon3:
            charDto.Weapon3Id = itemId;
            break;
        }
      }

      // Skills
      item = @char.Skills.GetItem(SkillSlot.Skill);
      charDto.SkillId = item != null ? (int?)item.Id : null;

      // Costumes
      for (var slot = CostumeSlot.Hair; slot <= CostumeSlot.Pet; slot++)
      {
        item = @char.Costumes.GetItem(slot);
        var itemId = item != null ? (int?)item.Id : null;

        switch (slot)
        {
          case CostumeSlot.Hair:
            charDto.HairId = itemId;
            break;

          case CostumeSlot.Face:
            charDto.FaceId = itemId;
            break;

          case CostumeSlot.Shirt:
            charDto.ShirtId = itemId;
            break;

          case CostumeSlot.Pants:
            charDto.PantsId = itemId;
            break;

          case CostumeSlot.Gloves:
            charDto.GlovesId = itemId;
            break;

          case CostumeSlot.Shoes:
            charDto.ShoesId = itemId;
            break;

          case CostumeSlot.Accessory:
            charDto.AccessoryId = itemId;
            break;

          case CostumeSlot.Pet:
            charDto.PetId = itemId;
            break;
        }
      }
    }
  }
}