// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.DateTokenTextRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public sealed class DateTokenTextRun : FormattedTextRun
  {
    private readonly double _fontSize;
    private readonly string _token;
    private string _color;

    public DateTokenTextRun(
      FormattedTextElement element,
      TextRunProperties properties,
      string token,
      double fontSize,
      string color)
      : base(element, properties)
    {
      this._color = color;
      this._token = token;
      this._fontSize = fontSize;
    }

    public override void Draw(
      DrawingContext drawingContext,
      Point origin,
      bool rightToLeft,
      bool sideways)
    {
      FormattedText formattedText = new FormattedText(this._token, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), this._fontSize, (Brush) ThemeUtil.GetAlphaColor(this._color, 100));
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      drawingContext.DrawText(new FormattedText(this._token, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), this._fontSize, (Brush) ThemeUtil.GetAlphaColor(this._color, 100)), new Point(origin.X + 3.0, Math.Max(0.0, origin.Y - this._fontSize) + 0.5));
      drawingContext.DrawRoundedRectangle((Brush) ThemeUtil.GetAlphaColor(this._color, 30), (Pen) null, new Rect(origin.X, Math.Max(0.0, origin.Y - this._fontSize), embeddedObjectMetrics.Width + 6.0, embeddedObjectMetrics.Height), 2.0, 2.0);
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      FormattedText formattedText = new FormattedText(this._token, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), this._fontSize, (Brush) ThemeUtil.GetAlphaColor(this._color, 100));
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      return new TextEmbeddedObjectMetrics(embeddedObjectMetrics.Width + 7.0, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
