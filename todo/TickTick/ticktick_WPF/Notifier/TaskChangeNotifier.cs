// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.TaskChangeNotifier
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public class TaskChangeNotifier
  {
    private readonly TasksChangeEventArgs _tasksChangeEventArgs = new TasksChangeEventArgs();
    public static readonly TaskChangeNotifier Notifier = new TaskChangeNotifier();

    public event EventHandler<TasksChangeEventArgs> TasksChanged;

    private TaskChangeNotifier()
    {
    }

    private async Task TryNotify(int delay = 10, bool needSync = true)
    {
      DelayActionHandlerCenter.TryDoAction("TasksChangeNotify", (EventHandler) ((sender, args) => ThreadUtil.DetachedRunOnUiThread(new Action(this.NotifyTasksChanged))), delay);
      if (!needSync)
        return;
      SyncManager.TryDelaySync();
    }

    private void NotifyTasksChanged()
    {
      TasksChangeEventArgs eventArgs = this.GetEventArgs();
      SearchHelper.ClearTaskSearchModels();
      EventHandler<TasksChangeEventArgs> tasksChanged = this.TasksChanged;
      if (tasksChanged != null)
        tasksChanged((object) TaskChangeNotifier.Notifier, eventArgs);
      TickFocusManager.OnTasksChanged(eventArgs);
      if (!ABTestManager.IsNewRemindCalculate())
        return;
      TaskReminderCalculator.OnTasksChanged(eventArgs);
      CheckItemReminderCalculator.OnTasksChanged(eventArgs);
    }

    private TasksChangeEventArgs GetEventArgs()
    {
      TasksChangeEventArgs eventArgs = this._tasksChangeEventArgs.Copy();
      this._tasksChangeEventArgs.Clear();
      return eventArgs;
    }

    public static void NotifyTaskChanged(TasksChangeEventArgs args)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.Merge(args);
      TaskChangeNotifier.Notifier.TryNotify();
    }

    public static void NotifyBatchDeleteUndo(List<string> taskIds)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.UndoDeletedIds.AddRange((IEnumerable<string>) taskIds, true);
      TaskChangeNotifier.Notifier.TryNotify(needSync: false);
    }

    public static void NotifyTaskBatchDeletedChanged(List<string> taskIds, bool needSync = true)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.DeletedChangedIds.AddRange((IEnumerable<string>) taskIds, true);
      TaskChangeNotifier.Notifier.TryNotify(needSync: needSync);
    }

    public static void NotifyTaskBatchAdded(List<string> taskIds)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.AddIds.AddRange((IEnumerable<string>) taskIds, true);
      TaskChangeNotifier.Notifier.TryNotify();
    }

    public static void NotifyDeleteUndo(string taskId)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.UndoDeletedIds.Add(taskId);
      TaskChangeNotifier.Notifier.TryNotify(100, false);
    }

    public static async void NotifyTaskDeleted(string taskId)
    {
      if (taskId == null)
        return;
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.DeletedChangedIds.Add(taskId);
      TaskChangeNotifier.Notifier.TryNotify(100, false);
    }

    public static async void NotifyTaskAdded(TaskModel task)
    {
      if (task == null)
        return;
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.AddIds.Add(task.id);
      TaskChangeNotifier.Notifier.TryNotify(100);
    }

    public static async void NotifyTaskSortChanged(IEnumerable<string> taskIds)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.SortOrderChangedIds.AddRange(taskIds, true);
      TaskChangeNotifier.Notifier.TryNotify();
    }

    public static async void NotifyTaskStatusChanged(TaskCloseExtra extra)
    {
      TaskModel originalTask = extra.OriginalTask;
      if ((originalTask != null ? (originalTask.status != 0 ? 1 : 0) : 1) != 0 || extra.RepeatTask == null)
        TaskChangeNotifier.Notifier._tasksChangeEventArgs.StatusChangedIds.Add(extra.OriginalTask?.id);
      else if (!string.IsNullOrEmpty(extra.OriginalTask?.id))
        TaskChangeNotifier.Notifier._tasksChangeEventArgs.DateChangedIds.Add(extra.OriginalTask?.id);
      if (!string.IsNullOrEmpty(extra.RepeatTask?.id))
        TaskChangeNotifier.Notifier._tasksChangeEventArgs.AddIds.Add(extra.RepeatTask?.id);
      TaskChangeNotifier.Notifier.TryNotify(100);
    }

    public static async void NotifyTaskDateChanged(string taskId, object sender = null)
    {
      if (taskId == null)
        return;
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.DateChangedIds.Add(taskId);
      TaskChangeNotifier.Notifier.TryNotify(100);
    }

    public static async void NotifyTaskCopied(List<string> taskIds, bool waitNotify = false)
    {
      if (waitNotify)
        await Task.Delay(350);
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.AddIds.AddRange((IEnumerable<string>) taskIds);
      TaskChangeNotifier.Notifier.TryNotify(100);
    }

    public static void NotifyTaskProjectChanged(TaskModel task)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.ProjectChangedIds.Add(task.id);
      TaskChangeNotifier.Notifier.TryNotify(100);
    }

    public static void NotifyTaskTextChanged(string taskId)
    {
      if (string.IsNullOrEmpty(taskId))
        return;
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.TaskTextChangedIds.Add(taskId);
      TaskChangeNotifier.Notifier.TryNotify(100, false);
    }

    public static async void NotifyBatchDateChanged(List<string> taskIds, object sender = null, bool sync = true)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.DateChangedIds.AddRange((IEnumerable<string>) taskIds);
      TaskChangeNotifier.Notifier.TryNotify(100, sync);
    }

    public static void NotifyTaskPriorityChanged(
      string id,
      int priority,
      List<string> ids = null,
      object sender = null)
    {
      List<string> stringList;
      if (!string.IsNullOrEmpty(id))
        stringList = new List<string>() { id };
      else
        stringList = ids;
      ids = stringList;
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.PriorityChangedIds.AddRange((IEnumerable<string>) ids);
      TaskChangeNotifier.Notifier.TryNotify(100);
    }

    public static void NotifyTaskTagsChanged(TagExtra tags)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.TagChangedIds.AddRange((IEnumerable<string>) tags?.GetIds());
      TaskChangeNotifier.Notifier.TryNotify(100);
    }

    public static void NotifyDropToItem(string taskId)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.DeletedChangedIds.Add(taskId);
      TaskChangeNotifier.Notifier.TryNotify(needSync: false);
    }

    public static void NotifyTasksMerged(List<string> taskIds)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.BatchChangedIds.AddRange((IEnumerable<string>) taskIds);
      TaskChangeNotifier.Notifier.TryNotify(needSync: false);
    }

    public static void NotifyTaskOpenChanged(string taskId)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.TasksOpenChangedIds.Add(taskId);
      TaskChangeNotifier.Notifier.TryNotify(needSync: false);
    }

    public static void NotifyTaskPomoAdded(List<string> taskIds)
    {
      // ISSUE: explicit non-virtual call
      if (taskIds == null || __nonvirtual (taskIds.Count) <= 0)
        return;
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.PomoChangedIds.AddRange((IEnumerable<string>) taskIds);
      TaskChangeNotifier.Notifier.TryNotify(needSync: false);
    }

    public static void NotifyTaskKindChanged(List<string> ids)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.KindChangedIds.AddRange((IEnumerable<string>) ids);
      TaskChangeNotifier.Notifier.TryNotify();
    }

    public static void NotifyTasksCopied(List<string> ids)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.BatchChangedIds.AddRange((IEnumerable<string>) ids);
      TaskChangeNotifier.Notifier.TryNotify();
    }

    public static void NotifyTaskAssigneeChanged(List<string> ids, string assignee)
    {
      if (ids == null)
        return;
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.AssignChangedIds.AddRange((IEnumerable<string>) ids);
      TaskChangeNotifier.Notifier.TryNotify(50);
    }

    public static void NotifyAttachmentChanged(string taskId, object sender)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.AttachmentChangedIds.Add(taskId);
      TaskChangeNotifier.Notifier.TryNotify(50, false);
    }

    public static void NotifyTaskBatchChanged(List<string> ids)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.BatchChangedIds.AddRange((IEnumerable<string>) ids);
      TaskChangeNotifier.Notifier.TryNotify(50, false);
    }

    public static async void OnServerTaskChanged(List<string> ids)
    {
      List<string> stringList = ids;
      // ISSUE: explicit non-virtual call
      if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      await Task.Delay(1000);
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.BatchChangedIds.AddRange((IEnumerable<string>) ids);
      TaskChangeNotifier.Notifier.TryNotify(50, false);
    }

    public static void NotifyBatchTaskStarredChanged(TaskPinExtra ext)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.PinChangedIds.AddRange((IEnumerable<string>) ext.Ids);
      TaskChangeNotifier.Notifier.TryNotify(50);
    }

    public static void OnCheckItemChanged(string itemId, bool needSync = false)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.CheckItemChangedIds.Add(itemId);
      TaskChangeNotifier.Notifier.TryNotify(50, needSync);
    }

    public static void NotifyTaskParentChanged(List<string> parentChangedIds)
    {
      TaskChangeNotifier.Notifier._tasksChangeEventArgs.SortOrderChangedIds.AddRange((IEnumerable<string>) parentChangedIds);
      TaskChangeNotifier.Notifier.TryNotify();
    }
  }
}
