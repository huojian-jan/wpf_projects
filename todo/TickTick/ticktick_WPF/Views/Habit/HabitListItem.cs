// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitListItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitListItem : Grid, IComponentConnector, IStyleConnector
  {
    private HabitListControl _listParent;
    private UIElement _clickElement;
    private HabitDayCheckModel _manualDayCheck;
    private bool _isInChecking;
    private bool _canShowPop;
    internal DockPanel Container;
    internal Grid IconGrid;
    internal Ellipse IconEllipse;
    internal EmjTextBlock IconText;
    internal Border IconHoverBorder;
    internal ItemsControl CheckInItems;
    internal EmjTextBlock NameTextBlock;
    internal TextBlock CompletedCyclesDisplayStrTextBlock;
    internal Line BottomLine;
    private bool _contentLoaded;

    public HabitItemViewModel Model => this.DataContext as HabitItemViewModel;

    public HabitListItem() => this.InitializeComponent();

    private async void OnCheckItemClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (!(sender is FrameworkElement ui))
      {
        ui = (FrameworkElement) null;
      }
      else
      {
        ui.IsEnabled = false;
        try
        {
          if (this._isInChecking)
          {
            ui = (FrameworkElement) null;
          }
          else
          {
            HabitItemViewModel model = this.Model;
            if (model == null)
            {
              ui = (FrameworkElement) null;
            }
            else
            {
              this._isInChecking = true;
              HabitDayCheckModel dayCheck = (HabitDayCheckModel) null;
              this._clickElement = (UIElement) null;
              if (sender is Border border && border.DataContext is HabitDayCheckModel dataContext)
              {
                this._clickElement = (UIElement) border;
                dayCheck = dataContext;
              }
              if (sender.Equals((object) this.IconGrid))
              {
                ObservableCollection<HabitDayCheckModel> weekCheckIns = model.WeekCheckIns;
                // ISSUE: explicit non-virtual call
                if ((weekCheckIns != null ? (__nonvirtual (weekCheckIns.Count) == 7 ? 1 : 0) : 0) != 0)
                {
                  this._clickElement = (UIElement) this.IconGrid;
                  dayCheck = model.WeekCheckIns[6];
                }
              }
              if (dayCheck != null && dayCheck.CheckInModel != null && this._clickElement != null)
              {
                try
                {
                  if (dayCheck.Status != 1 && dayCheck.CheckInModel.Value < dayCheck.CheckInModel.Goal)
                    await this.CheckHabit(dayCheck);
                  else
                    await this.ResetHabit(dayCheck);
                  await model.UpdateTotalCheckDaysAndCurrentStreak();
                }
                finally
                {
                  this._isInChecking = false;
                }
              }
              this._isInChecking = false;
              model = (HabitItemViewModel) null;
              ui = (FrameworkElement) null;
            }
          }
        }
        finally
        {
          ui.IsEnabled = true;
        }
      }
    }

    private async Task ResetHabit(HabitDayCheckModel dayCheck)
    {
      HabitListItem sender = this;
      if (dayCheck.IsBooleanHabit || Math.Abs(sender.Model.Habit.Step) < 0.001)
      {
        if (sender._clickElement.Equals((object) sender.IconGrid))
          sender.GetParent()?.ShowAddStepPop(sender._clickElement, Utils.GetString("Reseted"));
        dayCheck.CheckInModel.Value = 0.0;
        dayCheck.Percent = 0.0;
        dayCheck.Status = 0;
        await Task.Delay(10);
      }
      if (!await HabitService.ResetCheckInHabit(sender.Model.Id, dayCheck.Date, sender.Model.Habit.Type, (object) sender))
        return;
      dayCheck.SetData((HabitCheckInModel) null, sender.Model.Habit);
    }

    private async Task CheckHabit(HabitDayCheckModel dayCheck)
    {
      HabitModel habit = this.Model.Habit;
      if (habit == null)
        return;
      if (habit.IsBoolHabit())
      {
        await this.CheckInBooleanHabit(dayCheck);
        if (!this._clickElement.Equals((object) this.IconGrid))
          return;
        this.GetParent()?.ShowAddStepPop(this._clickElement, Utils.GetString("Done"));
      }
      else if (Math.Abs(habit.Step + 1.0) <= 0.001)
        this.ManuallyRecordCheckIn(dayCheck);
      else
        await this.CheckInRealHabit(dayCheck, habit.Step);
    }

    public async Task ManuallyRecordCheckIn(HabitDayCheckModel dayCheck)
    {
      HabitListItem habitListItem = this;
      ManualRecordCheckinControl manual;
      if (dayCheck == null)
      {
        manual = (ManualRecordCheckinControl) null;
      }
      else
      {
        habitListItem._manualDayCheck = dayCheck;
        dayCheck.Select = true;
        int num = 7 - (DateTime.Today - dayCheck.Date).Days;
        EscPopup escPopup = new EscPopup();
        escPopup.PlacementTarget = (UIElement) habitListItem.CheckInItems;
        escPopup.Placement = PlacementMode.Bottom;
        escPopup.StaysOpen = false;
        escPopup.HorizontalOffset = (double) (num * 18 - 5);
        escPopup.VerticalOffset = -20.0;
        escPopup.PopupAnimation = PopupAnimation.Fade;
        EscPopup popup = escPopup;
        manual = new ManualRecordCheckinControl();
        popup.Child = (UIElement) manual;
        manual.Init(habitListItem.Model.Habit.Unit);
        popup.Closed += new EventHandler(habitListItem.CancelSelect);
        manual.Save += new EventHandler<double>(habitListItem.SaveManualCheck);
        manual.Save += (EventHandler<double>) ((obj, e) => popup.IsOpen = false);
        manual.Cancel += (EventHandler) ((obj, e) => popup.IsOpen = false);
        await Task.Delay(10);
        popup.IsOpen = true;
        Utils.Focus((UIElement) manual.CheckInText);
        Keyboard.Focus((IInputElement) manual.CheckInText);
        HwndHelper.SetFocus((Popup) popup, false);
        manual = (ManualRecordCheckinControl) null;
      }
    }

    public void CancelSelect(object sender, EventArgs e)
    {
      if (this._manualDayCheck != null)
      {
        this._manualDayCheck.Select = false;
        this._manualDayCheck = (HabitDayCheckModel) null;
      }
      this._isInChecking = false;
    }

    public async void SaveManualCheck(object sender, double step)
    {
      if (this._manualDayCheck == null || step <= 0.0)
        return;
      await this.CheckInRealHabit(this._manualDayCheck, step);
    }

    private async Task CheckInRealHabit(HabitDayCheckModel dayCheck, double step)
    {
      HabitListItem sender = this;
      Utils.PlayCompletionSound();
      dayCheck.CheckInModel.Value += step > 0.0 ? step : dayCheck.CheckInModel.Goal;
      if (dayCheck.CheckInModel.Value > dayCheck.CheckInModel.Goal)
        dayCheck.CheckInModel.Value = dayCheck.CheckInModel.Goal;
      dayCheck.Percent = (double) Math.Min((int) (dayCheck.CheckInModel.Value / dayCheck.CheckInModel.Goal * 100.0), 100);
      if (dayCheck.Percent >= 100.0)
        dayCheck.Status = 2;
      if (step > 0.0)
      {
        string toastText = "+" + step.ToString() + " " + HabitUtils.GetUnitText(sender.Model.Habit.Unit);
        if (sender._clickElement != null && sender._clickElement == sender.IconGrid && dayCheck.CheckInModel.Value >= dayCheck.CheckInModel.Goal)
          toastText = Utils.GetString("Done");
        sender.GetParent()?.ShowAddStepPop(sender._clickElement, toastText);
      }
      else if (Math.Abs(step) <= 0.0 && sender._clickElement != null && sender._clickElement == sender.IconGrid)
        sender.GetParent()?.ShowAddStepPop(sender._clickElement, Utils.GetString("Done"));
      await Task.Delay(150);
      await HabitService.CheckInHabit(sender.Model.Id, dayCheck.Date, step, sender: (object) sender);
    }

    private async Task CheckInBooleanHabit(HabitDayCheckModel dayCheck)
    {
      HabitListItem sender = this;
      if (dayCheck?.CheckInModel == null || sender.Model == null)
        return;
      dayCheck.CheckInModel.Value = dayCheck.CheckInModel.Goal;
      dayCheck.Status = 2;
      Utils.PlayCompletionSound();
      await Task.Delay(16);
      await HabitService.CheckInHabit(sender.Model.Id, dayCheck.Date, sender: (object) sender);
    }

    private HabitListControl GetParent()
    {
      this._listParent = this._listParent ?? Utils.FindParent<HabitListControl>((DependencyObject) this);
      return this._listParent;
    }

    private void OnItemSelected(object sender, MouseButtonEventArgs e)
    {
      if (this.CheckInItems.IsMouseOver || this.IconGrid.IsMouseOver || !(this.DataContext is HabitItemViewModel dataContext))
        return;
      this.GetParent()?.OnItemSelected(dataContext.Id);
    }

    private void CheckInRightClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._canShowPop)
        return;
      this._canShowPop = false;
      HabitDayCheckModel dayCheck = (HabitDayCheckModel) null;
      if (sender is Border border && border.DataContext is HabitDayCheckModel dataContext)
        dayCheck = dataContext;
      if (sender.Equals((object) this.IconGrid))
      {
        ObservableCollection<HabitDayCheckModel> weekCheckIns = this.Model.WeekCheckIns;
        // ISSUE: explicit non-virtual call
        if ((weekCheckIns != null ? (__nonvirtual (weekCheckIns.Count) == 7 ? 1 : 0) : 0) != 0)
          dayCheck = this.Model.WeekCheckIns[6];
      }
      if (dayCheck == null)
        return;
      if (!dayCheck.Uncompleted && !dayCheck.Completed)
      {
        dayCheck.Select = true;
        OperationDialog operationDialog = new OperationDialog(dayCheck.CheckInModel.HabitId, new List<OperationItemViewModel>()
        {
          new OperationItemViewModel(ActionType.UnComplete),
          new OperationItemViewModel(ActionType.EditHabitLog)
        });
        operationDialog.SetPlaceTarget((UIElement) this);
        operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) ((obj, arg) =>
        {
          if (arg.Value == ActionType.UnComplete)
            this.UnCompleteHabit(dayCheck);
          else
            HabitUtils.ShowHabitRecordWindow(this.Model.Habit.Id, this.Model.Habit.Name, dayCheck.Date, unCompleted: !dayCheck.Completed);
          dayCheck.Select = false;
        });
        operationDialog.Closed += (EventHandler) ((obj, arg) => dayCheck.Select = false);
        operationDialog.Show();
      }
      else
      {
        dayCheck.Select = true;
        OperationDialog operationDialog = new OperationDialog(this.Model.Habit.Id, new OperationItemViewModel(ActionType.EditHabitLog));
        operationDialog.SetPlaceTarget((UIElement) this);
        operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) (async (obj, arg) =>
        {
          HabitUtils.ShowHabitRecordWindow(this.Model.Habit.Id, this.Model.Habit.Name, dayCheck.Date, unCompleted: dayCheck.Uncompleted);
          dayCheck.Select = false;
        });
        operationDialog.Closed += (EventHandler) ((obj, arg) => dayCheck.Select = false);
        operationDialog.Show();
      }
    }

    private void UnCompleteHabit(HabitDayCheckModel dayCheck)
    {
      HabitItemViewModel model = this.Model;
      if (dayCheck == null || model?.Habit == null)
        return;
      dayCheck.Status = 1;
      HabitService.UncompleteHabit(model.Habit.Id, dayCheck.Date);
    }

    private async void ShowOptionPopup(object sender, MouseButtonEventArgs e)
    {
      HabitListItem ele = this;
      HabitItemViewModel model;
      List<OperationItemViewModel> typeList;
      if (ele.CheckInItems.IsMouseOver)
      {
        model = (HabitItemViewModel) null;
        typeList = (List<OperationItemViewModel>) null;
      }
      else
      {
        model = ele.Model;
        if (!ele._canShowPop)
        {
          model = (HabitItemViewModel) null;
          typeList = (List<OperationItemViewModel>) null;
        }
        else if (model?.Habit == null)
        {
          model = (HabitItemViewModel) null;
          typeList = (List<OperationItemViewModel>) null;
        }
        else
        {
          ele._canShowPop = false;
          if (model.Habit.Status != 1)
          {
            typeList = new List<OperationItemViewModel>()
            {
              new OperationItemViewModel(ActionType.EditHabit),
              new OperationItemViewModel(ActionType.Archive),
              new OperationItemViewModel(ActionType.Delete)
            };
            if (LocalSettings.Settings.EnableFocus)
            {
              HabitCheckInModel byHabitIdAndStamp = await HabitCheckInDao.GetHabitCheckInByHabitIdAndStamp(model.Habit.Id, DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
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
          OperationDialog operationDialog = new OperationDialog(model.Habit.Id, typeList);
          operationDialog.SetPlaceTarget((UIElement) ele);
          operationDialog.Operated += new EventHandler<KeyValuePair<string, ActionType>>(ele.OnOptionClick);
          operationDialog.Show();
          model = (HabitItemViewModel) null;
          typeList = (List<OperationItemViewModel>) null;
        }
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
          this.OnEditClick(e.Key);
          break;
        case ActionType.Delete:
          this.OnDeleteClick(e.Key);
          break;
        case ActionType.StartTiming:
          UserActCollectUtils.AddClickEvent("focus", "start_from", "cm_single_habit");
          TickFocusManager.TryStartFocusHabit(e.Key, false);
          break;
        case ActionType.StartPomo:
          UserActCollectUtils.AddClickEvent("focus", "start_from", "cm_single_habit");
          TickFocusManager.TryStartFocusHabit(e.Key, true);
          break;
      }
    }

    private async void OnEditClick(string habitId)
    {
      HabitListItem habitListItem = this;
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById == null)
        return;
      UserActCollectUtils.AddClickEvent("habit", "list_cm", "edit");
      AddOrEditHabitDialog orEditHabitDialog = new AddOrEditHabitDialog(habitById);
      orEditHabitDialog.Owner = Window.GetWindow((DependencyObject) habitListItem);
      orEditHabitDialog.ShowDialog();
    }

    private async void ChangeHabitArchiveStatus(string habitId, bool isArchive)
    {
      if (!isArchive)
      {
        if (!await HabitUtils.CheckHabitLimit())
          return;
      }
      Utils.Toast(Utils.GetString(isArchive ? "Archived" : "Recovered"));
      UserActCollectUtils.AddClickEvent("habit", "list_cm", isArchive ? "archive" : "restore");
      this.GetParent()?.RemoveItem(habitId);
      await Task.Delay(100);
      await HabitDao.ChangeHabitArchiveStatus(habitId, isArchive);
      DataChangedNotifier.NotifyHabitsChanged();
      HabitSyncService.CommitHabits();
    }

    private async void OnDeleteClick(string habitId)
    {
      HabitListItem habitListItem = this;
      if (!new CustomerDialog(Utils.GetString("DeleteHabit"), Utils.GetString("DeleteHabitMakeSure"), Utils.GetString("Delete"), Utils.GetString("Cancel"), Window.GetWindow((DependencyObject) habitListItem)).ShowDialog().GetValueOrDefault())
        return;
      UserActCollectUtils.AddClickEvent("habit", "list_cm", "delete");
      habitListItem.GetParent()?.RemoveItem(habitId);
      await HabitDao.DeleteHabit(habitId);
    }

    private void OnMouseRightDown(object sender, MouseButtonEventArgs e) => this._canShowPop = true;

    private void OnSwitchShowInfoClicked(object sender, MouseButtonEventArgs e)
    {
      HabitItemDisplayModel.DisplayModel.ShowCurrentStreak = !HabitItemDisplayModel.DisplayModel.ShowCurrentStreak;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (this.Model == null)
        return;
      this.Model.SelfControl = this;
      this.IconHoverBorder.Cursor = this.Model.Archived ? Cursors.No : Cursors.Hand;
      if (string.IsNullOrEmpty(this.Model.Color))
      {
        this.IconHoverBorder.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity10");
        this.IconText.SetResourceReference(TextBlock.ForegroundProperty, (object) "PrimaryColor");
        this.IconEllipse.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor10");
      }
      else
      {
        this.IconHoverBorder.Background = (Brush) ThemeUtil.GetColorInString("#20191919");
        this.IconText.Foreground = (Brush) Brushes.White;
        this.IconEllipse.Fill = (Brush) ThemeUtil.GetColorInString(this.Model.Color);
      }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (!e.WidthChanged)
        return;
      double num = this.ActualWidth - 280.0 - this.CompletedCyclesDisplayStrTextBlock.ActualWidth;
      this.NameTextBlock.MaxWidth = num > 0.0 ? num : 0.0;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitlistitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
          ((FrameworkElement) target).SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
          break;
        case 3:
          this.Container = (DockPanel) target;
          this.Container.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemSelected);
          this.Container.PreviewMouseRightButtonDown += new MouseButtonEventHandler(this.OnMouseRightDown);
          this.Container.PreviewMouseRightButtonUp += new MouseButtonEventHandler(this.ShowOptionPopup);
          break;
        case 4:
          this.IconGrid = (Grid) target;
          this.IconGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckItemClick);
          break;
        case 5:
          this.IconEllipse = (Ellipse) target;
          break;
        case 6:
          this.IconText = (EmjTextBlock) target;
          break;
        case 7:
          this.IconHoverBorder = (Border) target;
          break;
        case 8:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSwitchShowInfoClicked);
          break;
        case 9:
          this.CheckInItems = (ItemsControl) target;
          break;
        case 10:
          this.NameTextBlock = (EmjTextBlock) target;
          break;
        case 11:
          this.CompletedCyclesDisplayStrTextBlock = (TextBlock) target;
          break;
        case 12:
          this.BottomLine = (Line) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckItemClick);
      ((UIElement) target).MouseRightButtonUp += new MouseButtonEventHandler(this.CheckInRightClick);
    }
  }
}
