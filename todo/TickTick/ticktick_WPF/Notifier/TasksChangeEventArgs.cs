// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.TasksChangeEventArgs
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public class TasksChangeEventArgs : EventArgs
  {
    public BlockingSet<string> DeletedChangedIds = new BlockingSet<string>();
    public BlockingSet<string> UndoDeletedIds = new BlockingSet<string>();
    public BlockingSet<string> StatusChangedIds = new BlockingSet<string>();
    public BlockingSet<string> KindChangedIds = new BlockingSet<string>();
    public BlockingSet<string> PinChangedIds = new BlockingSet<string>();
    public BlockingSet<string> ProjectChangedIds = new BlockingSet<string>();
    public BlockingSet<string> PriorityChangedIds = new BlockingSet<string>();
    public BlockingSet<string> TagChangedIds = new BlockingSet<string>();
    public BlockingSet<string> PomoChangedIds = new BlockingSet<string>();
    public BlockingSet<string> SortOrderChangedIds = new BlockingSet<string>();
    public BlockingSet<string> DateChangedIds = new BlockingSet<string>();
    public BlockingSet<string> AssignChangedIds = new BlockingSet<string>();
    public BlockingSet<string> AttachmentChangedIds = new BlockingSet<string>();
    public BlockingSet<string> TasksOpenChangedIds = new BlockingSet<string>();
    public BlockingSet<string> CheckItemChangedIds = new BlockingSet<string>();
    public BlockingSet<string> TaskTextChangedIds = new BlockingSet<string>();
    public BlockingSet<string> AddIds = new BlockingSet<string>();
    public BlockingSet<string> BatchChangedIds = new BlockingSet<string>();

    public TasksChangeEventArgs Copy()
    {
      TasksChangeEventArgs tasksChangeEventArgs = new TasksChangeEventArgs();
      tasksChangeEventArgs.DeletedChangedIds.AddRange(this.DeletedChangedIds);
      tasksChangeEventArgs.UndoDeletedIds.AddRange(this.UndoDeletedIds);
      tasksChangeEventArgs.StatusChangedIds.AddRange(this.StatusChangedIds);
      tasksChangeEventArgs.KindChangedIds.AddRange(this.KindChangedIds);
      tasksChangeEventArgs.PinChangedIds.AddRange(this.PinChangedIds);
      tasksChangeEventArgs.ProjectChangedIds.AddRange(this.ProjectChangedIds);
      tasksChangeEventArgs.PriorityChangedIds.AddRange(this.PriorityChangedIds);
      tasksChangeEventArgs.TagChangedIds.AddRange(this.TagChangedIds);
      tasksChangeEventArgs.PomoChangedIds.AddRange(this.PomoChangedIds);
      tasksChangeEventArgs.SortOrderChangedIds.AddRange(this.SortOrderChangedIds);
      tasksChangeEventArgs.DateChangedIds.AddRange(this.DateChangedIds);
      tasksChangeEventArgs.AssignChangedIds.AddRange(this.AssignChangedIds);
      tasksChangeEventArgs.AttachmentChangedIds.AddRange(this.AttachmentChangedIds);
      tasksChangeEventArgs.TasksOpenChangedIds.AddRange(this.TasksOpenChangedIds);
      tasksChangeEventArgs.CheckItemChangedIds.AddRange(this.CheckItemChangedIds);
      tasksChangeEventArgs.TaskTextChangedIds.AddRange(this.TaskTextChangedIds);
      tasksChangeEventArgs.AddIds.AddRange(this.AddIds);
      tasksChangeEventArgs.BatchChangedIds.AddRange(this.BatchChangedIds);
      return tasksChangeEventArgs;
    }

    public void Clear()
    {
      this.DeletedChangedIds.Clear();
      this.UndoDeletedIds.Clear();
      this.StatusChangedIds.Clear();
      this.KindChangedIds.Clear();
      this.PinChangedIds.Clear();
      this.ProjectChangedIds.Clear();
      this.PriorityChangedIds.Clear();
      this.TagChangedIds.Clear();
      this.PomoChangedIds.Clear();
      this.SortOrderChangedIds.Clear();
      this.DateChangedIds.Clear();
      this.AssignChangedIds.Clear();
      this.AttachmentChangedIds.Clear();
      this.TasksOpenChangedIds.Clear();
      this.CheckItemChangedIds.Clear();
      this.TaskTextChangedIds.Clear();
      this.AddIds.Clear();
      this.BatchChangedIds.Clear();
    }

    public void Merge(TasksChangeEventArgs args)
    {
      this.DeletedChangedIds.AddRange(args.DeletedChangedIds);
      this.UndoDeletedIds.AddRange(args.UndoDeletedIds);
      this.StatusChangedIds.AddRange(args.StatusChangedIds);
      this.KindChangedIds.AddRange(args.KindChangedIds);
      this.PinChangedIds.AddRange(args.PinChangedIds);
      this.ProjectChangedIds.AddRange(args.ProjectChangedIds);
      this.PriorityChangedIds.AddRange(args.PriorityChangedIds);
      this.TagChangedIds.AddRange(args.TagChangedIds);
      this.PomoChangedIds.AddRange(args.PomoChangedIds);
      this.SortOrderChangedIds.AddRange(args.SortOrderChangedIds);
      this.DateChangedIds.AddRange(args.DateChangedIds);
      this.AssignChangedIds.AddRange(args.AssignChangedIds);
      this.AttachmentChangedIds.AddRange(args.AttachmentChangedIds);
      this.TasksOpenChangedIds.AddRange(args.TasksOpenChangedIds);
      this.CheckItemChangedIds.AddRange(args.CheckItemChangedIds);
      this.TaskTextChangedIds.AddRange(args.TaskTextChangedIds);
      this.AddIds.AddRange(args.AddIds);
      this.BatchChangedIds.AddRange(args.BatchChangedIds);
    }
  }
}
