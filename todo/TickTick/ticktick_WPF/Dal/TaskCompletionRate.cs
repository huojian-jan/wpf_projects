// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskCompletionRate
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;

#nullable disable
namespace ticktick_WPF.Dal
{
  [Serializable]
  public class TaskCompletionRate
  {
    public bool IsFromDb;

    [PrimaryKey]
    public string TaskId { get; set; } = string.Empty;

    public int CompletedCount { get; set; }

    public int TotalCount { get; set; }
  }
}
