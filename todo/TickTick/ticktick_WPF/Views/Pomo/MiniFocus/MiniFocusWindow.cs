// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MiniFocus.MiniFocusWindow
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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Widget;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MiniFocus
{
  public class MiniFocusWindow : Window, IComponentConnector
  {
    private readonly Timer _statusCheckTimer = new Timer(100.0);
    private IMiniFocus _focusControl;
    private WidgetLocationModel _location = new WidgetLocationModel();
    private WidgetShowStatus _showState;
    private int _mouseMoveOverCount;
    internal MiniFocusWindow Root;
    internal DropShadowEffect WindowEffect;
    internal ContentControl FocusContent;
    internal TranslateTransform ContentTransform;
    internal Border SideContainer;
    internal RotateTransform SideRotateTransform;
    internal Grid BesideGrid;
    internal TranslateTransform SideTransform;
    internal Border BesideBorder;
    internal ContentControl SideFocusContent;
    private bool _contentLoaded;

    private static bool IsCircle => LocalSettings.Settings.PomoLocalSetting.DisplayType == "Circle";

    private static bool IsRect => LocalSettings.Settings.PomoLocalSetting.DisplayType == "Mini";

    public bool AutoHide
    {
      get => this._location.HideType != HideType.None && this._location.HideType != HideType.Reset;
    }

    public MiniFocusWindow()
    {
      this.InitializeComponent();
      this._statusCheckTimer.Elapsed += new ElapsedEventHandler(this.OnCheckMouseOver);
      this._focusControl = this.GetFocusControl();
      this.SideFocusContent.Content = !TickFocusManager.IsPomo ? (object) new BesideTiming() : (object) new BesidePomoProgress();
      this.FocusContent.Content = (object) this._focusControl;
      this._focusControl.Init();
      this.SetTheme(LocalSettings.Settings.PomoWindowTheme);
      this.Loaded += (RoutedEventHandler) ((o, e) =>
      {
        this.RegisterGlobalEvents();
        this.OnWindowLoaded();
        this.ResetPosition();
        this.LocationChanged += new EventHandler(this.OnLocationChanged);
      });
      this.Unloaded += (RoutedEventHandler) ((o, e) => this.UnregisterGlobalEvents());
      UserActCollectUtils.AddClickEvent("focus", "show", "mini_mode");
      UserActCollectUtils.AddClickEvent("focus_mini", "style", MiniFocusWindow.IsCircle ? "circle" : (MiniFocusWindow.IsRect ? "square" : "detailed"));
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnThemeChanged);
    }

    private void OnThemeChanged(object sender, EventArgs e)
    {
      this.SetTheme(LocalSettings.Settings.PomoWindowTheme);
    }

    private void OnLocationChanged(object sender, EventArgs e)
    {
      Matrix? transformFromDevice = PresentationSource.FromVisual((Visual) this)?.CompositionTarget?.TransformFromDevice;
      WidgetLocationModel locationSafely = WidgetLocationHelper.GetLocationSafely(this.Left + this._focusControl.GetLeftMargin(), this.Top, this._focusControl.GetActualWidth(), this._focusControl.GetActualHeight(), transformFromDevice);
      if (this._focusControl is CircleMiniFocusControl focusControl)
      {
        bool isRight = locationSafely.ScreenX + locationSafely.ScreenWidth - locationSafely.Left - focusControl.ActualWidth + 100.0 < 0.0;
        focusControl.SetPositionMode(isRight);
      }
      LocalSettings.Settings.PomoWindowLeft = (int) this.Left;
      LocalSettings.Settings.PomoWindowTop = (int) this.Top;
      if (this._showState != WidgetShowStatus.Hidden || locationSafely.HideType == this._location.HideType)
        return;
      this.ShowWindowStory();
    }

    private IMiniFocus GetFocusControl()
    {
      if ((string.IsNullOrEmpty(LocalSettings.Settings.PomoLocalSetting.DisplayType) ? 1 : (LocalSettings.Settings.PomoLocalSetting.DisplayType == "Normal" ? 1 : 0)) != 0)
        return (IMiniFocus) new PomoWindow();
      return MiniFocusWindow.IsCircle ? (IMiniFocus) new CircleMiniFocusControl() : (IMiniFocus) new RectMiniFocusControl();
    }

    private void OnWindowLoaded()
    {
      NativeUtils.SetWindowLong(new WindowInteropHelper((Window) this).Handle, -20, 128L);
      Matrix? transformFromDevice = PresentationSource.FromVisual((Visual) this)?.CompositionTarget?.TransformFromDevice;
      this._location = WidgetLocationHelper.GetLocationSafely(this.Left + this._focusControl.GetLeftMargin(), this.Top, this._focusControl.GetActualWidth(), this._focusControl.GetActualHeight(), transformFromDevice);
      this.Topmost = LocalSettings.Settings.PomoTopMost || this._location.HideType != 0;
      this._focusControl.SetHideStyle(this._location.HideType != 0);
      this.CheckPosition();
      if (this._location.HideType == HideType.None)
        return;
      this.SetSideDisplay();
    }

    private void OnCheckMouseOver(object sender, ElapsedEventArgs e)
    {
      this.Dispatcher?.InvokeAsync(new Action(this.TryHideWindow), DispatcherPriority.Background);
    }

    private void UnregisterGlobalEvents()
    {
      ticktick_WPF.Notifier.GlobalEventManager.PomoWindowOpacityChanged -= new EventHandler<double>(this.OnOpacityChanged);
      ticktick_WPF.Notifier.GlobalEventManager.PomoWindowThemeChanged -= new EventHandler<string>(this.OnThemeChanged);
      TickFocusManager.CurrentSecondChanged -= new FocusChange(this.OnCurrentSecondChanged);
      TickFocusManager.TypeChanged -= new FocusChange(this.OnFocusTypeChanged);
      TickFocusManager.StatusChanged -= new FocusChange(this.OnStatusChanged);
    }

    private void OnStatusChanged()
    {
      this.Dispatcher?.Invoke((Action) (() =>
      {
        this._focusControl.OnStatusChanged();
        if (!(this.SideFocusContent.Content is ISideMiniFocus content2))
          return;
        content2.OnStatusChanged();
      }));
    }

    private void OnFocusTypeChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._focusControl.OnFocusTypeChanged();
        if (TickFocusManager.IsPomo)
          this.SideFocusContent.Content = (object) new BesidePomoProgress();
        else
          this.SideFocusContent.Content = (object) new BesideTiming();
      }));
    }

    private void OnCurrentSecondChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._focusControl.SetCountText();
        if (!(this.SideFocusContent.Content is ISideMiniFocus content2))
          return;
        content2.SetPercent();
      }));
    }

    private void OnThemeChanged(object sender, string e)
    {
      this.SetTheme(LocalSettings.Settings.PomoWindowTheme);
    }

    private void OnOpacityChanged(object sender, double e) => this._focusControl.SetOpacity(e);

    private void RegisterGlobalEvents()
    {
      TickFocusManager.CurrentSecondChanged += new FocusChange(this.OnCurrentSecondChanged);
      TickFocusManager.TypeChanged += new FocusChange(this.OnFocusTypeChanged);
      TickFocusManager.StatusChanged += new FocusChange(this.OnStatusChanged);
      ticktick_WPF.Notifier.GlobalEventManager.PomoWindowOpacityChanged += new EventHandler<double>(this.OnOpacityChanged);
      ticktick_WPF.Notifier.GlobalEventManager.PomoWindowThemeChanged += new EventHandler<string>(this.OnThemeChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnTopMostChanged), "PomoTopMost");
    }

    private void OnTopMostChanged(object sender, PropertyChangedEventArgs e)
    {
      int num;
      if (!LocalSettings.Settings.PomoTopMost)
      {
        WidgetLocationModel location = this._location;
        num = location != null ? (location.HideType != 0 ? 1 : 0) : 1;
      }
      else
        num = 1;
      this.Topmost = num != 0;
      this.ShowInTaskbar = !this.Topmost;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || this._showState != WidgetShowStatus.Normal || !this._focusControl.CanDragMove())
        return;
      this._focusControl.SetMoving(true);
      double left1 = this.Left;
      double top = this.Top;
      this.DragMove();
      Matrix? transformFromDevice = PresentationSource.FromVisual((Visual) this)?.CompositionTarget?.TransformFromDevice;
      WidgetLocationModel widgetLocationModel = WidgetLocationHelper.GetLocationSafely(this.Left + this._focusControl.GetLeftMargin(), this.Top, this._focusControl.GetActualWidth(), this._focusControl.GetActualHeight(), transformFromDevice);
      if (widgetLocationModel.HideType == HideType.Reset)
      {
        this.Left = this._location.Left;
        this.Top = this._location.Top;
        widgetLocationModel = this._location;
      }
      this._location = widgetLocationModel;
      this._focusControl.SetHideStyle(this._location.HideType != 0);
      this.SetSideDisplay();
      int num;
      if (!LocalSettings.Settings.PomoTopMost)
      {
        WidgetLocationModel location = this._location;
        num = location != null ? (location.HideType != 0 ? 1 : 0) : 1;
      }
      else
        num = 1;
      this.Topmost = num != 0;
      this.ShowInTaskbar = !this.Topmost;
      double left2 = this.Left;
      if (Math.Abs(left1 - left2) > 1.0 || Math.Abs(top - this.Top) > 1.0)
      {
        this.CheckPosition();
        this.StartHide();
      }
      e.Handled = true;
    }

    private void StartHide()
    {
      switch (this._location.HideType)
      {
        case HideType.None:
          this.Topmost = LocalSettings.Settings.PomoTopMost;
          break;
        case HideType.Top:
          this.Topmost = true;
          this.PopinTop();
          break;
        case HideType.Left:
          this.Topmost = true;
          this.PopinLeft();
          break;
        case HideType.Right:
          this.Topmost = true;
          this.PopinRight();
          break;
      }
      this.ShowInTaskbar = !this.Topmost;
      this.StopOrStartAutoHide(true);
    }

    public void SetSideDisplay()
    {
      this.BesideGrid.VerticalAlignment = MiniFocusWindow.IsRect ? VerticalAlignment.Top : VerticalAlignment.Center;
      this.BesideGrid.Margin = !MiniFocusWindow.IsRect || this._location.HideType == HideType.Top ? new Thickness(0.0, 1.0, 0.0, 1.0) : new Thickness(0.0);
      this.SideFocusContent.Margin = new Thickness(this._location.HideType != HideType.Right ? 2.0 : 0.0, 4.0, this._location.HideType != HideType.Right ? 0.0 : 2.0, 4.0);
      if (!MiniFocusWindow.IsRect && !MiniFocusWindow.IsCircle)
      {
        if (this._location.HideType == HideType.Top)
        {
          this.SideContainer.Height = 196.0;
          this.BesideGrid.Height = 194.0;
          this.SideFocusContent.Height = 186.0;
          this.BesideBorder.CornerRadius = this._location.HideType != HideType.Right ? new CornerRadius(0.0, 6.0, 6.0, 0.0) : new CornerRadius(6.0, 0.0, 0.0, 6.0);
        }
        else
        {
          double actualHeight = this._focusControl.GetActualHeight();
          this.SideContainer.Height = actualHeight + 2.0;
          this.BesideGrid.Height = actualHeight;
          this.SideFocusContent.Height = actualHeight - 8.0;
          this.BesideBorder.CornerRadius = this._location.HideType != HideType.Right ? new CornerRadius(0.0, 6.0, 6.0, 0.0) : new CornerRadius(6.0, 0.0, 0.0, 6.0);
        }
      }
      else if (this._location.HideType != HideType.Top)
      {
        this.SideContainer.Height = 70.0;
        this.BesideGrid.Height = MiniFocusWindow.IsRect ? 36.0 : 42.0;
        this.SideFocusContent.Height = MiniFocusWindow.IsRect ? 28.0 : 34.0;
        this.BesideBorder.CornerRadius = this._location.HideType == HideType.Left ? new CornerRadius(0.0, 4.0, 4.0, 0.0) : new CornerRadius(4.0, 0.0, 0.0, 4.0);
      }
      else
      {
        this.SideContainer.Height = this._location.HideType == HideType.Top ? 72.0 : 70.0;
        this.BesideGrid.Height = 70.0;
        this.SideFocusContent.Height = 62.0;
        this.BesideBorder.CornerRadius = this._location.HideType != HideType.Right ? new CornerRadius(0.0, 6.0, 6.0, 0.0) : new CornerRadius(6.0, 0.0, 0.0, 6.0);
      }
    }

    private async void CheckPosition()
    {
      if (this._focusControl is CircleMiniFocusControl focusControl)
      {
        bool isRight = this._location.ScreenX + this._location.ScreenWidth - this._location.Left - focusControl.ActualWidth + 100.0 < 0.0;
        await focusControl.SetPositionMode(isRight);
        this._focusControl.SetMoving(false);
      }
      this.StopOrStartAutoHide(this._location.HideType == HideType.None);
    }

    private void SetTheme(string theme)
    {
      theme = theme == "dark" ? "dark" : "light";
      ThemeUtil.SetTheme(theme, (FrameworkElement) this);
    }

    private void TryHideWindow()
    {
      if (this._location.HideType == HideType.None || this._showState != WidgetShowStatus.Normal)
        return;
      if (!this.IsMouseOver && this._focusControl.CanHide())
        ++this._mouseMoveOverCount;
      else
        this._mouseMoveOverCount = 0;
      if (this._mouseMoveOverCount < 3)
        return;
      this._mouseMoveOverCount = 0;
      this.StartHide();
    }

    private void PopinLeft()
    {
      if (this._showState != WidgetShowStatus.Normal)
        return;
      this._showState = WidgetShowStatus.InAction;
      this.BeforeHide();
      this.BeginPopinLeftAnim();
      this._focusControl.OnWindowStartHide();
    }

    private async void BeginPopinLeftAnim()
    {
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), -2.0 - this._focusControl.GetActualWidth() - this._focusControl.GetExtraWidth(), MiniFocusWindow.IsRect ? 180 : 240);
      DoubleAnimation doubleAnimation2 = doubleAnimation1;
      QuadraticEase quadraticEase = new QuadraticEase();
      quadraticEase.EasingMode = EasingMode.EaseOut;
      doubleAnimation2.EasingFunction = (IEasingFunction) quadraticEase;
      this.ContentTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation1);
      DoubleAnimation doubleAnimation3 = AnimationUtils.GetDoubleAnimation(new double?(-20.0), 0.0, 1);
      doubleAnimation3.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(MiniFocusWindow.IsRect ? 120.0 : 160.0));
      this.SideTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation3);
      await Task.Delay(300);
      this.FocusContent.Visibility = Visibility.Hidden;
      this._showState = WidgetShowStatus.Hidden;
    }

    private async void BeginPopoutLeftAnim()
    {
      this.FocusContent.Visibility = Visibility.Visible;
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), -20.0, 10);
      this.SideTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation1);
      DoubleAnimation doubleAnimation2 = AnimationUtils.GetDoubleAnimation(new double?(), 0.0, MiniFocusWindow.IsRect ? 180 : 240);
      DoubleAnimation doubleAnimation3 = doubleAnimation2;
      QuadraticEase quadraticEase = new QuadraticEase();
      quadraticEase.EasingMode = EasingMode.EaseOut;
      doubleAnimation3.EasingFunction = (IEasingFunction) quadraticEase;
      this.ContentTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation2);
      await Task.Delay(300);
      this._showState = WidgetShowStatus.Normal;
      this.StopOrStartAutoHide(false);
      this.WindowEffect.Opacity = 0.2;
    }

    private void PopinRight()
    {
      if (this._showState != WidgetShowStatus.Normal)
        return;
      this._showState = WidgetShowStatus.InAction;
      this.BeforeHide();
      this.BeginPopinRightAnim();
      this._focusControl.OnWindowStartHide();
    }

    private async void BeginPopinRightAnim()
    {
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), 2.0 + this._focusControl.GetActualWidth() + this._focusControl.GetExtraWidth(), MiniFocusWindow.IsCircle ? 300 : (MiniFocusWindow.IsRect ? 180 : 210));
      DoubleAnimation doubleAnimation2 = doubleAnimation1;
      QuadraticEase quadraticEase = new QuadraticEase();
      quadraticEase.EasingMode = EasingMode.EaseOut;
      doubleAnimation2.EasingFunction = (IEasingFunction) quadraticEase;
      this.ContentTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation1);
      DoubleAnimation doubleAnimation3 = AnimationUtils.GetDoubleAnimation(new double?(20.0), 0.0, 1);
      doubleAnimation3.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(60.0));
      this.SideTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation3);
      await Task.Delay(300);
      this.FocusContent.Visibility = Visibility.Hidden;
      this._showState = WidgetShowStatus.Hidden;
      this.WindowEffect.Opacity = 0.2;
    }

    private async void BeginPopoutRightAnim()
    {
      this.FocusContent.Visibility = Visibility.Visible;
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), 20.0, 1);
      doubleAnimation1.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(60.0));
      this.SideTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation1);
      DoubleAnimation doubleAnimation2 = AnimationUtils.GetDoubleAnimation(new double?(), 0.0, MiniFocusWindow.IsRect ? 180 : 240);
      DoubleAnimation doubleAnimation3 = doubleAnimation2;
      QuadraticEase quadraticEase = new QuadraticEase();
      quadraticEase.EasingMode = EasingMode.EaseOut;
      doubleAnimation3.EasingFunction = (IEasingFunction) quadraticEase;
      this.ContentTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation2);
      await Task.Delay(300);
      this._showState = WidgetShowStatus.Normal;
      this.StopOrStartAutoHide(false);
      this.SideContainer.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
    }

    private void PopinTop()
    {
      if (this._showState != WidgetShowStatus.Normal)
        return;
      this._showState = WidgetShowStatus.InAction;
      this.BeforeHide();
      this.BeginPopinTopAnim();
      this._focusControl.OnWindowStartHide();
    }

    private void BeforeHide()
    {
      this.ContentTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
      this.ContentTransform.BeginAnimation(TranslateTransform.YProperty, (AnimationTimeline) null);
      this.ContentTransform.X = 0.0;
      this.ContentTransform.Y = 0.0;
      switch (this._location.HideType)
      {
        case HideType.Top:
          this.Top = this._location.ScreenY;
          this.SideContainer.HorizontalAlignment = HorizontalAlignment.Center;
          this.SideTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
          this.SideRotateTransform.Angle = 90.0;
          this.SideTransform.X = -20.0;
          this.SideContainer.Margin = new Thickness(0.0, -2.0, 0.0, 0.0);
          break;
        case HideType.Left:
          this.Left = this._location.ScreenX - this._focusControl.GetLeftMargin();
          this.SideContainer.HorizontalAlignment = HorizontalAlignment.Left;
          this.SideContainer.Margin = new Thickness(this._focusControl.GetLeftMargin() - 2.0, 0.0, 0.0, 0.0);
          this.SideTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
          this.SideRotateTransform.Angle = 0.0;
          this.SideTransform.X = -20.0;
          break;
        case HideType.Right:
          this.Left = this._location.ScreenX + this._location.ScreenWidth - this._focusControl.GetLeftMargin() - this._focusControl.GetActualWidth();
          this.SideContainer.HorizontalAlignment = HorizontalAlignment.Left;
          this.SideContainer.Margin = new Thickness(this._focusControl.GetLeftMargin() + this._focusControl.GetActualWidth() - 11.0, 0.0, 0.0, 0.0);
          this.SideTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
          this.SideRotateTransform.Angle = 0.0;
          this.SideTransform.X = 20.0;
          break;
      }
    }

    private async void BeginPopinTopAnim()
    {
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), -this._focusControl.GetActualHeight(), 160);
      this.ContentTransform.BeginAnimation(TranslateTransform.YProperty, (AnimationTimeline) doubleAnimation1);
      DoubleAnimation doubleAnimation2 = AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 1);
      doubleAnimation2.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(120.0));
      this.SideTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation2);
      await Task.Delay(200);
      this._showState = WidgetShowStatus.Hidden;
      this.FocusContent.Visibility = Visibility.Hidden;
      this.WindowEffect.Opacity = 0.2;
    }

    private async void BeginPopoutTopAnim()
    {
      this.FocusContent.Visibility = Visibility.Visible;
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), -20.0, 10);
      doubleAnimation1.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(60.0));
      this.SideTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation1);
      DoubleAnimation doubleAnimation2 = AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 180);
      DoubleAnimation doubleAnimation3 = doubleAnimation2;
      QuadraticEase quadraticEase = new QuadraticEase();
      quadraticEase.EasingMode = EasingMode.EaseOut;
      doubleAnimation3.EasingFunction = (IEasingFunction) quadraticEase;
      this.ContentTransform.BeginAnimation(TranslateTransform.YProperty, (AnimationTimeline) doubleAnimation2);
      await Task.Delay(200);
      this._showState = WidgetShowStatus.Normal;
      this.StopOrStartAutoHide(false);
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

    private async void OnSideBorderMouseEnter(object sender, MouseEventArgs e)
    {
      await Task.Delay(300);
      if (!this.SideContainer.IsMouseOver)
        return;
      this.ShowWindowStory();
    }

    private void ShowWindowStory()
    {
      if (this._showState != WidgetShowStatus.Hidden)
        return;
      this._showState = WidgetShowStatus.InAction;
      switch (this._location.HideType)
      {
        case HideType.Top:
          this.BeginPopoutTopAnim();
          this.WindowEffect.Opacity = 0.0;
          break;
        case HideType.Left:
          this.BeginPopoutLeftAnim();
          this.WindowEffect.Opacity = 0.0;
          break;
        case HideType.Right:
          this.BeginPopoutRightAnim();
          this.WindowEffect.Opacity = 0.0;
          break;
      }
    }

    public void TryShowWindow()
    {
      this.Visibility = Visibility.Visible;
      LocalSettings.Settings.PomoLocalSetting.OpenWidget = true;
      LocalSettings.Settings.Save(true);
      this.Show();
      this.Activate();
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.P)
        return;
      LocalSettings.Settings.PomoTopMost = !LocalSettings.Settings.PomoTopMost;
      LocalSettings.Settings.Save();
    }

    private async void ResetPosition()
    {
      MiniFocusWindow miniFocusWindow = this;
      double left = miniFocusWindow.Left;
      double top = miniFocusWindow.Top;
      WindowHelper.MoveTo((Window) miniFocusWindow, (int) miniFocusWindow.Left, (int) miniFocusWindow.Top);
      miniFocusWindow.Left = left;
      miniFocusWindow.Top = top;
      Matrix? transform = PresentationSource.FromVisual((Visual) miniFocusWindow)?.CompositionTarget?.TransformFromDevice;
      await Task.Delay(1000);
      System.Windows.Point defaultPoint = new System.Windows.Point(SystemParameters.PrimaryScreenWidth / 2.0, SystemParameters.PrimaryScreenHeight / 2.0);
      // ISSUE: explicit non-virtual call
      System.Windows.Point pomoLocationSafely = WidgetLocationHelper.GetPomoLocationSafely(miniFocusWindow.Left, miniFocusWindow.Top, miniFocusWindow._focusControl.GetLeftMargin() + miniFocusWindow._focusControl.GetActualWidth(), __nonvirtual (miniFocusWindow.Height), transform, defaultPoint);
      miniFocusWindow.Left = pomoLocationSafely.X;
      miniFocusWindow.Top = pomoLocationSafely.Y;
      UtilLog.Info(string.Format("PomoWindow ResetPosition : {0},{1}", (object) pomoLocationSafely.X, (object) pomoLocationSafely.Y));
    }

    public async void SetDisplayType()
    {
      MiniFocusWindow miniFocusWindow = this;
      IMiniFocus control = miniFocusWindow.GetFocusControl();
      if (!(miniFocusWindow._focusControl.GetType() != control.GetType()))
      {
        control = (IMiniFocus) null;
      }
      else
      {
        miniFocusWindow.SideContainer.Opacity = 0.0;
        miniFocusWindow.FocusContent.Opacity = 0.0;
        miniFocusWindow.FocusContent.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(1.0), 0.0, 80));
        await Task.Delay(100);
        miniFocusWindow.UpdateLayout();
        if (!MiniFocusWindow.IsCircle && miniFocusWindow._focusControl is CircleMiniFocusControl)
          miniFocusWindow.Left += 76.0;
        else if (MiniFocusWindow.IsCircle && !(miniFocusWindow._focusControl is CircleMiniFocusControl))
          miniFocusWindow.Left -= 76.0;
        miniFocusWindow._focusControl = control;
        miniFocusWindow.FocusContent.Content = (object) miniFocusWindow._focusControl;
        miniFocusWindow._focusControl.Init();
        miniFocusWindow._focusControl.SetHideStyle(miniFocusWindow._location.HideType != 0);
        await Task.Delay(50);
        miniFocusWindow.FocusContent.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(0.0), 1.0, 80));
        miniFocusWindow.SideContainer.Opacity = 1.0;
        if (miniFocusWindow._location.HideType == HideType.None)
        {
          control = (IMiniFocus) null;
        }
        else
        {
          miniFocusWindow.SetSideDisplay();
          miniFocusWindow.FocusContent.Visibility = Visibility.Visible;
          miniFocusWindow._showState = WidgetShowStatus.Normal;
          miniFocusWindow.BeforeHide();
          miniFocusWindow._mouseMoveOverCount = -1;
          miniFocusWindow.StopOrStartAutoHide(false);
          control = (IMiniFocus) null;
        }
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (TickFocusManager.SaveAfterClose)
        LocalSettings.Settings.PomoLocalSetting.OpenWidget = false;
      UtilLog.Info("ClosePomoWindow openWidget : " + LocalSettings.Settings.PomoLocalSetting.OpenWidget.ToString());
      this.UnregisterGlobalEvents();
      base.OnClosing(e);
    }

    public void OnStatisticsChanged()
    {
      if (!(this._focusControl is PomoWindow focusControl))
        return;
      focusControl.ResetStatistics();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/minifocus/minifocuswindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (MiniFocusWindow) target;
          this.Root.MouseMove += new MouseEventHandler(this.OnMouseMove);
          this.Root.KeyDown += new KeyEventHandler(this.OnWindowKeyDown);
          break;
        case 2:
          this.WindowEffect = (DropShadowEffect) target;
          break;
        case 3:
          this.FocusContent = (ContentControl) target;
          break;
        case 4:
          this.ContentTransform = (TranslateTransform) target;
          break;
        case 5:
          this.SideContainer = (Border) target;
          this.SideContainer.MouseEnter += new MouseEventHandler(this.OnSideBorderMouseEnter);
          break;
        case 6:
          this.SideRotateTransform = (RotateTransform) target;
          break;
        case 7:
          this.BesideGrid = (Grid) target;
          break;
        case 8:
          this.SideTransform = (TranslateTransform) target;
          break;
        case 9:
          this.BesideBorder = (Border) target;
          break;
        case 10:
          this.SideFocusContent = (ContentControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
