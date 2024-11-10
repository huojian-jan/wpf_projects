// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.MarkDownTagMargin
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class MarkDownTagMargin : AbstractMargin
  {
    private readonly TextArea _area;
    private readonly double _width;
    private MarkDownEditor _editor;

    public MarkDownTagMargin(TextArea area, MarkDownEditor editor, double width = 12.0)
    {
      this._area = area;
      this._editor = editor;
      this._width = width;
    }

    protected override Size MeasureOverride(Size availableSize) => new Size(this._width, 0.0);

    protected override void OnRender(DrawingContext drawingContext)
    {
      TextView textView = this.TextView;
      if (this._area.Caret.Offset <= 0 || textView == null || !textView.VisualLinesValid)
        return;
      int index = textView.Document.GetLineByOffset(this._area.Caret.Offset).LineNumber - 1;
      VisualLine visualLine = textView.VisualLines[index];
      if (this._editor.BetweenCode(visualLine.StartOffset))
        return;
      string checkText = visualLine.Document.Text.Substring(visualLine.StartOffset, visualLine.FirstDocumentLine.Length);
      this.CheckDrawHeading(drawingContext, checkText, visualLine, (IScrollInfo) textView, this._width);
    }

    private void CheckDrawHeading(
      DrawingContext drawingContext,
      string checkText,
      VisualLine line,
      IScrollInfo textView,
      double width)
    {
      if (checkText.StartsWith("# "))
        this.DrawHeading(drawingContext, line, textView, 1, width);
      else if (checkText.StartsWith("## "))
        this.DrawHeading(drawingContext, line, textView, 2, width);
      else if (checkText.StartsWith("### "))
        this.DrawHeading(drawingContext, line, textView, 3, width);
      else if (checkText.StartsWith("#### "))
        this.DrawHeading(drawingContext, line, textView, 4, width);
      else if (checkText.StartsWith("##### "))
      {
        this.DrawHeading(drawingContext, line, textView, 5, width);
      }
      else
      {
        if (!checkText.StartsWith("###### "))
          return;
        this.DrawHeading(drawingContext, line, textView, 6, width);
      }
    }

    private void DrawHeading(
      DrawingContext drawingContext,
      VisualLine line,
      IScrollInfo textView,
      int level,
      double width)
    {
      SolidColorBrush color = ThemeUtil.GetColor("BaseColorOpacity40", (FrameworkElement) this._editor);
      FormattedText formattedText = new FormattedText("H", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(Constants.DefaultFont), 9.0, (Brush) color);
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop) - textView.VerticalOffset + embeddedObjectMetrics.Height * 0.25;
      double x = (width - embeddedObjectMetrics.Width) / 2.0;
      drawingContext.DrawText(new FormattedText("H", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 8.0, (Brush) color), new Point(x, y));
      drawingContext.DrawText(new FormattedText(level.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 7.0, (Brush) color), new Point(x + 7.0, y + 1.0));
    }
  }
}
