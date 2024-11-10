// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusImmerseWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusImmerseWindow : MyWindow
  {
    private ClockControl _clock;
    private PageTurnClock _pageTurn;
    private TextBlock _textBlock;
    private TextBlock _textBlock1;
    private Border _clockEll;
    private Border _pageTurnEll;
    private Border _closeButton;
    private FocusImmerseButtons _buttons;
    private FocusImmerseButtons _buttons1;
    private DispatcherTimer _hideTimer = new DispatcherTimer()
    {
      Interval = TimeSpan.FromMilliseconds(4000.0)
    };
    private System.Windows.Point _mouseDownPoint;
    private static bool _usePageTurn = true;
    private TranslateTransform _pageTurnTranslate;
    private TranslateTransform _clockTranslate;
    private bool _switchingClock;
    private StackPanel _switchClockPanel;

    public FocusImmerseWindow()
    {
      this.WindowStyle = WindowStyle.None;
      this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      this.AllowsTransparency = true;
      this.Topmost = true;
      this.KeyDown += new KeyEventHandler(this.OnWindowKeyDown);
      this.Background = (Brush) Brushes.Transparent;
      this.Title = Utils.GetString("PomoTimer");
      this._hideTimer.Tick += new EventHandler(this.HideButton);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.MouseMove += new MouseEventHandler(this.OnMouseMove);
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
      this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
      WindowTouchPadHandler.AddHook((Window) this, new Action<bool>(this.HandleTouchPadScroll));
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      TickFocusManager.ImmerseWindow = (FocusImmerseWindow) null;
      this._hideTimer.Stop();
      TickFocusManager.StatusChanged -= new FocusChange(this.OnStatusChanged);
      TickFocusManager.CurrentSecondChanged -= new FocusChange(this.SetCountText);
      TickFocusManager.TypeChanged -= new FocusChange(this.OnFocusTypeChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusChanged), "NoTask");
      base.OnClosing(e);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.InitChildren();
      this.SetCountText();
      this.SetText();
      TickFocusManager.StatusChanged += new FocusChange(this.OnStatusChanged);
      TickFocusManager.CurrentSecondChanged += new FocusChange(this.SetCountText);
      TickFocusManager.TypeChanged += new FocusChange(this.OnFocusTypeChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusChanged), "NoTask");
    }

    private void OnFocusTypeChanged()
    {
    }

    private void SetCountText()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        int displaySecond = TickFocusManager.DisplaySecond;
        this._pageTurn.SetSecond(displaySecond);
        this._clock.SetTime(displaySecond);
      }));
    }

    private void OnStatusChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._buttons?.SetButtons();
        this._buttons1?.SetButtons();
        this.SetText();
      }));
    }

    private void SetText()
    {
      switch (TickFocusManager.Status)
      {
        case PomoStatus.Working:
          this._textBlock.Text = string.IsNullOrEmpty(TickFocusManager.Config.FocusVModel.FocusId) ? Utils.GetString("Focusing") : TickFocusManager.Config.FocusVModel.Title;
          break;
        case PomoStatus.Relaxing:
          this._textBlock.Text = Utils.GetString("ImmerseRelaxing");
          break;
        case PomoStatus.WaitingWork:
          this._textBlock.Text = TickFocusManager.IsPomo ? Utils.GetString(TickFocusManager.Config.FromRelax ? "GoonNextPomo" : "ImmerseWaitPomo") : Utils.GetString("ImmerseWaitTiming");
          break;
        case PomoStatus.WaitingRelax:
          this._textBlock.Text = LocalSettings.Settings.LongBreakEvery <= 1 || TickFocusManager.Config.PomoCount >= 1 && TickFocusManager.Config.PomoCount % LocalSettings.Settings.LongBreakEvery == 0 ? string.Format(Utils.GetString("ImmerseWaitRelaxL"), (object) LocalSettings.Settings.LongBreakEvery) : Utils.GetString("ImmerseWaitRelaxS");
          break;
        case PomoStatus.Pause:
          this._textBlock.Text = Utils.GetString("Paused") + (TickFocusManager.IsPomo ? ", " + Utils.GetString("PausingText") : "");
          break;
      }
      this._textBlock1.Text = this._textBlock.Text;
    }

    private void OnFocusChanged(object sender, PropertyChangedEventArgs e)
    {
      if (TickFocusManager.Status != PomoStatus.Working)
        return;
      this._textBlock.Text = string.IsNullOrEmpty(TickFocusManager.Config.FocusVModel.FocusId) ? Utils.GetString("Focusing") : TickFocusManager.Config.FocusVModel.Title;
      this._textBlock1.Text = this._textBlock.Text;
    }

    private void InitChildren()
    {
      this.WindowState = WindowState.Maximized;
      this._clockTranslate = new TranslateTransform()
      {
        X = FocusImmerseWindow._usePageTurn ? this.ActualWidth : 0.0
      };
      ClockControl clockControl = new ClockControl();
      clockControl.FontSize = this.ActualHeight / 8.0;
      clockControl.Foreground = (Brush) ThemeUtil.GetColorInString("#999999");
      clockControl.HorizontalAlignment = HorizontalAlignment.Center;
      clockControl.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      clockControl.RenderTransform = (Transform) this._clockTranslate;
      clockControl.Margin = new Thickness(0.0, 0.0, 0.0, 70.0);
      this._clock = clockControl;
      this._pageTurnTranslate = new TranslateTransform()
      {
        X = !FocusImmerseWindow._usePageTurn ? -1.0 * this.ActualWidth : 0.0
      };
      PageTurnClock pageTurnClock = new PageTurnClock();
      pageTurnClock.VerticalAlignment = VerticalAlignment.Center;
      pageTurnClock.HorizontalAlignment = HorizontalAlignment.Center;
      pageTurnClock.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      pageTurnClock.RenderTransform = (Transform) this._pageTurnTranslate;
      pageTurnClock.Margin = new Thickness(0.0, 0.0, 0.0, 70.0);
      this._pageTurn = pageTurnClock;
      this._pageTurn.SetWidth(Math.Min(this.ActualWidth / 4.0, this.ActualHeight / 3.0));
      TextBlock textBlock1 = new TextBlock();
      textBlock1.FontSize = 18.0;
      textBlock1.Foreground = (Brush) ThemeUtil.GetColorInString("#5C5C5C");
      textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
      textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock1.Margin = new Thickness(0.0, 0.0, 0.0, this.ActualHeight / 3.0 - 20.0);
      textBlock1.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      textBlock1.RenderTransform = (Transform) this._pageTurnTranslate;
      this._textBlock = textBlock1;
      TextBlock textBlock2 = new TextBlock();
      textBlock2.FontSize = 18.0;
      textBlock2.Foreground = (Brush) ThemeUtil.GetColorInString("#5C5C5C");
      textBlock2.VerticalAlignment = VerticalAlignment.Center;
      textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock2.Margin = new Thickness(0.0, this.ActualHeight / 8.0 + 40.0, 0.0, 0.0);
      textBlock2.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      textBlock2.RenderTransform = (Transform) this._clockTranslate;
      this._textBlock1 = textBlock2;
      StackPanel stackPanel = new StackPanel();
      stackPanel.Orientation = Orientation.Horizontal;
      stackPanel.VerticalAlignment = VerticalAlignment.Bottom;
      stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
      stackPanel.Margin = new Thickness(0.0, 0.0, 0.0, 40.0);
      this._switchClockPanel = stackPanel;
      this._clockEll = GetBottomEll();
      this._clockEll.MouseLeftButtonUp += (MouseButtonEventHandler) ((o, arg) => this.SwitchClock(false));
      this._switchClockPanel.Children.Add((UIElement) this._clockEll);
      this._pageTurnEll = GetBottomEll();
      this._pageTurnEll.MouseLeftButtonUp += (MouseButtonEventHandler) ((o, arg) => this.SwitchClock(true));
      this._switchClockPanel.Children.Add((UIElement) this._pageTurnEll);
      FocusImmerseButtons focusImmerseButtons1 = new FocusImmerseButtons();
      focusImmerseButtons1.VerticalAlignment = VerticalAlignment.Bottom;
      focusImmerseButtons1.HorizontalAlignment = HorizontalAlignment.Center;
      focusImmerseButtons1.Margin = new Thickness(0.0, 0.0, 0.0, this.ActualHeight / 3.0 - 100.0);
      focusImmerseButtons1.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      focusImmerseButtons1.RenderTransform = (Transform) this._pageTurnTranslate;
      this._buttons = focusImmerseButtons1;
      this._buttons.SetButtons();
      FocusImmerseButtons focusImmerseButtons2 = new FocusImmerseButtons();
      focusImmerseButtons2.VerticalAlignment = VerticalAlignment.Center;
      focusImmerseButtons2.HorizontalAlignment = HorizontalAlignment.Center;
      focusImmerseButtons2.Margin = new Thickness(0.0, this.ActualHeight / 8.0 + 180.0, 0.0, 0.0);
      focusImmerseButtons2.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      focusImmerseButtons2.RenderTransform = (Transform) this._clockTranslate;
      this._buttons1 = focusImmerseButtons2;
      this._buttons1.SetButtons();
      Border border1 = new Border();
      border1.Width = 24.0;
      border1.Height = 24.0;
      border1.Cursor = Cursors.Hand;
      border1.HorizontalAlignment = HorizontalAlignment.Right;
      border1.VerticalAlignment = VerticalAlignment.Bottom;
      border1.Margin = new Thickness(0.0, 0.0, 40.0, 40.0);
      Border border2 = new Border();
      border2.Style = (Style) this.FindResource((object) "SmoothHoverBorderStyle40_60");
      Path path = new Path();
      path.VerticalAlignment = VerticalAlignment.Center;
      path.HorizontalAlignment = HorizontalAlignment.Center;
      path.Width = 18.0;
      path.Height = 18.0;
      path.Data = Utils.GetIcon("IcShrink");
      path.Fill = (Brush) ThemeUtil.GetColorInString("#FFFFFF");
      border2.Child = (UIElement) path;
      border1.Child = (UIElement) border2;
      this._closeButton = border1;
      this._closeButton.MouseLeftButtonUp += (MouseButtonEventHandler) ((o, arg) => this.TryClose());
      this._buttons.SetBinding(UIElement.OpacityProperty, (BindingBase) new Binding("Opacity")
      {
        Source = (object) this._closeButton
      });
      this._buttons1.SetBinding(UIElement.OpacityProperty, (BindingBase) new Binding("Opacity")
      {
        Source = (object) this._closeButton
      });
      this._switchClockPanel.SetBinding(UIElement.OpacityProperty, (BindingBase) new Binding("Opacity")
      {
        Source = (object) this._closeButton
      });
      Grid grid = new Grid();
      grid.Background = (Brush) Brushes.Black;
      grid.Margin = new Thickness(-1.0);
      grid.Children.Add((UIElement) this._clock);
      grid.Children.Add((UIElement) this._pageTurn);
      grid.Children.Add((UIElement) this._textBlock);
      grid.Children.Add((UIElement) this._textBlock1);
      grid.Children.Add((UIElement) this._switchClockPanel);
      grid.Children.Add((UIElement) this._buttons);
      grid.Children.Add((UIElement) this._buttons1);
      grid.Children.Add((UIElement) this._closeButton);
      this.Content = (object) grid;

      static Border GetBottomEll()
      {
        Border bottomEll = new Border();
        bottomEll.Width = 16.0;
        bottomEll.Height = 16.0;
        bottomEll.Cursor = Cursors.Hand;
        bottomEll.Background = (Brush) Brushes.Transparent;
        bottomEll.Margin = new Thickness(4.0, 0.0, 4.0, 0.0);
        Ellipse ellipse = new Ellipse();
        ellipse.Width = 10.0;
        ellipse.Height = 10.0;
        ellipse.Fill = (Brush) Brushes.White;
        ellipse.Opacity = 0.4;
        ellipse.VerticalAlignment = VerticalAlignment.Center;
        ellipse.HorizontalAlignment = HorizontalAlignment.Center;
        bottomEll.Child = (UIElement) ellipse;
        return bottomEll;
      }
    }

    private void TryClose() => this.Close();

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      this.TryClose();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      this.Cursor = Cursors.Arrow;
      if (!this._hideTimer.IsEnabled || Math.Abs(this._closeButton.Opacity - 1.0) > 0.1)
      {
        foreach (FrameworkElement child in this._buttons.Children)
          child.Cursor = Cursors.Hand;
        foreach (FrameworkElement child in this._buttons1.Children)
          child.Cursor = Cursors.Hand;
        this._closeButton.Opacity = 1.0;
        if (FocusTimer.IsTiming)
          this._hideTimer.Start();
      }
      if (this._buttons.IsMouseOver || this._buttons1.IsMouseOver)
        this._hideTimer.Stop();
      if (e.LeftButton != MouseButtonState.Pressed || !(this._mouseDownPoint != new System.Windows.Point()))
        return;
      double x = (e.GetPosition((IInputElement) this) - this._mouseDownPoint).X;
      if (Math.Abs(x) <= 2.0)
        return;
      if (FocusImmerseWindow._usePageTurn)
      {
        this._pageTurnTranslate.X = x;
        this._clockTranslate.X = x > 0.0 ? x - this.ActualWidth : this.ActualWidth + x;
      }
      else
      {
        this._clockTranslate.X = x;
        this._pageTurnTranslate.X = x > 0.0 ? x - this.ActualWidth : this.ActualWidth + x;
      }
    }

    private async void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      FocusImmerseWindow relativeTo = this;
      if (!relativeTo._switchingClock && relativeTo._mouseDownPoint != new System.Windows.Point())
      {
        double x = (e.GetPosition((IInputElement) relativeTo) - relativeTo._mouseDownPoint).X;
        if (Math.Abs(x) > 2.0)
        {
          relativeTo._switchingClock = true;
          DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 400);
          DoubleAnimation doubleAnimation2 = AnimationUtils.GetDoubleAnimation(new double?(), x > 0.0 ? -1.0 * relativeTo.ActualWidth : relativeTo.ActualWidth, 400);
          DoubleAnimation doubleAnimation3 = doubleAnimation1;
          CubicEase cubicEase1 = new CubicEase();
          cubicEase1.EasingMode = EasingMode.EaseOut;
          doubleAnimation3.EasingFunction = (IEasingFunction) cubicEase1;
          DoubleAnimation doubleAnimation4 = doubleAnimation2;
          CubicEase cubicEase2 = new CubicEase();
          cubicEase2.EasingMode = EasingMode.EaseOut;
          doubleAnimation4.EasingFunction = (IEasingFunction) cubicEase2;
          if (Math.Abs(x) > relativeTo.ActualWidth / 4.0)
          {
            FocusImmerseWindow._usePageTurn = !FocusImmerseWindow._usePageTurn;
            relativeTo._clockEll.Opacity = FocusImmerseWindow._usePageTurn ? 0.4 : 0.6;
            relativeTo._pageTurnEll.Opacity = FocusImmerseWindow._usePageTurn ? 0.6 : 0.4;
            doubleAnimation2.To = new double?(x < 0.0 ? -1.0 * relativeTo.ActualWidth : relativeTo.ActualWidth);
          }
          relativeTo._pageTurnTranslate.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
          relativeTo._clockTranslate.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
          relativeTo._pageTurnTranslate.BeginAnimation(TranslateTransform.XProperty, FocusImmerseWindow._usePageTurn ? (AnimationTimeline) doubleAnimation1 : (AnimationTimeline) doubleAnimation2);
          relativeTo._clockTranslate.BeginAnimation(TranslateTransform.XProperty, !FocusImmerseWindow._usePageTurn ? (AnimationTimeline) doubleAnimation1 : (AnimationTimeline) doubleAnimation2);
          await Task.Delay(600);
          relativeTo._switchingClock = false;
        }
      }
      relativeTo._mouseDownPoint = new System.Windows.Point();
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this._switchingClock)
        return;
      this._pageTurnTranslate.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
      this._pageTurnTranslate.X = FocusImmerseWindow._usePageTurn ? 0.0 : -1.0 * this.ActualWidth;
      this._clockTranslate.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
      this._clockTranslate.X = !FocusImmerseWindow._usePageTurn ? 0.0 : this.ActualWidth;
      this._mouseDownPoint = e.GetPosition((IInputElement) this);
    }

    private void SwitchClock(bool isPageTurn)
    {
      if (isPageTurn == FocusImmerseWindow._usePageTurn)
        return;
      this.SwitchClock(true, !isPageTurn);
    }

    private void HandleTouchPadScroll(bool isLeft)
    {
      if (this._switchingClock)
        return;
      this.SwitchClock(true, !isLeft);
    }

    private async void SwitchClock(bool withAnim, bool rightSwitch)
    {
      FocusImmerseWindow focusImmerseWindow = this;
      if (focusImmerseWindow._switchingClock)
        return;
      if (withAnim)
      {
        focusImmerseWindow._switchingClock = true;
        FocusImmerseWindow._usePageTurn = !FocusImmerseWindow._usePageTurn;
        DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 800);
        DoubleAnimation doubleAnimation2 = AnimationUtils.GetDoubleAnimation(new double?(), rightSwitch ? -1.0 * focusImmerseWindow.ActualWidth : focusImmerseWindow.ActualWidth, 800);
        DoubleAnimation doubleAnimation3 = doubleAnimation1;
        CubicEase cubicEase1 = new CubicEase();
        cubicEase1.EasingMode = EasingMode.EaseOut;
        doubleAnimation3.EasingFunction = (IEasingFunction) cubicEase1;
        DoubleAnimation doubleAnimation4 = doubleAnimation2;
        CubicEase cubicEase2 = new CubicEase();
        cubicEase2.EasingMode = EasingMode.EaseOut;
        doubleAnimation4.EasingFunction = (IEasingFunction) cubicEase2;
        focusImmerseWindow._pageTurnTranslate.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
        focusImmerseWindow._clockTranslate.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
        focusImmerseWindow._pageTurnTranslate.BeginAnimation(TranslateTransform.XProperty, FocusImmerseWindow._usePageTurn ? (AnimationTimeline) doubleAnimation1 : (AnimationTimeline) doubleAnimation2);
        focusImmerseWindow._clockTranslate.BeginAnimation(TranslateTransform.XProperty, !FocusImmerseWindow._usePageTurn ? (AnimationTimeline) doubleAnimation1 : (AnimationTimeline) doubleAnimation2);
      }
      focusImmerseWindow._clockEll.Opacity = FocusImmerseWindow._usePageTurn ? 0.4 : 0.6;
      focusImmerseWindow._pageTurnEll.Opacity = !FocusImmerseWindow._usePageTurn ? 0.4 : 0.6;
      focusImmerseWindow.UpdateLayout();
      await Task.Delay(600);
      focusImmerseWindow._switchingClock = false;
    }

    private void HideButton(object sender, EventArgs e)
    {
      this.Cursor = Cursors.None;
      foreach (FrameworkElement child in this._buttons.Children)
        child.Cursor = Cursors.None;
      foreach (FrameworkElement child in this._buttons1.Children)
        child.Cursor = Cursors.None;
      this._closeButton.Opacity = 0.0;
      this._hideTimer.Stop();
    }
  }
}
