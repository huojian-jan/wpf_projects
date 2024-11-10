// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.HabitSectionModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.ComponentModel;
using ticktick_WPF.Util;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class HabitSectionModel : BaseModel
  {
    private static HabitSectionModel _default = new HabitSectionModel()
    {
      UserId = string.Empty,
      Id = "-1",
      SortOrder = 0,
      Name = Utils.GetString("HabitSectionOthers")
    };

    public HabitSectionModel()
    {
      this.UserId = Utils.GetCurrentUserIdInt().ToString();
      this.Id = Utils.GetGuid();
      this.SortOrder = 0L;
      this.CreatedTime = DateTime.Now;
    }

    public static HabitSectionModel GetDefault() => HabitSectionModel._default;

    public override bool Equals(object obj)
    {
      return obj is HabitSectionModel habitSectionModel && this.Id == habitSectionModel.Id;
    }

    public HabitSectionModel(string name)
    {
      this.UserId = Utils.GetCurrentUserIdInt().ToString();
      this.Id = Utils.GetGuid();
      this.SortOrder = -1L;
      this.CreatedTime = DateTime.Now;
      this.Name = name;
    }

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "userId")]
    public string UserId { get; set; }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonIgnore]
    [Ignore]
    public string DisplayName
    {
      get
      {
        return this.Name.StartsWith("_") && (this.Name == "_morning" || this.Name == "_afternoon" || this.Name == "_night") ? Utils.GetString("HabitSection" + this.Name.Substring(1, 1).ToUpper() + this.Name.Substring(2)) : this.Name;
      }
    }

    [JsonIgnore]
    public bool IsOpen { get; set; } = true;

    [JsonProperty(PropertyName = "sortOrder")]
    public long SortOrder { get; set; }

    [JsonProperty(PropertyName = "etag")]
    public string Etag { get; set; }

    [JsonProperty(PropertyName = "cratedTime")]
    public DateTime CreatedTime { get; set; }

    [JsonProperty(PropertyName = "modifiedTime")]
    public DateTime ModifiedTime { get; set; }

    [JsonIgnore]
    [NotNull]
    [DefaultValue("2")]
    public int SyncStatus { get; set; }
  }
}
