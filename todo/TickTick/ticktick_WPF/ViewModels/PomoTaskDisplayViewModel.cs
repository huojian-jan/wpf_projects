// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.PomoTaskDisplayViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class PomoTaskDisplayViewModel : BaseViewModel
  {
    private string _title;
    private bool _bind;

    public PomoTaskDisplayViewModel()
    {
    }

    public PomoTaskDisplayViewModel(PomoTask pomoTask, PomodoroModel pomo)
    {
      this._title = pomoTask.GetTitle();
      this.PomoId = pomo.Id;
      this.TaskId = pomoTask.TaskId;
      this.HabitId = pomoTask.HabitId;
      this.TimerId = pomoTask.TimerSid;
      this.StartTime = pomoTask.StartTime;
      this.Duration = Utils.GetTotalSecond(pomoTask.StartTime, pomoTask.EndTime);
      this.DurationString = this.Duration >= 60L ? Utils.GetDurationString(this.Duration, true, true, false) : string.Format("{0}s", (object) this.Duration);
      this.TimeString = pomoTask.StartTime.ToString("yyyy'/'MM'/'dd HH':'mm") + " - " + pomoTask.EndTime.ToString("yyyy'/'MM'/'dd HH':'mm");
      this.Icon = Utils.GetIcon(pomo.Type == 0 ? "IcPomo" : "IcPomoTimer");
      this.Bind = !string.IsNullOrEmpty(this.TaskId) || !string.IsNullOrEmpty(this.HabitId) || !string.IsNullOrEmpty(this.TimerId) || !string.IsNullOrEmpty(this._title);
      if (!string.IsNullOrEmpty(this._title) || !this.Bind)
        return;
      this._title = Utils.GetString("NoTitle");
    }

    public string PomoId { get; set; }

    public string TaskId { get; set; }

    public string HabitId { get; set; }

    public string TimerId { get; set; }

    public DateTime StartTime { get; set; }

    public string DurationString { get; set; }

    public long Duration { get; set; }

    public string TimeString { get; set; }

    public Geometry Icon { get; set; }

    public bool ShowBottomLine { get; set; }

    public bool Enable { get; set; }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public bool Bind
    {
      get => this._bind;
      set
      {
        this._bind = value;
        this.OnPropertyChanged(nameof (Bind));
        this.OnPropertyChanged("BindTask");
      }
    }

    public bool BindTask => !string.IsNullOrEmpty(this.TaskId);

    public string GetId()
    {
      if (!string.IsNullOrEmpty(this.TimerId))
        return this.TimerId;
      if (!string.IsNullOrEmpty(this.TaskId))
        return this.TaskId;
      string.IsNullOrEmpty(this.HabitId);
      return this.HabitId;
    }
  }
}
