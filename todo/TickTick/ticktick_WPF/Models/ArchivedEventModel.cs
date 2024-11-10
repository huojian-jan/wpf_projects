// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ArchivedEventModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class ArchivedEventModel : BaseModel
  {
    public string UserId { get; set; }

    public string Key { get; set; }

    public DateTime? StartTime { get; set; }

    public string Title { get; set; }

    public int Kind { get; set; }

    public int SyncStatus { get; set; }
  }
}
