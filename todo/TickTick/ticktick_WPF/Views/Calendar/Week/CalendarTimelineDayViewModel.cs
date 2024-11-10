// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.CalendarTimelineDayViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public class CalendarTimelineDayViewModel : BaseViewModel
  {
    private DateTime _date;
    private MonthCellState _state;
    public List<TaskCellViewModel> Cells = new List<TaskCellViewModel>();
    public bool InWeekControl;

    public string Extra1 { get; set; }

    public string Extra2 { get; set; }

    public string Extra3 { get; set; }

    public string Extra4 { get; set; }

    public List<CalendarDisplayModel> TopTasks { get; set; }

    public List<CalendarDisplayModel> BotTasks { get; set; }

    public DateTime Date
    {
      get => this._date;
      set
      {
        this._date = value;
        this.OnPropertyChanged(nameof (Date));
        this.OnPropertyChanged("IsToday");
      }
    }

    public bool IsToday => this.Date.Date == DateTime.Today;

    public MonthCellState State
    {
      get => this._state;
      set
      {
        this._state = value;
        this.OnPropertyChanged(nameof (State));
      }
    }

    public static CalendarTimelineDayViewModel Copy(CalendarTimelineDayViewModel model)
    {
      return new CalendarTimelineDayViewModel()
      {
        Cells = model.Cells.Select<TaskCellViewModel, TaskCellViewModel>(new Func<TaskCellViewModel, TaskCellViewModel>(TaskCellViewModel.Copy)).ToList<TaskCellViewModel>(),
        Date = model.Date,
        TopTasks = model.TopTasks,
        BotTasks = model.BotTasks
      };
    }
  }
}
