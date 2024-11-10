// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchFilterModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Util.Provider;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchFilterModel
  {
    public List<string> GroupIds;
    public List<string> ProjectIds;
    public List<string> Tags;
    public List<int> Priorities;
    public TaskType TaskType = TaskType.TaskAndNote;
    public List<string> Assignees;
    public int? Status;
    public DateTime? Start;
    public DateTime? End;
    public string Key;
    public DateFilter DateFilter;

    public bool SearchEvent()
    {
      return (this.ProjectIds == null || this.ProjectIds.Count == 0) && (this.Tags == null || this.Tags.Count == 0) && (this.GroupIds == null || this.GroupIds.Count == 0) && (this.Priorities == null || this.Priorities.Count == 0) && (this.Assignees == null || this.Assignees.Count == 0) && this.TaskType == TaskType.TaskAndNote;
    }
  }
}
