// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.TaskDragHelpModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class TaskDragHelpModel : BaseViewModel
  {
    private bool _isDragging;

    private TaskDragHelpModel()
    {
    }

    public static TaskDragHelpModel DragHelp { get; set; } = new TaskDragHelpModel();

    public bool IsDragging
    {
      get => this._isDragging;
      set
      {
        this._isDragging = value;
        this.OnPropertyChanged(nameof (IsDragging));
      }
    }
  }
}
