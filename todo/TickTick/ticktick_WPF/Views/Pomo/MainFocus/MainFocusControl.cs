// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MainFocus.MainFocusControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MainFocus
{
  public class MainFocusControl : Grid
  {
    private GroupTitle _focusSwitch;
    private StackPanel _focusTitlePanel;
    private FocusOptionButtons _buttonsPanel;
    private FocusTimeClock _timeClock;
    private FocusBreakPanel _relaxPanel;
    private EmjTextBlock _focusTitle;
    private double _timeWidth = 268.0;
    private PomoFilterControl _pomoFilter;
    private List<TimerModel> _timers;
    public bool IsShowClockPanel;
    private HoverIconButton _soundButton;
    private HoverIconButton _addButton;
    private TextBlock _title;
    private Border _foldClockButton;
    private static bool _showArchiveList;
    private GroupTitle _archiveSwitch;
    private FocusTimerListView _timerListView;
    private SmallFocusView _smallFocusView;
    private string _selectedId;

    private void SetClockPanel()
    {
      this.SetFocusTimerSwitch(TickFocusManager.Status == PomoStatus.WaitingWork);
      this._addButton.Visibility = TickFocusManager.Status != PomoStatus.WaitingWork || this._timers != null && this._timers.Count != 0 ? Visibility.Collapsed : Visibility.Visible;
      bool show = TickFocusManager.Status == PomoStatus.WaitingRelax;
      this.SetFocusTitleAndClock(!show);
      this.SetBreakPanel(show);
      this._soundButton.Visibility = this._addButton.Visibility == Visibility.Visible | show ? Visibility.Collapsed : Visibility.Visible;
      this.SetButtonContainer(true);
      this._buttonsPanel.Margin = new Thickness(0.0, show ? 0.0 : -14.0, 0.0, 0.0);
      this._buttonsPanel.SetButtons();
      this._timeClock?.OnStatusChanged();
    }

    private void SetFocusTimerSwitch(bool show)
    {
      if (show && this._focusSwitch == null)
      {
        GroupTitle groupTitle = new GroupTitle();
        groupTitle.Titles = "PomoTimer2|Timing";
        groupTitle.VerticalAlignment = VerticalAlignment.Top;
        groupTitle.HorizontalAlignment = HorizontalAlignment.Center;
        groupTitle.Margin = new Thickness(0.0, 30.0, 0.0, 0.0);
        groupTitle.BorderHeight = 30.0;
        this._focusSwitch = groupTitle;
        this._focusSwitch.SetSelectedIndex(!TickFocusManager.IsPomo ? 1 : 0);
        this.Children.Add((UIElement) this._focusSwitch);
        this._focusSwitch.SelectedTitleChanged += new EventHandler<GroupTitleViewModel>(this.OnTypeSelected);
      }
      else
      {
        if (show || this._focusSwitch == null)
          return;
        this._focusSwitch.SelectedTitleChanged -= new EventHandler<GroupTitleViewModel>(this.OnTypeSelected);
        this.Children.Remove((UIElement) this._focusSwitch);
        this._focusSwitch = (GroupTitle) null;
      }
    }

    private void InitFocusTitle()
    {
      StackPanel stackPanel = new StackPanel();
      stackPanel.Orientation = Orientation.Horizontal;
      stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
      stackPanel.VerticalAlignment = VerticalAlignment.Bottom;
      stackPanel.Background = (Brush) Brushes.Transparent;
      stackPanel.Margin = new Thickness(4.0, 0.0, 0.0, 40.0);
      stackPanel.Cursor = Cursors.Hand;
      this._focusTitlePanel = stackPanel;
      EmjTextBlock emjTextBlock = new EmjTextBlock();
      emjTextBlock.FontSize = 14.0;
      emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      emjTextBlock.TextWrapping = TextWrapping.Wrap;
      emjTextBlock.MaxHeight = 18.0;
      emjTextBlock.ClipToBounds = true;
      this._focusTitle = emjTextBlock;
      this._focusTitle.MaxWidth = Math.Max(Math.Min(Math.Min(360.0, Math.Max(240.0, this.ActualWidth / 2.1)), Math.Min(360.0, Math.Max(240.0, this.ActualHeight - 460.0))) - 20.0, 200.0);
      this._focusTitle.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this._focusTitlePanel.Children.Add((UIElement) this._focusTitle);
      this._focusTitle.Text = TickFocusManager.NoFocus ? Utils.GetString("Focus") : TickFocusManager.Config.FocusVModel.Title;
      this._focusTitle.Opacity = TickFocusManager.NoFocus ? 0.6 : 1.0;
      Path path = new Path();
      path.Data = Utils.GetIcon("ArrowThinLine");
      path.Margin = new Thickness(4.0, 0.0, 0.0, 0.0);
      path.Height = 14.0;
      path.Width = 14.0;
      path.Stretch = Stretch.Fill;
      path.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      Path element = path;
      element.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity40");
      element.RenderTransform = (Transform) new RotateTransform()
      {
        Angle = 270.0
      };
      this._focusTitlePanel.Children.Add((UIElement) element);
      this._focusTitlePanel.SetValue(Grid.RowProperty, (object) 1);
      this.Children.Add((UIElement) this._focusTitlePanel);
      this._focusTitlePanel.Margin = this._timeClock.Margin;
      this._focusTitlePanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTitleClick);
    }

    private void SetFocusTitle(bool show)
    {
      if (this._focusTitlePanel == null & show)
      {
        this.InitFocusTitle();
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusTitleChanged), "Title");
      }
      else
      {
        if (this._focusTitlePanel == null || show)
          return;
        this._focusTitlePanel.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnTitleClick);
        this.Children.Remove((UIElement) this._focusTitlePanel);
        this._focusTitlePanel.Children.Clear();
        this._focusTitle = (EmjTextBlock) null;
        this._focusTitlePanel = (StackPanel) null;
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusTitleChanged), "Title");
      }
    }

    private void SetTimePanel(bool show)
    {
      if (this._timeClock == null & show)
      {
        this._timeClock = new FocusTimeClock();
        this._timeClock.SetSize(this._timeWidth);
        this._timeClock.Margin = new Thickness(0.0, 0.0, 0.0, Math.Min(100.0, Math.Max(36.0, (this.ActualHeight - this._timeClock.Height - 380.0) / 2.0)));
        this._timeClock.SetValue(Grid.RowProperty, (object) 2);
        this.Children.Add((UIElement) this._timeClock);
      }
      else
      {
        if (this._timeClock == null || show)
          return;
        this.Children.Remove((UIElement) this._timeClock);
        this._timeClock = (FocusTimeClock) null;
      }
    }

    private void SetFocusTitleAndClock(bool show)
    {
      this.SetTimePanel(show);
      this.SetFocusTitle(show);
    }

    private void SetBreakPanel(bool show)
    {
      if (this._relaxPanel == null & show)
      {
        FocusBreakPanel focusBreakPanel = new FocusBreakPanel();
        focusBreakPanel.Margin = new Thickness(0.0, -100.0, 0.0, 0.0);
        this._relaxPanel = focusBreakPanel;
        this._relaxPanel.SetValue(Grid.RowProperty, (object) 2);
        this.Children.Add((UIElement) this._relaxPanel);
        this._relaxPanel.SetRelaxText();
      }
      else
      {
        if (this._relaxPanel == null || show)
          return;
        this.Children.Remove((UIElement) this._relaxPanel);
        this._relaxPanel = (FocusBreakPanel) null;
      }
    }

    private void SetButtonContainer(bool show)
    {
      if (this._buttonsPanel == null & show)
      {
        FocusOptionButtons focusOptionButtons = new FocusOptionButtons();
        focusOptionButtons.HorizontalAlignment = HorizontalAlignment.Center;
        focusOptionButtons.VerticalAlignment = VerticalAlignment.Top;
        focusOptionButtons.Margin = new Thickness(0.0, -14.0, 0.0, 0.0);
        this._buttonsPanel = focusOptionButtons;
        this._buttonsPanel.SetValue(Grid.RowProperty, (object) 3);
        this._buttonsPanel.SetValue(Grid.RowSpanProperty, (object) 2);
        double width = 140.0 + (this._timeWidth - 300.0) / 2.0;
        this._buttonsPanel.SetButtonWidth(width, Math.Min(48.0, 48.0 + (width - 140.0) / 2.5));
        this.Children.Add((UIElement) this._buttonsPanel);
        this._buttonsPanel.SetButtons();
      }
      else
      {
        if (this._buttonsPanel == null || show)
          return;
        this.Children.Remove((UIElement) this._buttonsPanel);
        this._buttonsPanel = (FocusOptionButtons) null;
      }
    }

    private void OnTypeSelected(object sender, GroupTitleViewModel e)
    {
      TickFocusManager.SetFocusType(e.Index);
      UserActCollectUtils.AddClickEvent("focus", "focus_tab", e.Index == 0 ? "tab_pomo" : "tab_stopwatch");
    }

    private void OnTitleClick(object sender, MouseButtonEventArgs e)
    {
      if (this._pomoFilter == null)
      {
        EscPopup escPopup = new EscPopup();
        escPopup.StaysOpen = false;
        escPopup.PlacementTarget = sender as UIElement;
        escPopup.Placement = PlacementMode.Center;
        escPopup.HorizontalOffset = -3.0;
        escPopup.VerticalOffset = 208.0;
        PomoFilterControl pomoFilterControl = new PomoFilterControl((Popup) escPopup);
        pomoFilterControl.DataContext = (object) TickFocusManager.Config.FocusVModel;
        this._pomoFilter = pomoFilterControl;
      }
      Popup parentPopup = this._pomoFilter.GetParentPopup();
      parentPopup.PlacementTarget = sender as UIElement;
      parentPopup.Placement = PlacementMode.Center;
      this._pomoFilter.Open();
    }

    private void HideClockPanel()
    {
      this.SetFocusTimerSwitch(false);
      this.SetFocusTitleAndClock(false);
      this.SetBreakPanel(false);
      this.SetButtonContainer(false);
    }

    public MainFocusControl(List<TimerModel> timers, bool isShowClockPanel)
    {
      this.IsShowClockPanel = isShowClockPanel;
      this._timers = timers;
      this.InitChildren();
      this.InitEvent();
      this.OnFocusSecondChanged();
      this.UseLayoutRounding = true;
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    private void InitChildren()
    {
      this.InitMainContainer();
      this.InitTitle();
      this.InitTopOption();
      this.SetPanel();
    }

    private void SetPanel()
    {
      List<TimerModel> timers1 = this._timers;
      // ISSUE: explicit non-virtual call
      if ((timers1 != null ? (__nonvirtual (timers1.Count) > 0 ? 1 : 0) : 0) != 0 && !this.IsShowClockPanel)
      {
        this.HideClockPanel();
        this._addButton.Visibility = Visibility.Visible;
        this._soundButton.Visibility = Visibility.Collapsed;
        this._title.Visibility = Visibility.Visible;
        this._foldClockButton.Visibility = Visibility.Collapsed;
        this.SetTimerPanels();
      }
      else
      {
        TextBlock title = this._title;
        List<TimerModel> timers2 = this._timers;
        // ISSUE: explicit non-virtual call
        int num1 = (timers2 != null ? (__nonvirtual (timers2.Count) > 0 ? 1 : 0) : 0) != 0 ? 2 : 0;
        title.Visibility = (Visibility) num1;
        Border foldClockButton = this._foldClockButton;
        List<TimerModel> timers3 = this._timers;
        // ISSUE: explicit non-virtual call
        int num2 = (timers3 != null ? (__nonvirtual (timers3.Count) > 0 ? 1 : 0) : 0) != 0 ? 0 : 2;
        foldClockButton.Visibility = (Visibility) num2;
        this.HideTimerPanel();
        this.SetClockPanel();
      }
    }

    private void InitMainContainer()
    {
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(1.0, GridUnitType.Star),
        MinHeight = 60.0
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(1.0, GridUnitType.Star)
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(40.0)
      });
    }

    private void InitTitle()
    {
      TextBlock textBlock = new TextBlock();
      textBlock.Text = Utils.GetString("PomoFocus");
      textBlock.Margin = new Thickness(20.0, 32.0, 0.0, 0.0);
      textBlock.FontWeight = FontWeights.Bold;
      textBlock.FontSize = 20.0;
      this._title = textBlock;
      this._title.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.Children.Add((UIElement) this._title);
      Border border = new Border();
      border.Cursor = Cursors.Hand;
      border.Width = 24.0;
      border.Height = 24.0;
      border.Margin = new Thickness(20.0, 32.0, 0.0, 0.0);
      border.Visibility = Visibility.Collapsed;
      border.VerticalAlignment = VerticalAlignment.Top;
      border.HorizontalAlignment = HorizontalAlignment.Left;
      this._foldClockButton = border;
      this._foldClockButton.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_60");
      this._foldClockButton.Child = (UIElement) UiUtils.GetArrow(18.0, 0.0, "BaseColorOpacity100");
      this._foldClockButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.FoldClockPanel);
      this.Children.Add((UIElement) this._foldClockButton);
    }

    private void InitTopOption()
    {
      HoverIconButton hoverIconButton1 = new HoverIconButton();
      hoverIconButton1.HorizontalAlignment = HorizontalAlignment.Right;
      hoverIconButton1.VerticalAlignment = VerticalAlignment.Top;
      hoverIconButton1.Margin = new Thickness(0.0, 30.0, 14.0, 0.0);
      HoverIconButton element = hoverIconButton1;
      element.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "MoreDrawingImage");
      element.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
      this.Children.Add((UIElement) element);
      HoverIconButton hoverIconButton2 = new HoverIconButton();
      hoverIconButton2.IconData = TickFocusManager.GetSoundIcon();
      hoverIconButton2.IsImage = false;
      hoverIconButton2.HorizontalAlignment = HorizontalAlignment.Right;
      hoverIconButton2.VerticalAlignment = VerticalAlignment.Top;
      hoverIconButton2.Margin = new Thickness(0.0, 30.0, 44.0, 0.0);
      hoverIconButton2.Visibility = Visibility.Collapsed;
      this._soundButton = hoverIconButton2;
      this._soundButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSoundClick);
      this.Children.Add((UIElement) this._soundButton);
      HoverIconButton hoverIconButton3 = new HoverIconButton();
      hoverIconButton3.IconData = Utils.GetIcon("IcAdd");
      hoverIconButton3.IsImage = false;
      hoverIconButton3.HorizontalAlignment = HorizontalAlignment.Right;
      hoverIconButton3.VerticalAlignment = VerticalAlignment.Top;
      hoverIconButton3.Margin = new Thickness(0.0, 30.0, 44.0, 0.0);
      this._addButton = hoverIconButton3;
      this._addButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAddTimerClick);
      this.Children.Add((UIElement) this._addButton);
    }

    private void InitEvent()
    {
      TickFocusManager.StatusChanged += new FocusChange(this.OnFocusStatusChanged);
      TickFocusManager.TypeChanged += new FocusChange(this.OnFocusTypeChanged);
      TickFocusManager.CurrentSecondChanged += new FocusChange(this.OnFocusSecondChanged);
      PomoNotifier.TimerChanged += new EventHandler(this.OnTimerChanged);
      PomoNotifier.Changed += new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
      PomoNotifier.ServiceChanged += new EventHandler(this.OnPomoChanged);
      PomoNotifier.LinkChanged += new EventHandler<PomoLinkArgs>(this.OnPomoLinkChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnSoundChanged), "PomoSound");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusChanged), "NoTask");
    }

    private void OnPomoLinkChanged(object sender, PomoLinkArgs e)
    {
      if (string.IsNullOrEmpty(e.NewTimerId) && string.IsNullOrEmpty(e.PreviewTimerId))
        return;
      this.Dispatcher.Invoke(new Action(this.SetPanel));
    }

    private void OnPomoChanged(object sender, object e)
    {
      this.Dispatcher.Invoke(new Action(this.SetPanel));
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this._timeWidth = Math.Min(Math.Min(360.0, Math.Max(240.0, e.NewSize.Width / 2.1)), Math.Min(360.0, Math.Max(240.0, e.NewSize.Height - 460.0)));
      double width = 140.0 + (this._timeWidth - 300.0) / 2.0;
      this._buttonsPanel?.SetButtonWidth(width, Math.Min(48.0, 48.0 + (width - 140.0) / 2.5));
      if (this._timeClock == null)
        return;
      this._timeClock.SetSize(this._timeWidth);
      double bottom = Math.Min(100.0, Math.Max(36.0, (e.NewSize.Height - this._timeClock.Height - 380.0) / 2.0));
      this._timeClock.Margin = new Thickness(0.0, 0.0, 0.0, bottom);
      if (this._focusTitlePanel != null)
        this._focusTitlePanel.Margin = new Thickness(0.0, 0.0, 0.0, bottom);
      if (this._focusTitle == null)
        return;
      this._focusTitle.MaxWidth = this._timeWidth - 20.0;
    }

    private void OnFocusTypeChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._timeClock?.SetProgressMode();
        this._focusSwitch?.SetSelectedIndex(!TickFocusManager.IsPomo ? 1 : 0);
        this._timeClock?.SetTime();
        this._smallFocusView?.OnFocusTypeChanged();
      }));
    }

    private void OnFocusSecondChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._timeClock?.SetTime();
        if (TickFocusManager.Status != PomoStatus.WaitingRelax)
          return;
        this._relaxPanel?.SetRelaxText();
      }));
    }

    private void OnFocusStatusChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        List<TimerModel> timers = this._timers;
        // ISSUE: explicit non-virtual call
        if ((timers != null ? (__nonvirtual (timers.Count) > 0 ? 1 : 0) : 0) != 0 && !this.IsShowClockPanel)
        {
          this._smallFocusView?.OnStatusChanged();
          this._timerListView?.SetItemFocusing();
        }
        else
          this.SetClockPanel();
      }));
    }

    private void OnFocusTitleChanged(object sender, PropertyChangedEventArgs e)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (this._focusTitle == null)
          return;
        this._focusTitle.Text = TickFocusManager.NoFocus ? Utils.GetString("Focus") : TickFocusManager.Config.FocusVModel.Title;
        this._focusTitle.Opacity = TickFocusManager.NoFocus ? 0.6 : 1.0;
      }));
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      EscPopup escPopup1 = new EscPopup();
      escPopup1.PlacementTarget = sender as UIElement;
      escPopup1.StaysOpen = false;
      escPopup1.Placement = PlacementMode.Bottom;
      EscPopup escPopup2 = escPopup1;
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "immerse", Utils.GetString("ImmerseMode"), Utils.GetImageSource("FullScreenDrawingImage")),
        new CustomMenuItemViewModel((object) "minimode", Utils.GetString("MiniMode"), Utils.GetImageSource("MiniWindowDrawingImage")),
        new CustomMenuItemViewModel((object) "statistics", Utils.GetString("Statistics"), Utils.GetImageSource("StatisticsDrawingImage")),
        new CustomMenuItemViewModel((object) "settings", Utils.GetString("FocusSetting"), Utils.GetImageSource("FocusSettingsDrawingImage"))
      }, (Popup) escPopup2);
      customMenuList.Operated += (EventHandler<object>) ((o, val) =>
      {
        if (!(val is string str2))
          return;
        switch (str2)
        {
          case "immerse":
            UserActCollectUtils.AddClickEvent("focus", "om", "full_screen_mode");
            TickFocusManager.ShowImmerseWindow(Window.GetWindow((DependencyObject) this));
            break;
          case "minimode":
            UserActCollectUtils.AddClickEvent("focus", "om", "mini_mode");
            TickFocusManager.HideOrShowFocusWidget(true);
            break;
          case "settings":
            SettingsHelper.PullRemoteSettings();
            UserActCollectUtils.AddClickEvent("focus", "om", "focus-settings");
            PomoSettings.ShowInstance(Window.GetWindow((DependencyObject) this));
            break;
          case "statistics":
            Utils.TryProcessStartUrlWithToken("/webapp/#statistics/pomo?enablePomo=true");
            UserActCollectUtils.AddClickEvent("focus", "om", "statistics");
            break;
        }
      });
      customMenuList.Show();
    }

    private void OnAddTimerClick(object sender, MouseButtonEventArgs e)
    {
      AddTimerWindow.ShowAddTimerWindow(Window.GetWindow((DependencyObject) this));
      UserActCollectUtils.AddClickEvent("focus", "focus_tab", "add_timer");
    }

    private void OnSoundClick(object sender, MouseButtonEventArgs e)
    {
      EscPopup escPopup1 = new EscPopup();
      escPopup1.StaysOpen = false;
      escPopup1.PlacementTarget = sender as UIElement;
      escPopup1.Placement = PlacementMode.Bottom;
      EscPopup escPopup2 = escPopup1;
      SelectSoundControl selectSoundControl = new SelectSoundControl()
      {
        Popup = (Popup) escPopup2
      };
      escPopup2.Closed += (EventHandler) ((o, arg) => PomoSoundPlayer.TryStopSound());
      escPopup2.Child = (UIElement) selectSoundControl;
      escPopup2.IsOpen = true;
      UserActCollectUtils.AddClickEvent("focus", TickFocusManager.GetActCType(), "white_noise");
    }

    private void OnSoundChanged(object sender, PropertyChangedEventArgs e)
    {
      this._soundButton.IconData = TickFocusManager.GetSoundIcon();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      TickFocusManager.StatusChanged -= new FocusChange(this.OnFocusStatusChanged);
      TickFocusManager.TypeChanged -= new FocusChange(this.OnFocusTypeChanged);
      TickFocusManager.CurrentSecondChanged -= new FocusChange(this.OnFocusSecondChanged);
      PomoNotifier.Changed -= new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
      PomoNotifier.ServiceChanged -= new EventHandler(this.OnPomoChanged);
      PomoNotifier.TimerChanged -= new EventHandler(this.OnTimerChanged);
      PomoNotifier.LinkChanged -= new EventHandler<PomoLinkArgs>(this.OnPomoLinkChanged);
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnSoundChanged), "PomoSound");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusChanged), "NoTask");
      this.Children.Clear();
      this._pomoFilter = (PomoFilterControl) null;
    }

    public void ShowClockPanel()
    {
      this.IsShowClockPanel = true;
      this.SetPanel();
      Utils.FindParent<FocusView>((DependencyObject) this)?.OnTimerSelect((string) null);
    }

    private void FoldClockPanel(object sender, MouseButtonEventArgs e)
    {
      this.IsShowClockPanel = false;
      this.SetPanel();
      Utils.FindParent<FocusView>((DependencyObject) this)?.OnTimerSelect(this._selectedId);
    }

    public void TryFoldClockPanel()
    {
      if (!this.IsShowClockPanel)
        return;
      this._pomoFilter?.ResetSelectTitle();
      this.IsShowClockPanel = false;
      this.SetPanel();
      Utils.FindParent<FocusView>((DependencyObject) this)?.OnTimerSelect(this._selectedId);
    }

    public bool ClockPanelShow()
    {
      return this._timers == null || this._timers.Count == 0 || this.IsShowClockPanel;
    }

    public bool ExistTimer()
    {
      List<TimerModel> timers = this._timers;
      // ISSUE: explicit non-virtual call
      return timers != null && __nonvirtual (timers.Count) > 0;
    }

    private async void OnTimerChanged(object sender, EventArgs e)
    {
      MainFocusControl mainFocusControl = this;
      List<TimerModel> displayTimersAsync = await TimerDao.GetDisplayTimersAsync();
      mainFocusControl._timers = displayTimersAsync;
      // ISSUE: reference to a compiler-generated method
      mainFocusControl.Dispatcher.Invoke(new Action(mainFocusControl.\u003COnTimerChanged\u003Eb__49_0));
    }

    public bool IsListMouseOver
    {
      get
      {
        FocusTimerListView timerListView = this._timerListView;
        return timerListView != null && __nonvirtual (timerListView.IsMouseOver);
      }
    }

    private void SetTimerPanels()
    {
      List<TimerModel> list1 = this._timers.Where<TimerModel>((Func<TimerModel, bool>) (t => t.Status == 0)).ToList<TimerModel>();
      List<TimerModel> list2 = this._timers.Where<TimerModel>((Func<TimerModel, bool>) (t => t.Status == 1)).ToList<TimerModel>();
      this.SetArchiveSwitch(true);
      this.SetTimerList(true);
      this.SetTimerItems(MainFocusControl._showArchiveList ? list2 : list1);
      this.SetSmallFocus(true);
    }

    private void SetTimerItems(List<TimerModel> timers)
    {
      if (!string.IsNullOrEmpty(this._selectedId) && (timers == null || timers.All<TimerModel>((Func<TimerModel, bool>) (t => t.Id != this._selectedId))))
        this.OnTimerSelect((string) null);
      this._timerListView?.SetItems(timers, MainFocusControl._showArchiveList, this._selectedId);
    }

    private void SetTimerList(bool show)
    {
      if (show && this._timerListView == null)
      {
        FocusTimerListView focusTimerListView = new FocusTimerListView();
        focusTimerListView.VerticalAlignment = VerticalAlignment.Stretch;
        focusTimerListView.Margin = new Thickness(0.0, 76.0, 0.0, 71.0);
        this._timerListView = focusTimerListView;
        this._timerListView.SetValue(Grid.RowProperty, (object) 0);
        this._timerListView.SetValue(Grid.RowSpanProperty, (object) 6);
        this.Children.Add((UIElement) this._timerListView);
      }
      else
      {
        if (show || this._timerListView == null)
          return;
        this.Children.Remove((UIElement) this._timerListView);
        this._timerListView = (FocusTimerListView) null;
      }
    }

    private void SetArchiveSwitch(bool show)
    {
      if (show && this._archiveSwitch == null)
      {
        if (this._archiveSwitch == null)
        {
          GroupTitle groupTitle = new GroupTitle();
          groupTitle.Titles = "Keeping|Archived";
          groupTitle.VerticalAlignment = VerticalAlignment.Top;
          groupTitle.HorizontalAlignment = HorizontalAlignment.Center;
          groupTitle.Margin = new Thickness(0.0, 30.0, 0.0, 0.0);
          groupTitle.BorderHeight = 30.0;
          this._archiveSwitch = groupTitle;
          this.Children.Add((UIElement) this._archiveSwitch);
          this._archiveSwitch.SelectedTitleChanged += new EventHandler<GroupTitleViewModel>(this.OnArchiveTitleSelected);
        }
        this._archiveSwitch.SetSelectedIndex(MainFocusControl._showArchiveList ? 1 : 0);
      }
      else
      {
        if (show || this._archiveSwitch == null)
          return;
        this._archiveSwitch.SelectedTitleChanged -= new EventHandler<GroupTitleViewModel>(this.OnArchiveTitleSelected);
        this.Children.Remove((UIElement) this._archiveSwitch);
        this._archiveSwitch = (GroupTitle) null;
      }
    }

    private void OnArchiveTitleSelected(object sender, GroupTitleViewModel e)
    {
      MainFocusControl._showArchiveList = e.Index == 1;
      List<TimerModel> list = this._timers.Where<TimerModel>((Func<TimerModel, bool>) (t => t.Status == e.Index)).ToList<TimerModel>();
      this.OnTimerSelect((string) null);
      this._timerListView?.SetItems(list, MainFocusControl._showArchiveList, this._selectedId);
    }

    private void SetSmallFocus(bool show)
    {
      if (show)
      {
        if (this._smallFocusView == null)
        {
          SmallFocusView smallFocusView = new SmallFocusView();
          smallFocusView.VerticalAlignment = VerticalAlignment.Bottom;
          smallFocusView.HorizontalAlignment = HorizontalAlignment.Stretch;
          smallFocusView.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
          this._smallFocusView = smallFocusView;
          this._smallFocusView.SetValue(Grid.RowProperty, (object) 0);
          this._smallFocusView.SetValue(Grid.RowSpanProperty, (object) 6);
          this.Children.Add((UIElement) this._smallFocusView);
          this._smallFocusView.MouseLeftButtonUp += new MouseButtonEventHandler(this.SmallFocusClick);
        }
        this._smallFocusView.ReloadIcon();
      }
      else
      {
        if (show || this._smallFocusView == null)
          return;
        this.Children.Remove((UIElement) this._smallFocusView);
        this._smallFocusView = (SmallFocusView) null;
      }
    }

    private void SmallFocusClick(object sender, MouseButtonEventArgs e)
    {
      this.ShowClockPanel();
      UserActCollectUtils.AddClickEvent("timer", "action_bar", "expand");
    }

    private void OnFocusChanged(object sender, PropertyChangedEventArgs e)
    {
      this._timerListView?.SetItemFocusing();
    }

    private void HideTimerPanel()
    {
      this.SetArchiveSwitch(false);
      this.SetTimerList(false);
      this.SetSmallFocus(false);
    }

    public void OnTimerSelect(string timerId)
    {
      this._selectedId = timerId;
      Utils.FindParent<FocusView>((DependencyObject) this)?.OnTimerSelect(timerId);
    }

    public void ClearSelect()
    {
      this._selectedId = (string) null;
      this._timerListView?.ClearSelect();
    }
  }
}
