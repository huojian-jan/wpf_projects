// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MainFocus.SmallFocusView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MainFocus
{
  public class SmallFocusView : Grid
  {
    private FocusObjIcon _icon;
    private ClockControl _timeClock;
    private EmjTextBlock _titleText;
    private TextBlock _add1Text;
    private HoverIconButton _option;
    private HoverIconButton _dropOption;

    public SmallFocusView()
    {
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.ColumnDefinitions.Add(new ColumnDefinition());
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.Height = 72.0;
      Border element = new Border()
      {
        BorderThickness = new Thickness(0.0, 1.0, 0.0, 0.0)
      };
      element.SetResourceReference(Panel.BackgroundProperty, (object) "SmallFocusBackground");
      element.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity5");
      element.SetValue(Grid.ColumnSpanProperty, (object) 3);
      this.Children.Add((UIElement) element);
      this.InitIcon();
      this.InitTimePanel();
      this.InitButtons();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnload);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      TickFocusManager.CurrentSecondChanged += new FocusChange(this.SetCountText);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnTitleChanged), "Title");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusChanged), "NoTask");
    }

    private void OnUnload(object sender, RoutedEventArgs e)
    {
      TickFocusManager.CurrentSecondChanged -= new FocusChange(this.SetCountText);
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnTitleChanged), "Title");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusChanged), "NoTask");
    }

    private void InitButtons()
    {
      StackPanel stackPanel = new StackPanel();
      stackPanel.Margin = new Thickness(0.0, 0.0, 20.0, 0.0);
      stackPanel.VerticalAlignment = VerticalAlignment.Center;
      stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
      stackPanel.Orientation = Orientation.Horizontal;
      StackPanel element = stackPanel;
      element.SetValue(Grid.ColumnProperty, (object) 2);
      HoverIconButton hoverIconButton1 = new HoverIconButton();
      hoverIconButton1.Height = 36.0;
      hoverIconButton1.Width = 36.0;
      this._option = hoverIconButton1;
      this._option.BackBorder.CornerRadius = new CornerRadius(18.0);
      this._option.BackBorder.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle10_20");
      this._option.BackBorder.SetResourceReference(Panel.BackgroundProperty, (object) "PrimaryColor");
      this._option.IsImage = false;
      this._option.ImageWidth = 20.0;
      this._option.ImageOpacity = 1.0;
      this._option.SetIconColor("PrimaryColor");
      this._option.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOptionClick);
      element.Children.Add((UIElement) this._option);
      HoverIconButton hoverIconButton2 = new HoverIconButton();
      hoverIconButton2.Height = 36.0;
      hoverIconButton2.Width = 36.0;
      hoverIconButton2.Margin = new Thickness(16.0, 0.0, 0.0, 0.0);
      this._dropOption = hoverIconButton2;
      this._dropOption.BackBorder.CornerRadius = new CornerRadius(18.0);
      this._dropOption.BackBorder.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle10_20");
      this._dropOption.BackBorder.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity60");
      this._dropOption.IsImage = false;
      this._dropOption.ImageWidth = 20.0;
      this._dropOption.ImageOpacity = 1.0;
      this._dropOption.IconData = Utils.GetIcon("IcPomoStop");
      this._dropOption.SetIconColor("BaseColorOpacity60");
      this._dropOption.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDropClick);
      element.Children.Add((UIElement) this._dropOption);
      this.Children.Add((UIElement) element);
      this.SetButtons();
    }

    private void InitIcon()
    {
      FocusObjIcon focusObjIcon = new FocusObjIcon(30.0);
      focusObjIcon.VerticalAlignment = VerticalAlignment.Center;
      focusObjIcon.HorizontalAlignment = HorizontalAlignment.Left;
      focusObjIcon.Margin = new Thickness(20.0, 0.0, 0.0, 0.0);
      this._icon = focusObjIcon;
      this.Children.Add((UIElement) this._icon);
      this.SetPomoGotIcon();
    }

    private void InitTimePanel()
    {
      StackPanel stackPanel = new StackPanel();
      stackPanel.Margin = new Thickness(10.0, 4.0, 10.0, 0.0);
      stackPanel.VerticalAlignment = VerticalAlignment.Center;
      StackPanel element = stackPanel;
      element.SetValue(Grid.ColumnProperty, (object) 1);
      EmjTextBlock emjTextBlock = new EmjTextBlock();
      emjTextBlock.TextWrapping = TextWrapping.Wrap;
      emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      emjTextBlock.ClipToBounds = true;
      emjTextBlock.Height = 16.0;
      this._titleText = emjTextBlock;
      this._titleText.SetResourceReference(FrameworkElement.StyleProperty, (object) "Tag02");
      element.Children.Add((UIElement) this._titleText);
      this.SetTitleText();
      TextBlock textBlock = new TextBlock();
      textBlock.Text = "+1";
      textBlock.Foreground = (Brush) ThemeUtil.GetColor("PomoRed");
      textBlock.FontSize = 24.0;
      textBlock.Margin = new Thickness(0.0, -4.0, 0.0, 0.0);
      this._add1Text = textBlock;
      element.Children.Add((UIElement) this._add1Text);
      ClockControl clockControl = new ClockControl();
      clockControl.FontSize = 24.0;
      clockControl.Margin = new Thickness(0.0, -4.0, 0.0, 0.0);
      this._timeClock = clockControl;
      element.Children.Add((UIElement) this._timeClock);
      this.SetTimeText();
      this.Children.Add((UIElement) element);
    }

    private void SetTimeText()
    {
      this._timeClock.Visibility = TickFocusManager.Status != PomoStatus.WaitingRelax ? Visibility.Visible : Visibility.Collapsed;
      this._add1Text.Visibility = TickFocusManager.Status == PomoStatus.WaitingRelax ? Visibility.Visible : Visibility.Collapsed;
      this._timeClock.SetTime(TickFocusManager.DisplaySecond);
      this._timeClock.SetResourceReference(ClockControl.ForegroundProperty, TickFocusManager.InRelax ? (object) "PomoGreen" : (object) "BaseColorOpacity90");
    }

    private void SetTitleText()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._titleText.Visibility = TickFocusManager.Status != PomoStatus.WaitingRelax ? Visibility.Visible : Visibility.Collapsed;
        if (string.IsNullOrEmpty(TickFocusManager.Config.FocusVModel.FocusId))
        {
          switch (TickFocusManager.Status)
          {
            case PomoStatus.Working:
              this._titleText.Text = Utils.GetString("Focusing");
              break;
            case PomoStatus.Relaxing:
              this._titleText.Text = Utils.GetString("Relaxing");
              break;
            case PomoStatus.WaitingWork:
              this._titleText.Text = Utils.GetString("Focus");
              break;
            case PomoStatus.Pause:
              this._titleText.Text = Utils.GetString("Paused");
              break;
          }
        }
        else
          this._titleText.Text = TickFocusManager.Config.FocusVModel.Title;
      }));
    }

    private void SetButtons()
    {
      this._dropOption.Visibility = TickFocusManager.Status == PomoStatus.Relaxing || TickFocusManager.Status == PomoStatus.Pause ? Visibility.Visible : Visibility.Collapsed;
      this._option.Visibility = TickFocusManager.Status != PomoStatus.Relaxing ? Visibility.Visible : Visibility.Collapsed;
      string str = TickFocusManager.Status == PomoStatus.WaitingRelax ? "PomoGreen" : "PrimaryColor";
      this._option.BackBorder.SetResourceReference(Panel.BackgroundProperty, (object) str);
      this._option.IconData = Utils.GetIcon(TickFocusManager.Status == PomoStatus.Working ? "IcPomoPause" : "IcPomoStart");
      this._option.SetIconColor(str);
    }

    public void OnStatusChanged()
    {
      this._icon.SetIcon();
      this.SetPomoGotIcon();
      this.SetTitleText();
      this.SetTimeText();
      this.SetButtons();
    }

    private void SetPomoGotIcon()
    {
      if (TickFocusManager.Status == PomoStatus.WaitingRelax)
      {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.DecodePixelWidth = 60;
        bitmapImage.UriSource = new Uri("pack://application:,,,/Assets/get_pomo.png");
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        this._icon.Background = (Brush) new ImageBrush()
        {
          ImageSource = (ImageSource) bitmapImage
        };
        this._icon.Width = 40.0;
        this._icon.Height = 40.0;
        this._icon.Child = (UIElement) null;
      }
      else
      {
        this._icon.Width = 30.0;
        this._icon.Height = 30.0;
        this._icon.SetFocusIcon(TickFocusManager.Config.FocusVModel.FocusId, TickFocusManager.Config.FocusVModel.Type);
      }
    }

    private void OnFocusChanged(object sender, PropertyChangedEventArgs e)
    {
      this.Dispatcher.Invoke((Action) (() => this.SetPomoGotIcon()));
    }

    private void OnTitleChanged(object sender, PropertyChangedEventArgs e) => this.SetTitleText();

    public void OnFocusTypeChanged()
    {
      this.Dispatcher.Invoke((Action) (() => this._icon.SetIcon()));
    }

    private void SetCountText()
    {
      this.Dispatcher.Invoke((Action) (() => this._timeClock.SetTime(TickFocusManager.DisplaySecond)));
    }

    private void OnDropClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      FocusTimer.Drop();
      UserActCollectUtils.AddClickEvent("timer", "action_bar", "end");
    }

    private void OnOptionClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      switch (TickFocusManager.Status)
      {
        case PomoStatus.Working:
          FocusTimer.Pause(DateTime.Now);
          UserActCollectUtils.AddClickEvent("timer", "action_bar", "pause");
          break;
        case PomoStatus.WaitingWork:
          UserActCollectUtils.AddClickEvent("timer", "action_bar", TickFocusManager.Config.FromRelax ? "again" : "start");
          FocusTimer.BeginTimer();
          UserActCollectUtils.AddClickEvent("focus", "start_from_tab", "action_bar");
          UserActCollectUtils.AddClickEvent("focus", "start_from", "tab");
          break;
        case PomoStatus.WaitingRelax:
          FocusTimer.BeginTimer();
          UserActCollectUtils.AddClickEvent("timer", "action_bar", "relax");
          break;
        case PomoStatus.Pause:
          FocusTimer.Continue(new DateTime?(DateTime.Now));
          UserActCollectUtils.AddClickEvent("timer", "action_bar", "continue");
          break;
      }
    }

    public void ReloadIcon() => this.SetPomoGotIcon();
  }
}
