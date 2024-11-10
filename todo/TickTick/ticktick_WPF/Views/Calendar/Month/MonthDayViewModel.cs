// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Month.MonthDayViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Month
{
  public class MonthDayViewModel : BaseViewModel
  {
    private static readonly Geometry ShowWorkRestEllipsePath = (Geometry) new CombinedGeometry()
    {
      GeometryCombineMode = GeometryCombineMode.Exclude,
      Geometry1 = (Geometry) new System.Windows.Media.EllipseGeometry()
      {
        Center = new Point(12.0, 12.0),
        RadiusX = 12.0,
        RadiusY = 12.0
      },
      Geometry2 = (Geometry) new System.Windows.Media.EllipseGeometry()
      {
        Center = new Point(24.0, 3.0),
        RadiusX = 6.0,
        RadiusY = 6.0
      }
    };
    private static readonly Geometry HideWorkRestEllipsePath = (Geometry) new System.Windows.Media.EllipseGeometry()
    {
      Center = new Point(12.0, 12.0),
      RadiusX = 12.0,
      RadiusY = 12.0
    };
    private MonthCellState _state;
    private DateTime _date;
    private double _textWidth = 14.0;

    public DateTime Date
    {
      get => this._date;
      set
      {
        this._date = value;
        this.OnPropertyChanged(nameof (Date));
        if (this.Date.Day != 1)
          return;
        this._textWidth = Utils.MeasureString(DateUtils.FormatShortMonthDay(this.Date), new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 14.0).Width;
      }
    }

    public bool IsToday { get; set; }

    public bool IsWeekend { get; set; }

    public bool IsCurrentMonth { get; set; }

    public bool IsPast => this.Date.Date < DateTime.Today;

    public bool ShowMonthday { get; set; }

    public MonthCellState State
    {
      get => this._state;
      set
      {
        this._state = value;
        this.OnPropertyChanged(nameof (State));
      }
    }

    public bool IsWorkDay { get; set; }

    public bool IsRestDay { get; set; }

    public bool DayMode { get; set; }

    public double WorkRestOpacity => this.IsToday || this.IsCurrentMonth ? 1.0 : 0.4;

    public double IconWidth
    {
      get => this.ShowMonthday && this.Date.Day == 1 ? this._textWidth + 10.0 : 24.0;
    }

    public Geometry EllipseGeometry
    {
      get
      {
        if (this.ShowMonthday && this.Date.Day == 1)
        {
          if (this.IsWorkDay || this.IsRestDay)
            return (Geometry) new CombinedGeometry()
            {
              GeometryCombineMode = GeometryCombineMode.Exclude,
              Geometry1 = (Geometry) new RectangleGeometry()
              {
                RadiusX = 12.0,
                RadiusY = 12.0,
                Rect = new Rect(0.0, 0.0, this.IconWidth, 24.0)
              },
              Geometry2 = (Geometry) new System.Windows.Media.EllipseGeometry()
              {
                Center = new Point(this.IconWidth, 6.0),
                RadiusX = 7.0,
                RadiusY = 7.0
              }
            };
          return (Geometry) new RectangleGeometry()
          {
            RadiusX = 12.0,
            RadiusY = 12.0,
            Rect = new Rect(0.0, 0.0, this.IconWidth, 24.0)
          };
        }
        return !this.IsWorkDay && !this.IsRestDay ? MonthDayViewModel.HideWorkRestEllipsePath : MonthDayViewModel.ShowWorkRestEllipsePath;
      }
    }

    public void SetDate(DateTime date)
    {
      if (!(date != this.Date))
        return;
      this.Date = date;
      List<HolidayModel> holidayModelList = !LocalSettings.Settings.EnableHoliday || !Utils.IsDida() ? new List<HolidayModel>() : HolidayManager.GetCacheHolidays();
      bool flag1 = this.Date.DayOfWeek == DayOfWeek.Saturday || this.Date.DayOfWeek == DayOfWeek.Sunday;
      bool flag2 = holidayModelList.Exists((Predicate<HolidayModel>) (day => day.date == this.Date && day.type == 1)) && SettingsHelper.GetShowHoliday();
      bool flag3 = holidayModelList.Exists((Predicate<HolidayModel>) (day => day.date == this.Date && day.type == 0)) && SettingsHelper.GetShowHoliday();
      this.IsToday = this.Date == DateTime.Today;
      this.IsWeekend = flag1;
      this.IsWorkDay = flag2;
      this.IsRestDay = flag3;
    }

    public void AddDays(int days) => this.SetDate(this.Date.AddDays((double) days));

    public void SetCurrentMonth(DateTime currentMonth)
    {
      if (this.Date.Month == currentMonth.Month == this.IsCurrentMonth)
        return;
      this.IsCurrentMonth = this.Date.Month == currentMonth.Month;
      this.OnPropertyChanged("IsCurrentMonth");
    }
  }
}
