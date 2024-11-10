// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.UserModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;

#nullable disable
namespace ticktick_WPF.Models
{
  public class UserModel
  {
    public string token { get; set; }

    [PrimaryKey]
    public string userId { get; set; }

    public string code { get; set; }

    public string userCode { get; set; }

    public string username { get; set; }

    public DateTime? proEndDate { get; set; }

    public string subscribeType { get; set; }

    public string subscribeFreq { get; set; }

    public bool needSubscribe { get; set; }

    public string inboxId { get; set; }

    public bool pro { get; set; }

    public long checkPoint { get; set; }

    public long columnCheckPoint { get; set; }

    public bool teamUser { get; set; }

    public bool activeTeamUser { get; set; }

    public bool teamPro { get; set; }

    public string phone { get; set; }
  }
}
