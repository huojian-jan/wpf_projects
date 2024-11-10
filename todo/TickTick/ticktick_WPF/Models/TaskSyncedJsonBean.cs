// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskSyncedJsonBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskSyncedJsonBean
  {
    private List<TaskModel> added = new List<TaskModel>();
    private List<TaskModel> updated = new List<TaskModel>();

    public List<TaskModel> Added
    {
      get => this.added;
      set => this.added = value;
    }

    public List<TaskModel> Updated
    {
      get => this.updated;
      set => this.updated = value;
    }

    public bool Empty => this.Added.Count == 0 && this.Updated.Count == 0;
  }
}
