// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.HideMarkTextRun
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
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class HideMarkTextRun : FormattedTextRun
  {
    private double _width;

    public HideMarkTextRun(
      double width,
      FormattedTextElement element,
      TextRunProperties properties)
      : base(element, properties)
    {
      this._width = width;
    }

    public override void Draw(
      DrawingContext drawingContext,
      Point origin,
      bool rightToLeft,
      bool sideways)
    {
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      FormattedText formattedText = new FormattedText(string.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), this._width, (Brush) ThemeUtil.GetPrimaryColor(0.0));
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      return new TextEmbeddedObjectMetrics(this._width, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
