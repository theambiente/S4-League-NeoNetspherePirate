﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoNetsphere.Database.Auth
{
  [Table("accounts")]
  public class AccountDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Username { get; set; }
    public string Nickname { get; set; }
    public string Password { get; set; }
    public string Salt { get; set; }
    public byte SecurityLevel { get; set; }
    public string AuthToken { get; set; }
    public string newToken { get; set; }
    public string LoginToken { get; set; }
    public string LastLogin { get; set; }

    public IList<BanDto> Bans { get; set; } = new List<BanDto>();
    public IList<HwidBanDto> HwidBans { get; set; } = new List<HwidBanDto>();
    public IList<LoginHistoryDto> LoginHistory { get; set; } = new List<LoginHistoryDto>();
    public IList<NicknameHistoryDto> NicknameHistory { get; set; } = new List<NicknameHistoryDto>();
  }

  [Table("bans")]
  public class BanDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Account))] public int AccountId { get; set; }

    public AccountDto Account { get; set; }

    public long Date { get; set; }
    public long? Duration { get; set; }
    public string Reason { get; set; }
  }

  [Table("hwid_bans")]
  public class HwidBanDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public long Date { get; set; }
    public string Hwid { get; set; }
  }

  [Table("auth_history")]
  public class AuthHistoryDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Account))] public int AccountId { get; set; }

    public AccountDto Account { get; set; }

    public long Date { get; set; }
    public string HWID { get; set; }
  }

  [Table("login_history")]
  public class LoginHistoryDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Account))] public int AccountId { get; set; }

    public AccountDto Account { get; set; }

    public long Date { get; set; }
    public string IP { get; set; }
  }

  [Table("nickname_history")]
  public class NicknameHistoryDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Account))] public int AccountId { get; set; }

    public AccountDto Account { get; set; }

    public string OldName { get; set; }
    public string NewNickname { get; set; }

    public long? ExpireDate { get; set; }
  }
}
