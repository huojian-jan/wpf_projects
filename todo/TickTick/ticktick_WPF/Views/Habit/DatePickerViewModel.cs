// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.DatePickerViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class DatePickerViewModel : BaseViewModel
  {
    private DateTime _pivotDate;
    private DateTime? _selectedDate;
    private DateTime? _selectEndDate;
    private DateTime? _selectStartDate;

    public DateTime PivotDate
    {
      get => this._pivotDate;
      set
      {
        this._pivotDate = value;
        this.OnPropertyChanged(nameof (PivotDate));
      }
    }

    public DateTime? SelectedDate
    {
      get => this._selectedDate;
      set
      {
        this._selectedDate = value;
        this.OnPropertyChanged(nameof (SelectedDate));
      }
    }

    public DateTime? SelectStartDate
    {
      get => this._selectStartDate;
      set
      {
        this._selectStartDate = value;
        this.OnPropertyChanged(nameof (SelectStartDate));
      }
    }

    public DateTime? SelectEndDate
    {
      get => this._selectEndDate;
      set
      {
        this._selectEndDate = value;
        this.OnPropertyChanged(nameof (SelectEndDate));
      }
    }
  }
}
