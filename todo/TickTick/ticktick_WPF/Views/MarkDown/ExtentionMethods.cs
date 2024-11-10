// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.ExtentionMethods
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Drawing;
using System.Windows;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public static class ExtentionMethods
  {
    private static System.Windows.Point ToWpf(this System.Drawing.Point p)
    {
      return new System.Windows.Point((double) p.X, (double) p.Y);
    }

    private static System.Windows.Size ToWpf(this System.Drawing.Size s)
    {
      return new System.Windows.Size((double) s.Width, (double) s.Height);
    }

    public static Rect ToWpf(this Rectangle rect)
    {
      return new Rect(rect.Location.ToWpf(), rect.Size.ToWpf());
    }

    public static System.Drawing.Point ToSystemDrawing(this System.Windows.Point p)
    {
      return new System.Drawing.Point((int) p.X, (int) p.Y);
    }

    public static System.Windows.Size TransformToDevice(this System.Windows.Size size, Visual visual)
    {
      Matrix? transformToDevice = PresentationSource.FromVisual(visual)?.CompositionTarget?.TransformToDevice;
      return new System.Windows.Size(size.Width * (transformToDevice.HasValue ? transformToDevice.GetValueOrDefault().M11 : 1.0), size.Height * (transformToDevice.HasValue ? transformToDevice.GetValueOrDefault().M22 : 1.0));
    }

    public static Rect TransformFromDevice(this Rect rect, Visual visual)
    {
      Matrix? transformFromDevice = PresentationSource.FromVisual(visual)?.CompositionTarget?.TransformFromDevice;
      return transformFromDevice.HasValue ? Rect.Transform(rect, transformFromDevice.Value) : rect;
    }
  }
}
