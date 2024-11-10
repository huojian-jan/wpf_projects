// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitStatisticsView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitStatisticsView : UserControl, IComponentConnector
  {
    private HabitType _type;
    private bool _showCurrentStreak = true;
    internal Grid Container;
    internal ColumnDefinition ThirdColumn;
    internal RowDefinition ThirdRow;
    internal Border MonthCheckCount;
    internal Border TotalCheckCount;
    internal Border MonthCount;
    internal TextBlock MonthCountText;
    internal TextBlock UnitText;
    internal Border TotalCount;
    internal Border MonthRatio;
    internal Border CurrentStreak;
    internal TextBlock StreakNum;
    internal TextBlock StreakUnit;
    internal TextBlock StreakText;
    private bool _contentLoaded;

    public HabitStatisticsView() => this.InitializeComponent();

    public void Load(
      HabitModel habit,
      DateTime startDate,
      DateTime endDate,
      List<HabitCheckInModel> checkIns,
      List<HabitRecordModel> records)
    {
      this._type = habit.Type.ToLower() == "boolean" ? HabitType.Boolean : HabitType.Real;
      this.SetHabitType(this._type);
      if (habit.GetStartDate() > startDate)
      {
        DateTime firstCheckDay = habit.GetStartDate();
        foreach (HabitCheckInModel habitCheckInModel in checkIns.TakeWhile<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (checkIn => firstCheckDay > startDate)))
        {
          DateTime result;
          if ((habitCheckInModel.CheckStatus != 0 || habitCheckInModel.Value > 0.0) && DateTime.TryParseExact(habitCheckInModel.CheckinStamp, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result) && result < firstCheckDay)
            firstCheckDay = result;
        }
        if (records != null)
        {
          foreach (HabitRecordModel habitRecordModel in records.TakeWhile<HabitRecordModel>((Func<HabitRecordModel, bool>) (record => firstCheckDay > startDate)))
          {
            DateTime result;
            if (DateTime.TryParseExact(habitRecordModel.Stamp.ToString(), "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result) && result.Date < firstCheckDay)
              firstCheckDay = result.Date;
          }
        }
        if (startDate < firstCheckDay)
          startDate = firstCheckDay;
      }
      DateTime dateTime1;
      if (habit.Status == 1)
      {
        DateTime dateTime2 = endDate;
        dateTime1 = habit.ModifiedTime;
        DateTime date = dateTime1.Date;
        if (dateTime2 > date)
        {
          dateTime1 = habit.ModifiedTime;
          endDate = dateTime1.Date;
        }
      }
      MonthPlanInfo planDaysInMonth = HabitStatisticsUtils.GetPlanDaysInMonth(habit.RepeatRule, startDate, endDate, checkIns);
      dateTime1 = startDate.Date;
      int startStamp = int.Parse(dateTime1.ToString("yyyyMMdd"));
      dateTime1 = endDate.Date;
      int endStamp = int.Parse(dateTime1.ToString("yyyyMMdd"));
      List<HabitCheckInModel> list = checkIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (checkIn => int.Parse(checkIn.CheckinStamp) >= startStamp && int.Parse(checkIn.CheckinStamp) <= endStamp)).OrderBy<HabitCheckInModel, int>((Func<HabitCheckInModel, int>) (checkIn => int.Parse(checkIn.CheckinStamp))).ToList<HabitCheckInModel>();
      int num1 = list.Count<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (item => item.Value >= item.Goal && item.CheckStatus != 1));
      int num2 = (int) Math.Round(planDaysInMonth.Count <= 0 ? (num1 > 0 ? 100.0 : 0.0) : (double) num1 / (double) planDaysInMonth.Count * 100.0, 0, MidpointRounding.AwayFromZero);
      HabitStatisticsModel habitStatInfo = HabitStatisticsUtils.CalculateHabitStatInfo(checkIns, habit.RepeatRule);
      HabitStatisticsViewModel statisticsViewModel = new HabitStatisticsViewModel()
      {
        MonthCheckinCount = num1,
        TotalCheckinCount = habitStatInfo.TotalCheckIns,
        MonthCheckinRate = num2,
        BestStreak = habitStatInfo.MaxStreak,
        CurrentStreak = habitStatInfo.CurrentStreak,
        Unit = habit.Unit
      };
      if (this._type == HabitType.Real)
      {
        double num3 = list.Sum<HabitCheckInModel>((Func<HabitCheckInModel, double>) (checkIn => checkIn.Value));
        statisticsViewModel.MonthCompletion = Math.Round(num3, 2);
        double num4 = checkIns.Sum<HabitCheckInModel>((Func<HabitCheckInModel, double>) (checkIn => checkIn.Value));
        statisticsViewModel.TotalCompletion = Math.Round(num4, 2);
      }
      this.DataContext = (object) statisticsViewModel;
    }

    private void SetHabitType(HabitType type)
    {
      this._type = type;
      if (this._type == HabitType.Boolean)
      {
        this.Container.Height = 168.0;
        this.MonthCount.Visibility = Visibility.Collapsed;
        this.TotalCount.Visibility = Visibility.Collapsed;
        this.ThirdRow.Height = new GridLength(0.0);
      }
      else
      {
        this.ThirdRow.Height = new GridLength(1.0, GridUnitType.Star);
        this.Container.Height = 252.0;
        this.MonthCount.Visibility = Visibility.Visible;
        this.TotalCount.Visibility = Visibility.Visible;
      }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
    }

    private void SwitchStreakDisplay(object sender, MouseButtonEventArgs e)
    {
      this._showCurrentStreak = !this._showCurrentStreak;
      if (this._showCurrentStreak)
      {
        this.StreakNum.SetBinding(TextBlock.TextProperty, "CurrentStreak");
        this.StreakText.SetResourceReference(TextBlock.TextProperty, (object) "CurrentStreak");
        this.StreakUnit.SetBinding(TextBlock.TextProperty, "CurrentStreakUnit");
      }
      else
      {
        this.StreakNum.SetBinding(TextBlock.TextProperty, "BestStreak");
        this.StreakText.SetResourceReference(TextBlock.TextProperty, (object) "BestStreak");
        this.StreakUnit.SetBinding(TextBlock.TextProperty, "BestStreakUnit");
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitstatisticsview.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          this.ThirdColumn = (ColumnDefinition) target;
          break;
        case 4:
          this.ThirdRow = (RowDefinition) target;
          break;
        case 5:
          this.MonthCheckCount = (Border) target;
          break;
        case 6:
          this.TotalCheckCount = (Border) target;
          break;
        case 7:
          this.MonthCount = (Border) target;
          break;
        case 8:
          this.MonthCountText = (TextBlock) target;
          break;
        case 9:
          this.UnitText = (TextBlock) target;
          break;
        case 10:
          this.TotalCount = (Border) target;
          break;
        case 11:
          this.MonthRatio = (Border) target;
          break;
        case 12:
          this.CurrentStreak = (Border) target;
          break;
        case 13:
          this.StreakNum = (TextBlock) target;
          break;
        case 14:
          this.StreakUnit = (TextBlock) target;
          break;
        case 15:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchStreakDisplay);
          break;
        case 16:
          this.StreakText = (TextBlock) target;
          this.StreakText.MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchStreakDisplay);
          break;
        case 17:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchStreakDisplay);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
