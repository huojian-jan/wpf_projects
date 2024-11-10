// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.SmoothScrollViewer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.CustomControl
{
  public class SmoothScrollViewer : ScrollViewer
  {
    private double LastLocation;
    private DateTime _lastScrollTime;

    public void HandleSmoothMouseWheel(MouseWheelEventArgs e)
    {
      Mouse.OverrideCursor = Cursors.Arrow;
      DelayActionHandlerCenter.TryDoAction("SmoothScrollViewer.HandleSmoothMouseWheel", (EventHandler) ((sender, args) => this.Dispatcher.Invoke((Action) (() => Mouse.OverrideCursor = (Cursor) null))));
      DateTime now = DateTime.Now;
      if ((now - this._lastScrollTime).TotalMilliseconds > 200.0)
        this.LastLocation = this.VerticalOffset;
      this._lastScrollTime = now;
      if (Math.Abs(e.Delta) < 60)
      {
        this.LastLocation -= (double) e.Delta;
        this.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, (AnimationTimeline) null);
        this.ScrollToVerticalOffset(this.LastLocation);
        e.Handled = true;
      }
      else
      {
        double ToValue = this.LastLocation - (double) e.Delta * 0.8;
        this.ScrollToVerticalOffset(this.LastLocation);
        if (ToValue < 0.0)
          ToValue = 0.0;
        if (ToValue > this.ScrollableHeight)
          ToValue = this.ScrollableHeight;
        this.AnimateScroll(ToValue);
        this.LastLocation = ToValue;
        e.Handled = true;
      }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e) => this.HandleSmoothMouseWheel(e);

    private void AnimateScroll(double ToValue)
    {
      this.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, (AnimationTimeline) null);
      DoubleAnimation animation = new DoubleAnimation();
      animation.From = new double?(this.VerticalOffset);
      animation.To = new double?(ToValue);
      animation.Duration = (Duration) TimeSpan.FromMilliseconds(150.0);
      this.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, (AnimationTimeline) animation);
    }
  }
}
