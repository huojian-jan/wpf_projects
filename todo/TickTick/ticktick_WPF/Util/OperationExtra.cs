// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.OperationExtra
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Util
{
  public class OperationExtra
  {
    public string TaskId;
    public int Priority = -1;
    public string ProjectId = string.Empty;
    public string ColumnId = string.Empty;
    public string Assignee = string.Empty;
    public TimeData TimeModel;
    public TagSelectData Tags;
    public bool ShowCopy;
    public bool ShowCopyLink;
    public bool ShowPomo;
    public bool ShowSkip;
    public bool ShowDate = true;
    public bool ShowAssignTo;
    public bool ShowCreateSubTask;
    public bool ShowMerge;
    public bool InNoteProject;
    public TaskType TaskType;
    public bool? IsAbandoned;
    public bool? IsPinned;
    public DateTime? CompleteTime;
    public bool ShowCopyText;
    public bool InBatch;
    public bool CanSwitch;
    public bool ShowSwitch;
    public string FailedSwitchTips = string.Empty;
  }
}
