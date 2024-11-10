// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineYearViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineYearViewModel : BaseViewModel
  {
    private string _yearName;
    private double _left;
    private Visibility _visibility;

    public string YearName
    {
      get => this._yearName;
      set
      {
        this._yearName = value;
        this.OnPropertyChanged(nameof (YearName));
      }
    }

    public double Left
    {
      get => this._left;
      set
      {
        this._left = value;
        this.OnPropertyChanged(nameof (Left));
      }
    }

    public Visibility Visibility
    {
      get => this._visibility;
      set
      {
        this._visibility = value;
        this.OnPropertyChanged(nameof (Visibility));
      }
    }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is TimelineYearViewModel timelineYearViewModel && this.YearName == timelineYearViewModel.YearName && Math.Abs(this.Left - timelineYearViewModel.Left) < 0.1 && this.Visibility == timelineYearViewModel.Visibility;
    }
  }
}
