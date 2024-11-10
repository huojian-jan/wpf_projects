// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LineTextRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class LineTextRun : FormattedTextRun
  {
    private readonly TextEditor _editor;

    public LineTextRun(
      FormattedTextElement element,
      TextEditor editor,
      TextRunProperties properties)
      : base(element, properties)
    {
      this._editor = editor;
    }

    public override void Draw(
      DrawingContext drawingContext,
      Point origin,
      bool rightToLeft,
      bool sideways)
    {
      SolidColorBrush color = ThemeUtil.GetColor("BaseColorOpacity20", (FrameworkElement) this._editor);
      drawingContext.DrawRectangle((Brush) color, (Pen) null, new Rect(0.0, origin.Y - 6.0, this._editor.ActualWidth, 1.0));
      base.Draw(drawingContext, origin, rightToLeft, sideways);
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      FormattedText formattedText = new FormattedText("", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 14.0, (Brush) ThemeUtil.GetPrimaryColor(1.0));
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      return new TextEmbeddedObjectMetrics(this._editor.ActualWidth - 23.0, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
