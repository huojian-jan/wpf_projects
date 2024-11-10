// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.IndentListItemTextRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class IndentListItemTextRun : FormattedTextRun
  {
    private readonly int _flagLength;

    public IndentListItemTextRun(FormattedTextElement element, TextRunProperties properties)
      : base(element, properties)
    {
    }

    public override void Draw(
      DrawingContext drawingContext,
      Point origin,
      bool rightToLeft,
      bool sideways)
    {
      Point origin1 = new Point(origin.X + 1.5, origin.Y + 3.0);
      double y = (base.Format(double.PositiveInfinity).Height - 6.0) / 2.0;
      drawingContext.DrawRoundedRectangle((Brush) ThemeUtil.GetPrimaryColor(1.0), (Pen) null, new Rect(23.0, y, 6.0, 6.0), 3.0, 3.0);
      base.Draw(drawingContext, origin1, rightToLeft, sideways);
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      TextEmbeddedObjectMetrics embeddedObjectMetrics = base.Format(remainingParagraphWidth);
      return new TextEmbeddedObjectMetrics(40.0, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
