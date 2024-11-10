// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.CalendarEventBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class CalendarEventBean
  {
    [JsonProperty(PropertyName = "calendarId")]
    public string CalendarId { get; set; }

    [JsonProperty(PropertyName = "add")]
    public List<CalendarEventModel> Add { get; set; } = new List<CalendarEventModel>();

    [JsonProperty(PropertyName = "update")]
    public List<CalendarEventModel> Update { get; set; } = new List<CalendarEventModel>();

    [JsonProperty(PropertyName = "delete")]
    public List<string> Delete { get; set; } = new List<string>();

    [JsonProperty(PropertyName = "move")]
    public List<EventMoveModel> Move { get; set; } = new List<EventMoveModel>();
  }
}
