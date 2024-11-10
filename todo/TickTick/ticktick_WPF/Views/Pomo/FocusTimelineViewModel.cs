// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusTimelineViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusTimelineViewModel : BaseViewModel
  {
    private string _time;
    private string _date;
    private double _lineWidth;
    private double _timeWidth = 16.0;
    private double _height = 20.0;
    private bool _isZero;
    private Thickness _margin = new Thickness(0.0, 0.0, 0.0, 40.0);

    public bool LastOne { get; set; }

    public bool IsZero
    {
      get => this._isZero;
      set
      {
        this._isZero = value;
        this.OnPropertyChanged(nameof (IsZero));
      }
    }

    public string Date
    {
      get => this._date;
      set
      {
        this._date = value;
        this.OnPropertyChanged(nameof (Date));
      }
    }

    public string Time
    {
      get => this._time;
      set
      {
        this._time = value;
        this.OnPropertyChanged(nameof (Time));
      }
    }

    public double LineWidth
    {
      get => this._lineWidth;
      set
      {
        this._lineWidth = value;
        this.OnPropertyChanged(nameof (LineWidth));
      }
    }

    public double TimeWidth
    {
      get => this._timeWidth;
      set
      {
        this._timeWidth = value;
        this.OnPropertyChanged(nameof (TimeWidth));
      }
    }

    public double Height
    {
      get => this._height;
      set
      {
        this._height = value;
        this.OnPropertyChanged(nameof (Height));
      }
    }

    public Thickness Margin
    {
      get => this._margin;
      set
      {
        this._margin = value;
        this.OnPropertyChanged(nameof (Margin));
      }
    }

    private FocusTimelineViewModel(DateTime date) => this.SetHour(date);

    public void SetHour(DateTime date)
    {
      if (LocalSettings.Settings.TimeFormat == "24Hour")
      {
        this.TimeWidth = 36.0;
        this.Time = date.Hour.ToString() ?? "";
      }
      else
      {
        this.TimeWidth = 58.0;
        int hour = date.Hour;
        int num = hour % 12;
        if (num == 0)
          num = 12;
        this.Time = num.ToString() + " " + (hour >= 12 || hour < 0 ? "PM" : "AM");
      }
      if (date.Hour == 0)
      {
        this.IsZero = true;
        this.Date = date.ToString("MM'.'dd");
      }
      else
      {
        this.IsZero = false;
        this.Date = (string) null;
      }
    }

    public static List<FocusTimelineViewModel> InitModels(DateTime date)
    {
      return new List<FocusTimelineViewModel>()
      {
        new FocusTimelineViewModel(date),
        new FocusTimelineViewModel(date.AddHours(1.0)),
        new FocusTimelineViewModel(date.AddHours(2.0)),
        new FocusTimelineViewModel(date.AddHours(3.0)),
        new FocusTimelineViewModel(date.AddHours(4.0))
        {
          LastOne = true,
          Margin = new Thickness(0.0)
        }
      };
    }

    public void SetMargin(double bottom)
    {
      if (this.LastOne)
        return;
      this.Margin = new Thickness(0.0, 0.0, 0.0, bottom);
    }
  }
}
