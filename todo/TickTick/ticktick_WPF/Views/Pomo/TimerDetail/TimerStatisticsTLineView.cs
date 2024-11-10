// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerDetail.TimerStatisticsTLineView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo.TimerDetail
{
  public class TimerStatisticsTLineView : Grid
  {
    private TextBlock _totalDuration;
    private Border _switchButton;
    private TimerDetailLineView _lineView;
    private TimerDetailLineView _lineViewExtra;
    private TextBlock _popText;
    private Border _popBorder;
    private double _offset = -1.0;
    private Border _rightBorder;

    public TimerStatisticsTLineView()
    {
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(36.0)
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(26.0)
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(170.0)
      });
      this.Background = (Brush) Brushes.Transparent;
      this.InitTotalText();
      this.InitIntervalSwitch();
      this.InitDateSpan();
      this.InitTimeline();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.MouseMove += new MouseEventHandler(this.OnMouseMove);
      this.Loaded += (RoutedEventHandler) ((o, e) =>
      {
        DataChangedNotifier.WeekStartFromChanged += new EventHandler(this.OnWeekStartChanged);
        DataChangedNotifier.IsDarkChanged += new EventHandler(this.OnIsDarkChanged);
      });
      this.Unloaded += (RoutedEventHandler) ((o, e) =>
      {
        DataChangedNotifier.WeekStartFromChanged -= new EventHandler(this.OnWeekStartChanged);
        DataChangedNotifier.IsDarkChanged -= new EventHandler(this.OnIsDarkChanged);
      });
    }

    private void OnIsDarkChanged(object sender, EventArgs e) => this.ReloadData();

    private async void OnWeekStartChanged(object sender, EventArgs e)
    {
      TimerStatisticsTLineView statisticsTlineView = this;
      if (!(statisticsTlineView.DataContext is TimerTimelineItemViewModel model))
        model = (TimerTimelineItemViewModel) null;
      else if (!(model.Interval == "week"))
      {
        model = (TimerTimelineItemViewModel) null;
      }
      else
      {
        model.StartDate = Utils.GetWeekStart(DateTime.Today);
        model.EndDate = model.StartDate.AddDays(6.0);
        model.SetDateText();
        model.NotifySpanChanged();
        Dictionary<string, long> statistics = await model.GetStatistics();
        statisticsTlineView.SetData(model, statistics);
        model = (TimerTimelineItemViewModel) null;
      }
    }

    private void InitTimeline()
    {
      TimerDetailLineView timerDetailLineView = new TimerDetailLineView();
      timerDetailLineView.Margin = new Thickness(20.0, 0.0, 0.0, 0.0);
      timerDetailLineView.RenderTransform = (Transform) new TranslateTransform()
      {
        X = 0.0
      };
      timerDetailLineView.Background = (Brush) Brushes.Transparent;
      this._lineView = timerDetailLineView;
      this._lineView.SetValue(Grid.RowProperty, (object) 2);
      this.Children.Add((UIElement) this._lineView);
    }

    private void InitDateSpan()
    {
      StackPanel stackPanel = new StackPanel();
      stackPanel.Orientation = Orientation.Horizontal;
      stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
      stackPanel.VerticalAlignment = VerticalAlignment.Top;
      stackPanel.Margin = new Thickness(18.0, 0.0, 0.0, 0.0);
      StackPanel element1 = stackPanel;
      Border border1 = new Border();
      border1.Child = (UIElement) UiUtils.GetArrow(12.0, 90.0, "BaseColorOpacity100");
      border1.Width = 14.0;
      border1.Height = 20.0;
      border1.Cursor = Cursors.Hand;
      Border element2 = border1;
      element2.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_60");
      element2.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLeftClick);
      element1.Children.Add((UIElement) element2);
      TextBlock textBlock = new TextBlock();
      textBlock.Margin = new Thickness(3.0, 0.0, 3.0, 0.0);
      TextBlock element3 = textBlock;
      element3.SetResourceReference(FrameworkElement.StyleProperty, (object) "Tag02");
      element3.SetBinding(TextBlock.TextProperty, "DateText");
      element1.Children.Add((UIElement) element3);
      Border border2 = new Border();
      border2.Child = (UIElement) UiUtils.GetArrow(12.0, -90.0, "BaseColorOpacity100");
      border2.Width = 14.0;
      border2.Height = 20.0;
      border2.Cursor = Cursors.Hand;
      Border border3 = border2;
      border3.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_60");
      Border border4 = new Border();
      border4.Child = (UIElement) border3;
      this._rightBorder = border4;
      element1.Children.Add((UIElement) this._rightBorder);
      this._rightBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnRightClick);
      element1.SetValue(Grid.RowProperty, (object) 1);
      this.Children.Add((UIElement) element1);
    }

    private void InitIntervalSwitch()
    {
      Border border = new Border();
      border.Height = 24.0;
      border.HorizontalAlignment = HorizontalAlignment.Right;
      border.VerticalAlignment = VerticalAlignment.Bottom;
      border.CornerRadius = new CornerRadius(12.0);
      border.Margin = new Thickness(0.0, 0.0, 20.0, 0.0);
      border.BorderThickness = new Thickness(1.0);
      this._switchButton = border;
      this._switchButton.SetResourceReference(FrameworkElement.StyleProperty, (object) "HoverBorderStyle");
      StackPanel stackPanel1 = new StackPanel();
      stackPanel1.Orientation = Orientation.Horizontal;
      stackPanel1.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
      StackPanel stackPanel2 = stackPanel1;
      TextBlock textBlock = new TextBlock();
      textBlock.Margin = new Thickness(0.0, 0.0, 6.0, 0.0);
      TextBlock element = textBlock;
      element.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body01");
      element.SetBinding(TextBlock.TextProperty, "IntervalText");
      stackPanel2.Children.Add((UIElement) element);
      Path arrow = UiUtils.GetArrow(12.0, 0.0, "BaseColorOpacity40");
      stackPanel2.Children.Add((UIElement) arrow);
      this._switchButton.Child = (UIElement) stackPanel2;
      this.Children.Add((UIElement) this._switchButton);
      this._switchButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSwitchClick);
    }

    private void InitTotalText()
    {
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 24.0;
      textBlock.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock.VerticalAlignment = VerticalAlignment.Bottom;
      textBlock.Margin = new Thickness(20.0, 10.0, 0.0, 0.0);
      this._totalDuration = textBlock;
      this._totalDuration.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity90");
      this.Children.Add((UIElement) this._totalDuration);
    }

    private void SetTotalDurationText(long duration)
    {
      UiUtils.SetTimeTextRun(this._totalDuration, duration, 14, "BaseColorOpacity80");
    }

    private void OnSwitchClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TimerTimelineItemViewModel dataContext))
        return;
      Mouse.Capture((IInputElement) null);
      EscPopup escPopup1 = new EscPopup();
      escPopup1.PlacementTarget = sender as UIElement;
      escPopup1.StaysOpen = false;
      escPopup1.Placement = PlacementMode.Bottom;
      escPopup1.VerticalOffset = -5.0;
      escPopup1.HorizontalOffset = -5.0;
      EscPopup escPopup2 = escPopup1;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "week", Utils.GetString("TimelineWeek"), (DrawingImage) null);
      menuItemViewModel1.Selected = dataContext.Interval == "week";
      types.Add(menuItemViewModel1);
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "month", Utils.GetString("TimelineMonth"), (DrawingImage) null);
      menuItemViewModel2.Selected = dataContext.Interval == "month";
      types.Add(menuItemViewModel2);
      CustomMenuItemViewModel menuItemViewModel3 = new CustomMenuItemViewModel((object) "year", Utils.GetString("TimelineYear"), (DrawingImage) null);
      menuItemViewModel3.Selected = dataContext.Interval == "year";
      types.Add(menuItemViewModel3);
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) escPopup2);
      customMenuList.Operated += new EventHandler<object>(this.OnActionSelected);
      customMenuList.Show();
    }

    private async void OnActionSelected(object sender, object e)
    {
      TimerStatisticsTLineView statisticsTlineView = this;
      if (!(statisticsTlineView.DataContext is TimerTimelineItemViewModel model))
      {
        model = (TimerTimelineItemViewModel) null;
      }
      else
      {
        string interval = e.ToString();
        if (interval != "week" && !ProChecker.CheckPro(ProType.MoreCharts, Window.GetWindow((DependencyObject) statisticsTlineView)))
        {
          model = (TimerTimelineItemViewModel) null;
        }
        else
        {
          model.SetInterval(interval);
          Dictionary<string, long> statistics = await model.GetStatistics();
          statisticsTlineView.SetData(model, statistics);
          model = (TimerTimelineItemViewModel) null;
        }
      }
    }

    private async void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      TimerStatisticsTLineView statisticsTlineView = this;
      if (!(statisticsTlineView.DataContext is TimerTimelineItemViewModel model))
      {
        model = (TimerTimelineItemViewModel) null;
      }
      else
      {
        DateTime dateTime1 = model.StartDate;
        DateTime dateTime2 = model.EndDate;
        switch (model.Interval)
        {
          case "week":
            dateTime1 = model.StartDate.AddDays(7.0);
            dateTime2 = dateTime1.AddDays(6.0);
            break;
          case "month":
            dateTime1 = model.StartDate.AddMonths(1);
            dateTime2 = dateTime1.AddMonths(1).AddDays(-1.0);
            break;
          case "year":
            dateTime1 = model.StartDate.AddYears(1);
            dateTime2 = dateTime1.AddYears(1).AddDays(-1.0);
            break;
        }
        if (dateTime1 >= DateTime.Today.AddDays(1.0))
        {
          model = (TimerTimelineItemViewModel) null;
        }
        else
        {
          model.StartDate = dateTime1;
          model.EndDate = dateTime2;
          model.SetDateText();
          model.NotifySpanChanged(false);
          Dictionary<string, long> statistics = await model.GetStatistics();
          statisticsTlineView.SetLineAnimation(false);
          statisticsTlineView.SetData(model, statistics);
          model = (TimerTimelineItemViewModel) null;
        }
      }
    }

    private async void OnLeftClick(object sender, MouseButtonEventArgs e)
    {
      TimerStatisticsTLineView statisticsTlineView = this;
      if (!(statisticsTlineView.DataContext is TimerTimelineItemViewModel model))
      {
        model = (TimerTimelineItemViewModel) null;
      }
      else
      {
        switch (model.Interval)
        {
          case "week":
            model.StartDate = model.StartDate.AddDays(-7.0);
            model.EndDate = model.StartDate.AddDays(6.0);
            break;
          case "month":
            model.StartDate = model.StartDate.AddMonths(-1);
            model.EndDate = model.StartDate.AddMonths(1).AddDays(-1.0);
            break;
          case "year":
            model.StartDate = model.StartDate.AddYears(-1);
            model.EndDate = model.StartDate.AddYears(1).AddDays(-1.0);
            break;
        }
        model.SetDateText();
        model.NotifySpanChanged();
        Dictionary<string, long> statistics = await model.GetStatistics();
        statisticsTlineView.SetLineAnimation(true);
        statisticsTlineView.SetData(model, statistics);
        model = (TimerTimelineItemViewModel) null;
      }
    }

    private void SetLineAnimation(bool left)
    {
      if (this._lineViewExtra == null)
      {
        TimerDetailLineView timerDetailLineView = new TimerDetailLineView();
        timerDetailLineView.Margin = new Thickness(20.0, 0.0, 0.0, 0.0);
        timerDetailLineView.RenderTransform = (Transform) new TranslateTransform()
        {
          X = (double) ((left ? -1 : 1) * 50)
        };
        timerDetailLineView.Visibility = Visibility.Collapsed;
        this._lineViewExtra = timerDetailLineView;
        this._lineViewExtra.SetValue(Grid.RowProperty, (object) 2);
        this.Children.Add((UIElement) this._lineViewExtra);
      }
      TimerDetailLineView lineView = this._lineView;
      this._lineView = this._lineViewExtra;
      this._lineViewExtra = lineView;
      if (this._lineView.RenderTransform is TranslateTransform renderTransform)
      {
        this._lineView.Visibility = Visibility.Visible;
        DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?((double) ((left ? -1 : 1) * 80)), 0.0, 180);
        DoubleAnimation doubleAnimation2 = doubleAnimation1;
        QuadraticEase quadraticEase = new QuadraticEase();
        quadraticEase.EasingMode = EasingMode.EaseInOut;
        doubleAnimation2.EasingFunction = (IEasingFunction) quadraticEase;
        doubleAnimation1.Completed += (EventHandler) ((o, e) => this._lineViewExtra.Visibility = Visibility.Collapsed);
        renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation1);
      }
      this._lineView.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(0.0), 1.0, 100));
      this._lineViewExtra.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(1.0), 0.0, 100));
    }

    private async void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      TimerStatisticsTLineView statisticsTlineView = this;
      if (!(e.NewValue is TimerTimelineItemViewModel model))
      {
        model = (TimerTimelineItemViewModel) null;
      }
      else
      {
        Dictionary<string, long> statistics = await model.GetStatistics();
        statisticsTlineView.SetData(model, statistics);
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) model, new EventHandler<PropertyChangedEventArgs>(statisticsTlineView.Reload), "Reload");
        model = (TimerTimelineItemViewModel) null;
      }
    }

    private void Reload(object sender, PropertyChangedEventArgs e) => this.ReloadData();

    private void ReloadData()
    {
      this.Dispatcher.Invoke<Task>((Func<Task>) (async () =>
      {
        TimerStatisticsTLineView statisticsTlineView = this;
        if (!(statisticsTlineView.DataContext is TimerTimelineItemViewModel model2))
        {
          model2 = (TimerTimelineItemViewModel) null;
        }
        else
        {
          Dictionary<string, long> statistics = await model2.GetStatistics();
          statisticsTlineView.SetData(model2, statistics);
          model2 = (TimerTimelineItemViewModel) null;
        }
      }));
    }

    private void SetData(TimerTimelineItemViewModel model, Dictionary<string, long> statisticsData)
    {
      this.SetTotalDurationText(statisticsData.Count > 0 ? statisticsData.Values.Sum() : 0L);
      this._rightBorder.IsEnabled = model.EndDate <= DateTime.Today;
      this._rightBorder.Opacity = this._rightBorder.IsEnabled ? 1.0 : 0.3;
      this._lineView.SetData(model, statisticsData);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (this._lineView.IsMouseOver)
      {
        (double offset, TimerDayItemViewModel dayItemViewModel) = this._lineView.GetHoverItem(e);
        if (dayItemViewModel != null && dayItemViewModel.Minutes > 0L)
        {
          if (Math.Abs(this._offset - offset) <= 1.0)
            return;
          this._offset = offset;
          this.DelayShowItemPopup(offset, dayItemViewModel);
          return;
        }
      }
      this._offset = -1.0;
      this.HideItemPopup();
    }

    private async void DelayShowItemPopup(double offset, TimerDayItemViewModel item)
    {
      await Task.Delay(200);
      if (Math.Abs(this._offset - offset) >= 0.1)
        return;
      this.ShowItemPopup(offset, item);
    }

    private void HideItemPopup()
    {
      if (this._popBorder == null)
        return;
      this._popBorder.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
      this._popBorder.Opacity = 0.0;
    }

    private void InitItemPop()
    {
      if (this._popBorder != null)
        return;
      Border border = new Border();
      border.HorizontalAlignment = HorizontalAlignment.Left;
      border.VerticalAlignment = VerticalAlignment.Top;
      border.Opacity = 0.0;
      border.MinWidth = 40.0;
      border.IsHitTestVisible = false;
      this._popBorder = border;
      this._popBorder.SetValue(Grid.RowSpanProperty, (object) 3);
      this._popBorder.SetValue(Panel.ZIndexProperty, (object) 1000);
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 13.0;
      textBlock.Margin = new Thickness(10.0, 0.0, 10.0, 0.0);
      this._popText = textBlock;
      this._popText.SetResourceReference(TextBlock.ForegroundProperty, (object) "PrimaryColor");
      ContentControl contentControl1 = new ContentControl();
      contentControl1.Margin = new Thickness(0.0, 4.0, 0.0, 4.0);
      contentControl1.MinWidth = 50.0;
      ContentControl contentControl2 = contentControl1;
      contentControl2.SetResourceReference(FrameworkElement.StyleProperty, (object) "PopupContentStyle");
      contentControl2.Content = (object) this._popText;
      this._popBorder.Child = (UIElement) contentControl2;
      this.Children.Add((UIElement) this._popBorder);
    }

    private void ShowItemPopup(double offset, TimerDayItemViewModel item)
    {
      this.InitItemPop();
      this._popText.Text = item.GetDateText() + ", " + Utils.GetShortDurationString(item.Minutes * 60L);
      double width = Utils.MeasureString(this._popText.Text, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 13.0).Width;
      this._popBorder.Margin = new Thickness(Math.Max(10.0, 8.0 + offset - width / 2.0), (double) (152 - item.ColumnHeight), 0.0, 0.0);
      this._popBorder.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(0.0), 1.0, 150));
    }
  }
}
