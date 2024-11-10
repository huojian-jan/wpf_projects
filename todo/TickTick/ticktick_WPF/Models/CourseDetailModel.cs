// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CourseDetailModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;

#nullable disable
namespace ticktick_WPF.Models
{
  public class CourseDetailModel
  {
    [JsonProperty("startLesson")]
    public int StartLesson { get; set; }

    [JsonProperty("endLesson")]
    public int EndLesson { get; set; }

    [JsonProperty("weekday")]
    public int Weekday { get; set; }

    [JsonProperty("room")]
    public string Room { get; set; }

    [JsonProperty("teacher")]
    public string Teacher { get; set; }

    [JsonProperty("weeks")]
    public int[] Weeks { get; set; }
  }
}
