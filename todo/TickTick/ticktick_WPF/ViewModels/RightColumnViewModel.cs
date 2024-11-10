// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.RightColumnViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class RightColumnViewModel
  {
    public string AvatarUrl { get; set; }

    public string FirstTag { get; set; }

    public string SecondTag { get; set; }

    public string MoreTag { get; set; }

    public string ProjectName { get; set; }

    public int Progress { get; set; }

    public int Status { get; set; }

    public bool ShowAttachment { get; set; }

    public bool ShowProgress { get; set; }

    public bool ShowLocation { get; set; }

    public bool ShowComment { get; set; }

    public bool ShowDescription { get; set; }

    public bool ShowRepeat { get; set; }

    public bool ShowReminder { get; set; }

    public bool? IsAllDay { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsNote { get; set; }

    public RightColumnViewModel(TaskPrintViewModel model, bool hideProjectName)
    {
      this.AvatarUrl = model.AvatarUrl;
      this.FirstTag = TagDataHelper.GetTagDisplayName(model.FirstTag);
      this.SecondTag = TagDataHelper.GetTagDisplayName(model.SecondTag);
      this.MoreTag = model.MoreTag;
      this.ProjectName = hideProjectName ? "" : model.ProjectName;
      this.Progress = model.Progress;
      this.Status = model.Status;
      this.ShowAttachment = model.ShowAttachment;
      this.ShowProgress = model.ShowProgress;
      this.ShowLocation = model.ShowLocation;
      this.ShowComment = model.ShowComment;
      this.ShowDescription = model.ShowDescription;
      this.ShowRepeat = model.ShowRepeat;
      this.ShowReminder = model.ShowReminder;
      this.IsAllDay = model.IsAllDay;
      this.StartDate = model.StartDate;
      this.DueDate = model.DueDate;
      this.IsNote = model.IsNote;
    }
  }
}
