// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.DayViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class DayViewModel : BaseViewModel
  {
    private static readonly Geometry ShowWorkRestEllipsePath = (Geometry) new CombinedGeometry()
    {
      GeometryCombineMode = GeometryCombineMode.Exclude,
      Geometry1 = (Geometry) new System.Windows.Media.EllipseGeometry()
      {
        Center = new Point(15.0, 15.0),
        RadiusX = 15.0,
        RadiusY = 15.0
      },
      Geometry2 = (Geometry) new System.Windows.Media.EllipseGeometry()
      {
        Center = new Point(25.0, 5.0),
        RadiusX = 6.0,
        RadiusY = 6.0
      }
    };
    private static readonly Geometry HideWorkRestEllipsePath = (Geometry) new System.Windows.Media.EllipseGeometry()
    {
      Center = new Point(15.0, 15.0),
      RadiusX = 15.0,
      RadiusY = 15.0
    };
    private DateTime _date;
    private bool _hasTasks;
    private bool _hover;
    private bool _isRepeat;
    private bool _selected;
    private bool _canselect = true;
    private bool _isCurrentWeek;
    private Geometry _ellipseGeometry;
    private double _tabBorderThickness;
    private bool _useInSlideMenu;
    private SelectionMode _selection;
    private ShowMode _showMode = ShowMode.CurrentMonth;
    private bool _isFixed;
    private FrameworkElement _context;

    public DayViewModel(DateTime date, FrameworkElement context)
    {
      this.Date = date;
      this._context = context;
      this.SetHoliday();
    }

    public void SetHoliday()
    {
      List<HolidayModel> source = !LocalSettings.Settings.EnableHoliday || !Utils.IsDida() ? new List<HolidayModel>() : HolidayManager.GetCacheHolidays();
      HolidayModel holidayModel = source != null ? source.FirstOrDefault<HolidayModel>((Func<HolidayModel, bool>) (h => h.date == this.Date.Date)) : (HolidayModel) null;
      this.EllipseGeometry = holidayModel == null ? DayViewModel.HideWorkRestEllipsePath : DayViewModel.ShowWorkRestEllipsePath;
      if (holidayModel != null)
      {
        this.IsWork = holidayModel.type == 1;
        this.IsRest = holidayModel.type == 0;
      }
      else
      {
        this.IsWork = false;
        this.IsRest = false;
      }
      this.OnPropertyChanged("IsRest");
    }

    public bool IsRest { get; set; }

    public bool IsWork { get; set; }

    public double WorkTextOpacity
    {
      get
      {
        return !this.Selected && this.ShowMode != ShowMode.CurrentMonth && this.ShowMode != ShowMode.Today ? 0.4 : 1.0;
      }
    }

    public SolidColorBrush EllipseColor
    {
      get
      {
        if (this.IsFixed)
          return Brushes.Transparent;
        if (this.Selected)
          return !this.UseInSlideMenu ? ThemeUtil.GetColor("PrimaryColor", this._context) : ThemeUtil.GetDateSelectedColorInMenu(this._context);
        if (this.Hover || this.IsRepeat)
          return !this.UseInSlideMenu ? ThemeUtil.GetColor("BaseColorOpacity5", this._context) : ThemeUtil.GetColor("ProjectSelectedBackground", this._context);
        if (this.ShowMode != ShowMode.Today)
          return Brushes.Transparent;
        return !this.UseInSlideMenu ? ThemeUtil.GetColor("PrimaryColor10", this._context) : ThemeUtil.GetColor("ProjectSelectedBackground", this._context);
      }
    }

    public SolidColorBrush DateTextBrush
    {
      get
      {
        if (this.IsFixed)
          return this.GetDefaultTextColor();
        if (!this.CanSelect)
          return this.GetMuteTextColor();
        if (this.Selected)
          return Brushes.White;
        return this.ShowMode == ShowMode.Today ? (!this.UseInSlideMenu ? ThemeUtil.GetColor("TextAccentColor", this._context) : ThemeUtil.GetTodayColorInMenu(this._context)) : (this.Hover || this.ShowMode == ShowMode.CurrentMonth || this.ShowMode == ShowMode.CurrentMonth ? this.GetDefaultTextColor() : this.GetMuteTextColor());
      }
    }

    private SolidColorBrush GetMuteTextColor()
    {
      return ThemeUtil.GetColor(this._useInSlideMenu ? "ProjectSectionColor" : "BaseColorOpacity40", this._context);
    }

    private SolidColorBrush GetDefaultTextColor()
    {
      return ThemeUtil.GetColor(this._useInSlideMenu ? "ProjectMenuColorOpacity100_80" : "BaseColorOpacity100_80", this._context);
    }

    public void NotifyColorChanged()
    {
      this.OnPropertyChanged("EllipseColor");
      this.OnPropertyChanged("DateTextBrush");
    }

    public Geometry EllipseGeometry
    {
      get => this._ellipseGeometry;
      set
      {
        this._ellipseGeometry = value;
        this.OnPropertyChanged(nameof (EllipseGeometry));
      }
    }

    public double TabBorderThickness
    {
      get => this._tabBorderThickness;
      set
      {
        this._tabBorderThickness = value;
        this.OnPropertyChanged(nameof (TabBorderThickness));
      }
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
        this.OnPropertyChanged("EllipseColor");
        this.OnPropertyChanged("DateTextBrush");
        this.OnPropertyChanged("WorkTextOpacity");
        this.OnPropertyChanged("ShowLunarMonthFirstDay");
      }
    }

    public bool CanSelect
    {
      get => this._canselect;
      set
      {
        this._canselect = value;
        this.OnPropertyChanged(nameof (CanSelect));
        this.OnPropertyChanged("EllipseColor");
        this.OnPropertyChanged("DateTextBrush");
        this.OnPropertyChanged("WorkRestOpacity");
      }
    }

    public bool CanDoubleSelect { get; set; } = true;

    public bool LunarMonthFirstDay { get; set; }

    public bool ShowLunarMonthFirstDay => this.LunarMonthFirstDay && !this.Selected;

    public string LunarText { get; set; }

    public DateTime Date
    {
      get => this._date;
      set
      {
        this._date = value;
        this.OnPropertyChanged(nameof (Date));
      }
    }

    public bool IsFixed
    {
      get => this._isFixed;
      set
      {
        this._isFixed = value;
        this.OnPropertyChanged("EllipseColor");
        this.OnPropertyChanged("DateTextBrush");
      }
    }

    public ShowMode ShowMode
    {
      get => this._showMode;
      set
      {
        this._showMode = value;
        this.OnPropertyChanged("WorkRestOpacity");
      }
    }

    public SelectionMode Selection
    {
      get => this._selection;
      set
      {
        this._selection = value;
        this.OnPropertyChanged(nameof (Selection));
      }
    }

    public bool IsRepeat
    {
      get => this._isRepeat;
      set
      {
        this._isRepeat = value;
        this.OnPropertyChanged("EllipseColor");
        this.OnPropertyChanged("DateTextBrush");
      }
    }

    public bool Hover
    {
      get => this._hover;
      set
      {
        this._hover = value;
        this.OnPropertyChanged("EllipseColor");
        this.OnPropertyChanged("DateTextBrush");
      }
    }

    public bool HasTasks
    {
      get => this._hasTasks;
      set
      {
        this._hasTasks = value;
        this.OnPropertyChanged(nameof (HasTasks));
      }
    }

    public bool UseInSlideMenu
    {
      get => this._useInSlideMenu;
      set
      {
        this._useInSlideMenu = value;
        this.OnPropertyChanged(nameof (UseInSlideMenu));
      }
    }

    public double WorkRestOpacity
    {
      get
      {
        return !this.CanSelect || this.ShowMode != ShowMode.Today && !this.Selected && this.ShowMode != ShowMode.CurrentMonth && this.ShowMode != ShowMode.CurrentMonth ? 0.4 : 1.0;
      }
    }
  }
}
