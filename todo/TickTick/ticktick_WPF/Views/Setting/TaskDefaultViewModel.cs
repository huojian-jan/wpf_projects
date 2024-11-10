// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.TaskDefaultViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class TaskDefaultViewModel
  {
    public TaskDefaultViewModel(TaskDefaultModel model)
    {
      this.DateMode = model.DateMode;
      this.PriorityIndex = TaskDefaultViewModel.PriorityToIndex(model.Priority);
      this.AddTo = model.AddTo;
      this.DateIndex = Constants.DateValue.ToSelectIndex(model.Date);
      this.Duration = model.Duration;
      this.Tags = model.Tags;
    }

    public List<string> Tags { get; set; }

    public int DateMode { get; set; }

    public int PriorityIndex { get; set; }

    public int DateIndex { get; set; }

    public int AddTo { get; set; }

    public int Duration { get; set; }

    public TaskDefaultModel ToModel()
    {
      return new TaskDefaultModel()
      {
        DateMode = this.DateMode,
        Priority = TaskDefaultViewModel.IndexToPriority(this.PriorityIndex),
        AddTo = this.AddTo,
        Date = Constants.DateValue.ToValue(this.DateIndex),
        Duration = this.Duration,
        Tags = this.Tags
      };
    }

    private static int PriorityToIndex(int priority)
    {
      switch (priority)
      {
        case 0:
          return 3;
        case 1:
          return 2;
        case 3:
          return 1;
        case 5:
          return 0;
        default:
          return 3;
      }
    }

    private static int IndexToPriority(int index)
    {
      switch (index)
      {
        case 0:
          return 5;
        case 1:
          return 3;
        case 2:
          return 1;
        case 3:
          return 0;
        default:
          return 0;
      }
    }
  }
}
