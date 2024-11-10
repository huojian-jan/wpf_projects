// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.SetBatchTaskGridArgs
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class SetBatchTaskGridArgs
  {
    public bool CanSwitchTaskOrNote = true;
    public bool CanDelete = true;
    public bool CanPinOrUnPin = true;
    public bool HasSamePriority = true;
    public bool HasSameDate = true;
    public bool HasSameProject = true;
    public bool HasSameTag = true;
    public string TagStr = string.Empty;
    public bool HasTask;
    public bool HasNote;
    public bool HasAbandoned;
    public bool IsDeleted;
    public bool IsAllPinned = true;
    public bool IsAllCompleted = true;

    public bool CanSwitchTask => this.CanSwitchTaskOrNote && this.OnlyNote;

    public bool CanSwitchNote => this.CanSwitchTaskOrNote && this.OnlyTask;

    public bool CanMerge => !this.IsDeleted && this.OnlyTask;

    public bool OnlyNote => this.HasNote && !this.HasTask;

    public bool OnlyTask => this.HasTask && !this.HasNote;
  }
}
