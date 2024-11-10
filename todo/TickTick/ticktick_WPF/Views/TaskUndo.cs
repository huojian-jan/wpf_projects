// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskUndo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class TaskUndo : UndoController
  {
    private readonly TaskModel _task;
    private readonly string _title;
    private readonly string _content;
    private readonly List<TaskModel> _children;
    private readonly List<TaskDetailItemModel> _checkItems;

    public TaskUndo(TaskModel task, string title, string content)
    {
      UndoHelper.CanUndoIds.Clear();
      this._task = task;
      this._title = title;
      this._content = content;
      if (task == null)
        return;
      UndoHelper.CanUndoIds.Add(task.id);
    }

    public TaskUndo(
      TaskModel task,
      string title,
      string content,
      List<TaskModel> children,
      List<TaskDetailItemModel> checkItems)
      : this(task, title, content)
    {
      this._children = children;
      this._checkItems = checkItems;
      children?.ForEach((Action<TaskModel>) (c =>
      {
        if (c == null)
          return;
        UndoHelper.CanUndoIds.Add(c.id);
      }));
    }

    public override string GetTitle() => this._title;

    public override string GetContent() => this._content;

    public override async void Undo()
    {
      UndoHelper.UndoingIds.AddRange(UndoHelper.CanUndoIds);
      UndoHelper.CanUndoIds.Clear();
      CheckSyncStatus(this._task);
      TaskService.UndoTask(this._task, this._checkItems);
      this._children?.ForEach((Action<TaskModel>) (t =>
      {
        CheckSyncStatus(t);
        TaskService.UndoTask(t);
      }));
      UndoHelper.UndoingIds.Clear();

      static async Task CheckSyncStatus(TaskModel task)
      {
        if (task == null)
          return;
        foreach (SyncStatusModel syncStatusModel in await SyncStatusDao.GetSyncStatusById(task.id))
        {
          switch (syncStatusModel.Type)
          {
            case 2:
              if (syncStatusModel.MoveFromId == task.projectId)
                break;
              continue;
            case 16:
              if (!(syncStatusModel.OldParentId == task.parentId))
                continue;
              break;
            default:
              continue;
          }
          int num = await App.Connection.DeleteAsync((object) syncStatusModel);
        }
      }
    }

    public override void Finished()
    {
      UndoHelper.CanUndoIds.Clear();
      SyncManager.TryDelaySync();
    }
  }
}
