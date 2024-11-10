// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusRecordItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class FocusRecordItemViewModel : FocusStatisticsPanelItemViewModel
  {
    private bool _showTitle;

    public string DateText { get; set; }

    public int Type { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Note { get; set; }

    public bool IsLastItem { get; set; }

    public List<string> Titles { get; set; }

    public string FocusId { get; set; }

    public bool Editable { get; set; }

    public long PauseDuration { get; set; }

    public DateTime Date { get; set; }

    public FocusRecordItemViewModel() => this.IsRecord = true;

    public FocusRecordItemViewModel(DateTime dateTime)
      : this()
    {
      this.DateText = DateUtils.FormatShortDate(dateTime);
    }

    public FocusRecordItemViewModel(
      PomodoroModel focus,
      DateTime dateTime,
      List<PomoTask> tasks,
      bool showTitle = true)
      : this()
    {
      this._showTitle = showTitle;
      this.FocusId = focus.Id;
      this.Type = focus.Type;
      this.Titles = new List<string>();
      this.Date = dateTime;
      this.StartTime = dateTime.Date == focus.StartTime.Date ? focus.StartTime : dateTime.Date;
      this.EndTime = dateTime.Date == focus.EndTime.Date ? focus.EndTime : focus.EndTime.Date;
      this.Note = focus.Note;
      this.PauseDuration = focus.PauseDuration;
      TimeSpan timeSpan = DateTime.Today - focus.StartTime.Date;
      this.Editable = timeSpan.TotalDays <= 30.0;
      if (showTitle)
      {
        foreach (PomoTask task in tasks)
        {
          if ((task.StartTime.Date == dateTime || task.EndTime.Date == dateTime) && !string.IsNullOrEmpty(task.GetTitle()) && !this.Titles.Contains(task.GetTitle()))
            this.Titles.Add(task.GetTitle());
        }
      }
      DateTime date1 = focus.StartTime.Date;
      DateTime dateTime1 = focus.EndTime;
      DateTime date2 = dateTime1.Date;
      if (!(date1 != date2) || this.PauseDuration <= 30L)
        return;
      double num1 = 0.0;
      foreach (PomoTask task in tasks)
      {
        dateTime1 = task.StartTime;
        if (dateTime1.Date == dateTime)
        {
          dateTime1 = task.EndTime;
          if (dateTime1.Date == dateTime)
          {
            double num2 = num1;
            timeSpan = task.EndTime - task.StartTime;
            double totalSeconds = timeSpan.TotalSeconds;
            num1 = num2 + totalSeconds;
            continue;
          }
        }
        dateTime1 = task.StartTime;
        if (!(dateTime1.Date == dateTime))
        {
          dateTime1 = task.EndTime;
          if (!(dateTime1.Date == dateTime))
            continue;
        }
        DateTime date3 = dateTime.Date;
        dateTime1 = task.StartTime;
        DateTime date4 = dateTime1.Date;
        DateTime dateTime2 = date3 == date4 ? task.StartTime : dateTime.Date;
        DateTime date5 = dateTime.Date;
        dateTime1 = task.EndTime;
        DateTime date6 = dateTime1.Date;
        DateTime dateTime3;
        if (!(date5 == date6))
        {
          dateTime1 = task.EndTime;
          dateTime3 = dateTime1.Date;
        }
        else
          dateTime3 = task.EndTime;
        DateTime dateTime4 = dateTime3;
        double num3 = num1;
        timeSpan = dateTime4 - dateTime2;
        double totalSeconds1 = timeSpan.TotalSeconds;
        num1 = num3 + totalSeconds1;
      }
      this.PauseDuration = (long) (int) ((this.EndTime - this.StartTime).TotalSeconds - num1);
    }

    public string TimeText
    {
      get => DateUtils.GetTimeText(this.StartTime) + " - " + DateUtils.GetTimeText(this.EndTime);
    }

    public string DurationText
    {
      get
      {
        return Utils.GetShortDurationString((long) (this.EndTime - this.StartTime).TotalSeconds - this.PauseDuration, false);
      }
    }

    public async Task ResetTitle()
    {
      this.Note = (await PomoDao.GetPomoById(this.FocusId))?.Note;
      if (!this._showTitle)
        return;
      List<PomoTask> pomoTasksByPomoId = await PomoDao.GetPomoTasksByPomoId(this.FocusId);
      this.Titles = new List<string>();
      foreach (PomoTask pomoTask in pomoTasksByPomoId)
      {
        if ((pomoTask.StartTime.Date == this.Date || pomoTask.EndTime.Date == this.Date) && !string.IsNullOrEmpty(pomoTask.GetTitle()) && !this.Titles.Contains(pomoTask.GetTitle()))
          this.Titles.Add(pomoTask.GetTitle());
      }
    }
  }
}
