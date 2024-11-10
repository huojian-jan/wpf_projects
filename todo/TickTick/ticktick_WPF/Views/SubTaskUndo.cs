// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.SubTaskUndo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class SubTaskUndo : UndoController
  {
    private readonly TaskDetailItemModel _subTaskModel;

    public SubTaskUndo(TaskDetailItemModel subTaskModel) => this._subTaskModel = subTaskModel;

    public override string GetTitle() => Utils.GetString("OneCheckitem");

    public override string GetContent() => Utils.GetString("Deleted");

    public override async void Undo() => await TaskService.UndoDeletedCheckItem(this._subTaskModel);

    public override void Finished()
    {
    }
  }
}
