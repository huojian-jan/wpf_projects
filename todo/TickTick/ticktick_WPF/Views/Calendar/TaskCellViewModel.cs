// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TaskCellViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TaskCellViewModel : CalendarDisplayViewModel
  {
    private bool _dragging;
    private double _height;
    private bool _hideIndicator = true;
    private double _horizontalOffset;
    private string _textColor;
    private double _verticalOffset;
    private double _width;
    public bool NewAdd;
    private TaskCell _taskCell;

    public bool ShowDragHandle
    {
      get
      {
        return (this.Type == DisplayType.Task || this.Type == DisplayType.Derivative) && !this.NewAdd && this.Type != DisplayType.Note && this.CanDrag;
      }
    }

    public string Identity
    {
      get
      {
        string id = this.SourceViewModel.Id;
        DateTime? displayStartDate = this.DisplayStartDate;
        ref DateTime? local = ref displayStartDate;
        string str = local.HasValue ? local.GetValueOrDefault().ToString("_ddHHmm") : (string) null;
        return id + str;
      }
    }

    public double VerticalOffset
    {
      get => this._verticalOffset;
      set
      {
        this._verticalOffset = value;
        this.OnPropertyChanged("Margin");
      }
    }

    public Thickness Margin
    {
      get
      {
        return new Thickness((double) (2 + (int) this._horizontalOffset), (double) (int) this._verticalOffset, 0.0, 0.0);
      }
    }

    public double HorizontalOffset
    {
      get => this._horizontalOffset;
      set
      {
        this._horizontalOffset = value;
        this.OnPropertyChanged("Margin");
      }
    }

    public double Width
    {
      get => this._width;
      set
      {
        this._width = value;
        this.OnPropertyChanged(nameof (Width));
      }
    }

    public double Height
    {
      get => this._height;
      set
      {
        this._height = Math.Max(value, CalendarGeoHelper.MinHeight);
        this.OnPropertyChanged(nameof (Height));
      }
    }

    public TaskCellViewModel() => this.ShowSpanTimeText = true;

    public static TaskCellViewModel Build(CalendarDisplayModel model, DateTime targetDate)
    {
      DateTime? displayStartDate = model.DisplayStartDate;
      string str = model.CourseDisplayModel != null ? model.SourceViewModel.Color : CalendarDisplayViewModel.TryGetColor(model.SourceViewModel.Color, model.SourceViewModel.Tag, model.Priority);
      TaskCellViewModel taskCellViewModel = new TaskCellViewModel();
      taskCellViewModel.Color = str;
      taskCellViewModel.RepeatDiff = model.repeatDiff;
      taskCellViewModel.BaseOnStart = !displayStartDate.HasValue || targetDate.Date == displayStartDate.Value.Date;
      taskCellViewModel.Course = model.CourseDisplayModel;
      taskCellViewModel.TimeText = model.CourseDisplayModel?.Room;
      taskCellViewModel.SetSourceModel(model.SourceViewModel);
      taskCellViewModel.SetIcon();
      return taskCellViewModel;
    }

    public DateTime? BaseDate => !this.BaseOnStart ? this.DisplayDueDate : this.DisplayStartDate;

    public bool BaseOnStart { get; set; }

    public bool BarMode { get; set; }

    public bool IsCourse => this.Course != null;

    public int ZIndex { get; set; }

    public static TaskCellViewModel Copy(TaskCellViewModel model)
    {
      TaskCellViewModel taskCellViewModel = new TaskCellViewModel();
      taskCellViewModel.SourceViewModel = model.SourceViewModel.Copy();
      taskCellViewModel.Color = model.Color;
      taskCellViewModel.Width = model.Width;
      taskCellViewModel.Height = model.Height;
      taskCellViewModel.VerticalOffset = model.VerticalOffset;
      taskCellViewModel.HorizontalOffset = model.HorizontalOffset;
      taskCellViewModel.Icon = model.Icon;
      taskCellViewModel.BackColor = model.BackColor;
      taskCellViewModel.TitleColor = model.TitleColor;
      taskCellViewModel.BackBorderColor = model.BackBorderColor;
      taskCellViewModel.BackBorderThickness = model.BackBorderThickness;
      taskCellViewModel.BaseOnStart = model.BaseOnStart;
      taskCellViewModel.DragMode = model.DragMode;
      taskCellViewModel.BarMode = model.BarMode;
      taskCellViewModel.RepeatDiff = model.RepeatDiff;
      return taskCellViewModel;
    }

    private static DisplayType ConvertDisplayType(int type)
    {
      switch (type)
      {
        case 0:
          return DisplayType.Task;
        case 1:
          return DisplayType.CheckItem;
        case 2:
          return DisplayType.Derivative;
        case 3:
          return DisplayType.Event;
        case 5:
          return DisplayType.Note;
        case 6:
          return DisplayType.Pomo;
        default:
          return DisplayType.Task;
      }
    }

    public void GetColor(bool dark)
    {
      this.Color = CalendarDisplayViewModel.TryGetColor(this.SourceViewModel.Color, this.SourceViewModel.Tag, this.SourceViewModel.Priority);
      this.SetTheme(dark);
    }
  }
}
