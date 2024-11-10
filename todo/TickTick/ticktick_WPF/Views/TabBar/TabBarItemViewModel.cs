// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TabBar.TabBarItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.TabBar
{
  public class TabBarItemViewModel : BaseViewModel
  {
    public const string Task = "Task";
    public const string Calendar = "Calendar";
    public const string Habit = "Habit";
    public const string Pomo = "Pomo";
    public const string Matrix = "Matrix";
    public const string Search = "Search";
    private string _toolTip;
    private bool _selected;
    private bool _show = true;
    private bool _dragging;
    private long _sortOrder;
    private string _guideText;
    public string Module;

    public Geometry Icon { get; set; }

    public string GuideText
    {
      get => this._guideText;
      set
      {
        this._guideText = value;
        this.OnPropertyChanged(nameof (GuideText));
      }
    }

    public string ToolTip
    {
      get => this._toolTip;
      set
      {
        this._toolTip = value;
        this.OnPropertyChanged("Tooltip");
      }
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public bool Show
    {
      get => this._show;
      set
      {
        if (this._show == value)
          return;
        this._show = value;
        this.OnPropertyChanged(nameof (Show));
      }
    }

    public bool Dragging
    {
      get => this._dragging;
      set
      {
        this._dragging = value;
        this.OnPropertyChanged(nameof (Dragging));
      }
    }

    public long SortOrder
    {
      get => this._sortOrder;
      set
      {
        if (this._sortOrder == value)
          return;
        this._sortOrder = value;
        this.OnPropertyChanged(nameof (SortOrder));
      }
    }

    public string Name { get; set; }

    public TabBarItemViewModel()
    {
    }

    public TabBarItemViewModel(string module, string selected)
    {
      this.Module = module;
      switch (module)
      {
        case nameof (Task):
          this.Icon = Utils.GetIcon("TaskViewPath");
          this.Name = Utils.GetString(nameof (Task));
          break;
        case nameof (Calendar):
          this.Icon = Utils.GetIcon("IcCalendarView");
          this.Name = Utils.GetString(nameof (Calendar));
          break;
        case nameof (Matrix):
          this.Icon = Utils.GetIcon("EMViewPath");
          this.Name = Utils.GetString(nameof (Matrix));
          break;
        case nameof (Habit):
          this.Icon = Utils.GetIcon("HabitViewPath");
          this.Name = Utils.GetString("HabitView");
          break;
        case nameof (Search):
          this.Icon = Utils.GetIcon("SearchViewPath");
          this.Name = Utils.GetString("QuickSearch");
          break;
        case nameof (Pomo):
          this.Icon = Utils.GetIcon("FocusViewPath");
          this.Name = Utils.GetString("PomoFocus");
          break;
      }
      this.Selected = module == selected;
    }

    public bool CanHide()
    {
      return this.Module == "Matrix" || this.Module == "Habit" || this.Module == "Pomo";
    }

    public bool CanOpenWindow() => this.Module != "Search";

    public bool IsWindowOpened()
    {
      switch (this.Module)
      {
        case "Calendar":
          return CalendarWindow.IsShowing;
        case "Matrix":
          return MatrixWindow.IsShowing;
        case "Habit":
          return HabitWindow.IsShowing;
        case "Task":
          return TaskWindow.IsShowing;
        case "Pomo":
          return FocusWindow.IsShowing;
        default:
          return false;
      }
    }

    public async void OpenOrCloseWindow(Window window)
    {
      await System.Threading.Tasks.Task.Delay(100);
      switch (this.Module)
      {
        case "Calendar":
          CalendarWindow.OpenOrCloseWindow(window);
          break;
        case "Matrix":
          MatrixWindow.OpenOrCloseWindow(window);
          break;
        case "Habit":
          HabitWindow.OpenOrCloseWindow(window);
          break;
        case "Pomo":
          FocusWindow.OpenOrCloseWindow(window);
          break;
        case "Task":
          TaskWindow.OpenOrCloseWindow(window);
          break;
      }
    }
  }
}
