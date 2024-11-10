// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TeamModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class TeamModel : BaseModel
  {
    public string id { get; set; }

    public string name { get; set; }

    public DateTime createdTime { get; set; }

    public DateTime modifiedTime { get; set; }

    public DateTime joinedTime { get; set; }

    public bool expired { get; set; }

    public DateTime expiredDate { get; set; }

    public string userId { get; set; }

    public bool open { get; set; } = true;

    public bool needAuditCode { get; set; }

    public bool needAuditUrl { get; set; }

    public string industry { get; set; }

    public string scale { get; set; }

    public string logo { get; set; }

    [JsonIgnore]
    public bool expiredChecked { get; set; }

    public bool teamPro { get; set; }

    public bool IsPro() => this.teamPro && this.expiredDate >= DateTime.Now;
  }
}
