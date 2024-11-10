// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TaskPrintViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TaskPrintViewModel : BaseViewModel
  {
    public string Title { get; set; }

    public string Content { get; set; }

    public string Desc { get; set; }

    public string AvatarUrl { get; set; }

    public string FirstTag { get; set; }

    public string SecondTag { get; set; }

    public string MoreTag { get; set; }

    public string ProjectName { get; set; }

    public DisplayType Type { get; set; }

    public string Kind { get; set; }

    public int Progress { get; set; }

    public int Status { get; set; }

    public int Count { get; set; }

    public int Priority { get; set; }

    public bool ShowAttachment { get; set; }

    public bool ShowProgress { get; set; }

    public bool ShowLocation { get; set; }

    public bool ShowComment { get; set; }

    public bool ShowDescription { get; set; }

    public bool IsOpen { get; set; }

    public bool ShowRepeat { get; set; }

    public bool ShowReminder { get; set; }

    public string AttendId { get; set; }

    public bool? IsAllDay { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public List<SubtaskPrintViewModel> SubtaskPrintViewModels { get; set; }

    public bool IsNote { get; set; }

    public int Level { get; set; }

    public int CalendarType { get; set; }

    public TaskPrintViewModel(DisplayItemModel model)
    {
      this.Title = model.Title;
      this.AvatarUrl = model.AvatarUrl;
      string[] tags1 = model.Tags;
      this.FirstTag = (tags1 != null ? (tags1.Length != 0 ? 1 : 0) : 0) != 0 ? model.Tags[0] : (string) null;
      string[] tags2 = model.Tags;
      this.SecondTag = (tags2 != null ? (tags2.Length > 1 ? 1 : 0) : 0) != 0 ? model.Tags[1] : (string) null;
      string[] tags3 = model.Tags;
      this.MoreTag = (tags3 != null ? (tags3.Length > 2 ? 1 : 0) : 0) != 0 ? "+" + (model.Tags.Length - 2).ToString() : (string) null;
      this.ProjectName = model.ProjectName;
      this.Type = model.Type;
      this.Kind = model.Kind;
      this.Progress = model.Progress;
      this.Status = model.Status;
      this.Priority = model.Priority;
      this.ShowAttachment = model.GetShowAttachment();
      this.ShowProgress = model.ShowProgress;
      this.ShowLocation = model.ShowLocation;
      this.ShowComment = model.ShowComment;
      this.ShowDescription = model.ShowDescription;
      this.ShowRepeat = model.ShowRepeat;
      this.ShowReminder = model.ShowReminder.GetValueOrDefault();
      this.IsAllDay = model.IsAllDay;
      this.StartDate = model.StartDate;
      this.DueDate = model.DueDate;
      this.Count = model.Num;
      this.IsOpen = model.IsOpen;
      this.IsNote = model.IsNote;
      this.Level = model.Level;
      this.AttendId = model.AttendId;
      this.CalendarType = model.CalendarType;
    }
  }
}
