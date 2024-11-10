// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.TagTokenRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public sealed class TagTokenRun : FormattedTextRun
  {
    private readonly double _fontSize;
    private readonly QuickAddToken _token;
    private string _color;

    public TagTokenRun(
      FormattedTextElement element,
      TextRunProperties properties,
      QuickAddToken token,
      double fontSize,
      string color)
      : base(element, properties)
    {
      this._token = token;
      this._color = color;
      this._fontSize = fontSize;
    }

    public override void Draw(
      DrawingContext drawingContext,
      Point origin,
      bool rightToLeft,
      bool sideways)
    {
      FormattedText formattedText = new FormattedText(this._token.Value, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), this._fontSize, (Brush) ThemeUtil.GetAlphaColor(this._color, 100));
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      drawingContext.DrawText(new FormattedText(this._token.Value, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), this._fontSize, (Brush) ThemeUtil.GetAlphaColor(this._color, 100)), new Point(origin.X + 4.0, 0.5));
      drawingContext.DrawRoundedRectangle((Brush) ThemeUtil.GetAlphaColor(this._color, 20), (Pen) null, new Rect(origin.X, 0.0, embeddedObjectMetrics.Width + 8.0, embeddedObjectMetrics.Height), 2.0, 2.0);
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      FormattedText formattedText = new FormattedText(this._token.Value, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), this._fontSize, (Brush) ThemeUtil.GetAlphaColor(this._color, 100));
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      return new TextEmbeddedObjectMetrics(embeddedObjectMetrics.Width + 9.0, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
