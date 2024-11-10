// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Undo.CloseUndoHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Undo
{
  public static class CloseUndoHandler
  {
    private static CloseUndoModel _undoModel = new CloseUndoModel();

    public static void AddUndoModel(string taskId, TaskModel task)
    {
      CloseUndoHandler._undoModel.Finished();
      CloseUndoHandler._undoModel = new CloseUndoModel();
      CloseUndoHandler._undoModel.InitModel(taskId, task);
    }

    public static void AddUndoCheckItem(string taskId, TaskDetailItemModel item)
    {
      CloseUndoHandler._undoModel.Finished();
      CloseUndoHandler._undoModel = new CloseUndoModel();
      CloseUndoHandler._undoModel.InitModel(taskId, item);
    }

    public static void AddUndoOriginTasks(string taskId, List<TaskModel> tasks)
    {
      if (!(CloseUndoHandler._undoModel.UndoId == taskId) || CloseUndoHandler._undoModel.Fired)
        return;
      CloseUndoHandler._undoModel.AddOriginTasks(tasks);
    }

    public static void AddUndoOriginTask(string taskId, TaskModel task)
    {
      if (!(CloseUndoHandler._undoModel.UndoId == taskId) || CloseUndoHandler._undoModel.Fired)
        return;
      CloseUndoHandler._undoModel.AddOriginTask(task);
    }

    public static List<string> GetAffectedTaskIds()
    {
      return CloseUndoHandler._undoModel.GetAffectedTaskIds();
    }

    public static void AddUndoNewTasks(string taskId, List<TaskModel> tasks)
    {
      if (!(CloseUndoHandler._undoModel.UndoId == taskId) || CloseUndoHandler._undoModel.Fired)
        return;
      CloseUndoHandler._undoModel.AddNewTasks(tasks);
    }

    public static void TryToast(IToastShowWindow window, int closeStatus)
    {
      if (CloseUndoHandler._undoModel.Fired)
        return;
      CloseUndoToast undo = new CloseUndoToast(CloseUndoHandler._undoModel, closeStatus);
      window.TaskComplete(undo);
    }

    public static bool TryUndoInDetail(string taskId)
    {
      if (!(CloseUndoHandler._undoModel.UndoId == taskId) || CloseUndoHandler._undoModel.Fired)
        return false;
      CloseUndoHandler._undoModel.Undo();
      return true;
    }

    public static void TryStartTimer()
    {
      if (CloseUndoHandler._undoModel.Fired)
        return;
      CloseUndoHandler._undoModel.StartTimer();
    }
  }
}
