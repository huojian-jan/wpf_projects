// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitDetailControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitDetailControl : UserControl, IComponentConnector
  {
    private string _habitId;
    private DateTime _pivotDate = DateTime.Today;
    public bool IsIndependent;
    private static ConcurrentDictionary<string, DateTime> _pullRemoteDict = new ConcurrentDictionary<string, DateTime>();
    internal ScrollViewer Scroller;
    internal Grid Container;
    internal Border BackGrid;
    internal TextBlock ArchivedText;
    internal HoverIconButton MoreImage;
    internal EscPopup MorePopup;
    internal HabitStatisticsView StatisticsView;
    internal HabitCompletedCyclesControl CompletedCyclesControl;
    internal HabitMonthGrid HabitMonthControl;
    internal HabitCheckinChartControl DailyGoalsControl;
    internal HabitLogControl LogControl;
    private bool _contentLoaded;

    public event EventHandler HideDetail;

    public HabitDetailControl()
    {
      this.InitializeComponent();
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    private void BindEvents()
    {
      DataChangedNotifier.HabitCheckInChanged += new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      DataChangedNotifier.HabitsChanged += new EventHandler(this.OnHabitChanged);
      DataChangedNotifier.HabitLogChanged += new EventHandler(this.ReloadHabit);
      DataChangedNotifier.HabitsSyncDone += new EventHandler(this.ReloadHabit);
      this.HabitMonthControl.PivotDateChanged += new EventHandler<DateTime>(this.OnDateChanged);
    }

    private void UnbindEvents()
    {
      DataChangedNotifier.HabitCheckInChanged -= new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      DataChangedNotifier.HabitsChanged -= new EventHandler(this.OnHabitChanged);
      DataChangedNotifier.HabitLogChanged -= new EventHandler(this.ReloadHabit);
      DataChangedNotifier.HabitsSyncDone -= new EventHandler(this.ReloadHabit);
      this.HabitMonthControl.PivotDateChanged -= new EventHandler<DateTime>(this.OnDateChanged);
      this.HabitMonthControl.Dispose();
      this.CompletedCyclesControl.Dispose();
    }

    private void OnHabitChanged(object sender, EventArgs e)
    {
      this.Load(this._habitId, this._pivotDate);
    }

    private void OnCheckInChanged(object sender, HabitCheckInModel checkIn)
    {
      if (this.Visibility != Visibility.Visible || !(checkIn.HabitId == this._habitId))
        return;
      this.Load(this._habitId, this._pivotDate);
    }

    private void OnDateChanged(object sender, DateTime pivotDate)
    {
      if (string.IsNullOrEmpty(this._habitId))
        return;
      this.Load(this._habitId, pivotDate);
    }

    private void ReloadHabit(object sender, EventArgs e) => this.Reload();

    public void Reload() => this.Load(this._habitId, this._pivotDate);

    public void Load(string habitId, DateTime pivotDate, bool pullRemote = false, bool force = true)
    {
      ThreadUtil.DetachedRunOnUiThread((Action) (async () =>
      {
        HabitModel habit;
        if (!force && !string.IsNullOrEmpty(this._habitId) && !string.IsNullOrEmpty(habitId) && this._habitId != habitId)
        {
          habit = (HabitModel) null;
        }
        else
        {
          this._habitId = habitId;
          this._pivotDate = pivotDate;
          habit = await HabitDao.GetHabitById(habitId);
          if (habit != null)
          {
            this.SetArchiveView(habit.Status == 1);
            DateTime startDate = pivotDate.AddDays((double) (pivotDate.Day * -1 + 1));
            DateTime endDate = startDate.AddMonths(1).AddDays(-1.0);
            HabitDetailViewModel vm = new HabitDetailViewModel(habit)
            {
              PivotDate = pivotDate
            };
            int startDateStamp = ticktick_WPF.Util.DateUtils.GetDateNum(startDate);
            int endDateStamp = ticktick_WPF.Util.DateUtils.GetDateNum(endDate);
            int startDatExtraStamp = ticktick_WPF.Util.DateUtils.GetDateNum(startDate.AddDays(-14.0));
            int endDateExtraStamp = ticktick_WPF.Util.DateUtils.GetDateNum(endDate.AddDays(14.0));
            List<HabitCheckInModel> checkIns = await HabitCheckInDao.GetHabitCheckInsByHabitId(habitId);
            List<HabitCheckInModel> monthCheckIns = new List<HabitCheckInModel>();
            List<HabitCheckInModel> monthCheckInsExtra = new List<HabitCheckInModel>();
            foreach (HabitCheckInModel habitCheckInModel in checkIns)
            {
              int num = int.Parse(habitCheckInModel.CheckinStamp);
              if (num >= startDateStamp && num <= endDateStamp)
              {
                monthCheckIns.Add(habitCheckInModel);
                monthCheckInsExtra.Add(habitCheckInModel);
              }
              else if (num >= startDatExtraStamp && num <= endDateExtraStamp)
                monthCheckInsExtra.Add(habitCheckInModel);
            }
            this.DataContext = (object) vm;
            this.CompletedCyclesControl.Habit = habit;
            HabitCompletedCyclesControl completedCyclesControl = this.CompletedCyclesControl;
            int? targetDays = habit.TargetDays;
            int num1 = 0;
            int num2 = targetDays.GetValueOrDefault() > num1 & targetDays.HasValue ? 0 : 2;
            completedCyclesControl.Visibility = (Visibility) num2;
            List<HabitRecordModel> recordsByHabitId = await HabitRecordDao.GetHabitRecordsByHabitId(habit.Id);
            this.StatisticsView.Load(habit, startDate, endDate, checkIns, recordsByHabitId);
            this.HabitMonthControl.Load(vm, pivotDate, monthCheckInsExtra);
            this.LoadDailyGoals(habit, startDate, endDate, monthCheckIns);
            this.LogControl.Load(habitId, startDate, endDate, monthCheckIns, recordsByHabitId);
            vm = (HabitDetailViewModel) null;
            checkIns = (List<HabitCheckInModel>) null;
            monthCheckIns = (List<HabitCheckInModel>) null;
            monthCheckInsExtra = (List<HabitCheckInModel>) null;
          }
          if (!pullRemote)
            habit = (HabitModel) null;
          else if (string.IsNullOrEmpty(habit?.Id))
          {
            habit = (HabitModel) null;
          }
          else
          {
            DateTime dateTime;
            if (HabitDetailControl._pullRemoteDict.TryGetValue(habit.Id, out dateTime) && (DateTime.Now - dateTime).TotalMinutes < 2.0)
              habit = (HabitModel) null;
            else if (!await HabitService.PullRemoteCheckinAndRecordData(habit))
            {
              habit = (HabitModel) null;
            }
            else
            {
              this.Load(this._habitId, this._pivotDate);
              HabitDetailControl._pullRemoteDict[habit.Id] = DateTime.Now;
              habit = (HabitModel) null;
            }
          }
        }
      }));
    }

    public void ManuallyRecord(DateTime time) => this.HabitMonthControl.ManuallyRecord(time, false);

    private void SetArchiveView(bool isArchive)
    {
      this.LogControl.OnShowEditChanged(!isArchive);
      this.HabitMonthControl.DayCells.IsEnabled = !isArchive;
      this.ArchivedText.Visibility = isArchive ? Visibility.Visible : Visibility.Collapsed;
    }

    private void LoadDailyGoals(
      HabitModel habit,
      DateTime startDate,
      DateTime endDate,
      List<HabitCheckInModel> monthCheckIns)
    {
      if (habit.Type.ToLower() != "boolean")
      {
        this.DailyGoalsControl.Load(habit, startDate, endDate, monthCheckIns);
        this.DailyGoalsControl.Visibility = Visibility.Visible;
      }
      else
        this.DailyGoalsControl.Visibility = Visibility.Collapsed;
    }

    public void ScrollToTop() => this.Scroller.ScrollToTop();

    private void OnBackClick(object sender, MouseButtonEventArgs e)
    {
      HabitContainer parent = Utils.FindParent<HabitContainer>((DependencyObject) this);
      if (parent != null)
        parent.TryHideDetail();
      else
        Utils.FindParent<ListViewContainer>((DependencyObject) this)?.TryExtractDetail();
    }

    private async void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      HabitDetailControl habitDetailControl = this;
      HabitModel habit = await HabitDao.GetHabitById(habitDetailControl._habitId);
      List<OperationItemViewModel> typeList;
      if (habit == null)
      {
        habit = (HabitModel) null;
        typeList = (List<OperationItemViewModel>) null;
      }
      else
      {
        if (habit.Status != 1)
        {
          typeList = new List<OperationItemViewModel>()
          {
            new OperationItemViewModel(ActionType.EditHabit),
            new OperationItemViewModel(ActionType.Archive),
            new OperationItemViewModel(ActionType.Delete)
          };
          if (LocalSettings.Settings.EnableFocus)
          {
            HabitCheckInModel byHabitIdAndStamp = await HabitCheckInDao.GetHabitCheckInByHabitIdAndStamp(habitDetailControl._habitId, DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
            if (byHabitIdAndStamp == null || !byHabitIdAndStamp.IsComplete() && !byHabitIdAndStamp.IsUnComplete())
              typeList.Insert(1, new OperationItemViewModel(ActionType.StartFocus)
              {
                SubActions = new List<OperationItemViewModel>()
                {
                  new OperationItemViewModel(ActionType.StartPomo)
                  {
                    Enable = !TickFocusManager.Working || LocalSettings.Settings.PomoType == FocusConstance.Focus
                  },
                  new OperationItemViewModel(ActionType.StartTiming)
                  {
                    Enable = !TickFocusManager.Working || LocalSettings.Settings.PomoType == FocusConstance.Timing
                  }
                }
              });
          }
        }
        else
          typeList = new List<OperationItemViewModel>()
          {
            new OperationItemViewModel(ActionType.RecoverHabit),
            new OperationItemViewModel(ActionType.Delete)
          };
        OperationDialog operationDialog = new OperationDialog(habit.Id, typeList, habitDetailControl.MorePopup);
        operationDialog.Operated += new EventHandler<KeyValuePair<string, ActionType>>(habitDetailControl.OnOptionClick);
        operationDialog.Show();
        habit = (HabitModel) null;
        typeList = (List<OperationItemViewModel>) null;
      }
    }

    private void OnOptionClick(object sender, KeyValuePair<string, ActionType> e)
    {
      switch (e.Value)
      {
        case ActionType.Archive:
          this.ChangeHabitArchiveStatus(e.Key, true);
          break;
        case ActionType.RecoverHabit:
          this.ChangeHabitArchiveStatus(e.Key, false);
          break;
        case ActionType.EditHabit:
          this.ShowEditWindow(e.Key);
          break;
        case ActionType.Delete:
          this.OnDeleteClick(e.Key);
          break;
        case ActionType.StartTiming:
          UserActCollectUtils.AddClickEvent("focus", "start_from", "habit_detail");
          TickFocusManager.TryStartFocusHabit(e.Key, false);
          break;
        case ActionType.StartPomo:
          UserActCollectUtils.AddClickEvent("focus", "start_from", "habit_detail");
          TickFocusManager.TryStartFocusHabit(e.Key, true);
          break;
      }
    }

    private async void OnDeleteClick(string habitId)
    {
      HabitDetailControl sender = this;
      if (!new CustomerDialog(Utils.GetString("DeleteHabit"), Utils.GetString("DeleteHabitMakeSure"), Utils.GetString("Delete"), Utils.GetString("Cancel"), Window.GetWindow((DependencyObject) sender)).ShowDialog().GetValueOrDefault())
        return;
      UserActCollectUtils.AddClickEvent("habit", "detail_om", "delete");
      EventHandler hideDetail = sender.HideDetail;
      if (hideDetail != null)
        hideDetail((object) sender, (EventArgs) null);
      await HabitDao.DeleteHabit(habitId);
    }

    private async void ChangeHabitArchiveStatus(string habitId, bool isArchive)
    {
      HabitDetailControl sender = this;
      if (!isArchive)
      {
        if (!await HabitUtils.CheckHabitLimit())
          return;
      }
      Utils.Toast(Utils.GetString(isArchive ? "Archived" : "Recovered"));
      UserActCollectUtils.AddClickEvent("habit", "detail_om", isArchive ? "archive" : "restore");
      EventHandler hideDetail = sender.HideDetail;
      if (hideDetail != null)
        hideDetail((object) sender, (EventArgs) null);
      await HabitDao.ChangeHabitArchiveStatus(habitId, isArchive);
      DataChangedNotifier.NotifyHabitsChanged();
      HabitSyncService.CommitHabits();
    }

    private async void ShowEditWindow(string habitId)
    {
      HabitDetailControl habitDetailControl = this;
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById == null)
        return;
      UserActCollectUtils.AddClickEvent("habit", "detail_om", "edit");
      AddOrEditHabitDialog orEditHabitDialog = new AddOrEditHabitDialog(habitById);
      orEditHabitDialog.Owner = Window.GetWindow((DependencyObject) habitDetailControl);
      orEditHabitDialog.ShowDialog();
    }

    public string GetHabitId() => this._habitId;

    public void ClearEvent() => this.HideDetail = (EventHandler) null;

    public void ShowBackMenu() => this.BackGrid.Visibility = Visibility.Visible;

    public void HideBackMenu() => this.BackGrid.Visibility = Visibility.Collapsed;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitdetailcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Scroller = (ScrollViewer) target;
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          this.BackGrid = (Border) target;
          this.BackGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBackClick);
          break;
        case 4:
          this.ArchivedText = (TextBlock) target;
          break;
        case 5:
          this.MoreImage = (HoverIconButton) target;
          break;
        case 6:
          this.MorePopup = (EscPopup) target;
          break;
        case 7:
          this.StatisticsView = (HabitStatisticsView) target;
          break;
        case 8:
          this.CompletedCyclesControl = (HabitCompletedCyclesControl) target;
          break;
        case 9:
          this.HabitMonthControl = (HabitMonthGrid) target;
          break;
        case 10:
          this.DailyGoalsControl = (HabitCheckinChartControl) target;
          break;
        case 11:
          this.LogControl = (HabitLogControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
