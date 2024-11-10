// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.MoveColumnArgs
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class MoveColumnArgs : BaseModel
  {
    public string columnId { get; set; }

    public string fromProjectId { get; set; }

    public string toProjectId { get; set; }

    public long sortOrder { get; set; }

    public List<string> taskIds { get; set; }

    [JsonIgnore]
    public List<SyncStatusModel> tasks { get; set; }
  }
}
