// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.DeleteRecurrenceUndo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class DeleteRecurrenceUndo : UndoController
  {
    private readonly TaskDeleteRecurrenceUndoEntity _undoEntity;

    public DeleteRecurrenceUndo(TaskDeleteRecurrenceUndoEntity undoEntity)
    {
      this._undoEntity = undoEntity;
    }

    public override string GetTitle() => string.Empty;

    public override string GetContent() => Utils.GetString("Deleted");

    public override async void Undo() => TaskService.UndoDeleteTaskRecurrence(this._undoEntity);

    public override async void Finished()
    {
    }
  }
}
