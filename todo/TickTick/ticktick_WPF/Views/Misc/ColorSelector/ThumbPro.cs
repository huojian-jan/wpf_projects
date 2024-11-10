// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ColorSelector.ThumbPro
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls.Primitives;

#nullable disable
namespace ticktick_WPF.Views.Misc.ColorSelector
{
  public class ThumbPro : Thumb
  {
    public static readonly DependencyProperty TopProperty = DependencyProperty.Register(nameof (Top), typeof (double), typeof (ThumbPro), new PropertyMetadata((object) 0.0));
    public static readonly DependencyProperty LeftProperty = DependencyProperty.Register(nameof (Left), typeof (double), typeof (ThumbPro), new PropertyMetadata((object) 0.0));
    private double FirstTop;
    private double FirstLeft;

    public double Top
    {
      get => (double) this.GetValue(ThumbPro.TopProperty);
      set => this.SetValue(ThumbPro.TopProperty, (object) value);
    }

    public double Left
    {
      get => (double) this.GetValue(ThumbPro.LeftProperty);
      set => this.SetValue(ThumbPro.LeftProperty, (object) value);
    }

    public double Xoffset { get; set; }

    public double Yoffset { get; set; }

    public bool VerticalOnly { get; set; }

    public bool HorizontalOnly { get; set; }

    public double Xpercent => (this.Left + this.Xoffset) / this.ActualWidth;

    public double Ypercent => (this.Top + this.Yoffset) / this.ActualHeight;

    public void SetTopLeftByPercent(double xpercent, double ypercent)
    {
      if (!this.HorizontalOnly)
        this.Top = ypercent * this.ActualHeight - this.Yoffset;
      if (this.VerticalOnly)
        return;
      this.Left = xpercent * this.ActualWidth - this.Xoffset;
    }

    public event Action<double, double> ValueChanged;

    public ThumbPro()
    {
      this.Loaded += (RoutedEventHandler) ((sender, e) =>
      {
        if (!this.VerticalOnly)
          this.Left = -this.Xoffset;
        if (this.HorizontalOnly)
          return;
        this.Top = -this.Yoffset;
      });
      this.DragStarted += (DragStartedEventHandler) ((sender, e) =>
      {
        if (!this.VerticalOnly)
        {
          this.Left = e.HorizontalOffset - this.Xoffset;
          this.FirstLeft = this.Left;
        }
        if (!this.HorizontalOnly)
        {
          this.Top = e.VerticalOffset - this.Yoffset;
          this.FirstTop = this.Top;
        }
        Action<double, double> valueChanged = this.ValueChanged;
        if (valueChanged == null)
          return;
        valueChanged(this.Xpercent, this.Ypercent);
      });
      this.DragDelta += (DragDeltaEventHandler) ((sender, e) =>
      {
        if (!this.VerticalOnly)
        {
          double num = this.FirstLeft + e.HorizontalChange;
          this.Left = num >= -this.Xoffset ? (num <= this.ActualWidth - this.Xoffset ? num : this.ActualWidth - this.Xoffset) : -this.Xoffset;
        }
        if (!this.HorizontalOnly)
        {
          double num = this.FirstTop + e.VerticalChange;
          this.Top = num >= -this.Yoffset ? (num <= this.ActualHeight - this.Yoffset ? num : this.ActualHeight - this.Yoffset) : -this.Yoffset;
        }
        Action<double, double> valueChanged = this.ValueChanged;
        if (valueChanged == null)
          return;
        valueChanged(this.Xpercent, this.Ypercent);
      });
    }
  }
}
