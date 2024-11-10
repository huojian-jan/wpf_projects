// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.Colorizer.SpellCheckBackgroundRenderer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.Colorizer
{
  public class SpellCheckBackgroundRenderer : IBackgroundRenderer
  {
    public SpellCheckBackgroundRenderer()
    {
      this.ErrorSegments = new TextSegmentCollection<TextSegment>();
    }

    public TextSegmentCollection<TextSegment> ErrorSegments { get; }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      Pen pen = new Pen((Brush) new SolidColorBrush(SpellCheckBackgroundRenderer.GetSpellCheckColor()), 0.6);
      pen.Freeze();
      foreach (TextSegment errorSegment in this.ErrorSegments)
      {
        foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, (ISegment) errorSegment))
        {
          Point bottomLeft = rect.BottomLeft;
          int count = Math.Max((int) ((rect.BottomRight.X - bottomLeft.X) / 2.0) + 1, 4);
          StreamGeometry streamGeometry = new StreamGeometry();
          using (StreamGeometryContext streamGeometryContext = streamGeometry.Open())
          {
            streamGeometryContext.BeginFigure(bottomLeft, false, false);
            streamGeometryContext.PolyLineTo((IList<Point>) SpellCheckBackgroundRenderer.CreatePoints(bottomLeft, 2.0, count).ToArray<Point>(), true, false);
          }
          streamGeometry.Freeze();
          drawingContext.DrawGeometry((Brush) Brushes.Transparent, pen, (Geometry) streamGeometry);
          if (stopwatch.ElapsedMilliseconds > 100L)
            break;
        }
        if (stopwatch.ElapsedMilliseconds > 100L)
          break;
      }
      stopwatch.Stop();
    }

    private static Color GetSpellCheckColor() => Colors.Red;

    public KnownLayer Layer => KnownLayer.Selection;

    private static IEnumerable<Point> CreatePoints(Point start, double offset, int count)
    {
      for (int i = 0; i < count; ++i)
        yield return new Point(start.X + (double) i * offset, start.Y - ((i + 1) % 2 == 0 ? offset : 0.0));
    }
  }
}
