// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.CalendarWidget
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.TouchPad;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Undo;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class CalendarWidget : UserControl, IToastShowWindow, IWidgetChild, IComponentConnector
  {
    private WidgetWindow _parentWindow;
    private const int WM_MOUSEHWHEEL = 526;
    public WidgetSettings _currentWidgetSettings;
    internal Grid UndoToastGrid;
    internal CalendarControl CalendarControl;
    private bool _contentLoaded;

    public CalendarWidget(WidgetViewModel model)
    {
      this.DataContext = (object) model;
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnWindowLoaded);
      this.InitEvents();
    }

    private WidgetViewModel Model => (WidgetViewModel) this.DataContext;

    public string ThemeId => this.Model.ThemeId;

    private WidgetWindow GetParentWindow()
    {
      this._parentWindow = this._parentWindow ?? Utils.FindParent<WidgetWindow>((DependencyObject) this);
      return this._parentWindow;
    }

    public void TryHideToast()
    {
      if (this.UndoToastGrid.Children.Count <= 0)
        return;
      if (this.UndoToastGrid.Children[0] is UndoToast child)
        child.OnFinished();
      this.UndoToastGrid.Children[0].Visibility = Visibility.Collapsed;
    }

    internal void CheckCalendarProEnable()
    {
      this.Dispatcher.Invoke(new Action(this.CalendarControl.ShowProToast));
    }

    public void TryToastString(object obj, string toastString)
    {
      WindowToastHelper.ToastString(this.UndoToastGrid, toastString);
    }

    public async Task<bool> BatchDeleteTask(List<TaskModel> tasks)
    {
      this.CalendarControl.GetFocus();
      return await TaskService.BatchDeleteTasks(tasks, undoGrid: this.UndoToastGrid);
    }

    public void TaskComplete(CloseUndoToast undo)
    {
    }

    public async void TaskDeleted(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task != null)
      {
        task.deleted = 1;
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnDeletedChanged(task);
        UndoToast uiElement = new UndoToast();
        uiElement.InitTaskUndo(taskId, task.title);
        WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, (FrameworkElement) uiElement);
      }
      this.CalendarControl.GetFocus();
      task = (TaskModel) null;
    }

    public void ToastDeleteRecUndo(List<TaskDeleteRecurrenceUndoEntity> undoModels)
    {
      if (undoModels == null || !undoModels.Any<TaskDeleteRecurrenceUndoEntity>())
        return;
      UndoToast undoToast = new UndoToast();
      undoToast.InitTaskUndo(undoModels[0]);
      this.ShowUndoToast(undoToast);
    }

    public async Task ShowUndoToast(UndoToast undoToast)
    {
      WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, (FrameworkElement) undoToast);
      this.CalendarControl.GetFocus();
    }

    public void Toast(FrameworkElement uiElement)
    {
      WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, uiElement);
    }

    public void ToastMoveProjectControl(string projectId, string taskName = null, MoveToastType moveType = MoveToastType.Move)
    {
      WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, (FrameworkElement) new MoveToastControl(projectId, (INavigateProject) null, taskName, moveType));
    }

    private void InitEvents()
    {
      this.CalendarControl.HeadView.MoreButton.Visibility = Visibility.Visible;
      this.CalendarControl.HeadView.MoreAction += new EventHandler<WidgetMoreAction>(this.OnMoreAction);
      this.CalendarControl.HeadView.DragPanel.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnDragBarMouseDown);
      this.CalendarControl.HeadView.DragPanel.MouseMove += new MouseEventHandler(this.OnDragMove);
      this.CalendarControl.HeadView.DragPanel.Cursor = Cursors.SizeAll;
      this.CalendarControl.HeadView.Height = 46.0;
      this.CalendarControl.ArrangePanel.SwitchTitle.BackBorder.Opacity = 0.2;
      this.CalendarControl.HeadView.UnlockButton.Click += new RoutedEventHandler(this.OnUnlockWidgetClick);
      this.CalendarControl.HeadView.LockedSyncButton.Click += new RoutedEventHandler(this.OnLockedSyncClick);
      this.CalendarControl.Background = (Brush) Brushes.Transparent;
    }

    private void OnDragMove(object sender, MouseEventArgs e)
    {
      this.GetParentWindow()?.OnDragMove(sender, e);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      this.Resources[(object) "MonthDayTextFontSize"] = (object) 12.0;
      this.Resources[(object) "MonthDayTextMargin"] = (object) new Thickness(2.0, 6.0, 0.0, 0.0);
      this.Resources[(object) "WeekDayTextMargin"] = (object) new Thickness(4.0, 48.0, 0.0, 0.0);
      this.CalendarControl.GotoDate(new DateTime?());
      this.CheckCalendarProEnable();
      PresentationSource presentationSource = PresentationSource.FromVisual((Visual) this);
      if (presentationSource != null)
      {
        ((HwndSource) presentationSource).AddHook(new HwndSourceHook(this.Hook));
        if (TouchPadHelper.TouchpadHelper.Exists())
          TouchPadHelper.TouchpadHelper.RegisterInput(((HwndSource) presentationSource).Handle);
      }
      this.LockWidget(this.Model.IsLocked);
    }

    private void OnDragBarMouseDown(object sender, MouseButtonEventArgs e)
    {
      this.GetParentWindow()?.OnDragBarMouseDown(sender, e);
    }

    private void SyncTask()
    {
      this.CalendarControl.HeadView.BeginSyncStory();
      if (!Utils.IsNetworkAvailable())
        this.TryToastString((object) null, Utils.GetString("NoNetwork"));
      SyncManager.Sync(1);
    }

    private void OnMoreAction(object sender, WidgetMoreAction e)
    {
      switch (e)
      {
        case WidgetMoreAction.Sync:
          this.SyncTask();
          break;
        case WidgetMoreAction.Setting:
          this.BindSettingEvent(CalendarConfigHelper.TryShowSettings(this.Model));
          break;
        case WidgetMoreAction.Lock:
          this.LockWidget(true);
          this._currentWidgetSettings?.Close();
          break;
        case WidgetMoreAction.Exit:
          this.CloseWidget();
          break;
      }
    }

    private void LockWidget(bool isLocked)
    {
      this.CalendarControl.HeadView.DragPanel.IsEnabled = !isLocked;
      this.CalendarControl.HeadView.LockedOptionGrid.Visibility = isLocked ? Visibility.Visible : Visibility.Collapsed;
      this.CalendarControl.HeadView.UnlockOptionGrid.Visibility = isLocked ? Visibility.Collapsed : Visibility.Visible;
      this.GetParentWindow().ResizeMode = isLocked ? ResizeMode.NoResize : ResizeMode.CanResize;
      this.Model.IsLocked = isLocked;
      this.Model.SaveSingleModel();
      this.CalendarControl.IsLocked = isLocked;
    }

    private void OnUnlockWidgetClick(object sender, RoutedEventArgs e) => this.LockWidget(false);

    private void OnLockedSyncClick(object sender, RoutedEventArgs e) => this.SyncTask();

    public void BindSettingEvent(WidgetSettings settings)
    {
      this._currentWidgetSettings = settings;
      if (settings == null)
        return;
      settings.DataContext = (object) this.Model;
      settings.Closed -= new EventHandler(this.OnSettingsClosed);
      settings.Closed += new EventHandler(this.OnSettingsClosed);
      settings.DisplayOptionChanged -= new EventHandler<string>(this.OnDisplayOptionChanged);
      settings.DisplayOptionChanged += new EventHandler<string>(this.OnDisplayOptionChanged);
      settings.OpacityChanged -= new EventHandler<float>(this.OnOpacityChanged);
      settings.OpacityChanged += new EventHandler<float>(this.OnOpacityChanged);
    }

    public void RemoveSettingEvent(WidgetSettings settings)
    {
      if (settings == null)
        return;
      settings.Closed -= new EventHandler(this.OnSettingsClosed);
      settings.DisplayOptionChanged -= new EventHandler<string>(this.OnDisplayOptionChanged);
      settings.OpacityChanged -= new EventHandler<float>(this.OnOpacityChanged);
    }

    private void OnOpacityChanged(object sender, float opacity)
    {
      this.GetParentWindow()?.EnableBlur((double) opacity > 0.0);
    }

    private async void OnDisplayOptionChanged(object sender, string option)
    {
      if (option == null)
        return;
      switch (option)
      {
        case "top":
        case "embed":
        case "bottom":
          this.GetParentWindow()?.SetTopMost();
          break;
        case "light":
        case "dark":
          this.GetParentWindow()?.SetTheme(option);
          break;
      }
    }

    private async void OnSettingsClosed(object sender, EventArgs e)
    {
      await this.Model.SaveSingleModel();
    }

    private void CloseWidget() => CalendarWidgetHelper.CloseWidget();

    private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      switch (msg)
      {
        case (int) byte.MaxValue:
          TouchpadContact[] input = TouchPadHelper.TouchpadHelper.ParseInput(lParam);
          if ((input != null ? (((IEnumerable<TouchpadContact>) input).Any<TouchpadContact>((Func<TouchpadContact, bool>) (c => c.ContactId == 1)) ? 1 : 0) : 0) != 0)
          {
            this.CalendarControl.OnDoubleFingerTouch();
            break;
          }
          break;
        case 526:
          this.OnMouseTilt(CalendarWidget.LOWORD(wParam));
          return (IntPtr) 1;
      }
      return IntPtr.Zero;
    }

    private static int HIWORD(IntPtr ptr) => ptr.ToInt32() >> 16 & (int) ushort.MaxValue;

    private static int LOWORD(IntPtr ptr) => ((int) ptr.ToInt64() >> 16) % 256;

    private void OnMouseTilt(int offset) => this.CalendarControl.OnTouchScroll(offset);

    public bool IsEditing() => this.CalendarControl.IsEditting();

    public void Save() => this.Model.SaveSingleModel();

    public void TryUndo()
    {
      if (this.UndoToastGrid.Children.Count <= 0 || !(this.UndoToastGrid.Children[0] is UndoToast child) || child.Visibility != Visibility.Visible)
        return;
      child.OnUndo();
    }

    private void OnRenewClick(object sender, RoutedEventArgs e)
    {
      Utils.StartUpgrade("calendar_view");
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      CalendarWidgetHelper.CloseWidget();
    }

    public void Reload() => this.CalendarControl.Reload(true, resetDatePickerColor: true);

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/widget/calendarwidget.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.CalendarControl = (CalendarControl) target;
        else
          this._contentLoaded = true;
      }
      else
        this.UndoToastGrid = (Grid) target;
    }
  }
}
