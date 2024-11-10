// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitMonthGrid
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitMonthGrid : UserControl, IComponentConnector
  {
    private const int DayColumns = 7;
    private const int DayRows = 6;
    public static readonly DependencyProperty StartDateProperty = DependencyProperty.Register(nameof (StartDate), typeof (DateTime), typeof (HabitMonthGrid), new PropertyMetadata((object) DateTime.Today, (PropertyChangedCallback) null));
    private HabitModel _habit;
    private DateTime _manualSelectDate;
    private DatePickerViewModel _model;
    private List<HabitCheckInModel> _stamps = new List<HabitCheckInModel>();
    internal HabitMonthGrid RootView;
    internal StackPanel DayGrid;
    internal ItemsControl DayCells;
    internal Popup ManuallyCheckInPopup;
    internal ManualRecordCheckinControl CheckInControl;
    private bool _contentLoaded;

    public bool IsDataLoaded { get; private set; }

    public HabitMonthGrid()
      : this(new DateTime?())
    {
    }

    private HabitMonthGrid(DateTime? selectedDate, DateTime? selectStart = null, DateTime? selectEnd = null)
    {
      this.InitData(selectedDate, selectStart, selectEnd);
      this.InitializeComponent();
      this.Loaded += (RoutedEventHandler) ((s, e) =>
      {
        DataChangedNotifier.WeekStartFromChanged -= new EventHandler(this.Reload);
        DataChangedNotifier.WeekStartFromChanged += new EventHandler(this.Reload);
      });
      this.Unloaded += (RoutedEventHandler) ((s, e) => DataChangedNotifier.WeekStartFromChanged -= new EventHandler(this.Reload));
    }

    private DateTime PivotDate => this._model.PivotDate;

    public DateTime StartDate
    {
      get => (DateTime) this.GetValue(HabitMonthGrid.StartDateProperty);
      set => this.SetValue(HabitMonthGrid.StartDateProperty, (object) value);
    }

    public event EventHandler<KeyValuePair<string, DateTime>> CheckInChanged;

    public event EventHandler<DateTime> PivotDateChanged;

    public void Load(
      HabitDetailViewModel habit,
      DateTime pivotDate,
      List<HabitCheckInModel> checkIns)
    {
      this._stamps = checkIns;
      this._habit = habit.Habit;
      this._model.PivotDate = pivotDate;
      this.InitCells(habit.Habit);
      this.IsDataLoaded = true;
    }

    private void Reload(object sender, EventArgs e)
    {
      if (this._habit == null)
        return;
      this.InitCells(this._habit);
    }

    private void InitCells(HabitModel habit) => this.SetDayCells(habit);

    private void InitData(DateTime? selectedDate, DateTime? selectStart, DateTime? selectEnd)
    {
      this._model = new DatePickerViewModel()
      {
        PivotDate = selectedDate ?? DateTime.Now.Date,
        SelectedDate = selectedDate,
        SelectStartDate = selectStart,
        SelectEndDate = selectEnd
      };
      this.DataContext = (object) this._model;
    }

    private async Task SetDayCells(HabitModel habit)
    {
      HabitRepeatInfo repeatInfo;
      List<(DateTime, bool?)> checkDates;
      List<HabitCheckInModel> checkIns;
      if (habit == null)
      {
        repeatInfo = (HabitRepeatInfo) null;
        checkDates = (List<(DateTime, bool?)>) null;
        checkIns = (List<HabitCheckInModel>) null;
      }
      else
      {
        repeatInfo = HabitUtils.BuildHabitRepeatInfo(habit.RepeatRule);
        checkDates = new List<(DateTime, bool?)>();
        this.StartDate = this.InitStartDate();
        DateTime lastDay = DateTime.Today;
        DateTime dateTime1;
        if (habit.Status != 0)
        {
          dateTime1 = habit.ModifiedTime;
          lastDay = dateTime1.Date;
        }
        dateTime1 = habit.GetStartDate();
        DateTime firstCheckDay = dateTime1.Date;
        checkIns = await HabitCheckInDao.GetHabitCheckInsByHabitId(habit.Id);
        if (firstCheckDay > this.StartDate)
        {
          List<HabitRecordModel> recordsByHabitId = await HabitRecordDao.GetHabitRecordsByHabitId(habit.Id);
          if (recordsByHabitId != null)
          {
            foreach (HabitRecordModel habitRecordModel in recordsByHabitId)
            {
              DateTime result;
              if (DateTime.TryParseExact(habitRecordModel.Stamp.ToString(), "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result) && result.Date < firstCheckDay)
                firstCheckDay = result.Date;
            }
          }
        }
        if (checkIns != null)
        {
          foreach (HabitCheckInModel habitCheckInModel in checkIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (checkIn => checkIn.CheckStatus != 0 || checkIn.Value > 0.0)))
          {
            DateTime result;
            if (DateTime.TryParseExact(habitCheckInModel.CheckinStamp, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
              if (result.Date < firstCheckDay)
                firstCheckDay = result.Date;
              if (repeatInfo.Type == HabitRepeatType.TimesInWeek || repeatInfo.Type == HabitRepeatType.Daily && repeatInfo.Interval > 1)
              {
                if (repeatInfo.Type == HabitRepeatType.TimesInWeek)
                {
                  if (!(result < this.StartDate))
                  {
                    DateTime dateTime2 = result;
                    dateTime1 = this.StartDate;
                    DateTime dateTime3 = dateTime1.AddDays(42.0);
                    if (dateTime2 > dateTime3)
                      continue;
                  }
                  else
                    continue;
                }
                checkDates.Add((result, habitCheckInModel.CheckStatus == 0 ? new bool?() : new bool?(habitCheckInModel.CheckStatus == 2)));
              }
            }
          }
        }
        int num = 0;
        List<DayCellViewModel> items = new List<DayCellViewModel>();
        for (int index1 = 0; index1 < 6; ++index1)
        {
          for (int index2 = 0; index2 < 7; ++index2)
          {
            dateTime1 = this.StartDate;
            DateTime date = dateTime1.AddDays((double) num++);
            HabitDayViewModel model = new HabitDayViewModel()
            {
              HabitId = habit.Id,
              Habit = habit,
              Date = date,
              IsBooleanHabit = habit.Type.ToLower() == "boolean"
            };
            this.InitShowMode(model, lastDay);
            bool flag = date >= firstCheckDay && date <= lastDay && HabitUtils.InitShowHint(repeatInfo, date, firstCheckDay, checkDates);
            model.ShowBoolHint = habit.Type.ToLower() == "boolean" & flag;
            model.ShowRealHint = habit.Type.ToLower() == "real" & flag;
            items.Add(new DayCellViewModel()
            {
              Row = index1,
              Column = index2,
              Data = model
            });
          }
        }
        foreach (DayCellViewModel dayCellViewModel in items)
        {
          dateTime1 = dayCellViewModel.Data.Date;
          string stamp = dateTime1.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture);
          HabitCheckInModel habitCheckInModel = this._stamps.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (dateStamp => stamp == dateStamp.CheckinStamp && dateStamp.Value >= 0.0));
          if (habitCheckInModel != null)
          {
            dayCellViewModel.Data.Completed = habitCheckInModel.Value >= habitCheckInModel.Goal;
            dayCellViewModel.Data.UnCompleted = habitCheckInModel.CheckStatus == 1;
            dayCellViewModel.Data.Percent = habit.IsBoolHabit() ? 0.0 : (double) Math.Min((int) (habitCheckInModel.Value / habitCheckInModel.Goal * 100.0), 100);
            dayCellViewModel.Data.HasPercent = dayCellViewModel.Data.Percent > 0.0;
          }
          else
          {
            dayCellViewModel.Data.Completed = false;
            dayCellViewModel.Data.Percent = 0.0;
            dayCellViewModel.Data.HasPercent = false;
          }
        }
        ItemsSourceHelper.SetHidableItemsSource<DayCellViewModel>(this.DayCells, items);
        repeatInfo = (HabitRepeatInfo) null;
        checkDates = (List<(DateTime, bool?)>) null;
        checkIns = (List<HabitCheckInModel>) null;
      }
    }

    private void InitShowMode(HabitDayViewModel model, DateTime lastDate)
    {
      DateTime dateTime;
      if (model.Date > lastDate)
      {
        model.ShowMode = ShowMode.Advanced;
      }
      else
      {
        int month1 = model.Date.Month;
        dateTime = this._model.PivotDate;
        dateTime = dateTime.Date;
        int month2 = dateTime.Month;
        if (month1 == month2)
        {
          model.ShowMode = ShowMode.CurrentMonth;
        }
        else
        {
          dateTime = model.Date;
          int month3 = dateTime.Month;
          dateTime = this._model.PivotDate;
          dateTime = dateTime.Date;
          int month4 = dateTime.Month;
          model.ShowMode = month3 >= month4 ? ShowMode.NextMonth : ShowMode.LastMonth;
        }
      }
      dateTime = model.Date;
      DateTime date1 = dateTime.Date;
      dateTime = DateTime.Now;
      DateTime date2 = dateTime.Date;
      if (!(date1 == date2))
        return;
      model.ShowMode = ShowMode.Today;
    }

    private DateTime InitStartDate()
    {
      if (Utils.IsEmptyDate(this._model.PivotDate))
        return DateTime.Today;
      DateTime dateTime1 = this._model.PivotDate;
      DateTime dateTime2 = dateTime1.AddDays((double) (1 - this._model.PivotDate.Day));
      dateTime1 = dateTime2.AddDays((double) ((int) dateTime2.DayOfWeek * -1));
      DateTime dateTime3 = dateTime1.AddDays((double) Utils.GetWeekFromDiff(new DateTime?(dateTime2)));
      int month1 = dateTime3.Month;
      dateTime1 = this.PivotDate;
      int month2 = dateTime1.Month;
      return month1 == month2 && dateTime3.Day != 1 ? dateTime3.AddDays(-7.0) : dateTime3;
    }

    private void OnClick(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void LastClick(object sender, MouseButtonEventArgs e) => this.NextMonth(-1);

    private void NextClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this._model.PivotDate < DateTime.Today))
        return;
      this.NextMonth(1);
    }

    private void NextMonth(int step)
    {
      this._model.PivotDate = this._model.PivotDate.AddMonths(step);
      this.SetDayCells(this._habit);
      this.NotifyPivotDateChanged(this._model.PivotDate);
    }

    private void NotifyPivotDateChanged(DateTime pivotDate)
    {
      EventHandler<DateTime> pivotDateChanged = this.PivotDateChanged;
      if (pivotDateChanged == null)
        return;
      pivotDateChanged((object) this, pivotDate);
    }

    private void OnTodayClick(object sender, MouseButtonEventArgs e)
    {
      this._model.PivotDate = DateTime.Today;
      this.SetDayCells(this._habit);
      this.NotifyPivotDateChanged(this._model.PivotDate);
    }

    public void ManuallyRecord(DateTime date, bool placeMouse)
    {
      this._manualSelectDate = date;
      this.CheckInControl.DateText.Visibility = Visibility.Visible;
      this.CheckInControl.DateText.Text = DateUtils.FormatMonthDay(date);
      HabitCheckInModel habitCheckInModel = this._stamps.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (stamp => stamp.CheckinStamp == date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)));
      this.CheckInControl.LeftLabel.Text = Utils.GetString("Total");
      this.CheckInControl.Init(habitCheckInModel != null ? habitCheckInModel.Value : 0.0, this._habit?.Unit ?? "Count", date == DateTime.Today);
      this.CheckInControl.Cancel -= new EventHandler(this.OnCheckInPopupCancel);
      this.CheckInControl.Cancel += new EventHandler(this.OnCheckInPopupCancel);
      this.CheckInControl.Save -= new EventHandler<double>(this.OnCheckInPopupSave);
      this.CheckInControl.Save += new EventHandler<double>(this.OnCheckInPopupSave);
      this.ManuallyCheckInPopup.Placement = PlacementMode.MousePoint;
      if (!placeMouse)
      {
        for (int index = 0; index < this.DayCells.Items.Count; ++index)
        {
          ContentPresenter contentPresenter = (ContentPresenter) this.DayCells.ItemContainerGenerator.ContainerFromIndex(index);
          if (contentPresenter != null && contentPresenter.DataContext is DayCellViewModel dataContext)
          {
            DateTime date1 = dataContext.Data.Date;
            date1 = date1.Date;
            if (date1.Equals(DateTime.Today))
            {
              this.ManuallyCheckInPopup.PlacementTarget = (UIElement) contentPresenter;
              this.ManuallyCheckInPopup.Placement = PlacementMode.Bottom;
              break;
            }
          }
        }
      }
      this.ManuallyCheckInPopup.Tag = (object) (habitCheckInModel == null ? new bool?() : new bool?(habitCheckInModel.Value < habitCheckInModel.Goal));
      this.ManuallyCheckInPopup.IsOpen = true;
    }

    private async void OnCheckInPopupSave(object sender, double amount)
    {
      bool? tag = (bool?) this.ManuallyCheckInPopup.Tag;
      this.ManuallyCheckInPopup.IsOpen = false;
      if (!tag.HasValue && Math.Abs(amount) < 0.001)
        return;
      await HabitService.CheckInHabit(this._habit.Id, this._manualSelectDate, Math.Max(amount, 0.0), false, !tag.HasValue || tag.Value);
      this.SetDayCells(this._habit);
    }

    private void OnCheckInPopupCancel(object sender, EventArgs e)
    {
      this.ManuallyCheckInPopup.IsOpen = false;
    }

    public void Dispose()
    {
      DataChangedNotifier.WeekStartFromChanged -= new EventHandler(this.Reload);
      this.CheckInControl.Cancel -= new EventHandler(this.OnCheckInPopupCancel);
      this.CheckInControl.Save -= new EventHandler<double>(this.OnCheckInPopupSave);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitmonthgrid.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.RootView = (HabitMonthGrid) target;
          this.RootView.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClick);
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.LastClick);
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTodayClick);
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.NextClick);
          break;
        case 5:
          this.DayGrid = (StackPanel) target;
          break;
        case 6:
          this.DayCells = (ItemsControl) target;
          break;
        case 7:
          this.ManuallyCheckInPopup = (Popup) target;
          break;
        case 8:
          this.CheckInControl = (ManualRecordCheckinControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
