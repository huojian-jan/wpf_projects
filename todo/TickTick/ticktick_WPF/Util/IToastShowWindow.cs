// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.IToastShowWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Views;
using ticktick_WPF.Views.Undo;

#nullable disable
namespace ticktick_WPF.Util
{
  public interface IToastShowWindow
  {
    void TaskDeleted(string taskId);

    void TryToastString(object sender, string e);

    Task<bool> BatchDeleteTask(List<TaskModel> tasks);

    void TaskComplete(CloseUndoToast undo);

    void TryHideToast();

    void ToastDeleteRecUndo(List<TaskDeleteRecurrenceUndoEntity> undoModels);

    void ToastMoveProjectControl(string projectId, string taskName = null, MoveToastType moveType = MoveToastType.Move);

    void Toast(FrameworkElement uiElement);
  }
}
