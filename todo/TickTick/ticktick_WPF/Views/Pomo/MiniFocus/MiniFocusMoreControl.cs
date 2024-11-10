// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MiniFocus.MiniFocusMoreControl
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
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MiniFocus
{
  public class MiniFocusMoreControl : UserControl, IComponentConnector
  {
    private System.Windows.Point _position;
    private bool _focusPopupShow;
    private bool _displayPopupShow;
    private Popup _popup;
    private Window _window;
    private PomoFilterControl _focusTaskFilter;
    private FocusWindowType _type;
    internal ContentControl Container;
    internal MiniFocusStatisticsView StatisticsView;
    internal ContentControl FocusOnItem;
    internal TextBlock BindText;
    internal ContentControl FocusPopupTarget;
    internal ContentControl ModifyFocusItem;
    internal EscPopup ModifyFocusPopup;
    internal ContentControl SetDurationBtn;
    internal ContentControl SwitchTiming;
    internal ContentControl SwitchPomo;
    internal Image PinImage;
    internal TextBlock PinText;
    private bool _contentLoaded;

    public MiniFocusMoreControl(Popup popup, Window window, FocusWindowType type)
    {
      this._type = type;
      this._popup = popup;
      this._window = window;
      this.InitializeComponent();
      if (type == FocusWindowType.Normal)
      {
        this.Container.Margin = new Thickness(0.0, 4.0, 0.0, 4.0);
        this.StatisticsView.Visibility = Visibility.Collapsed;
      }
      if (!TickFocusManager.IsPomo)
        this.ModifyFocusItem.Visibility = Visibility.Collapsed;
      this.PinImage.SetResourceReference(Image.SourceProperty, LocalSettings.Settings.PomoTopMost ? (object) "UnpinnedDrawingImage" : (object) "PinnedDrawingImage");
      this.PinText.Text = Utils.GetString(LocalSettings.Settings.PomoTopMost ? "Unpin" : "Pin");
      if (TickFocusManager.Working)
      {
        this.SwitchPomo.IsEnabled = false;
        this.SwitchPomo.Opacity = 0.6;
        this.SwitchTiming.IsEnabled = false;
        this.SwitchTiming.Opacity = 0.6;
      }
      this.SetDurationBtn.IsEnabled = TickFocusManager.Status == PomoStatus.WaitingWork;
      this.SetDurationBtn.Opacity = TickFocusManager.Status == PomoStatus.WaitingWork ? 1.0 : 0.6;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      this.BindText.Text = TickFocusManager.Config.FocusVModel.Title;
    }

    private void ShowPopup(object sender, MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      if (Math.Abs(this._position.Y - position.Y) <= 4.0 && Math.Abs(this._position.X - position.X) <= 4.0)
        return;
      this._position = position;
      this.TryShowModifyPomoPopup();
    }

    private void TryShowModifyPomoPopup()
    {
      if (!this.ModifyFocusItem.IsMouseOver && !this.ModifyFocusPopup.IsMouseOver)
      {
        this.TryHideFocusPopup();
      }
      else
      {
        if (this._focusPopupShow || this.ModifyFocusPopup.IsOpen)
          return;
        this._focusPopupShow = true;
        this.DelayShowFocusPopup();
      }
    }

    private async void DelayShowFocusPopup()
    {
      await Task.Delay(150);
      if (this.ModifyFocusPopup.IsOpen)
      {
        this._focusPopupShow = false;
      }
      else
      {
        if (!this._focusPopupShow)
          return;
        this.ModifyFocusPopup.IsOpen = true;
      }
    }

    private async Task TryHideFocusPopup()
    {
      this._focusPopupShow = false;
      if (!this.ModifyFocusPopup.IsOpen)
        return;
      await Task.Delay(100);
      if (!this.ModifyFocusItem.IsMouseOver && !this.ModifyFocusPopup.IsMouseOver)
        this.ModifyFocusPopup.IsOpen = false;
      else
        this._focusPopupShow = this.ModifyFocusPopup.IsOpen;
    }

    private void OnSoundClick(object sender, MouseButtonEventArgs e)
    {
      EscPopup escPopup1 = new EscPopup();
      escPopup1.PlacementTarget = (UIElement) sender;
      escPopup1.Placement = PlacementMode.Bottom;
      escPopup1.StaysOpen = false;
      escPopup1.HorizontalOffset = -20.0;
      EscPopup escPopup2 = escPopup1;
      SelectSoundControl selectSoundControl = new SelectSoundControl()
      {
        Popup = (Popup) escPopup2
      };
      selectSoundControl.ItemSelect += (EventHandler) ((o, args) => this.AddClickEvent("white_noise"));
      escPopup2.Child = (UIElement) selectSoundControl;
      escPopup2.Closed += (EventHandler) ((o, args) => PomoSoundPlayer.TryStopSound());
      escPopup2.IsOpen = true;
    }

    private void OnStatisticsClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
    }

    private void OnPinClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      LocalSettings.Settings.PomoTopMost = !LocalSettings.Settings.PomoTopMost;
      this.AddClickEvent("pin");
    }

    private void OnSettingsClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      PomoSettings.ShowInstance(this._window, true, true);
      this.AddClickEvent("mini_style");
    }

    private void ImmerseClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      TickFocusManager.ShowImmerseWindow(this._window);
      this.AddClickEvent("full_screen_mode");
    }

    private void OnFocusOnClick(object sender, MouseButtonEventArgs e)
    {
      if (this._focusTaskFilter == null)
      {
        EscPopup escPopup1 = new EscPopup();
        escPopup1.PlacementTarget = (UIElement) sender;
        escPopup1.Placement = PlacementMode.Bottom;
        escPopup1.StaysOpen = false;
        escPopup1.HorizontalOffset = -20.0;
        EscPopup escPopup2 = escPopup1;
        escPopup2.Closed += (EventHandler) ((o, a) => this.BindText.Text = TickFocusManager.Config.FocusVModel.Title);
        PomoFilterControl pomoFilterControl = new PomoFilterControl((Popup) escPopup2, showDetail: true);
        pomoFilterControl.DataContext = (object) TickFocusManager.Config.FocusVModel;
        this._focusTaskFilter = pomoFilterControl;
        this._focusTaskFilter.SetTheme(LocalSettings.Settings.PomoWindowTheme);
        this._focusTaskFilter.ItemSelected += (EventHandler<DisplayItemModel>) ((o, model) => this.AddClickEvent("focus_on"));
      }
      this._focusTaskFilter.Show();
    }

    private async void OnSetDurationClick(object sender, MouseButtonEventArgs e)
    {
      this.ModifyFocusPopup.IsOpen = false;
      this._popup.IsOpen = false;
      await Task.Delay(150);
      if (TickFocusManager.Working)
        return;
      EscPopup escPopup = new EscPopup();
      escPopup.PlacementTarget = this._popup.PlacementTarget;
      escPopup.Placement = this._popup.Placement;
      escPopup.StaysOpen = false;
      escPopup.HorizontalOffset = this._popup.HorizontalOffset;
      escPopup.VerticalOffset = this._popup.VerticalOffset;
      escPopup.Child = (UIElement) new PomoModitySpanConfig((Popup) escPopup, true);
      escPopup.IsOpen = true;
    }

    private async void OnSwitchTimingClick(object sender, MouseButtonEventArgs e)
    {
      TickFocusManager.SetFocusType(1);
      this.ModifyFocusPopup.IsOpen = false;
      this._popup.IsOpen = false;
      this.AddClickEvent("switch_stopwatch");
    }

    private void OnSwitchPomoClick(object sender, MouseButtonEventArgs e)
    {
      TickFocusManager.SetFocusType(0);
      this._popup.IsOpen = false;
      this.AddClickEvent("switch_pomo");
    }

    private void OnOpenMainWindowClick(object sender, MouseButtonEventArgs e)
    {
      App.Window.SwitchModule(DisplayModule.Pomo);
      App.Window.TryShowMainWindow();
      this._popup.IsOpen = false;
      this.AddClickEvent("open_main_window");
    }

    private void OnCloseClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      this._window?.Close();
      this.AddClickEvent("close");
    }

    private void AddClickEvent(string label)
    {
      UserActCollectUtils.AddClickEvent("focus_mini", "om", label);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/minifocus/minifocusmorecontrol.xaml", UriKind.Relative));
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
          this.Container = (ContentControl) target;
          break;
        case 2:
          ((UIElement) target).MouseMove += new MouseEventHandler(this.ShowPopup);
          break;
        case 3:
          this.StatisticsView = (MiniFocusStatisticsView) target;
          break;
        case 4:
          this.FocusOnItem = (ContentControl) target;
          this.FocusOnItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFocusOnClick);
          break;
        case 5:
          this.BindText = (TextBlock) target;
          break;
        case 6:
          this.FocusPopupTarget = (ContentControl) target;
          break;
        case 7:
          this.ModifyFocusItem = (ContentControl) target;
          break;
        case 8:
          this.ModifyFocusPopup = (EscPopup) target;
          break;
        case 9:
          this.SetDurationBtn = (ContentControl) target;
          this.SetDurationBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSetDurationClick);
          break;
        case 10:
          this.SwitchTiming = (ContentControl) target;
          this.SwitchTiming.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSwitchTimingClick);
          break;
        case 11:
          this.SwitchPomo = (ContentControl) target;
          this.SwitchPomo.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSwitchPomoClick);
          break;
        case 12:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSettingsClick);
          break;
        case 13:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnPinClick);
          break;
        case 14:
          this.PinImage = (Image) target;
          break;
        case 15:
          this.PinText = (TextBlock) target;
          break;
        case 16:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSoundClick);
          break;
        case 17:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ImmerseClick);
          break;
        case 18:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenMainWindowClick);
          break;
        case 19:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
