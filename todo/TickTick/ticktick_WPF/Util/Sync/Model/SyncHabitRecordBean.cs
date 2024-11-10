// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.SyncHabitRecordBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class SyncHabitRecordBean
  {
    [JsonProperty(PropertyName = "add")]
    public List<HabitRecordModel> Add { get; set; } = new List<HabitRecordModel>();

    [JsonProperty(PropertyName = "update")]
    public List<HabitRecordModel> Update { get; set; } = new List<HabitRecordModel>();

    [JsonProperty(PropertyName = "delete")]
    public List<HabitRecordModel> Delete { get; set; } = new List<HabitRecordModel>();
  }
}
