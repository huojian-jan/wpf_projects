// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Undo.TaskDragUndoModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Undo
{
  public class TaskDragUndoModel : IUndoModel
  {
    private string _undoId;
    private List<TaskDragUndoModel.TaskDragModel> _dragModels = new List<TaskDragUndoModel.TaskDragModel>();
    private bool _fired;
    public static List<TaskDragUndoModel> DragUndoModels = new List<TaskDragUndoModel>();

    private TaskDragUndoModel()
    {
    }

    private void TryDo() => this.Finished();

    public static bool TryUndo(string undoId)
    {
      TaskDragUndoModel taskDragUndoModel = TaskDragUndoModel.DragUndoModels.FirstOrDefault<TaskDragUndoModel>((Func<TaskDragUndoModel, bool>) (m => m._undoId == undoId && !m._fired));
      taskDragUndoModel?.Undo();
      return taskDragUndoModel != null;
    }

    public static void AddDragModel(
      string undoId,
      string taskId,
      string oldParentId,
      string newParentId,
      string oldProjectId,
      string newProjectId,
      string oldAssign,
      long oldSort)
    {
      TaskDragUndoModel taskDragUndoModel = TaskDragUndoModel.DragUndoModels.FirstOrDefault<TaskDragUndoModel>((Func<TaskDragUndoModel, bool>) (m => m._undoId == undoId));
      if (taskDragUndoModel != null && taskDragUndoModel._fired)
      {
        TaskDragUndoModel.DragUndoModels.Remove(taskDragUndoModel);
        taskDragUndoModel = (TaskDragUndoModel) null;
      }
      if (taskDragUndoModel == null)
      {
        taskDragUndoModel = new TaskDragUndoModel()
        {
          _undoId = undoId
        };
        TaskDragUndoModel.DragUndoModels.Add(taskDragUndoModel);
      }
      TaskDragUndoModel.TaskDragModel taskDragModel = new TaskDragUndoModel.TaskDragModel()
      {
        TaskId = taskId,
        OldParentId = oldParentId,
        OldProjectId = oldProjectId,
        NewProjectId = newProjectId,
        NewParentId = newParentId,
        OldAssignee = oldAssign,
        OldSortOrder = oldSort
      };
      taskDragUndoModel._dragModels.Add(taskDragModel);
    }

    public async void Undo()
    {
      TaskDragUndoModel taskDragUndoModel = this;
      if (taskDragUndoModel._fired)
        return;
      taskDragUndoModel._fired = true;
      TaskDragUndoModel.DragUndoModels.Remove(taskDragUndoModel);
      string projectName = "";
      string taskName = "";
      HashSet<string> taskIds = new HashSet<string>();
      foreach (TaskDragUndoModel.TaskDragModel dragModel in taskDragUndoModel._dragModels)
      {
        TaskDragUndoModel.TaskDragModel model = dragModel;
        TaskModel task = await TaskDao.GetThinTaskById(model.TaskId);
        if (task != null)
        {
          taskIds.Add(task.id);
          taskName = task.title.Trim();
          if (!string.IsNullOrEmpty(model.OldProjectId) && model.OldProjectId != model.NewProjectId)
          {
            task.projectId = model.OldProjectId;
            task.assignee = model.OldAssignee;
            task.sortOrder = model.OldSortOrder;
            projectName = string.IsNullOrEmpty(projectName) ? CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.OldProjectId))?.name : projectName;
            CommentDao.ChangeCommentProjectId(task, model.OldParentId);
          }
          if (model.OldParentId != model.NewParentId)
          {
            task.parentId = model.OldParentId;
            if (!string.IsNullOrEmpty(model.NewParentId))
              await TaskDao.AddOrRemoveTaskChildIds(model.NewParentId, new List<string>()
              {
                model.TaskId
              }, false);
          }
          await TaskService.UpdateTaskOnMoveUndo(task, CheckMatchedType.All);
          task = (TaskModel) null;
        }
      }
      Utils.Toast(string.Format(Utils.GetString("MoveTaskToProject"), taskIds.Count > 1 ? (object) (taskIds.Count.ToString() + Utils.GetString("CountTasks")) : (object) taskName, (object) projectName));
      projectName = (string) null;
      taskName = (string) null;
      taskIds = (HashSet<string>) null;
    }

    public async void Finished()
    {
      TaskDragUndoModel taskDragUndoModel = this;
      if (taskDragUndoModel._fired)
        return;
      taskDragUndoModel._fired = true;
      TaskDragUndoModel.DragUndoModels.Remove(taskDragUndoModel);
      foreach (TaskDragUndoModel.TaskDragModel dragModel in taskDragUndoModel._dragModels)
      {
        TaskDragUndoModel.TaskDragModel model = dragModel;
        TaskModel task = await TaskDao.GetThinTaskById(model.TaskId);
        if (task != null)
        {
          if (!string.IsNullOrEmpty(model.OldProjectId) && model.OldProjectId != model.NewProjectId)
          {
            await SyncStatusDao.AddMoveOrRestoreProjectStatus(task.id, model.OldProjectId);
            await SyncStatusDao.AddModifySyncStatus(task.id);
          }
          if (model.OldParentId != model.NewParentId)
          {
            await SyncStatusDao.AddSetParentSyncStatus(task.id, model.OldParentId);
            if (!string.IsNullOrEmpty(model.OldParentId))
              await TaskDao.AddOrRemoveTaskChildIds(model.OldParentId, new List<string>()
              {
                model.TaskId
              }, false);
          }
          task = (TaskModel) null;
          model = (TaskDragUndoModel.TaskDragModel) null;
        }
      }
      SyncManager.Sync();
    }

    public static bool CheckUndoIdExist(string undoId)
    {
      return TaskDragUndoModel.DragUndoModels.FirstOrDefault<TaskDragUndoModel>((Func<TaskDragUndoModel, bool>) (m => m._undoId == undoId && !m._fired)) != null;
    }

    public static void TryFinishAll()
    {
      try
      {
        foreach (TaskDragUndoModel dragUndoModel in TaskDragUndoModel.DragUndoModels)
          dragUndoModel.Finished();
        SyncManager.TryDelaySync();
      }
      catch (Exception ex)
      {
      }
    }

    private class TaskDragModel
    {
      public string TaskId { get; set; }

      public string OldParentId { get; set; }

      public string NewParentId { get; set; }

      public string OldProjectId { get; set; }

      public string NewProjectId { get; set; }

      public string OldAssignee { get; set; }

      public long OldSortOrder { get; set; }
    }
  }
}
