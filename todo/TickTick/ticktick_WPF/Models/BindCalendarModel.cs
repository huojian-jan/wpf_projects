// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.BindCalendarModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class BindCalendarModel : BaseModel
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("show")]
    public string Show { get; set; }

    [JsonProperty("timeZone")]
    public string TimeZone { get; set; }

    [JsonProperty("visible")]
    public bool Visible { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    [JsonIgnore]
    public string AccountId { get; set; }

    [JsonProperty("accessRole")]
    public string AccessRole { get; set; }

    [Ignore]
    [JsonProperty("events")]
    public List<CalendarEventModel> Events { get; set; }

    [Ignore]
    [JsonProperty("currentUserPrivilegeSet")]
    public List<string> CurrentUserPrivilegeSet { get; set; } = new List<string>();

    [JsonIgnore]
    public string LocalCurrentUserPrivilegeSet
    {
      get
      {
        return this.CurrentUserPrivilegeSet != null ? string.Join(",", (IEnumerable<string>) this.CurrentUserPrivilegeSet) : "";
      }
      set
      {
        List<string> stringList;
        if (value == null)
          stringList = (List<string>) null;
        else
          stringList = ((IEnumerable<string>) value.Split(',')).ToList<string>();
        this.CurrentUserPrivilegeSet = stringList;
      }
    }

    [Ignore]
    [JsonIgnore]
    public bool Accessible
    {
      get
      {
        BindCalendarAccountModel accountCalById = CacheManager.GetAccountCalById(this.AccountId);
        return accountCalById != null && !accountCalById.IsFeiShu() && ((string.IsNullOrEmpty(this.AccessRole) ? 0 : (this.AccessRole.ToLower().Trim() == "writer" ? 1 : (this.AccessRole.ToLower().Trim() == "owner" ? 1 : 0))) | (string.IsNullOrEmpty(this.LocalCurrentUserPrivilegeSet) ? (false ? 1 : 0) : (this.LocalCurrentUserPrivilegeSet.ToLower().Contains("write") ? (true ? 1 : 0) : (this.LocalCurrentUserPrivilegeSet.ToLower().Contains("all") ? 1 : 0)))) != 0;
      }
    }
  }
}
