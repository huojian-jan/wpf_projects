// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.MoveOrRestoreProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class MoveOrRestoreProject
  {
    [JsonProperty(PropertyName = "fromProjectId")]
    public string FromProjectId { get; set; }

    [JsonProperty(PropertyName = "toProjectId")]
    public string ToProjectId { get; set; }

    [JsonProperty(PropertyName = "taskId")]
    public string TaskId { get; set; }
  }
}
