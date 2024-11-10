// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.Pomo.FocusDetail;
using ticktick_WPF.Views.Pomo.MainFocus;
using TickTickDao;
using TickTickModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusView : Grid
  {
    private ColumnDefinition _column1;
    private ColumnDefinition _column2;
    private MainFocusControl _focusControl;
    private FocusDetailControl _focusDetail;
    private bool _animationPlaying;
    private bool _folded;
    private bool _initShowClockPanel;

    private static double _column2Width => LocalSettings.Settings.PomoLocalSetting.DetailWidth;

    public FocusView(bool initShowClockPanel = false)
    {
      this._initShowClockPanel = initShowClockPanel;
      this.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri("Views/Pomo/FocusResources.xaml", UriKind.Relative)
      });
      this._column1 = new ColumnDefinition()
      {
        MinWidth = 400.0,
        Width = new GridLength(1.0, GridUnitType.Star)
      };
      this._column2 = new ColumnDefinition()
      {
        MinWidth = 399.0,
        Width = new GridLength(FocusView._column2Width, GridUnitType.Star)
      };
      this.ColumnDefinitions.Add(this._column1);
      this.ColumnDefinitions.Add(this._column2);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
      this.SetResourceReference(Panel.BackgroundProperty, (object) "ShowAreaBackground");
      GridSplitter gridSplitter = new GridSplitter();
      gridSplitter.Width = 5.0;
      gridSplitter.FocusVisualStyle = (Style) null;
      gridSplitter.Background = (Brush) Brushes.Transparent;
      gridSplitter.IsTabStop = false;
      gridSplitter.HorizontalAlignment = HorizontalAlignment.Left;
      GridSplitter element1 = gridSplitter;
      element1.SetValue(Grid.ColumnProperty, (object) 1);
      element1.SetValue(Panel.ZIndexProperty, (object) 50);
      element1.DragCompleted += new DragCompletedEventHandler(this.OnDragSplitCompleted);
      this.Children.Add((UIElement) element1);
      Border border = new Border();
      border.Width = 5.0;
      border.FocusVisualStyle = (Style) null;
      border.Background = (Brush) Brushes.Transparent;
      border.BorderThickness = new Thickness(1.0, 0.0, 0.0, 0.0);
      border.HorizontalAlignment = HorizontalAlignment.Left;
      Border element2 = border;
      element2.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity5");
      element2.SetValue(Grid.ColumnProperty, (object) 1);
      this.Children.Add((UIElement) element2);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (e.NewSize.Width >= 800.0 && this._folded)
      {
        this._folded = false;
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(0.0), 1000.0, 240);
        this._focusDetail.SetValue(Grid.ColumnProperty, (object) 1);
        this._focusDetail.SetFoldStyle(false);
        doubleAnimation.Completed += (EventHandler) ((o, args) =>
        {
          this._animationPlaying = false;
          this._column2.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) null);
          this._column2.MaxWidth = double.PositiveInfinity;
          this._column2.MinWidth = 399.0;
          LocalSettings.Settings.PomoLocalSetting.DetailWidth = 1.0;
          this._column2.Width = new GridLength(1.0, GridUnitType.Star);
        });
        this._animationPlaying = true;
        this._column2.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) doubleAnimation);
      }
      if (e.NewSize.Width < 800.0 && !this._folded)
      {
        this._folded = true;
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(this._column2.ActualWidth), 0.0, 240);
        this._column2.MinWidth = 0.0;
        doubleAnimation.Completed += (EventHandler) ((o, args) =>
        {
          this._focusDetail.SetFoldStyle(true);
          this._focusDetail.SetValue(Grid.ColumnProperty, (object) 0);
        });
        this._column2.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) doubleAnimation);
      }
      if (this._animationPlaying || e.NewSize.Width < 800.0)
        return;
      this._column1.Width = new GridLength(1.0, GridUnitType.Star);
      LocalSettings.Settings.PomoLocalSetting.DetailWidth = (this.ActualWidth - this._column1.ActualWidth) / this._column1.ActualWidth;
      this._column2.Width = new GridLength(FocusView._column2Width, GridUnitType.Star);
    }

    private async void OnDragSplitCompleted(object sender, DragCompletedEventArgs e)
    {
      FocusView focusView = this;
      await Task.Delay(10);
      focusView._column1.Width = new GridLength(1.0, GridUnitType.Star);
      LocalSettings.Settings.PomoLocalSetting.DetailWidth = (focusView.ActualWidth - focusView._column1.ActualWidth) / focusView._column1.ActualWidth;
      focusView._column2.Width = new GridLength(FocusView._column2Width, GridUnitType.Star);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      this.Children.Remove((UIElement) this._focusControl);
      this.Children.Remove((UIElement) this._focusDetail);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      FocusView focusView1 = this;
      focusView1.Loaded -= new RoutedEventHandler(focusView1.OnLoaded);
      List<TimerModel> displayTimersAsync = await TimerDao.GetDisplayTimersAsync();
      FocusView focusView2 = focusView1;
      MainFocusControl mainFocusControl = new MainFocusControl(displayTimersAsync, focusView1._initShowClockPanel);
      mainFocusControl.MinWidth = 400.0;
      focusView2._focusControl = mainFocusControl;
      focusView1._focusControl.SetValue(Grid.ColumnProperty, (object) 0);
      focusView1.Children.Add((UIElement) focusView1._focusControl);
      TimerSyncService.PullRemoteTimers();
      FocusView focusView3 = focusView1;
      FocusDetailControl focusDetailControl = new FocusDetailControl();
      focusDetailControl.MinWidth = 399.0;
      focusView3._focusDetail = focusDetailControl;
      focusView1._focusDetail.SetValue(Grid.ColumnProperty, (object) 1);
      focusView1.Children.Add((UIElement) focusView1._focusDetail);
      await Task.Delay(20);
      if (focusView1.ActualWidth < 800.0)
      {
        focusView1._folded = true;
        focusView1._column2.MaxWidth = 0.0;
        focusView1._column2.MinWidth = 0.0;
        focusView1._focusDetail.SetFoldStyle(true);
        focusView1._focusDetail.SetValue(Grid.ColumnProperty, (object) 0);
      }
      focusView1.SizeChanged += new SizeChangedEventHandler(focusView1.OnSizeChanged);
    }

    public void ReloadStatistics() => this._focusDetail?.ReloadStatistics();

    public void OnTimerSelect(string id)
    {
      this._focusDetail?.OnTimerSelect(id);
      if (!string.IsNullOrEmpty(id))
      {
        if (!(this._focusDetail?.RenderTransform is TranslateTransform renderTransform))
          return;
        this._focusDetail.Visibility = Visibility.Visible;
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 240);
        renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation);
      }
      else
      {
        if (!(this._focusDetail?.RenderTransform is TranslateTransform renderTransform))
          return;
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(), 400.0, 240);
        doubleAnimation.Completed += (EventHandler) ((o, e) => this._focusDetail.Visibility = Visibility.Collapsed);
        renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation);
      }
    }

    public void TryFoldDetail(bool force = false)
    {
      if (this._focusDetail == null || this._focusControl == null || !force && (this._focusDetail.IsMouseOver || this._focusControl.IsListMouseOver) || !(this._focusDetail.RenderTransform is TranslateTransform renderTransform))
        return;
      DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(), 400.0, 240);
      doubleAnimation.Completed += (EventHandler) ((o, e) => this._focusDetail.Visibility = Visibility.Collapsed);
      renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation);
    }

    public void ClearTimerSelect()
    {
      this.TryFoldDetail(true);
      this._focusControl?.ClearSelect();
      this._focusDetail?.OnTimerSelect((string) null);
    }

    public bool GetClockPanelShow()
    {
      MainFocusControl focusControl = this._focusControl;
      return focusControl == null || focusControl.ClockPanelShow();
    }

    public bool ExistTimer()
    {
      MainFocusControl focusControl = this._focusControl;
      return focusControl == null || focusControl.ExistTimer();
    }

    public void OnDropFocus() => this._focusControl?.TryFoldClockPanel();

    public void Reload()
    {
    }

    public bool GetIsShowClockPanel()
    {
      MainFocusControl focusControl = this._focusControl;
      return focusControl != null && focusControl.IsShowClockPanel;
    }
  }
}
