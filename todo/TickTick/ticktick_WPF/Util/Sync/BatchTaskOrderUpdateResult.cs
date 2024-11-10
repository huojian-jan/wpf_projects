// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.BatchTaskOrderUpdateResult
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class BatchTaskOrderUpdateResult
  {
    private BatchUpdateResult taskOrderByDate;
    private BatchUpdateResult projectGroup;

    [JsonProperty(PropertyName = "taskOrderByDate")]
    public BatchUpdateResult TaskOrderByDate
    {
      get => this.taskOrderByDate;
      set => this.taskOrderByDate = value;
    }

    [JsonProperty(PropertyName = "taskOrderByProject")]
    public BatchUpdateResult ProjectGroup
    {
      get => this.projectGroup;
      set => this.projectGroup = value;
    }
  }
}
