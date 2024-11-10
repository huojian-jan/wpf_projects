// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskDeleteUndo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Dal;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class TaskDeleteUndo : UndoController
  {
    private readonly string _taskId;
    private readonly string _title;
    private readonly bool _isEmptyDelete;

    public TaskDeleteUndo(string taskId, string title, bool isEmptyDelete = false)
    {
      this._taskId = taskId;
      this._title = title;
      this._isEmptyDelete = isEmptyDelete;
      UndoHelper.DeletingIds.Add(taskId);
      TaskChangeNotifier.NotifyTaskDeleted(taskId);
    }

    public override string GetTitle() => this._title;

    public override string GetContent() => Utils.GetString("Deleted");

    public override async void Undo()
    {
      await TaskService.UndoDeletedTask(this._taskId);
      UndoHelper.DeletingIds.Remove(this._taskId);
    }

    public override async void Finished()
    {
      if (this._isEmptyDelete)
        await SyncStatusDao.BatchAddDeleteForeverSyncStatus((IEnumerable<string>) new List<string>()
        {
          this._taskId
        });
      else
        await TaskService.DeleteTask(this._taskId);
      UndoHelper.DeletingIds.Remove(this._taskId);
      SyncManager.TryDelaySync();
    }
  }
}
