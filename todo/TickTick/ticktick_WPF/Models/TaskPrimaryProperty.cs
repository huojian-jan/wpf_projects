// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskPrimaryProperty
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskPrimaryProperty
  {
    public int? Priority { get; set; }

    public TimeData TimeData { get; set; }

    public List<string> Tags { get; set; }

    public int TaskStatus { get; set; }

    public long SortOrder { get; set; }

    public string ProjectId { get; set; } = string.Empty;

    public string AssigneeId { get; set; } = string.Empty;

    public string ParentId { get; set; }
  }
}
