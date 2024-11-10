// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.CheckInLogViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class CheckInLogViewModel : BaseHidableViewModel
  {
    private string _content;
    private int _score;
    private bool _showEdit;
    public string Color;
    public string IconRes;

    public CheckInLogViewModel()
    {
    }

    public CheckInLogViewModel(
      HabitRecordModel record,
      HabitModel habit,
      bool completed,
      bool unCompleted,
      bool showEdit)
    {
      this.Id = record.Id;
      this.Content = record.Content;
      this.Completed = completed;
      this.UnCompleted = unCompleted;
      this.Status = this.Completed || this.UnCompleted ? 2 : 0;
      this.IsBoolHabit = habit.IsBoolHabit();
      this.HabitName = habit.Name;
      this.HabitId = habit.Id;
      DateTime exact = DateTime.ParseExact(record.Stamp.ToString(), "yyyyMMdd", (IFormatProvider) App.Ci);
      string str = exact.ToString("ddd", (IFormatProvider) App.Ci);
      this.Date = exact;
      this.DisplayDate = Utils.IsEn() ? str + ", " + DateUtils.FormatShortMonthDay(exact) : DateUtils.FormatShortMonthDay(exact) + " " + str;
      DateUtils.FormatMonthDay(exact);
      this.Score = record.Emoji;
      this.IconRes = habit.IconRes;
      this.Color = habit.Color;
      this.ShowEdit = showEdit;
    }

    public int Score
    {
      get => this._score;
      set
      {
        this._score = value;
        this.OnPropertyChanged(nameof (Score));
      }
    }

    public string Id { get; set; }

    public DateTime Date { get; set; }

    public string DisplayDate { get; set; }

    public string HabitId { get; set; }

    public string HabitName { get; set; }

    public bool IsBoolHabit { get; set; }

    public Visibility LineVisibility { get; set; }

    public string Content
    {
      get => this._content;
      set
      {
        this._content = value;
        this.OnPropertyChanged(nameof (Content));
      }
    }

    public bool Completed { get; set; }

    public bool UnCompleted { get; set; }

    public int Status { get; set; }

    public bool ShowEdit
    {
      get => this._showEdit;
      set
      {
        this._showEdit = value;
        this.OnPropertyChanged(nameof (ShowEdit));
      }
    }

    public CheckInLogViewModel Copy() => (CheckInLogViewModel) this.MemberwiseClone();
  }
}
