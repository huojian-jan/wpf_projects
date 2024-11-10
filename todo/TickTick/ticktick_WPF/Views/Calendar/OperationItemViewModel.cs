// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.OperationItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class OperationItemViewModel : UpDownSelectViewModel
  {
    public ActionType Type { get; set; }

    public bool Enable { get; set; } = true;

    public List<OperationItemViewModel> SubActions { get; set; }

    public string Text { get; set; }

    public double ImageWidth { get; set; } = 24.0;

    public Thickness ImageMargin { get; set; } = new Thickness(8.0, 0.0, 0.0, 0.0);

    public DrawingImage Image { get; set; }

    public OperationItemViewModel(ActionType type)
    {
      this.Type = type;
      switch (type)
      {
        case ActionType.Archive:
          this.Image = Utils.GetImageSource("ArchiveDrawingImage");
          this.ImageWidth = 26.0;
          this.Text = Utils.GetString("Archive");
          this.ImageMargin = new Thickness(7.0, 0.0, 0.0, 0.0);
          break;
        case ActionType.CancelArchive:
          this.Image = Utils.GetImageSource("CancelArchiveDrawingImage");
          this.ImageWidth = 26.0;
          this.ImageMargin = new Thickness(7.0, 0.0, 0.0, 0.0);
          this.Text = Utils.GetString("CancelArchive");
          break;
        case ActionType.RecoverHabit:
          this.Image = Utils.GetImageSource("CancelArchiveDrawingImage");
          this.Text = Utils.GetString("RecoverHabit");
          break;
        case ActionType.Skip:
          this.Image = Utils.GetImageSource("skipDrawingImage");
          this.Text = Utils.GetString("SkipHabit");
          break;
        case ActionType.UnComplete:
          this.Image = Utils.GetImageSource("HabitUnCompleteDrawingImage");
          this.Text = Utils.GetString("Uncompleted");
          break;
        case ActionType.EditHabitLog:
          this.Image = Utils.GetImageSource("HabitLogDrawingImage");
          this.Text = Utils.GetString("EditLog");
          break;
        case ActionType.EditHabit:
          this.Image = Utils.GetImageSource("EditDrawingImage");
          this.Text = Utils.GetString("Edit");
          break;
        case ActionType.Delete:
          this.Image = Utils.GetImageSource("DeleteDrawingLine");
          this.Text = Utils.GetString("Delete");
          break;
        case ActionType.DeleteForever:
          this.Image = Utils.GetImageSource("DeleteForeverDrawingImage");
          this.Text = Utils.GetString("DeleteForever");
          break;
        case ActionType.Restore:
          this.Image = Utils.GetImageSource("RestoreDrawingImage");
          this.Text = Utils.GetString("Restore");
          break;
        case ActionType.StartFocus:
          this.Image = Utils.GetImageSource("FocusDrawingLine");
          this.Text = Utils.GetString("StartFocus");
          break;
        case ActionType.StartTiming:
          this.ImageWidth = 0.0;
          this.Text = Utils.GetString("StartTiming");
          break;
        case ActionType.StartPomo:
          this.ImageWidth = 0.0;
          this.Text = Utils.GetString("StartPomo");
          break;
      }
    }
  }
}
