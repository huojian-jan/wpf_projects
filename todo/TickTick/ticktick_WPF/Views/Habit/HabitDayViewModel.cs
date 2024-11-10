// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitDayViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitDayViewModel : BaseViewModel
  {
    private bool _completed;
    private bool _unCompleted;
    private bool _unChecked;
    private DateTime _date;
    private bool _hasPercent;
    private bool _hover;
    private bool _isBooleanHabit;
    private double _percent;
    private bool _showBoolHint;
    private ShowMode _showMode = ShowMode.CurrentMonth;
    private bool _showRealHint;

    public string HabitId { get; set; }

    public HabitModel Habit { get; set; }

    public bool HasPercent
    {
      get => this._hasPercent;
      set
      {
        this._hasPercent = value;
        this.OnPropertyChanged(nameof (HasPercent));
      }
    }

    public bool UnChecked
    {
      get => this._unChecked;
      set
      {
        this._unChecked = value;
        this.OnPropertyChanged(nameof (UnChecked));
      }
    }

    public bool UnCompleted
    {
      get => this._unCompleted;
      set
      {
        this._unCompleted = value;
        this.OnPropertyChanged(nameof (UnCompleted));
      }
    }

    public bool Completed
    {
      get => this._completed;
      set
      {
        this._completed = value;
        this.OnPropertyChanged(nameof (Completed));
      }
    }

    public double Percent
    {
      get => this._percent;
      set
      {
        this._percent = value;
        this.OnPropertyChanged(nameof (Percent));
      }
    }

    public ShowMode ShowMode
    {
      get => this._showMode;
      set
      {
        this._showMode = value;
        this.OnPropertyChanged(nameof (ShowMode));
      }
    }

    public DateTime Date
    {
      get => this._date;
      set
      {
        this._date = value;
        this.OnPropertyChanged(nameof (Date));
      }
    }

    public bool Hover
    {
      get => this._hover;
      set
      {
        this._hover = value;
        this.OnPropertyChanged(nameof (Hover));
      }
    }

    public bool IsBooleanHabit
    {
      get => this._isBooleanHabit;
      set
      {
        this._isBooleanHabit = value;
        this.OnPropertyChanged(nameof (IsBooleanHabit));
      }
    }

    public bool ShowBoolHint
    {
      get => this._showBoolHint;
      set
      {
        this._showBoolHint = value;
        this.OnPropertyChanged(nameof (ShowBoolHint));
      }
    }

    public bool ShowRealHint
    {
      get => this._showRealHint;
      set
      {
        this._showRealHint = value;
        this.OnPropertyChanged(nameof (ShowRealHint));
      }
    }
  }
}
