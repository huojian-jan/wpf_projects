// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitCheckInWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitCheckInWindow : EscPopup, IComponentConnector
  {
    private readonly HabitModel _habit;
    private readonly string _stamp;
    private readonly int _status;
    private HabitAction _action = HabitAction.None;
    private double _amount;
    private IToastShowWindow _toastWindow;
    private DateTime _date;
    internal Border BaseBar;
    internal StackPanel ActionPanel;
    internal Popup ManuallyCheckInPopup;
    internal ManualRecordCheckinControl CheckInControl;
    internal Grid AutoCheckInGrid;
    internal Grid AutoCheckInGridWithReset;
    internal Grid ManualCheckInGrid;
    internal Button ManualCheckInButton;
    internal Button CheckInButton;
    internal Button ResetButton;
    private bool _contentLoaded;

    public HabitCheckInWindow()
    {
      this.InitializeComponent();
      this.Closed += (EventHandler) ((sender, args) =>
      {
        PopupStateManager.OnViewPopupClosed();
        EventHandler onAction = this.OnAction;
        if (onAction != null)
          onAction((object) this, (EventArgs) null);
        this.OnCheckInAction();
      });
    }

    public HabitCheckInWindow(
      HabitModel habit,
      int status,
      DateTime? startDate,
      IToastShowWindow toastWindow = null)
    {
      this.InitializeComponent();
      this._habit = habit;
      this._toastWindow = toastWindow;
      this._status = status;
      this._date = startDate ?? DateTime.Today;
      if (startDate.HasValue)
        this._stamp = startDate.Value.ToString("yyyyMMdd");
      this.Closed += (EventHandler) ((sender, args) =>
      {
        PopupStateManager.OnViewPopupClosed();
        EventHandler onAction = this.OnAction;
        if (onAction != null)
          onAction((object) this, (EventArgs) null);
        this.OnCheckInAction();
      });
    }

    public event EventHandler OnAction;

    private async Task InitData()
    {
      HabitCheckInWindow habitCheckInWindow = this;
      HabitDetailViewModel vm = new HabitDetailViewModel(habitCheckInWindow._habit)
      {
        PivotDate = habitCheckInWindow._date,
        Status = habitCheckInWindow._status
      };
      habitCheckInWindow.InitOperationButton(vm.Status, vm.Type, vm.Step);
      HabitCheckInModel byHabitIdAndStamp = await HabitCheckInDao.GetHabitCheckInsByHabitIdAndStamp(habitCheckInWindow._habit.Id, habitCheckInWindow._stamp);
      if (byHabitIdAndStamp != null)
      {
        vm.Value = byHabitIdAndStamp.Value;
        vm.CompletedRatio = (int) (byHabitIdAndStamp.Value * 1.0 / habitCheckInWindow._habit.Goal * 1.0 * 100.0);
      }
      habitCheckInWindow.DataContext = (object) vm;
      vm = (HabitDetailViewModel) null;
    }

    private void InitOperationButton(int status, string type, double step)
    {
      switch (HabitUtils.GetHabitCheckInType(status, type, step))
      {
        case HabitCheckInType.BoolCompleted:
        case HabitCheckInType.CompletedAllCompleted:
          this.ResetButton.Visibility = Visibility.Visible;
          break;
        case HabitCheckInType.BoolUnCompleted:
        case HabitCheckInType.CompletedAllUnCompleted:
          this.CheckInButton.Visibility = Visibility.Visible;
          break;
        case HabitCheckInType.AutoCompleted:
          this.AutoCheckInGridWithReset.Visibility = Visibility.Visible;
          break;
        case HabitCheckInType.AutoUncompleted:
          this.AutoCheckInGrid.Visibility = Visibility.Visible;
          break;
        case HabitCheckInType.ManualCompleted:
          this.ManualCheckInGrid.Visibility = Visibility.Visible;
          break;
        case HabitCheckInType.ManualUnCompleted:
          this.ManualCheckInButton.Visibility = Visibility.Visible;
          break;
      }
    }

    private async Task OnCheckInAction()
    {
      HabitCheckInWindow habitCheckInWindow = this;
      if (habitCheckInWindow.DataContext != null && habitCheckInWindow.DataContext is HabitDetailViewModel dataContext)
      {
        bool checkLog = dataContext.Status == 0;
        switch (habitCheckInWindow._action)
        {
          case HabitAction.CheckIn:
            Utils.PlayCompletionSound();
            await HabitService.CheckInHabit(dataContext.Id, DateTime.Today, window: habitCheckInWindow._toastWindow);
            break;
          case HabitAction.Reset:
            int num = await HabitService.ResetCheckInHabit(dataContext.Id, dataContext.PivotDate, dataContext.Type) ? 1 : 0;
            break;
          case HabitAction.ManualRecord:
            habitCheckInWindow._toastWindow?.TryToastString((object) null, "+" + habitCheckInWindow._amount.ToString() + " " + HabitUtils.GetUnitText(dataContext.Unit));
            await habitCheckInWindow.CheckInRealHabit(dataContext, habitCheckInWindow._amount, checkLog);
            break;
          case HabitAction.AutoRecord:
            habitCheckInWindow._toastWindow?.TryToastString((object) null, "+" + dataContext.Step.ToString() + " " + HabitUtils.GetUnitText(dataContext.Unit));
            await habitCheckInWindow.CheckInRealHabit(dataContext, dataContext.Step, checkLog);
            break;
        }
      }
      if (!(habitCheckInWindow._toastWindow is Window window))
      {
        window = (Window) null;
      }
      else
      {
        await Task.Delay(10);
        if (PopupStateManager.IsViewPopOpened())
        {
          window = (Window) null;
        }
        else
        {
          window.Activate();
          window = (Window) null;
        }
      }
    }

    private async Task CheckInRealHabit(HabitDetailViewModel model, double step, bool checkLog = true)
    {
      Utils.PlayCompletionSound();
      if (model.TodayCheckIn == null)
      {
        model.TodayCheckIn = new HabitCheckInModel()
        {
          Goal = model.Goal,
          Value = step
        };
      }
      else
      {
        model.TodayCheckIn.Value += step;
        model.TodayCheckIn = model.TodayCheckIn;
      }
      await HabitService.CheckInHabit(model.Id, model.PivotDate, step, checkShowRecord: checkLog, window: this._toastWindow);
      await Task.Delay(1000);
      if (model.TodayCheckIn.Value >= model.Goal)
        return;
      model.Status = 0;
    }

    public async void Show(UIElement target, double targetWidth, double addHeight, bool byMouse)
    {
      HabitCheckInWindow habitCheckInWindow = this;
      await habitCheckInWindow.InitData();
      PopupStateManager.OnViewPopupOpened();
      habitCheckInWindow.PlacementTarget = target;
      // ISSUE: explicit non-virtual call
      TaskPopupArgs popupLocation = PopupLocationCalculator.GetPopupLocation(target, targetWidth, __nonvirtual (habitCheckInWindow.Width), byMouse, addHeight);
      if (!popupLocation.ByMouse)
      {
        habitCheckInWindow.Placement = popupLocation.IsRight ? PlacementMode.Right : PlacementMode.Left;
        habitCheckInWindow.HorizontalOffset = popupLocation.IsRight ? -6.0 : 6.0;
        habitCheckInWindow.VerticalOffset = -8.0;
      }
      else
        habitCheckInWindow.Placement = PlacementMode.Mouse;
      habitCheckInWindow.IsOpen = true;
      await Task.Delay(200);
      if (!habitCheckInWindow.IsOpen)
        return;
      PopupStateManager.OnViewPopupOpened();
    }

    private void OnAutoCheckInClick(object sender, RoutedEventArgs e)
    {
      this._action = HabitAction.AutoRecord;
      this.IsOpen = false;
    }

    private void OnCheckInClick(object sender, RoutedEventArgs e)
    {
      this._action = HabitAction.CheckIn;
      this.IsOpen = false;
    }

    private void OnResetClick(object sender, RoutedEventArgs e)
    {
      this._action = HabitAction.Reset;
      this.IsOpen = false;
    }

    private void OnManualCheckInClick(object sender, RoutedEventArgs e)
    {
      this.ManuallyRecordCheckIn();
    }

    private void ManuallyRecordCheckIn()
    {
      this.ManuallyCheckInPopup.IsOpen = true;
      this.CheckInControl.Init(this._habit.Unit);
      this.CheckInControl.Cancel -= new EventHandler(this.OnCheckInPopupCancel);
      this.CheckInControl.Cancel += new EventHandler(this.OnCheckInPopupCancel);
      this.CheckInControl.Save -= new EventHandler<double>(this.OnCheckInPopupSave);
      this.CheckInControl.Save += new EventHandler<double>(this.OnCheckInPopupSave);
    }

    private void OnCheckInPopupSave(object sender, double amount)
    {
      this.ManuallyCheckInPopup.IsOpen = false;
      this._amount = amount;
      this._action = HabitAction.ManualRecord;
      this.IsOpen = false;
    }

    private void OnCheckInPopupCancel(object sender, EventArgs e)
    {
      this.ManuallyCheckInPopup.IsOpen = false;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitcheckinwindow.xaml", UriKind.Relative));
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
          this.BaseBar = (Border) target;
          break;
        case 2:
          this.ActionPanel = (StackPanel) target;
          break;
        case 3:
          this.ManuallyCheckInPopup = (Popup) target;
          break;
        case 4:
          this.CheckInControl = (ManualRecordCheckinControl) target;
          break;
        case 5:
          this.AutoCheckInGrid = (Grid) target;
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnAutoCheckInClick);
          break;
        case 7:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnManualCheckInClick);
          break;
        case 8:
          this.AutoCheckInGridWithReset = (Grid) target;
          break;
        case 9:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnAutoCheckInClick);
          break;
        case 10:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnManualCheckInClick);
          break;
        case 11:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnResetClick);
          break;
        case 12:
          this.ManualCheckInGrid = (Grid) target;
          break;
        case 13:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnManualCheckInClick);
          break;
        case 14:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnResetClick);
          break;
        case 15:
          this.ManualCheckInButton = (Button) target;
          this.ManualCheckInButton.Click += new RoutedEventHandler(this.OnManualCheckInClick);
          break;
        case 16:
          this.CheckInButton = (Button) target;
          this.CheckInButton.Click += new RoutedEventHandler(this.OnCheckInClick);
          break;
        case 17:
          this.ResetButton = (Button) target;
          this.ResetButton.Click += new RoutedEventHandler(this.OnResetClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
