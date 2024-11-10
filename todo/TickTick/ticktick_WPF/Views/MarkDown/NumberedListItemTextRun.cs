// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.NumberedListItemTextRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class NumberedListItemTextRun : FormattedTextRun
  {
    private readonly int _length;
    private readonly string _numberText;
    private readonly TextRunProperties _properties;
    private SolidColorBrush _brush;

    public NumberedListItemTextRun(
      FormattedTextElement element,
      string numberText,
      int length,
      TextRunProperties properties,
      SolidColorBrush brush)
      : base(element, properties)
    {
      this._brush = brush;
      this._numberText = numberText;
      this._properties = properties;
      this._length = length;
    }

    public override void Draw(
      DrawingContext drawingContext,
      Point origin,
      bool rightToLeft,
      bool sideways)
    {
      Point origin1 = new Point(origin.X + 2.0, origin.Y + 1.5);
      double x = origin1.X + (double) (this._length / 4 * 16);
      drawingContext.DrawText(new FormattedText(this._numberText.TrimStart(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(Constants.DefaultMonoFont), this._properties.FontRenderingEmSize, (Brush) this._brush), new Point(x, 1.0));
      base.Draw(drawingContext, origin1, rightToLeft, sideways);
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      FormattedText formattedText = new FormattedText(this._numberText.Trim(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(Constants.DefaultMonoFont), this._properties.FontRenderingEmSize, (Brush) this._brush);
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      int num = 16 + 16 * (this._length / 4);
      if (this._numberText.Trim().Length > 2)
        num = (int) embeddedObjectMetrics.Width + 16 * (this._length / 4);
      return new TextEmbeddedObjectMetrics(this._properties.FontRenderingEmSize <= 14.0 ? (double) (num + 4) : (double) (num + (int) (this._properties.FontRenderingEmSize - 10.0)), embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
