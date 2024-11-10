// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TabBar.TabBarItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.TabBar
{
  public class TabBarItem : Border
  {
    public const int ItemHeight = 48;
    private Path _icon;
    private static TabBarItem _mouseDownItem;
    private bool _selected;
    private static DoubleAnimation _enterAnim = AnimationUtils.GetDoubleAnimation(new double?(0.4), 0.6, 90);
    private static DoubleAnimation _leaveAnim = AnimationUtils.GetDoubleAnimation(new double?(0.6), 0.4, 90);
    private System.Windows.Point _mouseDownPoint;
    private EscPopup _popup;
    private Grid _focusPanel;
    private PomoProgressBar _focusProgress;
    private bool _forceHighlight;
    private Path _relaxIcon;

    public event EventHandler<TabBarItemViewModel> ItemClick;

    private TabBarItemViewModel Model => this.DataContext as TabBarItemViewModel;

    public TabBarItem()
    {
      this.Height = 48.0;
      this.Width = 50.0;
      this.Cursor = Cursors.Hand;
      this.BorderThickness = new Thickness(0.0);
      this.Opacity = 0.4;
      this.SetValue(ToolTipService.BetweenShowDelayProperty, (object) 0);
      this.SetValue(ToolTipService.InitialShowDelayProperty, (object) 500);
      this.InitView();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.Background = (Brush) Brushes.Transparent;
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnItemMouseDown);
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemMouseUp);
      this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      this.MouseMove += new MouseEventHandler(this.OnMouseMove);
      this.MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      TabBarItemViewModel model = this.DataContext as TabBarItemViewModel;
      if (model == null || !model.CanOpenWindow())
        return;
      bool flag = model.IsWindowOpened();
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      types.Add(new CustomMenuItemViewModel((object) "Window", Utils.GetString(flag ? "CloseWindow" : "OpenNewWindow"), Utils.GetIcon("IcNewWindow")));
      if (this._popup == null)
      {
        EscPopup escPopup = new EscPopup();
        escPopup.PlacementTarget = (UIElement) this;
        escPopup.Placement = PlacementMode.Right;
        escPopup.HorizontalOffset = -10.0;
        escPopup.VerticalOffset = -14.0;
        escPopup.StaysOpen = false;
        this._popup = escPopup;
      }
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this._popup);
      customMenuList.Operated += (EventHandler<object>) ((o, args) =>
      {
        if (!(args as string == "Window"))
          return;
        model.OpenOrCloseWindow(Window.GetWindow((DependencyObject) this));
      });
      customMenuList.Show();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || TabBarItem._mouseDownItem != this || (this._mouseDownPoint - e.GetPosition((IInputElement) this)).Length <= 4.0)
        return;
      TabBarItem._mouseDownItem = (TabBarItem) null;
      Utils.FindParent<LeftMenuBar>((DependencyObject) this)?.TryDragModel(this.DataContext as TabBarItemViewModel);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (this._selected || this._forceHighlight)
        return;
      this.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) TabBarItem._leaveAnim);
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (this._selected || this._forceHighlight)
        return;
      this.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) TabBarItem._enterAnim);
    }

    private void OnItemMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._mouseDownPoint = e.GetPosition((IInputElement) this);
      TabBarItem._mouseDownItem = this;
    }

    private void OnItemMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (TabBarItem._mouseDownItem != this)
      {
        TabBarItem._mouseDownItem = (TabBarItem) null;
      }
      else
      {
        EventHandler<TabBarItemViewModel> itemClick = this.ItemClick;
        if (itemClick != null)
          itemClick((object) this, this.DataContext as TabBarItemViewModel);
        TabBarItem._mouseDownItem = (TabBarItem) null;
      }
    }

    private void InitView()
    {
      System.Windows.Controls.ToolTip toolTip = new System.Windows.Controls.ToolTip()
      {
        Placement = PlacementMode.Right,
        VerticalOffset = 10.0
      };
      toolTip.SetBinding(ContentControl.ContentProperty, "ToolTip");
      this.ToolTip = (object) toolTip;
      Path path = new Path();
      path.Height = 22.0;
      path.Stretch = Stretch.Uniform;
      path.VerticalAlignment = VerticalAlignment.Center;
      path.HorizontalAlignment = HorizontalAlignment.Center;
      this._icon = path;
      this._icon.SetBinding(Path.DataProperty, "Icon");
      this._icon.SetResourceReference(Shape.FillProperty, (object) "LeftBarColorOpacity100");
      this.Child = (UIElement) this._icon;
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is TabBarItemViewModel oldValue)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnPropertyChanged), "");
      if (!(e.NewValue is TabBarItemViewModel newValue))
        return;
      this.Visibility = newValue.Show ? Visibility.Visible : Visibility.Collapsed;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnPropertyChanged), "");
      Utils.FindParent<ContentPresenter>((DependencyObject) this)?.SetValue(Canvas.TopProperty, (object) ((double) newValue.SortOrder * 48.0));
      this.SetStyle(newValue.Selected);
      this.SetValue(Panel.ZIndexProperty, (object) (newValue.Show ? 10 : 0));
      if (!(newValue.Module == "Pomo"))
        return;
      this.InitPomoIcon();
    }

    private void InitPomoIcon()
    {
      PomoProgressBar pomoProgressBar = new PomoProgressBar();
      pomoProgressBar.TimingSize = 2;
      pomoProgressBar.Height = 21.0;
      pomoProgressBar.Width = 21.0;
      pomoProgressBar.Angle = 0.0;
      pomoProgressBar.StrokeThickness = 4.0;
      pomoProgressBar.IsStrokeMode = TickFocusManager.IsPomo;
      this._focusProgress = pomoProgressBar;
      this._focusProgress.SetResourceReference(PomoProgressBar.UnderColorProperty, (object) "LeftBarColorOpacity20");
      this._focusProgress.SetResourceReference(PomoProgressBar.TopColorProperty, (object) "LeftBarSelectedIconColor");
      Path path = new Path();
      path.Height = 10.0;
      path.Width = 10.0;
      path.Data = Utils.GetIcon("IcPomoStart");
      path.Stretch = Stretch.Uniform;
      path.Fill = (Brush) ThemeUtil.GetColor("PomoGreen");
      path.VerticalAlignment = VerticalAlignment.Center;
      path.HorizontalAlignment = HorizontalAlignment.Center;
      this._relaxIcon = path;
      TickFocusManager.StatusChanged += new FocusChange(this.OnFocusStatusChanged);
      TickFocusManager.TypeChanged += new FocusChange(this.OnFocusTypeChanged);
      TickFocusManager.CurrentSecondChanged += new FocusChange(this.OnFocusSecondChanged);
      Grid grid = new Grid();
      grid.Width = 22.0;
      grid.Height = 22.0;
      grid.Children.Add((UIElement) this._focusProgress);
      grid.Children.Add((UIElement) this._relaxIcon);
      this._focusPanel = grid;
    }

    private void OnFocusTypeChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._focusProgress.IsStrokeMode = TickFocusManager.IsPomo;
        this._focusProgress.Reset();
      }));
    }

    private void OnFocusSecondChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._focusProgress.Angle = TickFocusManager.Config.GetDisplayAngle(false);
        if (this._focusProgress.IsStrokeMode || TickFocusManager.Config.CurrentSeconds < 30.0)
          return;
        this._focusProgress.HideLeftMask();
      }));
    }

    private void OnFocusStatusChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._forceHighlight = TickFocusManager.Status != PomoStatus.WaitingWork || TickFocusManager.Config.FromRelax;
        this.SetStyle(this._selected);
        this._focusProgress.SetResourceReference(PomoProgressBar.TopColorProperty, TickFocusManager.InRelax ? (object) "PomoGreen" : (object) "LeftBarSelectedIconColor");
        this._relaxIcon.Visibility = TickFocusManager.Status == PomoStatus.WaitingRelax || TickFocusManager.Status == PomoStatus.WaitingWork ? Visibility.Visible : Visibility.Collapsed;
        this._relaxIcon.SetResourceReference(Shape.FillProperty, TickFocusManager.Status == PomoStatus.WaitingRelax ? (object) "PomoGreen" : (object) "PrimaryColor");
      }));
    }

    private void SetStyle(bool selected)
    {
      this._selected = selected;
      if (selected || this._forceHighlight)
      {
        this.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        this.Opacity = 1.0;
        this._icon.SetResourceReference(Shape.FillProperty, (object) "LeftBarSelectedIconColor");
      }
      else
      {
        this.Opacity = 0.4;
        this._icon.SetResourceReference(Shape.FillProperty, (object) "LeftBarColorOpacity100");
      }
      if (this._selected || this._focusPanel == null)
        this.Child = (UIElement) this._icon;
      else if (TickFocusManager.Status == PomoStatus.WaitingWork && !TickFocusManager.Config.FromRelax)
      {
        this.Child = (UIElement) this._icon;
        this._focusProgress.Reset();
      }
      else
        this.Child = (UIElement) this._focusPanel;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(sender is TabBarItemViewModel barItemViewModel))
        return;
      switch (e.PropertyName)
      {
        case "Selected":
          this.SetStyle(barItemViewModel.Selected);
          break;
        case "Dragging":
          this.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
          if (barItemViewModel.Dragging)
          {
            this.Opacity = 0.0;
            break;
          }
          this.Opacity = barItemViewModel.Selected ? 1.0 : 0.4;
          break;
        case "SortOrder":
          ContentPresenter parent = Utils.FindParent<ContentPresenter>((DependencyObject) this);
          if (!barItemViewModel.Dragging)
          {
            DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?((double) parent.GetValue(Canvas.TopProperty)), (double) barItemViewModel.SortOrder * 48.0, 150);
            DoubleAnimation doubleAnimation2 = doubleAnimation1;
            CubicEase cubicEase = new CubicEase();
            cubicEase.EasingMode = EasingMode.EaseOut;
            doubleAnimation2.EasingFunction = (IEasingFunction) cubicEase;
            parent.BeginAnimation(Canvas.TopProperty, (AnimationTimeline) doubleAnimation1);
            break;
          }
          parent.BeginAnimation(Canvas.TopProperty, (AnimationTimeline) null);
          parent.SetValue(Canvas.TopProperty, (object) ((double) barItemViewModel.SortOrder * 48.0));
          break;
        case "Show":
          double num = barItemViewModel.Selected ? 1.0 : 0.4;
          this.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(barItemViewModel.Show ? 0.0 : num), barItemViewModel.Show ? num : 0.0, 80));
          this.SetValue(Panel.ZIndexProperty, (object) (barItemViewModel.Show ? 10 : 0));
          this.Visibility = barItemViewModel.Show ? Visibility.Visible : Visibility.Collapsed;
          break;
        case "GuideText":
          if (string.IsNullOrEmpty(barItemViewModel.GuideText))
            break;
          this.ShowPopupText(barItemViewModel.GuideText);
          break;
      }
    }

    private async Task ShowPopupText(string text)
    {
      TabBarItem tabBarItem = this;
      Popup popup = new Popup()
      {
        AllowsTransparency = true,
        StaysOpen = true
      };
      StackPanel stackPanel = new StackPanel()
      {
        Orientation = Orientation.Horizontal
      };
      UIElementCollection children = stackPanel.Children;
      Path element1 = new Path();
      element1.Data = Geometry.Parse("M 0,6 6,12 6,0 0,6z");
      element1.Fill = (Brush) ThemeUtil.GetColor("ToolTipTopColor");
      element1.Width = 6.0;
      element1.Height = 12.0;
      element1.VerticalAlignment = VerticalAlignment.Center;
      children.Add((UIElement) element1);
      Border border = new Border();
      border.Background = (Brush) ThemeUtil.GetColor("ToolTipTopColor");
      border.CornerRadius = new CornerRadius(2.0);
      border.Child = (UIElement) new TextBlock()
      {
        Text = text,
        Foreground = (Brush) Brushes.White,
        FontSize = 12.0,
        Padding = new Thickness(6.0)
      };
      Border element2 = border;
      stackPanel.Children.Add((UIElement) element2);
      popup.Child = (UIElement) stackPanel;
      popup.PlacementTarget = (UIElement) tabBarItem;
      popup.Placement = PlacementMode.Right;
      popup.IsOpen = true;
      popup.HorizontalOffset = -5.0;
      popup.VerticalOffset = 8.0;
      for (int i = 0; i < 2; ++i)
      {
        await Task.Delay(2000);
        popup.IsOpen = false;
        popup.IsOpen = true;
      }
      popup.IsOpen = false;
      popup = (Popup) null;
    }
  }
}
