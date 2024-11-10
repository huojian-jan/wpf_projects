// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.BatchTaskDeleteUndo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Dal;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class BatchTaskDeleteUndo : UndoController
  {
    private readonly List<string> _taskIds;
    private readonly List<string> _deleteForeverTaskIds;

    public BatchTaskDeleteUndo(List<string> taskIds, List<string> deleteForeverTaskIds = null)
    {
      this._taskIds = taskIds;
      this._deleteForeverTaskIds = deleteForeverTaskIds;
      UndoHelper.DeletingIds.AddRange((IEnumerable<string>) taskIds);
      TaskChangeNotifier.NotifyTaskBatchDeletedChanged(taskIds, false);
      if (deleteForeverTaskIds == null)
        return;
      UndoHelper.DeletingIds.AddRange((IEnumerable<string>) this._deleteForeverTaskIds);
      TaskChangeNotifier.NotifyTaskBatchDeletedChanged(this._deleteForeverTaskIds, false);
    }

    public override string GetTitle()
    {
      return this._taskIds.Count.ToString() + " " + Utils.GetString("CountTasks");
    }

    public override string GetContent() => Utils.GetString("Deleted");

    public override async void Undo()
    {
      BatchTaskDeleteUndo batchTaskDeleteUndo = this;
      await TaskService.UndoBatchDeletedTasks(batchTaskDeleteUndo._taskIds);
      // ISSUE: reference to a compiler-generated method
      UndoHelper.DeletingIds.RemoveAll(new Predicate<string>(batchTaskDeleteUndo.\u003CUndo\u003Eb__5_0));
      if (batchTaskDeleteUndo._deleteForeverTaskIds == null)
        return;
      await TaskService.UndoBatchDeletedTasks(batchTaskDeleteUndo._deleteForeverTaskIds);
      // ISSUE: reference to a compiler-generated method
      UndoHelper.DeletingIds.RemoveAll(new Predicate<string>(batchTaskDeleteUndo.\u003CUndo\u003Eb__5_1));
    }

    public override async void Finished()
    {
      BatchTaskDeleteUndo batchTaskDeleteUndo = this;
      await SyncStatusDao.BatchAddDeleteSyncStatus((IEnumerable<string>) batchTaskDeleteUndo._taskIds);
      // ISSUE: reference to a compiler-generated method
      UndoHelper.DeletingIds.RemoveAll(new Predicate<string>(batchTaskDeleteUndo.\u003CFinished\u003Eb__6_0));
      if (batchTaskDeleteUndo._deleteForeverTaskIds != null)
      {
        await SyncStatusDao.BatchAddDeleteForeverSyncStatus((IEnumerable<string>) batchTaskDeleteUndo._deleteForeverTaskIds);
        // ISSUE: reference to a compiler-generated method
        UndoHelper.DeletingIds.RemoveAll(new Predicate<string>(batchTaskDeleteUndo.\u003CFinished\u003Eb__6_1));
      }
      SyncManager.Sync();
    }
  }
}
