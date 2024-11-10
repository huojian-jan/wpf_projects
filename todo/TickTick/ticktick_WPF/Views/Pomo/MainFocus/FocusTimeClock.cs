// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MainFocus.FocusTimeClock
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MainFocus
{
  public class FocusTimeClock : Grid
  {
    private PomoProgressBar _progress;
    private ClockControl _timeText;
    private Border _subtractButton;
    private Border _addButton;
    private TextBlock _bottomText;

    public FocusTimeClock()
    {
      this.Width = 268.0;
      this.Height = 268.0;
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star)
      });
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star)
      });
      Ellipse ellipse = new Ellipse();
      ellipse.Fill = (Brush) Brushes.Transparent;
      Ellipse element = ellipse;
      element.SetValue(Grid.ColumnSpanProperty, (object) 3);
      this.Children.Add((UIElement) element);
      this.InitProgressBar();
      this.InitTimeText();
      this.InitAdjustButton();
      this.SetTime();
      this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      this.ShowOrHideAdjustButton(false);
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (TickFocusManager.Status != PomoStatus.Working || !TickFocusManager.IsPomo)
        return;
      this.ShowOrHideAdjustButton(true);
    }

    private void ShowOrHideAdjustButton(bool show)
    {
      DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(), (double) (show ? 1 : 0), 120);
      if (show)
        doubleAnimation.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(100.0));
      this._subtractButton.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) doubleAnimation);
      this._addButton.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) doubleAnimation);
      if (TickFocusManager.Status != PomoStatus.Pause)
        this._bottomText?.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) doubleAnimation);
      this._subtractButton.IsEnabled = show;
      this._addButton.IsEnabled = show;
    }

    private void InitAdjustButton()
    {
      Border border1 = new Border();
      border1.CornerRadius = new CornerRadius(2.0);
      border1.Width = 24.0;
      border1.Height = 24.0;
      border1.HorizontalAlignment = HorizontalAlignment.Right;
      border1.Margin = new Thickness(0.0, 0.0, 8.0, 0.0);
      border1.Opacity = 0.0;
      this._subtractButton = border1;
      this._subtractButton.SetResourceReference(FrameworkElement.StyleProperty, (object) "HoverBorderStyle");
      Path path1 = new Path();
      path1.Height = 18.0;
      path1.Width = 18.0;
      path1.Stretch = Stretch.Uniform;
      path1.Data = Utils.GetIcon("IcSubtract");
      Path path2 = path1;
      path2.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor");
      this._subtractButton.Child = (UIElement) path2;
      this._subtractButton.SetValue(Grid.ColumnProperty, (object) 0);
      this.Children.Add((UIElement) this._subtractButton);
      Border border2 = new Border();
      border2.CornerRadius = new CornerRadius(2.0);
      border2.Width = 24.0;
      border2.Height = 24.0;
      border2.HorizontalAlignment = HorizontalAlignment.Left;
      border2.Margin = new Thickness(8.0, 0.0, 0.0, 0.0);
      border2.Opacity = 0.0;
      this._addButton = border2;
      this._addButton.SetResourceReference(FrameworkElement.StyleProperty, (object) "HoverBorderStyle");
      Path path3 = new Path();
      path3.Height = 18.0;
      path3.Width = 18.0;
      path3.Stretch = Stretch.Uniform;
      path3.Data = Utils.GetIcon("IcAdd");
      Path path4 = path3;
      path4.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor");
      this._addButton.SetValue(Grid.ColumnProperty, (object) 2);
      this._addButton.Child = (UIElement) path4;
      this.Children.Add((UIElement) this._addButton);
      this._subtractButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.SubtractDuration);
      this._addButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.AddDuration);
    }

    private void AddDuration(object sender, MouseButtonEventArgs e)
    {
      TickFocusManager.AdjustDuration(true);
    }

    private void SubtractDuration(object sender, MouseButtonEventArgs e)
    {
      TickFocusManager.AdjustDuration(false);
    }

    private void InitProgressBar()
    {
      PomoProgressBar pomoProgressBar = new PomoProgressBar();
      pomoProgressBar.HorizontalAlignment = HorizontalAlignment.Center;
      pomoProgressBar.Height = 260.0;
      pomoProgressBar.Width = 260.0;
      pomoProgressBar.Angle = TickFocusManager.Config.GetDisplayAngle(false);
      pomoProgressBar.StrokeThickness = 4.0;
      pomoProgressBar.IsStrokeMode = TickFocusManager.IsPomo;
      this._progress = pomoProgressBar;
      this._progress.SetValue(Grid.ColumnSpanProperty, (object) 3);
      this._progress.SetResourceReference(PomoProgressBar.UnderColorProperty, (object) "BaseColorOpacity10");
      this._progress.SetResourceReference(PomoProgressBar.TopColorProperty, (object) "PrimaryColor");
      this.Children.Add((UIElement) this._progress);
    }

    private void InitTimeText()
    {
      ClockControl clockControl = new ClockControl();
      clockControl.HorizontalAlignment = HorizontalAlignment.Center;
      clockControl.VerticalAlignment = VerticalAlignment.Center;
      clockControl.FontSize = 45.0;
      clockControl.Background = (Brush) Brushes.Transparent;
      this._timeText = clockControl;
      this._timeText.SetValue(Grid.ColumnProperty, (object) 1);
      this._timeText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this._timeText.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTimeClick);
      this._timeText.ShowHour = !TickFocusManager.IsPomo;
      this.Children.Add((UIElement) this._timeText);
      TextBlock textBlock = new TextBlock();
      textBlock.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock.VerticalAlignment = VerticalAlignment.Center;
      textBlock.TextAlignment = TextAlignment.Center;
      textBlock.FontSize = 14.0;
      textBlock.Background = (Brush) Brushes.Transparent;
      textBlock.Margin = new Thickness(0.0, 72.0, 0.0, 0.0);
      textBlock.Opacity = 0.0;
      this._bottomText = textBlock;
      this._bottomText.SetValue(Grid.ColumnProperty, (object) 1);
      this._bottomText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
      this.Children.Add((UIElement) this._bottomText);
      this.SetTime();
    }

    private void OnTimeClick(object sender, MouseButtonEventArgs e)
    {
      if (!TickFocusManager.IsPomo || TickFocusManager.Status != PomoStatus.WaitingWork)
        return;
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.PlacementTarget = (UIElement) this._timeText;
      escPopup.Placement = PlacementMode.Center;
      escPopup.HorizontalOffset = 2.0;
      escPopup.VerticalOffset = 80.0;
      escPopup.Child = (UIElement) new PomoModitySpanConfig((Popup) escPopup);
      escPopup.IsOpen = true;
      UserActCollectUtils.AddClickEvent("focus", "focus_tab", "click_time");
    }

    public void SetTime()
    {
      if (!this._progress.IsStrokeMode && TickFocusManager.Config.CurrentSeconds == 0.0)
        this._progress.Reset();
      if (TickFocusManager.Status == PomoStatus.Working || TickFocusManager.Status == PomoStatus.Relaxing)
        this._progress.TargetAngel = TickFocusManager.Config.GetDisplayAngle(true);
      else
        this._progress.SetAngle(TickFocusManager.Config.GetDisplayAngle(false));
      if (!this._progress.IsStrokeMode && TickFocusManager.Config.CurrentSeconds >= 30.0)
        this._progress.HideLeftMask();
      this.SetTimeText();
    }

    private void SetTimeText()
    {
      this._timeText.SetTime(TickFocusManager.DisplaySecond);
      if (TickFocusManager.Status == PomoStatus.Pause)
        return;
      DateTime startTime1 = TickFocusManager.Config.StartTime;
      DateTime dateTime = TickFocusManager.Config.StartTime;
      dateTime = dateTime.AddSeconds((double) TickFocusManager.Config.Second);
      DateTime startTime2 = dateTime.AddSeconds(TickFocusManager.Config.PauseDuration);
      this._bottomText.Text = ticktick_WPF.Util.DateUtils.GetTimeText(startTime1) + " - " + ticktick_WPF.Util.DateUtils.GetTimeText(startTime2);
    }

    public void SetProgressMode()
    {
      this._progress.IsStrokeMode = TickFocusManager.IsPomo;
      this._timeText.ShowHour = !TickFocusManager.IsPomo;
      this._progress.Reset();
    }

    public void OnStatusChanged()
    {
      if (TickFocusManager.Status == PomoStatus.Working || TickFocusManager.Status == PomoStatus.Relaxing)
        this.SetTime();
      if (TickFocusManager.Status != PomoStatus.Working)
        this.ShowOrHideAdjustButton(false);
      if (TickFocusManager.Status == PomoStatus.Pause)
      {
        this._bottomText.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        this._bottomText.Opacity = 1.0;
        this._bottomText.Text = Utils.GetString("Paused");
      }
      else
        this._bottomText.Opacity = (double) (this.IsMouseOver ? 1 : 0);
      if (TickFocusManager.IsPomo && TickFocusManager.Status == PomoStatus.WaitingWork)
        this._timeText.Cursor = Cursors.Hand;
      else
        this._timeText.Cursor = Cursors.Arrow;
      if (TickFocusManager.Status == PomoStatus.Relaxing)
        this._progress.SetResourceReference(PomoProgressBar.TopColorProperty, (object) "PomoGreen");
      else
        this._progress.SetResourceReference(PomoProgressBar.TopColorProperty, (object) "PrimaryColor");
    }

    public void SetSize(double width)
    {
      width = Math.Round(width, 0, MidpointRounding.AwayFromZero);
      double width1 = width - (double) ((int) width % 4);
      if (width - width1 > 2.0)
        width1 += 4.0;
      this.Width = width1;
      this.Height = width1;
      this._progress.SetSize(width1);
      this._timeText.FontSize = width1 / 6.0;
      this._bottomText.Margin = new Thickness(0.0, 72.0 + width1 / 6.0 - 44.0, 0.0, 0.0);
    }
  }
}
