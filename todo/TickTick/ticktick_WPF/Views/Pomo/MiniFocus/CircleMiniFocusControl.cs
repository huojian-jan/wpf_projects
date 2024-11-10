// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MiniFocus.CircleMiniFocusControl
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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.Misc;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MiniFocus
{
  public class CircleMiniFocusControl : UserControl, IMiniFocus, IComponentConnector
  {
    private bool _showOp;
    public bool IsLeftMode;
    private bool _moving;
    private bool _morePopupShow;
    private bool _autoHideOp = true;
    private static readonly SemaphoreLocker SyncLock = new SemaphoreLocker();
    internal Grid ClockPanel;
    internal Ellipse BackEllipse;
    internal PomoProgressBar Progress;
    internal ClockControl Clock;
    internal Image GotPomo;
    internal ContentControl PopupTarget;
    internal Border OpBorder;
    internal TranslateTransform OpTransform;
    internal StackPanel OpPanel;
    internal Grid OpStart;
    internal Path OpStartIcon;
    internal Border OpStop;
    internal Border OpMore;
    internal EllipseGeometry Geometry1;
    internal EllipseGeometry Geometry2;
    private bool _contentLoaded;

    public PomoStatus Status => TickFocusManager.Status;

    public CircleMiniFocusControl()
    {
      this.InitializeComponent();
      this.Clock.ShowHourFont = new int?(12);
    }

    public void OnStatusChanged() => this.SetOptionStatus();

    private void SetOptionStatus()
    {
      bool flag1 = this.Status == PomoStatus.WaitingRelax;
      bool flag2 = this.Status == PomoStatus.Relaxing;
      bool flag3 = this.Status == PomoStatus.Pause;
      bool flag4 = this.OpStop.Visibility == Visibility.Visible;
      bool flag5 = this.OpStart.Visibility == Visibility.Visible;
      this.OpStart.Visibility = flag2 ? Visibility.Collapsed : Visibility.Visible;
      this.OpStop.Visibility = flag2 | flag3 | flag1 ? Visibility.Visible : Visibility.Collapsed;
      bool flag6 = this.OpStop.Visibility == Visibility.Visible;
      bool flag7 = this.OpStart.Visibility == Visibility.Visible;
      if (this._showOp && (flag4 != flag6 || flag5 != flag7))
      {
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(), (double) ((this.IsLeftMode ? 1 : -1) * (flag6 & flag7 ? 2 : 26)), 240);
        this.OpTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation);
      }
      this.OpStartIcon.Data = Utils.GetIcon(this.Status == PomoStatus.Working ? "IcPomoPause" : "IcPomoStart");
      this.OpStartIcon.SetResourceReference(Shape.FillProperty, flag2 | flag1 ? (object) "PomoGreen" : (object) "PrimaryColor");
      this.Clock.SetResourceReference(ClockControl.ForegroundProperty, flag2 | flag1 ? (object) "PomoGreen" : (object) "BaseColorOpacity100");
      this.Clock.Visibility = flag1 ? Visibility.Collapsed : Visibility.Visible;
      this.Progress.SetResourceReference(PomoProgressBar.TopColorProperty, flag2 | flag1 ? (object) "PomoGreen" : (object) "PrimaryColor");
      switch (this.Status)
      {
        case PomoStatus.Working:
          this.OpStart.ToolTip = (object) Utils.GetString("Pause");
          break;
        case PomoStatus.Relaxing:
          this.OpStop.ToolTip = (object) Utils.GetString("SkipRelax");
          break;
        case PomoStatus.WaitingWork:
          this.Progress.Reset();
          this.OpStart.ToolTip = (object) Utils.GetString(TickFocusManager.IsPomo ? "StartPomo" : "StartTiming");
          break;
        case PomoStatus.WaitingRelax:
          this.OpStart.ToolTip = (object) Utils.GetString("BeginRelax");
          this.OpStop.ToolTip = (object) Utils.GetString("SkipRelax");
          break;
        case PomoStatus.Pause:
          this.OpStart.ToolTip = (object) Utils.GetString("Continue");
          this.OpStop.ToolTip = (object) Utils.GetString("End");
          break;
      }
    }

    public void OnFocusTypeChanged()
    {
      this.Progress.IsStrokeMode = TickFocusManager.IsPomo;
      this.Progress.Reset();
      this.Clock.ShowHour = !TickFocusManager.IsPomo;
      this.SetCountText();
    }

    public double GetLeftMargin() => 86.0;

    public double GetActualWidth() => 70.0;

    public double GetExtraWidth() => 90.0;

    public double GetActualHeight() => 70.0;

    public void SetMoving(bool b) => this._moving = b;

    public bool CanDragMove() => !this.OpPanel.IsMouseOver;

    public bool CanHide() => !this._morePopupShow;

    public void SetHideStyle(bool isHide)
    {
      this._autoHideOp = !isHide;
      if (this._autoHideOp)
      {
        if (this.IsMouseOver || !this._autoHideOp || !this._showOp || this._moving || this._morePopupShow)
          return;
        this.HideOpAnim();
      }
      else
      {
        if (this._showOp)
          return;
        this.ShowOpAnim();
      }
    }

    public async void OnWindowStartHide()
    {
    }

    public void SetCountText()
    {
      int second = (int) Math.Round(TickFocusManager.Config.CurrentSeconds, 0, MidpointRounding.AwayFromZero);
      this.Progress.Angle = TickFocusManager.Config.GetDisplayAngle(false);
      if (!this.Progress.IsStrokeMode && TickFocusManager.Config.CurrentSeconds >= 30.0)
        this.Progress.HideLeftMask();
      this.Clock.SetTime(second);
    }

    public void Init()
    {
      this.OnStatusChanged();
      this.OnFocusTypeChanged();
      this.SetOpacity(LocalSettings.Settings.PomoWindowOpacity);
      this.GotPomo.Source = (ImageSource) ImageUtils.GetResourceImage("pack://application:,,,/Assets/get_pomo.png", 80);
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (this._showOp || this._moving)
        return;
      this.ShowOpAnim();
    }

    public void ShowOpAnim(int duration = 240)
    {
      this._showOp = true;
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), (double) ((this.IsLeftMode ? 1 : -1) * (!this.OpStop.IsVisible || !this.OpStart.IsVisible ? 26 : 2)), duration);
      DoubleAnimation doubleAnimation2 = doubleAnimation1;
      CubicEase cubicEase = new CubicEase();
      cubicEase.EasingMode = EasingMode.EaseOut;
      doubleAnimation2.EasingFunction = (IEasingFunction) cubicEase;
      this.OpTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation1);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (!this._autoHideOp || !this._showOp || this._moving || this._morePopupShow)
        return;
      this.HideOpAnim();
    }

    public void HideOpAnim(int duration = 240)
    {
      this._showOp = false;
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), (double) ((this.IsLeftMode ? 1 : -1) * 90), duration);
      DoubleAnimation doubleAnimation2 = doubleAnimation1;
      CubicEase cubicEase = new CubicEase();
      cubicEase.EasingMode = EasingMode.EaseOut;
      doubleAnimation2.EasingFunction = (IEasingFunction) cubicEase;
      this.OpTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation1);
    }

    public async Task SetPositionMode(bool isRight)
    {
      await CircleMiniFocusControl.SyncLock.LockAsync((Func<Task>) (async () =>
      {
        if (this.IsLeftMode == isRight)
          return;
        this.HideOpAnim(100);
        this.UpdateLayout();
        await Task.Delay(120);
        this.IsLeftMode = isRight;
        this.OpTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
        this.OpTransform.X = (double) ((this.IsLeftMode ? 1 : -1) * 90);
        this.OpPanel.HorizontalAlignment = isRight ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        this.OpBorder.HorizontalAlignment = isRight ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        this.Geometry1.Center = isRight ? new System.Windows.Point(-80.0, 21.0) : new System.Windows.Point(200.0, 21.0);
        this.Geometry2.Center = isRight ? new System.Windows.Point(117.5, 21.0) : new System.Windows.Point(-5.5, 21.0);
        if (!this.IsMouseOver)
          return;
        this.ShowOpAnim(100);
      }));
    }

    public void SetOpacity(double opacity) => this.BackEllipse.Opacity = Math.Max(0.01, opacity);

    private void OnOpClick(object sender, MouseButtonEventArgs e)
    {
      switch (this.Status)
      {
        case PomoStatus.Working:
          FocusTimer.Pause(DateTime.Now);
          break;
        case PomoStatus.Relaxing:
          FocusOptionUploader.AddOption(FocusOption.endBreak, DateTime.Now, true);
          break;
        case PomoStatus.WaitingWork:
          FocusTimer.BeginTimer();
          break;
        case PomoStatus.WaitingRelax:
          FocusTimer.BeginTimer();
          break;
        case PomoStatus.Pause:
          FocusTimer.Continue(new DateTime?(DateTime.Now));
          break;
      }
    }

    private void OnStopClick(object sender, MouseButtonEventArgs e) => FocusTimer.Drop();

    private void OnMoreClick(object sender, MouseButtonEventArgs e) => this.ShowOperation();

    private void ShowOperation()
    {
      int num = !this.OpStop.IsVisible || !this.OpStart.IsVisible ? 26 : 2;
      this.PopupTarget.Margin = new Thickness(this.IsLeftMode ? (double) num : 100.0, 0.0, this.IsLeftMode ? 100.0 : (double) num, 0.0);
      EscPopup escPopup = new EscPopup();
      escPopup.PlacementTarget = (UIElement) this.PopupTarget;
      escPopup.Placement = this.IsLeftMode ? PlacementMode.Left : PlacementMode.Right;
      escPopup.HorizontalOffset = this.IsLeftMode ? 3.0 : -3.0;
      escPopup.VerticalOffset = -8.0;
      escPopup.StaysOpen = false;
      this._morePopupShow = true;
      escPopup.Closed += (EventHandler) ((sender, args) =>
      {
        this._morePopupShow = false;
        if (this.IsMouseOver || !this._showOp)
          return;
        this.HideOpAnim();
      });
      escPopup.Child = (UIElement) new MiniFocusMoreControl((Popup) escPopup, Window.GetWindow((DependencyObject) this), FocusWindowType.Circle);
      escPopup.IsOpen = true;
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e) => this.ShowOperation();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/minifocus/circleminifocuscontrol.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnMouseLeave);
          ((UIElement) target).MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
          break;
        case 2:
          this.ClockPanel = (Grid) target;
          this.ClockPanel.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
          break;
        case 3:
          this.BackEllipse = (Ellipse) target;
          break;
        case 4:
          this.Progress = (PomoProgressBar) target;
          break;
        case 5:
          this.Clock = (ClockControl) target;
          break;
        case 6:
          this.GotPomo = (Image) target;
          break;
        case 7:
          this.PopupTarget = (ContentControl) target;
          break;
        case 8:
          this.OpBorder = (Border) target;
          break;
        case 9:
          this.OpTransform = (TranslateTransform) target;
          break;
        case 10:
          this.OpPanel = (StackPanel) target;
          break;
        case 11:
          this.OpStart = (Grid) target;
          this.OpStart.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpClick);
          break;
        case 12:
          this.OpStartIcon = (Path) target;
          break;
        case 13:
          this.OpStop = (Border) target;
          this.OpStop.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStopClick);
          break;
        case 14:
          this.OpMore = (Border) target;
          this.OpMore.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
          break;
        case 15:
          this.Geometry1 = (EllipseGeometry) target;
          break;
        case 16:
          this.Geometry2 = (EllipseGeometry) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
