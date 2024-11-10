// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WidgetWindow : MyWindow, IComponentConnector
  {
    public readonly ProjectWidget ProjectWidget;
    public readonly CalendarWidget CalendarWidget;
    public readonly MatrixWidget MatrixWidget;
    private readonly IWidgetChild _widgetChild;
    private Storyboard _hideAnim;
    private Storyboard _showAnim;
    private Storyboard _popInLeftAnim;
    private Storyboard _popInRightAnim;
    private Storyboard _popoutLeftAnim;
    private Storyboard _popoutRightAnim;
    private WidgetShowStatus _showState;
    private readonly DelayActionHandler _modelSaver = new DelayActionHandler(1000);
    private int _mouseMoveOverCount;
    private readonly System.Timers.Timer _statusCheckTimer = new System.Timers.Timer(150.0);
    private readonly DelayActionHandler _popoutDelayAction = new DelayActionHandler(200);
    private int _oldWindowLong;
    private WidgetLocationModel _location = new WidgetLocationModel();
    private IntPtr _desktopPointer;
    private bool _dragMouseDown;
    private bool _moveWindow;
    internal WidgetWindow ProjectWindow;
    internal Grid Container;
    internal Grid CollapseGrid;
    internal Grid StickLeftGrid;
    internal Grid StickRightGrid;
    private bool _contentLoaded;

    public WidgetViewModel Model => (WidgetViewModel) this.DataContext;

    public WidgetWindow(WidgetViewModel model)
    {
      ThemeUtil.SetTheme(model.ThemeId, (FrameworkElement) this);
      this.InitializeComponent();
      Utils.SetWindowChrome((Window) this, new Thickness(5.0));
      this.DataContext = (object) model;
      if (model.IsSingleMode)
      {
        this.MinHeight = 460.0;
        this.MinWidth = 700.0;
        this.Title = Utils.GetString("GridWidget");
        if (model.IsCal)
        {
          this.CalendarWidget = new CalendarWidget(model);
          this._widgetChild = (IWidgetChild) this.CalendarWidget;
          this.Container.Children.Clear();
          this.Container.Children.Add((UIElement) this.CalendarWidget);
        }
        else
        {
          this.MatrixWidget = new MatrixWidget(model);
          this._widgetChild = (IWidgetChild) this.MatrixWidget;
          this.Container.Children.Clear();
          this.Container.Children.Add((UIElement) this.MatrixWidget);
          model.CollapseTitle = Utils.GetString("Matrix");
        }
      }
      else
      {
        this.MinHeight = 292.0;
        this.MinWidth = 256.0;
        this.ProjectWidget = new ProjectWidget(model);
        this._widgetChild = (IWidgetChild) this.ProjectWidget;
        this.Container.Children.Clear();
        this.Container.Children.Add((UIElement) this.ProjectWidget);
      }
      this.InitSizeAndLocation();
      this.SourceInitialized += new EventHandler(this.OnWindowSourceInitialized);
      this.Loaded += new RoutedEventHandler(this.OnWidgetLoaded);
      UtilLog.Info(string.Format("ShowWidget: isSingleMode {0} isCal ", (object) model.IsSingleMode) + model.IsCal.ToString());
      this.Resources[(object) "IsDarkTheme"] = (object) (model.ThemeId == "dark");
    }

    private void OnWindowSourceInitialized(object sender, EventArgs e)
    {
      this.Topmost = this.Model.DisplayOption == "top";
      this.Activate();
      IntPtr handle = new WindowInteropHelper((Window) this).Handle;
      NativeUtils.SetWindowLong(handle, -20, 128L);
      if (!(this.Model.DisplayOption != "top"))
        return;
      IntPtr desktopPtr = NativeUtils.GetDesktopPtr();
      if (this.Model.DisplayOption == "bottom" && Screen.AllScreens.Length == 1)
      {
        PresentationSource presentationSource = PresentationSource.FromVisual((Visual) this);
        NativeUtils.SetParent(handle, desktopPtr);
        ((HwndSource) presentationSource)?.AddHook(new HwndSourceHook(this.Hook));
      }
      else
      {
        NativeUtils.SetWindowLong(handle, WinParameter.GWL_HWNDPARENT, desktopPtr.ToInt32());
        ++this.Left;
        --this.Left;
      }
    }

    private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg == 126 && Screen.AllScreens.Length > 1)
        DelayActionHandlerCenter.TryDoAction("DelayRestart", (EventHandler) ((o, e) => App.Instance.Restart()), 1000);
      return IntPtr.Zero;
    }

    public void ShowWidget() => this.Show();

    public void CloseWidget()
    {
      if (this._widgetChild is IToastShowWindow widgetChild && Utils.ToastWindow == widgetChild)
        Utils.ToastWindow = (IToastShowWindow) null;
      this.DisposeDelaySaver();
      this.Close();
      lock (this._statusCheckTimer)
      {
        this._statusCheckTimer.Close();
        this._statusCheckTimer.Dispose();
      }
    }

    private void InitSizeAndLocation()
    {
      if (this.Model == null)
        return;
      this.Width = this.Model.Width;
      this.Height = this.Model.Height;
      this.Left = this.Model.Left;
      this.Top = this.Model.Top;
    }

    private async void OnWidgetLoaded(object sender, EventArgs e)
    {
      WidgetWindow widgetWindow = this;
      WindowHelper.MoveTo((Window) widgetWindow, (int) widgetWindow.Model.Left, (int) widgetWindow.Model.Top);
      widgetWindow.Left = widgetWindow.Model.Left;
      widgetWindow.Top = widgetWindow.Model.Top;
      Matrix? transform = PresentationSource.FromVisual((Visual) widgetWindow)?.CompositionTarget?.TransformFromDevice;
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      widgetWindow._location = WidgetLocationHelper.GetLocationSafely(widgetWindow.Left, widgetWindow.Top, __nonvirtual (widgetWindow.Width), __nonvirtual (widgetWindow.Height), transform);
      widgetWindow.EnableBlur((double) widgetWindow.Model.Opacity > 0.0 && widgetWindow.Container.Opacity > 0.0);
      if (widgetWindow._location.HideType == HideType.Reset)
      {
        await Task.Delay(5000);
        // ISSUE: explicit non-virtual call
        // ISSUE: explicit non-virtual call
        widgetWindow._location = WidgetLocationHelper.GetLocationSafely(widgetWindow.Left, widgetWindow.Top, __nonvirtual (widgetWindow.Width), __nonvirtual (widgetWindow.Height), transform);
      }
      widgetWindow.InitStatusCheckTimer();
      widgetWindow.InitDelaySaver();
      if (widgetWindow.Model.DisplayOption == "top")
        widgetWindow.SetHideLocation();
      else if (widgetWindow.Model.DisplayOption != "top" && widgetWindow._location.HideType != HideType.None)
      {
        switch (widgetWindow._location.HideType)
        {
          case HideType.Top:
            widgetWindow._location.Top = widgetWindow._location.HideTopIn + widgetWindow._location.Height;
            break;
          case HideType.Left:
            widgetWindow._location.Left += widgetWindow._location.HideLeftIn + widgetWindow._location.Width;
            break;
          case HideType.Right:
            widgetWindow._location.Left = widgetWindow._location.HideRightIn - widgetWindow._location.Width;
            break;
        }
        widgetWindow.Top = widgetWindow._location.Top;
        widgetWindow.Left = widgetWindow._location.Left;
      }
      UtilLog.Info(string.Format("WidgetStartLocation top: {0} left: {1} isHide: {2}", (object) widgetWindow.Top, (object) widgetWindow.Left, (object) (widgetWindow.Model.DisplayOption == "top")));
      if (widgetWindow.CalendarWidget == null)
        return;
      widgetWindow.Model.CollapseTitle = DateUtils.FormatMonth(widgetWindow.CalendarWidget.CalendarControl.HeadView.StartDate);
    }

    private void SetHideLocation()
    {
      this.Container.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
      if (this._location.HideType == HideType.Top)
      {
        this.ResetOnHidden();
        this.CollapseGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        this.CollapseGrid.Opacity = 1.0;
        this.Top = this._location.HideTopIn + 20.0;
        this.Container.Opacity = 0.0;
      }
      else if (this._location.HideType == HideType.Left)
      {
        this.ResetOnHidden();
        this.StickLeftGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        this.StickLeftGrid.Opacity = 1.0;
        this.Left = this._location.HideLeftIn + 11.0;
        this.Container.Opacity = 0.0;
      }
      else if (this._location.HideType == HideType.Right)
      {
        this.ResetOnHidden();
        this.StickRightGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        this.StickRightGrid.Opacity = 1.0;
        this.Left = this._location.HideRightIn - 11.0;
        this.Container.Opacity = 0.0;
      }
      else
      {
        this.Container.Opacity = 1.0;
        this.StickRightGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        this.StickRightGrid.Opacity = 0.0;
        this.StickLeftGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        this.StickLeftGrid.Opacity = 0.0;
        this.CollapseGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        this.CollapseGrid.Opacity = 0.0;
        this.ResetOnShown();
        if (this._location.HideType != HideType.Reset)
          return;
        this.Left = this._location.Left;
        this.Top = this._location.Top;
      }
    }

    private void InitStatusCheckTimer()
    {
      this._statusCheckTimer.Elapsed -= new ElapsedEventHandler(this.OnCheckMouseOver);
      this._statusCheckTimer.Elapsed += new ElapsedEventHandler(this.OnCheckMouseOver);
    }

    private void OnCheckMouseOver(object sender, ElapsedEventArgs e)
    {
      this.Dispatcher?.InvokeAsync(new Action(this.CheckStatus), DispatcherPriority.Background);
    }

    private void CheckStatus() => this.TryHideWindow();

    private void InitDelaySaver()
    {
      this._modelSaver.SetAction(new EventHandler(this.OnSizeOrLocChanged));
    }

    private void DisposeDelaySaver() => this._modelSaver.StopAndClear();

    private void OnSizeOrLocChanged(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() =>
      {
        this.ResetLocation();
        UtilLog.Info(string.Format("WidgetChangeLocation top: {0} left: {1} isHide: {2}", (object) this.Top, (object) this.Left, (object) (this.Model.DisplayOption == "top")));
      }));
    }

    private void ResetLocation()
    {
      if (this._showState == WidgetShowStatus.Hidden)
      {
        if (this._location.HideType == HideType.Left && Math.Abs(this.Left - (this._location.HideLeftIn + 11.0)) > 1.0 || this._location.HideType == HideType.Top && Math.Abs(this.Top - (this._location.HideTopIn + 20.0)) > 1.0 || this._location.HideType == HideType.Right && Math.Abs(this.Left - (this._location.HideRightIn - 11.0)) > 1.0)
        {
          this._location = WidgetLocationHelper.GetLocationSafely(this.Left, this.Top, this.Width, this.Height, PresentationSource.FromVisual((Visual) this)?.CompositionTarget?.TransformFromDevice);
          this.SetHideLocation();
          return;
        }
      }
      else
      {
        this.ResetOnShown();
        this.Container.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        this.Container.Opacity = 1.0;
      }
      this.Model.Top = this.Top;
      this.Model.Left = this.Left;
      this.Model.Width = this.Width;
      this.Model.Height = this.Height;
      this._location.Height = this.Height;
      this._location.Width = this.Width;
      this._widgetChild.Save();
    }

    private void TryHideWindow()
    {
      IntPtr handle = new WindowInteropHelper((Window) this).Handle;
      NativeUtils.SetWindowLong(handle, WinParameter.GWL_STYLE, NativeUtils.GetWindowLong(handle, -16) & -65537 & -131073);
      if (this._location.HideType == HideType.None || !(this.Model.DisplayOption == "top") || !this.Model.AutoHide || this._showState != WidgetShowStatus.Normal || this._widgetChild.IsEditing())
        return;
      Matrix? transformFromDevice = PresentationSource.FromVisual((Visual) this)?.CompositionTarget?.TransformFromDevice;
      double num1 = transformFromDevice.HasValue ? transformFromDevice.GetValueOrDefault().M11 : 1.0;
      System.Windows.Point mousePosition = WidgetWindow.GetMousePosition();
      double num2 = mousePosition.X * num1;
      double num3 = mousePosition.Y * num1;
      if (num2 - 5.0 > this.Left + this.ActualWidth || num2 + 5.0 < this.Left || num3 + 5.0 < this.Top || num3 - 5.0 > this.Top + this.ActualHeight)
        ++this._mouseMoveOverCount;
      else
        this._mouseMoveOverCount = 0;
      if (this._mouseMoveOverCount < 3 || PopupStateManager.IsViewPopOpened())
        return;
      this._mouseMoveOverCount = 0;
      switch (this._location.HideType)
      {
        case HideType.Top:
          this.HideWindow();
          break;
        case HideType.Left:
          this.PopinLeft();
          break;
        case HideType.Right:
          this.PopinRight();
          break;
      }
      this.StopOrStartAutoHide(true);
    }

    private void StopOrStartAutoHide(bool stop)
    {
      lock (this._statusCheckTimer)
      {
        try
        {
          if (stop)
            this._statusCheckTimer.Stop();
          else
            this._statusCheckTimer.Start();
        }
        catch (Exception ex)
        {
        }
      }
    }

    private static System.Windows.Point GetMousePosition()
    {
      System.Drawing.Point mousePosition = System.Windows.Forms.Control.MousePosition;
      return new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y);
    }

    public async void SetTopMost()
    {
      if (this.Model.DisplayOption != "top" && this._location.HideType != HideType.None)
      {
        switch (this._location.HideType)
        {
          case HideType.Top:
            this.Model.Top = this._location.TopOut;
            break;
          case HideType.Left:
            this.Model.Left = this._location.LeftOut;
            break;
          case HideType.Right:
            this.Model.Left = this._location.RightOut;
            break;
        }
        await this.Model.Save();
      }
      WidgetSettings settings;
      if (this.ProjectWidget != null)
      {
        this.ProjectWidget.RemoveSettingEvents();
        (await ProjectWidgetsHelper.ReopenWidget(this.Model.Id))?.ProjectWidget?.AddSettingEvents();
      }
      else if (this.CalendarWidget != null)
      {
        settings = CalendarConfigHelper.TryShowSettings(this.Model);
        this.CalendarWidget.RemoveSettingEvent(settings);
        (await CalendarWidgetHelper.TryLoadWidget())?.CalendarWidget?.BindSettingEvent(settings);
        settings = (WidgetSettings) null;
      }
      else
      {
        if (this.MatrixWidget == null)
          return;
        settings = CalendarConfigHelper.TryShowSettings(this.Model);
        this.MatrixWidget.RemoveSettingEvent(settings);
        WidgetWindow widgetWindow = await MatrixWidgetHelper.TryLoadWidget();
        settings.Rebind(widgetWindow?.Model);
        widgetWindow?.MatrixWidget?.BindSettingEvent(settings);
        settings = (WidgetSettings) null;
      }
    }

    public void EnableBlur(bool enable = true) => NativeUtils.EnableBlur(enable, (Window) this);

    private void OnSizeOrLocationChanged(object sender, EventArgs e)
    {
      this._modelSaver?.TryDoAction();
    }

    private void OnWindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.F5)
        this.ProjectWidget?.SyncTask();
      this.CalendarWidget?.CalendarControl.OnKeyUp(sender, e);
      if (!Utils.IfCtrlPressed() || e.Key != Key.Z)
        return;
      this.CalendarWidget?.TryUndo();
    }

    private void OnCollapseGridEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this._showState != WidgetShowStatus.Hidden)
        return;
      this._popoutDelayAction.SetAction((EventHandler) ((o, args) => Utils.RunOnUiThread(this.Dispatcher, new Action(this.ShowWindow))));
      this._popoutDelayAction.TryDoAction();
    }

    private void OnPopLeftGridEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this._showState != WidgetShowStatus.Hidden)
        return;
      this._popoutDelayAction.SetAction((EventHandler) ((o, args) => Utils.RunOnUiThread(this.Dispatcher, new Action(this.PopoutLeft))));
      this._popoutDelayAction.TryDoAction();
    }

    private void OnPopoutGridLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      this._popoutDelayAction.CancelAction();
    }

    private void OnPopRightGridEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this._showState != WidgetShowStatus.Hidden)
        return;
      this._popoutDelayAction.SetAction((EventHandler) ((o, args) => Utils.RunOnUiThread(this.Dispatcher, new Action(this.PopoutRight))));
      this._popoutDelayAction.TryDoAction();
    }

    private void InitPopinRightAnim()
    {
      this._popInRightAnim = (Storyboard) this.FindResource((object) "PopInRightStoryBoard");
      if (this._popInRightAnim.Children[0] is DoubleAnimation child1)
        child1.To = new double?(this._location.HideRightIn);
      if (this._popInRightAnim.Children[1] is DoubleAnimation child2)
        child2.To = new double?(this._location.HideRightIn - 11.0);
      this._popInRightAnim.Completed -= new EventHandler(this.OnPopRightHidden);
      this._popInRightAnim.Completed += new EventHandler(this.OnPopRightHidden);
    }

    private void OnPopRightHidden(object sender, EventArgs e)
    {
      this.BeginAnimation(Window.LeftProperty, (AnimationTimeline) null);
      this.Left = this._location.HideRightIn - 11.0;
      this.ResetOnHidden();
      this._popInRightAnim = (Storyboard) null;
    }

    private void InitPopoutRightAnim()
    {
      this._popoutRightAnim = (Storyboard) this.FindResource((object) "PopOutRightStoryBoard");
      if (this._popoutRightAnim.Children[0] is DoubleAnimation child)
        child.To = new double?(this._location.RightOut);
      this._popoutRightAnim.Completed -= new EventHandler(this.OnPopRightShown);
      this._popoutRightAnim.Completed += new EventHandler(this.OnPopRightShown);
    }

    private void OnPopRightShown(object sender, EventArgs e)
    {
      this.BeginAnimation(Window.LeftProperty, (AnimationTimeline) null);
      this.Left = this._location.RightOut;
      this.ResetOnShown();
      this._popoutRightAnim = (Storyboard) null;
    }

    private void InitPopinLeftAnim()
    {
      this._popInLeftAnim = (Storyboard) this.FindResource((object) "PopInLeftStoryBoard");
      if (this._popInLeftAnim.Children[0] is DoubleAnimation child1)
        child1.To = new double?(this._location.HideLeftIn);
      if (this._popInLeftAnim.Children[1] is DoubleAnimation child2)
        child2.To = new double?(this._location.HideLeftIn + 11.0);
      this._popInLeftAnim.Completed -= new EventHandler(this.OnPopLeftHidden);
      this._popInLeftAnim.Completed += new EventHandler(this.OnPopLeftHidden);
    }

    private void OnPopLeftHidden(object sender, EventArgs e)
    {
      this.BeginAnimation(Window.LeftProperty, (AnimationTimeline) null);
      this.Left = this._location.HideLeftIn + 11.0;
      this.ResetOnHidden();
      this._popInLeftAnim = (Storyboard) null;
    }

    private void InitPopoutLeftAnim()
    {
      this._popoutLeftAnim = (Storyboard) this.FindResource((object) "PopOutLeftStoryBoard");
      if (this._popoutLeftAnim.Children[0] is DoubleAnimation child)
        child.To = new double?(this._location.LeftOut);
      this._popoutLeftAnim.Completed -= new EventHandler(this.OnPopLeftShown);
      this._popoutLeftAnim.Completed += new EventHandler(this.OnPopLeftShown);
    }

    private void OnPopLeftShown(object sender, EventArgs e)
    {
      this.BeginAnimation(Window.LeftProperty, (AnimationTimeline) null);
      this.Left = this._location.LeftOut;
      this.ResetOnShown();
      this._popoutLeftAnim = (Storyboard) null;
    }

    private void InitHideWindowAnim()
    {
      this._hideAnim = (Storyboard) this.FindResource((object) "HideStoryBoard");
      if (this._hideAnim.Children[0] is DoubleAnimation child1)
        child1.To = new double?(this._location.HideTopIn);
      if (this._hideAnim.Children[1] is DoubleAnimation child2)
        child2.To = new double?(this._location.HideTopIn + 20.0);
      this._hideAnim.Completed -= new EventHandler(this.OnWindowHidden);
      this._hideAnim.Completed += new EventHandler(this.OnWindowHidden);
    }

    private void OnWindowHidden(object sender, EventArgs e)
    {
      this.BeginAnimation(Window.TopProperty, (AnimationTimeline) null);
      this.Top = this._location.HideTopIn + 20.0;
      this.ResetOnHidden();
      this._hideAnim = (Storyboard) null;
    }

    private void InitShowWindowAnim()
    {
      this._showAnim = (Storyboard) this.FindResource((object) "ShowStoryBoard");
      if (this._showAnim.Children[0] is DoubleAnimation child)
        child.To = new double?(this._location.TopOut);
      this._showAnim.Completed -= new EventHandler(this.OnWindowShown);
      this._showAnim.Completed += new EventHandler(this.OnWindowShown);
    }

    private void OnWindowShown(object sender, EventArgs e)
    {
      this.BeginAnimation(Window.TopProperty, (AnimationTimeline) null);
      this.Top = this._location.TopOut;
      this.ResetOnShown();
      this._showAnim.Remove((FrameworkElement) this.ProjectWindow);
    }

    private void ResetOnShown()
    {
      this.SetStatus(WidgetShowStatus.Normal);
      this.EnableBlur((double) this.Model.Opacity > 0.0);
      this.StopOrStartAutoHide(false);
    }

    private void ShowWindow()
    {
      if (this._showState != WidgetShowStatus.Hidden)
        return;
      this.SetStatus(WidgetShowStatus.InAction);
      this.InitShowWindowAnim();
      this._showAnim?.Begin();
      this.EnableBlur();
    }

    private void ResetOnHidden()
    {
      this.EnableBlur(false);
      if (this.ResizeMode == ResizeMode.CanResize)
        this.ResizeMode = ResizeMode.NoResize;
      this.StopOrStartAutoHide(false);
      this.SetStatus(WidgetShowStatus.Hidden);
    }

    private void HideWindow()
    {
      if (this._showState != WidgetShowStatus.Normal)
        return;
      if (this.CalendarWidget != null)
        this.Model.CollapseTitle = DateUtils.FormatMonth(this.CalendarWidget.CalendarControl.HeadView.GetCurrentMonthDate());
      this.SetStatus(WidgetShowStatus.InAction);
      this.InitHideWindowAnim();
      this._hideAnim?.Begin();
      this.EnableBlur(false);
    }

    private void PopinRight()
    {
      if (this._showState != WidgetShowStatus.Normal)
        return;
      this.SetStatus(WidgetShowStatus.InAction);
      this.InitPopinRightAnim();
      this._popInRightAnim.Begin();
      this.EnableBlur(false);
    }

    private void PopoutRight()
    {
      if (this._showState != WidgetShowStatus.Hidden)
        return;
      this.SetStatus(WidgetShowStatus.InAction);
      this.InitPopoutRightAnim();
      this._popoutRightAnim?.Begin();
      this.EnableBlur();
    }

    private void PopinLeft()
    {
      if (this._showState != WidgetShowStatus.Normal)
        return;
      this.SetStatus(WidgetShowStatus.InAction);
      this.InitPopinLeftAnim();
      this._popInLeftAnim.Begin();
      this.EnableBlur(false);
    }

    private void PopoutLeft()
    {
      if (this._showState != WidgetShowStatus.Hidden)
        return;
      this.SetStatus(WidgetShowStatus.InAction);
      this.InitPopoutLeftAnim();
      this._popoutLeftAnim?.Begin();
      this.EnableBlur();
    }

    private void SetStatus(WidgetShowStatus status)
    {
      ticktick_WPF.Util.Log.d("set_status:" + status.ToString());
      this._showState = status;
    }

    public void Reload(bool loadDate) => this.ProjectWidget?.Reload(loadDate);

    public void OnDragMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this._dragMouseDown && e.LeftButton == MouseButtonState.Pressed)
      {
        this.ResizeMode = ResizeMode.NoResize;
        try
        {
          this._moveWindow = true;
          this.DragMove();
        }
        catch (Exception ex)
        {
        }
        finally
        {
          this._moveWindow = false;
        }
        this.ResizeMode = ResizeMode.CanResize;
        WidgetLocationModel locationSafely = WidgetLocationHelper.GetLocationSafely(this.Left, this.Top, this.Width, this.Height, PresentationSource.FromVisual((Visual) this)?.CompositionTarget?.TransformFromDevice);
        this._location = locationSafely;
        this.StopOrStartAutoHide(locationSafely.HideType == HideType.None);
      }
      this._dragMouseDown = false;
    }

    public void OnDragBarMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._dragMouseDown = true;
    }

    public void SetTheme(string option)
    {
      ThemeUtil.SetTheme(option, (FrameworkElement) this);
      this.Resources[(object) "IsDarkTheme"] = (object) (option == "dark");
      this.CalendarWidget?.Reload();
      this.MatrixWidget?.Reload();
    }

    private void OnStatusChanged(object sender, EventArgs e)
    {
      if (this.WindowState != WindowState.Maximized && this.WindowState != WindowState.Minimized)
        return;
      this.WindowState = WindowState.Normal;
    }

    protected override void OnActivated(EventArgs e)
    {
      MainWindowManager.Window.CheckUserStatus();
      if (this._widgetChild is IToastShowWindow widgetChild)
        Utils.ToastWindow = widgetChild;
      base.OnActivated(e);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this._modelSaver?.TryDoAction();
    }

    private void OnLocationChanged(object sender, EventArgs e) => this._modelSaver?.TryDoAction();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/widget/widgetwindow.xaml", UriKind.Relative));
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
          this.ProjectWindow = (WidgetWindow) target;
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          this.CollapseGrid = (Grid) target;
          this.CollapseGrid.MouseEnter += new System.Windows.Input.MouseEventHandler(this.OnCollapseGridEnter);
          this.CollapseGrid.MouseLeave += new System.Windows.Input.MouseEventHandler(this.OnPopoutGridLeave);
          break;
        case 4:
          this.StickLeftGrid = (Grid) target;
          this.StickLeftGrid.MouseEnter += new System.Windows.Input.MouseEventHandler(this.OnPopLeftGridEnter);
          this.StickLeftGrid.MouseLeave += new System.Windows.Input.MouseEventHandler(this.OnPopoutGridLeave);
          break;
        case 5:
          this.StickRightGrid = (Grid) target;
          this.StickRightGrid.MouseEnter += new System.Windows.Input.MouseEventHandler(this.OnPopRightGridEnter);
          this.StickRightGrid.MouseLeave += new System.Windows.Input.MouseEventHandler(this.OnPopoutGridLeave);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
