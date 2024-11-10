// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.EmojiTextRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class EmojiTextRun : FormattedTextRun
  {
    private BitmapSource _bitmap;
    private double _scale;
    private double _fontsize;

    public EmojiTextRun(FormattedTextElement element, string emoji, TextRunProperties properties)
      : base(element, properties)
    {
      this._fontsize = properties.FontRenderingEmSize;
      this._scale = Math.Pow(2.0, Math.Max(0.0, Math.Ceiling(5.0 - Math.Log(this._fontsize, 2.0))));
      this._bitmap = EmojiRenderer.RenderBitmap(emoji ?? "", this._fontsize * this._scale, properties.ForegroundBrush);
    }

    public override void Draw(
      DrawingContext drawingContext,
      Point origin,
      bool rightToLeft,
      bool sideways)
    {
      int num = this._fontsize >= 16.0 ? 2 : 0;
      Point location = new Point(origin.X, origin.Y - this._fontsize - (double) num);
      drawingContext.DrawImage((ImageSource) this._bitmap, new Rect(location, new Size(this._bitmap.Width / this._scale, this._bitmap.Height / this._scale)));
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      FormattedText formattedText = new FormattedText(string.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 14.0, (Brush) ThemeUtil.GetPrimaryColor(1.0));
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      return new TextEmbeddedObjectMetrics(this._bitmap.Width / this._scale, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
