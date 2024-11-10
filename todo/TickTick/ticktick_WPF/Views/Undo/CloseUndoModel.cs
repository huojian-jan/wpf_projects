// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Undo.CloseUndoModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Undo
{
  public class CloseUndoModel : IUndoModel
  {
    private List<TaskModel> _originTasks = new List<TaskModel>();
    private List<TaskModel> _newTasks = new List<TaskModel>();
    public string UndoId;
    private TaskDetailItemModel _originItem;
    private Timer _timer;
    private bool _handled;

    public bool Fired { get; set; }

    public void InitModel(string taskId, TaskModel task)
    {
      this._originTasks.Clear();
      this._originTasks.Add(task);
      this.UndoId = taskId;
    }

    public void InitModel(string taskId, TaskDetailItemModel item)
    {
      this._originTasks.Clear();
      this._originItem = item;
      this.UndoId = taskId;
    }

    public void AddOriginTasks(List<TaskModel> tasks)
    {
      this._originTasks.AddRange((IEnumerable<TaskModel>) tasks);
    }

    public void AddOriginTask(TaskModel task) => this._originTasks.Add(task);

    public void AddNewTasks(List<TaskModel> tasks)
    {
      this._newTasks.AddRange((IEnumerable<TaskModel>) tasks);
    }

    public async void Undo()
    {
      if (this.Fired)
        return;
      this.TryFire();
      List<string> newTaskIds = this._newTasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>();
      TaskModel newTask;
      foreach (TaskModel newTask1 in this._newTasks)
      {
        newTask = newTask1;
        if (!string.IsNullOrEmpty(newTask.parentId))
        {
          if (!newTaskIds.Contains(newTask.parentId))
            await TaskDao.AddOrRemoveTaskChildIds(newTask.parentId, new List<string>()
            {
              newTask.id
            }, false);
          SyncStatusDao.DeleteSyncStatus(newTask.id, 16, false);
        }
        if (newTask.kind == "CHECKLIST")
        {
          List<TaskDetailItemModel> taskDetailItemModelList1 = await TaskDetailItemDao.DeleteCheckItemsByTaskId(newTask.repeatTaskId);
          await ProjectCopyManager.CopyChecklistItems(newTask.id, newTask.repeatTaskId);
          List<TaskDetailItemModel> taskDetailItemModelList2 = await TaskDetailItemDao.DeleteCheckItemsByTaskId(newTask.id);
        }
        await TaskDao.DeleteTaskInDb(newTask.id);
        newTask = (TaskModel) null;
      }
      if (this._originItem != null)
      {
        TaskDetailItemModel checkItemById = await TaskDetailItemDao.GetCheckItemById(this._originItem.id);
        checkItemById.status = 0;
        checkItemById.completedTime = new DateTime?();
        await TaskDetailItemDao.SaveChecklistItem(checkItemById);
      }
      List<TaskBaseViewModel> vms = new List<TaskBaseViewModel>();
      foreach (TaskModel originTask in this._originTasks)
      {
        newTask = originTask;
        TaskModel task = await TaskDao.GetThinTaskById(newTask.id);
        task.status = newTask.status;
        task.completedTime = newTask.completedTime;
        task.startDate = newTask.startDate;
        task.dueDate = newTask.dueDate;
        task.repeatFlag = newTask.repeatFlag;
        task.repeatFrom = newTask.repeatFrom;
        task.repeatFirstDate = newTask.repeatFirstDate;
        if (newTask.parentId != task.parentId)
        {
          await TaskDao.UpdateParent(task.id, newTask.parentId);
          task.parentId = newTask.parentId;
        }
        if (task.id == this._originItem?.TaskServerId)
          task.progress = new int?(TaskService.CalculateProgress((IReadOnlyCollection<TaskDetailItemModel>) await TaskDetailItemDao.GetCheckItemsByTaskId(task.id)));
        vms.Add(await TaskService.UpdateTaskOnCompleteUndo(task));
        task = (TaskModel) null;
        newTask = (TaskModel) null;
      }
      ProjectAndTaskIdsCache.OnTasksChanged(vms, CheckMatchedType.All);
      TasksChangeEventArgs args = new TasksChangeEventArgs();
      args.DeletedChangedIds.AddRange((IEnumerable<string>) newTaskIds);
      List<string> list = vms.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (v => v.GetTaskId())).ToList<string>();
      args.DateChangedIds.AddRange((IEnumerable<string>) list);
      args.StatusChangedIds.AddRange((IEnumerable<string>) list);
      TaskChangeNotifier.NotifyTaskChanged(args);
      this._handled = true;
      newTaskIds = (List<string>) null;
      vms = (List<TaskBaseViewModel>) null;
    }

    public async void Finished()
    {
      if (string.IsNullOrEmpty(this.UndoId) || this.Fired)
        return;
      this.TryFire();
      foreach (TaskModel originTask in this._originTasks)
        await SyncStatusDao.AddSyncStatus(originTask.id, 0);
      foreach (TaskModel newTask in this._newTasks)
        await SyncStatusDao.AddSyncStatus(newTask.id, 4);
      SyncManager.Sync();
      this._handled = true;
    }

    private void TryFire()
    {
      this.Fired = true;
      this._timer?.Close();
    }

    public string GetUndoTitle()
    {
      if (this._originItem != null)
        return this._originItem.title;
      return this._originTasks.Count > 0 && this._originTasks.Count == 1 ? this._originTasks[0].title : string.Empty;
    }

    public void StartTimer()
    {
      this._timer = new Timer(6000.0);
      this._timer.Elapsed += new ElapsedEventHandler(this.TryDo);
      this._timer.Start();
    }

    private void TryDo(object sender, ElapsedEventArgs e)
    {
      Utils.RunOnUiThread(Application.Current?.Dispatcher, new Action(this.Finished));
      this._timer?.Stop();
      this._timer?.Close();
    }

    public List<string> GetAffectedTaskIds()
    {
      if (this._handled)
        return new List<string>();
      List<string> list = this._originTasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>();
      list.AddRange((IEnumerable<string>) this._newTasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>());
      return list;
    }
  }
}
