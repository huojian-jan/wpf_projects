// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.HabitDayCheckModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Habit;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class HabitDayCheckModel : BaseViewModel
  {
    private DateTime _date;
    private bool _needShowPercent;
    private bool _select;
    private double _percent;
    private string _toolTipText;
    private int _status;

    public HabitDayCheckModel(HabitCheckInModel checkIn, DateTime date, HabitModel habit)
    {
      this.Date = date;
      this.SetData(checkIn, habit);
    }

    public async void SetToolTip()
    {
      HabitDayCheckModel habitDayCheckModel1 = this;
      HabitModel habitById = await HabitDao.GetHabitById(habitDayCheckModel1.CheckInModel?.HabitId);
      string str1 = habitDayCheckModel1.Date.ToString("ddd");
      string str2 = Utils.IsEn() ? str1 + ", " + DateUtils.FormatShortMonthDay(habitDayCheckModel1.Date) : DateUtils.FormatShortMonthDay(habitDayCheckModel1.Date) + " " + str1;
      DateUtils.FormatMonthDay(habitDayCheckModel1.Date);
      HabitDayCheckModel habitDayCheckModel2 = habitDayCheckModel1;
      string str3;
      if (habitDayCheckModel1.CheckInModel != null)
      {
        string str4 = str2;
        string str5;
        if (!habitDayCheckModel1.NeedShowPercent)
        {
          str5 = "";
        }
        else
        {
          string[] strArray = new string[6];
          strArray[0] = "\r\n";
          double goal = habitDayCheckModel1.CheckInModel.Value;
          strArray[1] = goal.ToString();
          strArray[2] = "/";
          goal = habitDayCheckModel1.CheckInModel.Goal;
          strArray[3] = goal.ToString();
          strArray[4] = " ";
          strArray[5] = HabitUtils.GetUnitText(habitById?.Unit);
          str5 = string.Concat(strArray);
        }
        str3 = str4 + str5;
      }
      else
        str3 = (string) null;
      habitDayCheckModel2.ToolTipText = str3;
    }

    public HabitCheckInModel CheckInModel { get; set; }

    public bool IsBooleanHabit { get; set; }

    public bool BoolUnchecked
    {
      get
      {
        return this.IsBooleanHabit && this.Status != 1 && this.CheckInModel.Value < this.CheckInModel.Goal;
      }
    }

    public bool ShowPercent
    {
      get
      {
        return this.NeedShowPercent && this.Status != 1 && this.CheckInModel.Value < this.CheckInModel.Goal;
      }
    }

    public bool NeedShowPercent { get; set; }

    public double Percent
    {
      get => this._percent;
      set
      {
        this._percent = value;
        this.SetToolTip();
        this.OnPropertyChanged(nameof (Percent));
      }
    }

    public bool IsToday => this.Date == DateTime.Today;

    public int IconWidth => !this.IsToday ? 16 : 18;

    public DateTime Date
    {
      get => this._date;
      set
      {
        this._date = value;
        this.OnPropertyChanged(nameof (Date));
      }
    }

    public string ToolTipText
    {
      get => this._toolTipText;
      set
      {
        this._toolTipText = value;
        this.OnPropertyChanged(nameof (ToolTipText));
      }
    }

    public bool Select
    {
      get => this._select;
      set
      {
        this._select = value;
        this.OnPropertyChanged(nameof (Select));
      }
    }

    public int Status
    {
      get => this._status;
      set
      {
        this._status = value;
        this.OnPropertyChanged(nameof (Status));
        if (this.IsBooleanHabit)
          this.OnPropertyChanged("BoolUnchecked");
        if (this.NeedShowPercent)
          this.OnPropertyChanged("ShowPercent");
        this.OnPropertyChanged("Completed");
        this.OnPropertyChanged("Uncompleted");
      }
    }

    public bool Completed => this.Status != 1 && this.CheckInModel.Value >= this.CheckInModel.Goal;

    public bool Uncompleted => this.Status == 1;

    public void SetData(HabitCheckInModel checkIn, HabitModel habit)
    {
      if (checkIn == null || checkIn.Status == -1)
        checkIn = new HabitCheckInModel()
        {
          Value = 0.0,
          Goal = habit.Goal
        };
      this.CheckInModel = checkIn;
      this.IsBooleanHabit = habit.IsBoolHabit();
      this.NeedShowPercent = !habit.IsBoolHabit();
      this.Status = checkIn.CheckStatus;
      this.Percent = (double) Math.Min((int) (checkIn.Value / checkIn.Goal * 100.0), 100);
    }
  }
}
