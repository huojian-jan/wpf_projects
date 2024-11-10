// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LinkUrlTextRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

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
  public sealed class LinkUrlTextRun : FormattedTextRun
  {
    private readonly bool _isTaskLink;
    private bool _dark;

    public LinkUrlTextRun(
      FormattedTextElement element,
      bool isTaskLink,
      TextRunProperties properties,
      bool dark)
      : base(element, properties)
    {
      this._dark = dark;
      this._isTaskLink = isTaskLink;
    }

    public override void Draw(
      DrawingContext drawingContext,
      Point origin,
      bool rightToLeft,
      bool sideways)
    {
      Point location = new Point(origin.X, origin.Y - 12.0);
      string uriString = this._dark ? "pack://application:,,,/Assets/md_link_dark.png" : "pack://application:,,,/Assets/md_link_light.png";
      if (this._isTaskLink)
        uriString = this._dark ? "pack://application:,,,/Assets/md_task_link_dark.png" : "pack://application:,,,/Assets/md_task_link_light.png";
      BitmapImage bitmapImage = new BitmapImage(new Uri(uriString));
      drawingContext.DrawImage((ImageSource) bitmapImage, new Rect(location, new Size(16.0, 16.0)));
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      FormattedText formattedText = new FormattedText(string.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 14.0, (Brush) ThemeUtil.GetPrimaryColor(1.0));
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      return new TextEmbeddedObjectMetrics(16.0, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
