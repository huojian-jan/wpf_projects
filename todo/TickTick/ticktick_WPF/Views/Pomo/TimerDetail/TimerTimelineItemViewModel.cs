// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerDetail.TimerTimelineItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo.TimerDetail
{
  public class TimerTimelineItemViewModel : TimerDetailItemViewModel
  {
    public TimerModel TModel;
    private string _interval = "week";
    private string _intervalText = Utils.GetString("Week");
    private TimerDetailPanel _detail;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Interval
    {
      get => this._interval;
      set
      {
        if (!(this._interval != value))
          return;
        this._interval = value;
        this.OnPropertyChanged("IntervalText");
        this.OnPropertyChanged("DateText");
      }
    }

    public string DateText
    {
      get
      {
        switch (this._interval)
        {
          case "month":
            return DateUtils.FormatYearMonth(this.StartDate);
          case "year":
            return DateUtils.FormatYear(this.StartDate);
          default:
            return DateUtils.FormatShortDate(this.StartDate) + " - " + DateUtils.FormatShortDate(this.EndDate);
        }
      }
    }

    public string IntervalText
    {
      get
      {
        switch (this._interval)
        {
          case "month":
            return Utils.GetString("TimelineMonth");
          case "year":
            return Utils.GetString("TimelineYear");
          default:
            return Utils.GetString("TimelineWeek");
        }
      }
    }

    public TimerTimelineItemViewModel(TimerDetailPanel detail) => this._detail = detail;

    public void SetTimer(TimerModel timer)
    {
      this.TModel = timer;
      this.StartDate = Utils.GetWeekStart(DateTime.Today);
      this.EndDate = this.StartDate.AddDays(6.0);
      this.TimerTimeline = true;
    }

    public async Task<Dictionary<string, long>> GetStatistics()
    {
      return await TimerService.GetTimerStatistics(this.TModel, this.StartDate, this.EndDate, this.Interval);
    }

    public void SetDateText() => this.OnPropertyChanged("DateText");

    public void ReloadData() => this.OnPropertyChanged("Reload");

    public void SetInterval(string interval)
    {
      string interval1 = this.Interval;
      this.Interval = interval;
      switch (interval)
      {
        case "week":
          this.StartDate = Utils.GetWeekStart(DateTime.Today < this.EndDate ? DateTime.Today : this.EndDate);
          this.EndDate = this.StartDate.AddDays(6.0);
          break;
        case "month":
          DateTime dateTime1 = interval1 == "year" ? this.StartDate.AddMonths(11) : this.StartDate.AddDays((double) (1 - this.StartDate.Day));
          DateTime dateTime2;
          DateTime dateTime3;
          if (!(dateTime1 > DateTime.Today))
          {
            dateTime3 = dateTime1;
          }
          else
          {
            dateTime2 = DateTime.Today;
            dateTime3 = dateTime2.AddDays((double) (1 - DateTime.Today.Day));
          }
          this.StartDate = dateTime3;
          dateTime2 = this.StartDate;
          dateTime2 = dateTime2.AddMonths(1);
          this.EndDate = dateTime2.AddDays(-1.0);
          break;
        case "year":
          this.StartDate = new DateTime(this.StartDate.Year, 1, 1);
          DateTime dateTime4 = this.StartDate;
          dateTime4 = dateTime4.AddYears(1);
          this.EndDate = dateTime4.AddDays(-1.0);
          break;
      }
      this._detail.ReloadRecords();
      this._detail.PullTimeline(this);
    }

    public void NotifySpanChanged(bool getFront = true)
    {
      this._detail.ReloadRecords();
      this._detail.PullTimeline(this, getFront);
    }
  }
}
