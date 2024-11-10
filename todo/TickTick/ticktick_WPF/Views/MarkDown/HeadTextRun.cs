// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.HeadTextRun
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
  public sealed class HeadTextRun : FormattedTextRun
  {
    private readonly int _flagLength;

    public HeadTextRun(FormattedTextElement element, int flagLength, TextRunProperties properties)
      : base(element, properties)
    {
      this._flagLength = flagLength;
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      FormattedText formattedText = new FormattedText("#", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 14.0 * HeadTextRun.GetHeadingMagnify(this._flagLength - 1), (Brush) null);
      TextEmbeddedObjectMetrics embeddedObjectMetrics = new TextEmbeddedObjectMetrics(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height, formattedText.Baseline);
      return new TextEmbeddedObjectMetrics(0.0, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }

    private static double GetHeadingMagnify(int level)
    {
      switch (level)
      {
        case 1:
          return 1.28;
        case 2:
          return 1.14;
        case 3:
          return 1.0;
        case 4:
          return 1.0;
        case 5:
          return 1.0;
        case 6:
          return 1.0;
        default:
          return 1.28;
      }
    }
  }
}
