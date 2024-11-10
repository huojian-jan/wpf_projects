// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Undo.CloseUndoToast
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Undo
{
  public class CloseUndoToast : UndoToast
  {
    private CloseUndoModel _undo;

    public CloseUndoToast(CloseUndoModel undo, int closeStatus)
    {
      if (closeStatus == -1)
        this.DeletedOrCompleted.Text = Utils.GetString("Abandoned");
      else
        this.DeletedOrCompleted.Text = Utils.GetString("Completed");
      this.TitleText.Text = undo.GetUndoTitle();
      this._undo = undo;
    }

    public override void OnFinished() => this._undo.Finished();

    public override void OnUndo()
    {
      this.Visibility = Visibility.Collapsed;
      this._undo.Undo();
    }
  }
}
