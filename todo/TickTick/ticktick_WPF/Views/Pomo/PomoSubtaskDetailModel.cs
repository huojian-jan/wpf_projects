// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoSubtaskDetailModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoSubtaskDetailModel : BaseViewModel
  {
    public string Id;
    public long SortOrder;

    public string Title { get; set; }

    public int Status { get; set; }

    public DateTime? CompleteDate { get; set; }

    public PomoSubtaskDetailModel(TaskDetailItemModel item)
    {
      this.Id = item.id;
      this.Title = item.title;
      this.SortOrder = item.sortOrder;
      this.Status = item.status;
      this.CompleteDate = item.completedTime;
    }
  }
}
