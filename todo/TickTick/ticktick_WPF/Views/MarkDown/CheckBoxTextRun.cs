// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.CheckBoxTextRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class CheckBoxTextRun : FormattedTextRun
  {
    private readonly int _length;
    private readonly double _size;
    private bool _isDark;

    public CheckBoxTextRun(
      FormattedTextElement element,
      TextRunProperties properties,
      int length,
      bool isDark)
      : base(element, properties)
    {
      this._isDark = isDark;
      this._length = length;
      this._size = properties.FontRenderingEmSize + 2.0;
    }

    public override void Draw(
      DrawingContext drawingContext,
      Point origin,
      bool rightToLeft,
      bool sideways)
    {
      TextEmbeddedObjectMetrics embeddedObjectMetrics = base.Format(double.PositiveInfinity);
      Point location = new Point(origin.X - 1.0 + this._size * (double) (this._length / 4), origin.Y + (this._size - 10.0) / 4.0 - embeddedObjectMetrics.Baseline);
      BitmapImage bitmapImage = new BitmapImage(new Uri(this._isDark ? "pack://application:,,,/Assets/md_checked_dark.png" : "pack://application:,,,/Assets/md_checked_light.png"));
      drawingContext.DrawImage((ImageSource) bitmapImage, new Rect(location, new Size(this._size, this._size)));
      base.Draw(drawingContext, origin, rightToLeft, sideways);
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      TextEmbeddedObjectMetrics embeddedObjectMetrics = base.Format(remainingParagraphWidth);
      return new TextEmbeddedObjectMetrics(this._size + this._size * (double) (this._length / 4) + 4.0, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
