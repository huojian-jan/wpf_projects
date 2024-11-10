// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CourseModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System.Collections.Generic;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class CourseModel : BaseModel
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    [Ignore]
    [JsonProperty("items")]
    public List<CourseDetailModel> Items { get; set; }

    [JsonIgnore]
    public string ItemsStr { get; set; }

    [JsonIgnore]
    public string ScheduleId { get; set; }
  }
}
