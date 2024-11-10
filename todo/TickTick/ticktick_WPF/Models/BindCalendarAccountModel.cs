// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.BindCalendarAccountModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class BindCalendarAccountModel : BaseModel
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("account")]
    public string Account { get; set; } = string.Empty;

    [JsonProperty("site")]
    public string Site { get; set; }

    [JsonProperty("createdTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? CreatedTime { get; set; }

    [JsonProperty("modifiedTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? ModifiedTime { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    [Ignore]
    [JsonProperty("calendars")]
    public List<BindCalendarModel> Calendars { get; set; }

    [JsonProperty("kind")]
    public string Kind { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("domain")]
    public string Domain { get; set; }

    [JsonProperty("desc")]
    public string Description { get; set; }

    [JsonIgnore]
    public bool Expired { get; set; }

    public bool IsFeiShu() => this.Kind == "api" && this.Site == "feishu";

    public bool IsBindAccountPassword() => this.Kind == "caldav" || this.Kind == "exchange";

    public bool IsCalDav() => this.Kind == "caldav" || this.Kind == "icloud";
  }
}
