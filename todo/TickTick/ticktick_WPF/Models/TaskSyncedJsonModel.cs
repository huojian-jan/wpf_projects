// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskSyncedJsonModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskSyncedJsonModel : BaseModel
  {
    [Indexed]
    public string taskSid { get; set; }

    public string userId { get; set; }

    public string jsonString { get; set; }
  }
}
