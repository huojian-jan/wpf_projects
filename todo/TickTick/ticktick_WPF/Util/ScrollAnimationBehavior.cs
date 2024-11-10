// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ScrollAnimationBehavior
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ScrollAnimationBehavior
  {
    private static ScrollViewer _listBoxScroller = new ScrollViewer();
    public static DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof (double), typeof (ScrollAnimationBehavior), (PropertyMetadata) new UIPropertyMetadata((object) 0.0, new PropertyChangedCallback(ScrollAnimationBehavior.OnVerticalOffsetChanged)));
    public static DependencyProperty TimeDurationProperty = DependencyProperty.RegisterAttached("TimeDuration", typeof (TimeSpan), typeof (ScrollAnimationBehavior), new PropertyMetadata((object) new TimeSpan(0, 0, 0, 0, 0)));
    public static DependencyProperty PointsToScrollProperty = DependencyProperty.RegisterAttached("PointsToScroll", typeof (double), typeof (ScrollAnimationBehavior), new PropertyMetadata((object) 0.0));
    public static DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof (bool), typeof (ScrollAnimationBehavior), (PropertyMetadata) new UIPropertyMetadata((object) false, new PropertyChangedCallback(ScrollAnimationBehavior.OnIsEnabledChanged)));

    public static void SetVerticalOffset(FrameworkElement target, double value)
    {
      target.SetValue(ScrollAnimationBehavior.VerticalOffsetProperty, (object) value);
    }

    public static double GetVerticalOffset(FrameworkElement target)
    {
      return (double) target.GetValue(ScrollAnimationBehavior.VerticalOffsetProperty);
    }

    public static void SetTimeDuration(FrameworkElement target, TimeSpan value)
    {
      target.SetValue(ScrollAnimationBehavior.TimeDurationProperty, (object) value);
    }

    public static TimeSpan GetTimeDuration(FrameworkElement target)
    {
      return (TimeSpan) target.GetValue(ScrollAnimationBehavior.TimeDurationProperty);
    }

    public static void SetPointsToScroll(FrameworkElement target, double value)
    {
      target.SetValue(ScrollAnimationBehavior.PointsToScrollProperty, (object) value);
    }

    public static double GetPointsToScroll(FrameworkElement target)
    {
      return (double) target.GetValue(ScrollAnimationBehavior.PointsToScrollProperty);
    }

    private static void OnVerticalOffsetChanged(
      DependencyObject target,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(target is ScrollViewer scrollViewer))
        return;
      scrollViewer.ScrollToVerticalOffset((double) e.NewValue);
    }

    public static void SetIsEnabled(FrameworkElement target, bool value)
    {
      target.SetValue(ScrollAnimationBehavior.IsEnabledProperty, (object) value);
    }

    public static bool GetIsEnabled(FrameworkElement target)
    {
      return (bool) target.GetValue(ScrollAnimationBehavior.IsEnabledProperty);
    }

    private static void OnIsEnabledChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      DependencyObject dependencyObject = sender;
      if (dependencyObject == null || !(dependencyObject is ScrollViewer))
        return;
      (dependencyObject as ScrollViewer).Loaded += new RoutedEventHandler(ScrollAnimationBehavior.scrollerLoaded);
    }

    private static void AnimateScroll(ScrollViewer scrollViewer, double ToValue)
    {
      DoubleAnimation element = new DoubleAnimation();
      element.From = new double?(scrollViewer.VerticalOffset);
      element.To = new double?(ToValue);
      element.Duration = new Duration(ScrollAnimationBehavior.GetTimeDuration((FrameworkElement) scrollViewer));
      Storyboard storyboard = new Storyboard();
      storyboard.Children.Add((Timeline) element);
      Storyboard.SetTarget((DependencyObject) element, (DependencyObject) scrollViewer);
      Storyboard.SetTargetProperty((DependencyObject) element, new PropertyPath((object) ScrollAnimationBehavior.VerticalOffsetProperty));
      storyboard.Begin();
    }

    private static double NormalizeScrollPos(
      ScrollViewer scroll,
      double scrollChange,
      Orientation o)
    {
      double num = scrollChange;
      if (scrollChange < 0.0)
        num = 0.0;
      if (o == Orientation.Vertical && scrollChange > scroll.ScrollableHeight)
        num = scroll.ScrollableHeight;
      else if (o == Orientation.Horizontal && scrollChange > scroll.ScrollableWidth)
        num = scroll.ScrollableWidth;
      return num;
    }

    private static void UpdateScrollPosition(object sender)
    {
      if (!(sender is ListBox listBox))
        return;
      double ToValue = 0.0;
      for (int index = 0; index < listBox.SelectedIndex; ++index)
      {
        if (listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[index]) is ListBoxItem listBoxItem)
          ToValue += listBoxItem.ActualHeight;
      }
      ScrollAnimationBehavior.AnimateScroll(ScrollAnimationBehavior._listBoxScroller, ToValue);
    }

    private static void SetEventHandlersForScrollViewer(ScrollViewer scroller)
    {
      scroller.PreviewMouseWheel += new MouseWheelEventHandler(ScrollAnimationBehavior.ScrollViewerPreviewMouseWheel);
      scroller.PreviewKeyDown += new KeyEventHandler(ScrollAnimationBehavior.ScrollViewerPreviewKeyDown);
    }

    private static void scrollerLoaded(object sender, RoutedEventArgs e)
    {
      ScrollAnimationBehavior.SetEventHandlersForScrollViewer(sender as ScrollViewer);
    }

    private static void ScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      double delta = (double) e.Delta;
      ScrollViewer scrollViewer = (ScrollViewer) sender;
      double ToValue = ScrollAnimationBehavior.GetVerticalOffset((FrameworkElement) scrollViewer) - delta / 3.0;
      if (ToValue < 0.0)
        ScrollAnimationBehavior.AnimateScroll(scrollViewer, 0.0);
      else if (ToValue > scrollViewer.ScrollableHeight)
        ScrollAnimationBehavior.AnimateScroll(scrollViewer, scrollViewer.ScrollableHeight);
      else
        ScrollAnimationBehavior.AnimateScroll(scrollViewer, ToValue);
      e.Handled = true;
    }

    private static void ScrollViewerPreviewKeyDown(object sender, KeyEventArgs e)
    {
      ScrollViewer scrollViewer = (ScrollViewer) sender;
      Key key = e.Key;
      double ToValue = ScrollAnimationBehavior.GetVerticalOffset((FrameworkElement) scrollViewer);
      bool flag = false;
      switch (key)
      {
        case Key.Prior:
          ToValue = ScrollAnimationBehavior.NormalizeScrollPos(scrollViewer, ToValue - scrollViewer.ViewportHeight, Orientation.Vertical);
          flag = true;
          break;
        case Key.Next:
          ToValue = ScrollAnimationBehavior.NormalizeScrollPos(scrollViewer, ToValue + scrollViewer.ViewportHeight, Orientation.Vertical);
          flag = true;
          break;
        case Key.Up:
          ToValue = ScrollAnimationBehavior.NormalizeScrollPos(scrollViewer, ToValue - ScrollAnimationBehavior.GetPointsToScroll((FrameworkElement) scrollViewer), Orientation.Vertical);
          flag = true;
          break;
        case Key.Down:
          ToValue = ScrollAnimationBehavior.NormalizeScrollPos(scrollViewer, ToValue + ScrollAnimationBehavior.GetPointsToScroll((FrameworkElement) scrollViewer), Orientation.Vertical);
          flag = true;
          break;
      }
      if (ToValue != ScrollAnimationBehavior.GetVerticalOffset((FrameworkElement) scrollViewer))
        ScrollAnimationBehavior.AnimateScroll(scrollViewer, ToValue);
      e.Handled = flag;
    }

    private static void ListBoxLayoutUpdated(object sender, EventArgs e)
    {
      ScrollAnimationBehavior.UpdateScrollPosition(sender);
    }

    private static void ListBoxLoaded(object sender, RoutedEventArgs e)
    {
      ScrollAnimationBehavior.UpdateScrollPosition(sender);
    }

    private static void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ScrollAnimationBehavior.UpdateScrollPosition(sender);
    }
  }
}
