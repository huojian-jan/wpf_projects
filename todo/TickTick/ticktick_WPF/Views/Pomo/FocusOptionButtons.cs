// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusOptionButtons
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Pomo.MainFocus;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusOptionButtons : StackPanel
  {
    private const string Start = "Start";
    private const string Pause = "Pause";
    private const string Continue = "Continue";
    private const string StartBreak = "StartBreak";
    private const string SkipBreak = "SkipBreak";
    private const string Stop = "Stop";
    protected const string Exit = "Exit";
    private double _buttonWidth = 140.0;
    private double _buttonHeight = 48.0;

    public void SetButtons()
    {
      this.Children.Clear();
      switch (TickFocusManager.Status)
      {
        case PomoStatus.Working:
          this.AddButton("Pause", "Pause");
          break;
        case PomoStatus.Relaxing:
          this.AddButton("SkipBreak", "StopRelax", true, true);
          this.AddButton("Exit", "Exit", isRest: true);
          break;
        case PomoStatus.WaitingWork:
          bool fromRelax = TickFocusManager.Config.FromRelax;
          this.AddButton("Start", fromRelax ? "ContinuePomo" : "Start", true);
          if (!fromRelax)
            break;
          this.AddButton("Exit", "Exit");
          break;
        case PomoStatus.WaitingRelax:
          this.AddButton("StartBreak", "BeginRelax", true, true);
          this.AddButton("SkipBreak", "SkipRelax", isRest: true);
          this.AddButton("Exit", "Exit", isRest: true);
          break;
        case PomoStatus.Pause:
          this.AddButton("Continue", "Continue", true);
          this.AddButton("Stop", "End");
          break;
      }
    }

    protected virtual void AddButton(string option, string textKey, bool isFill = false, bool isRest = false)
    {
      double top = 16.0 - (48.0 - this._buttonHeight) / 4.0;
      Button button = new Button();
      button.Margin = new Thickness(0.0, top, 0.0, 0.0);
      button.Tag = (object) option;
      button.Content = (object) Utils.GetString(textKey);
      button.Width = this._buttonWidth;
      button.Height = this._buttonHeight;
      button.FontSize = top;
      Button element = button;
      element.SetResourceReference(FrameworkElement.StyleProperty, (object) "FocusOptionButtonStyle");
      if (!isFill)
      {
        element.Background = (Brush) Brushes.Transparent;
        element.BorderThickness = new Thickness(1.0);
        element.SetResourceReference(Control.BorderBrushProperty, isRest ? (object) "PomoGreen" : (object) "PrimaryColor");
        element.SetResourceReference(Control.ForegroundProperty, isRest ? (object) "PomoGreen" : (object) "PrimaryColor");
      }
      else if (isRest)
        element.SetResourceReference(Control.BackgroundProperty, (object) "PomoGreen");
      element.Click += new RoutedEventHandler(this.OnOptionClick);
      this.Children.Add((UIElement) element);
    }

    protected virtual async void AddTodayStatistic()
    {
      FocusOptionButtons focusOptionButtons = this;
      TextBlock textBlock = new TextBlock();
      textBlock.Height = 18.0;
      textBlock.FontSize = 13.0;
      textBlock.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock.TextAlignment = TextAlignment.Center;
      textBlock.Margin = new Thickness(0.0, 16.0, 0.0, 30.0);
      TextBlock text = textBlock;
      text.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
      focusOptionButtons.Children.Add((UIElement) text);
      PomoStat pomoStat = await PomoService.LoadStatistics(TickFocusManager.IsPomo);
      text.Text = string.Format(Utils.GetString("TodayStatistics"), (object) (pomoStat.TodayPomos.ToString() + Utils.GetString("Pomos")), (object) Utils.GetDurationString(pomoStat.TodayDuration, showZero: true, showSpan: false));
      text = (TextBlock) null;
    }

    protected void OnOptionClick(object sender, RoutedEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.Tag is string tag))
        return;
      string data = "";
      string actCtype = TickFocusManager.GetActCType();
      if (tag != null)
      {
        switch (tag.Length)
        {
          case 4:
            switch (tag[0])
            {
              case 'E':
                if (tag == "Exit")
                {
                  data = "exit";
                  FocusTimer.Reset();
                  break;
                }
                break;
              case 'S':
                if (tag == "Stop")
                {
                  data = "end";
                  FocusTimer.StopOrAbandon();
                  break;
                }
                break;
            }
            break;
          case 5:
            switch (tag[0])
            {
              case 'P':
                if (tag == "Pause")
                {
                  data = "pause";
                  FocusTimer.Pause(DateTime.Now);
                  break;
                }
                break;
              case 'S':
                if (tag == "Start")
                {
                  data = !TickFocusManager.IsPomo || !TickFocusManager.Config.FromRelax ? "start" : "continue";
                  MainFocusControl parent = Utils.FindParent<MainFocusControl>((DependencyObject) this);
                  if (parent != null)
                  {
                    UserActCollectUtils.AddClickEvent("focus", "start_from_tab", parent.ExistTimer() ? "action_bar_expand" : "default_page");
                    UserActCollectUtils.AddClickEvent("focus", "start_from", "tab");
                  }
                  FocusTimer.BeginTimer();
                  break;
                }
                break;
            }
            break;
          case 8:
            if (tag == "Continue")
            {
              data = "continue";
              FocusTimer.Continue(new DateTime?(DateTime.Now));
              break;
            }
            break;
          case 9:
            if (tag == "SkipBreak")
            {
              data = TickFocusManager.Status == PomoStatus.Relaxing ? "finish" : "skip";
              FocusTimer.SkipRelax();
              break;
            }
            break;
          case 10:
            if (tag == "StartBreak")
            {
              data = "relax";
              FocusTimer.BeginTimer();
              break;
            }
            break;
        }
      }
      UserActCollectUtils.AddClickEvent("focus", actCtype, data);
    }

    public void SetButtonWidth(double width, double height)
    {
      this._buttonWidth = Math.Round(width, 0, MidpointRounding.ToEven);
      height = Math.Round(height, 0, MidpointRounding.AwayFromZero);
      this._buttonHeight = height - (double) ((int) height % 4);
      if (height - this._buttonHeight > 2.0)
        this._buttonHeight += 4.0;
      double top = 16.0 - (48.0 - this._buttonHeight) / 4.0;
      foreach (object child in this.Children)
      {
        if (child is Button button)
        {
          button.Width = this._buttonWidth;
          button.Height = this._buttonHeight;
          button.FontSize = top;
          button.Margin = new Thickness(0.0, top, 0.0, 0.0);
        }
      }
    }
  }
}
