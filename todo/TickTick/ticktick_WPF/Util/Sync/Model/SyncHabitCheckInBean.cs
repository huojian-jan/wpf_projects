// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.SyncHabitCheckInBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class SyncHabitCheckInBean
  {
    [JsonProperty(PropertyName = "update")]
    public List<HabitCheckInModel> Update { get; set; } = new List<HabitCheckInModel>();

    [JsonProperty(PropertyName = "add")]
    public List<HabitCheckInModel> Add { get; set; } = new List<HabitCheckInModel>();

    [JsonProperty(PropertyName = "delete")]
    public List<HabitCheckInModel> Delete { get; set; } = new List<HabitCheckInModel>();
  }
}
