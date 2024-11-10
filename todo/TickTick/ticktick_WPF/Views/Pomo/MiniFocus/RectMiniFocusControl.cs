// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MiniFocus.RectMiniFocusControl
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
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MiniFocus
{
  public class RectMiniFocusControl : UserControl, IMiniFocus, IComponentConnector
  {
    private bool _popupShowing;
    internal ContentControl PopupRect;
    internal Rectangle BackRect;
    internal ClockControl Clock;
    internal BlurEffect ClockBlur;
    internal Image GotPomo;
    internal Grid OpPanel;
    internal Border OpMore;
    internal Border OpStart;
    internal Path OpStartIcon;
    internal Border OpStop;
    private bool _contentLoaded;

    public PomoStatus Status => TickFocusManager.Status;

    public RectMiniFocusControl()
    {
      this.InitializeComponent();
      this.Clock.ShowHourFont = new int?(14);
    }

    private async void OnMouseEnter(object sender, MouseEventArgs e)
    {
      RectMiniFocusControl miniFocusControl = this;
      if (Math.Abs(miniFocusControl.OpPanel.Opacity - 1.0) <= 0.1)
        return;
      await Task.Delay(600);
      // ISSUE: explicit non-virtual call
      if (!__nonvirtual (miniFocusControl.IsMouseOver) && !miniFocusControl._popupShowing)
        return;
      miniFocusControl.OpPanel.Opacity = 1.0;
      miniFocusControl.ClockBlur.Radius = 6.0;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (this._popupShowing)
        return;
      this.ClockBlur.Radius = 0.0;
      this.OpPanel.Opacity = 0.0;
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e) => this.ShowOperation();

    private void ShowOperation()
    {
      this._popupShowing = true;
      EscPopup escPopup = new EscPopup();
      escPopup.PlacementTarget = (UIElement) this.PopupRect;
      escPopup.Placement = PlacementMode.Right;
      escPopup.HorizontalOffset = -2.0;
      escPopup.VerticalOffset = -8.0;
      escPopup.StaysOpen = false;
      escPopup.Child = (UIElement) new MiniFocusMoreControl((Popup) escPopup, Window.GetWindow((DependencyObject) this), FocusWindowType.Mini);
      escPopup.Closed += (EventHandler) ((o, e) =>
      {
        this._popupShowing = false;
        if (this.IsMouseOver)
          return;
        this.ClockBlur.Radius = 0.0;
        this.OpPanel.Opacity = 0.0;
      });
      escPopup.IsOpen = true;
    }

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

    public void OnStatusChanged()
    {
      this.SetTimePanel();
      this.SetOptionStatus();
    }

    private void SetOptionStatus()
    {
      bool flag1 = this.Status == PomoStatus.Relaxing;
      bool flag2 = this.Status == PomoStatus.Pause;
      bool flag3 = this.Status == PomoStatus.WaitingRelax;
      this.OpStart.Visibility = flag1 ? Visibility.Collapsed : Visibility.Visible;
      this.OpStop.Visibility = flag1 | flag2 | flag3 ? Visibility.Visible : Visibility.Collapsed;
      this.OpStartIcon.Data = Utils.GetIcon(this.Status == PomoStatus.Working ? "IcPomoPause" : "IcPomoStart");
      this.OpStartIcon.SetResourceReference(Shape.FillProperty, flag1 | flag3 ? (object) "PomoGreen" : (object) "PrimaryColor");
      this.Clock.SetResourceReference(ClockControl.ForegroundProperty, flag1 | flag3 ? (object) "PomoGreen" : (object) "BaseColorOpacity100");
      this.Clock.Visibility = flag3 ? Visibility.Collapsed : Visibility.Visible;
      switch (this.Status)
      {
        case PomoStatus.Working:
          this.OpStart.ToolTip = (object) Utils.GetString("Pause");
          break;
        case PomoStatus.Relaxing:
          this.OpStop.ToolTip = (object) Utils.GetString("SkipRelax");
          break;
        case PomoStatus.WaitingWork:
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

    private void SetTimePanel()
    {
      if (TickFocusManager.IsPomo && (this.Status == PomoStatus.WaitingRelax || this.Status == PomoStatus.WaitingWork))
      {
        this.Clock.ToolTip = (object) Utils.GetString("ClickToEdit");
        this.Clock.Cursor = Cursors.Hand;
      }
      else
      {
        this.Clock.ToolTip = (object) null;
        this.Clock.Cursor = Cursors.Arrow;
      }
    }

    public void OnFocusTypeChanged()
    {
      this.Clock.ShowHour = !TickFocusManager.IsPomo;
      this.SetCountText();
    }

    public double GetLeftMargin() => 16.0;

    public double GetActualWidth() => 70.0;

    public double GetExtraWidth() => 0.0;

    public double GetActualHeight() => 36.0;

    public void SetMoving(bool b)
    {
    }

    public bool CanDragMove()
    {
      return !this.OpMore.IsMouseOver && !this.OpStart.IsMouseOver && !this.OpStop.IsMouseOver;
    }

    public bool CanHide() => !this._popupShowing;

    public void SetHideStyle(bool isHide)
    {
    }

    public void OnWindowStartHide()
    {
    }

    public void SetCountText()
    {
      this.Clock.SetTime((int) Math.Round(TickFocusManager.Config.CurrentSeconds, 0, MidpointRounding.AwayFromZero));
    }

    public void Init()
    {
      this.OnStatusChanged();
      this.OnFocusTypeChanged();
      this.SetOpacity(LocalSettings.Settings.PomoWindowOpacity);
      this.GotPomo.Source = (ImageSource) ImageUtils.GetResourceImage("pack://application:,,,/Assets/get_pomo.png", 52);
    }

    public void SetOpacity(double opacity) => this.BackRect.Opacity = Math.Max(0.01, opacity);

    private void OnRightClick(object sender, MouseButtonEventArgs e) => this.ShowOperation();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/minifocus/rectminifocuscontrol.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnMouseEnter);
          ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnMouseLeave);
          break;
        case 2:
          this.PopupRect = (ContentControl) target;
          break;
        case 3:
          this.BackRect = (Rectangle) target;
          break;
        case 4:
          this.Clock = (ClockControl) target;
          break;
        case 5:
          this.ClockBlur = (BlurEffect) target;
          break;
        case 6:
          this.GotPomo = (Image) target;
          break;
        case 7:
          this.OpPanel = (Grid) target;
          this.OpPanel.MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
          break;
        case 8:
          this.OpMore = (Border) target;
          this.OpMore.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
          break;
        case 9:
          this.OpStart = (Border) target;
          this.OpStart.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpClick);
          break;
        case 10:
          this.OpStartIcon = (Path) target;
          break;
        case 11:
          this.OpStop = (Border) target;
          this.OpStop.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStopClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
