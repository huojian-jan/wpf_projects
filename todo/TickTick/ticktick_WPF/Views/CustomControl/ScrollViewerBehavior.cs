// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.ScrollViewerBehavior
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views.CustomControl
{
  public static class ScrollViewerBehavior
  {
    public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof (double), typeof (ScrollViewerBehavior), (PropertyMetadata) new UIPropertyMetadata((object) 0.0, new PropertyChangedCallback(ScrollViewerBehavior.OnVerticalOffsetChanged)));

    public static void SetVerticalOffset(FrameworkElement target, double value)
    {
      target.SetValue(ScrollViewerBehavior.VerticalOffsetProperty, (object) value);
    }

    public static double GetVerticalOffset(FrameworkElement target)
    {
      return (double) target.GetValue(ScrollViewerBehavior.VerticalOffsetProperty);
    }

    private static void OnVerticalOffsetChanged(
      DependencyObject target,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(target is ScrollViewer scrollViewer))
        return;
      scrollViewer.ScrollToVerticalOffset((double) e.NewValue);
    }
  }
}
