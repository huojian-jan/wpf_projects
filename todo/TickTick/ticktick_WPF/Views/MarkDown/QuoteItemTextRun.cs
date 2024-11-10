// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.QuoteItemTextRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class QuoteItemTextRun : FormattedTextRun
  {
    public QuoteItemTextRun(FormattedTextElement element, TextRunProperties properties)
      : base(element, properties)
    {
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      TextEmbeddedObjectMetrics embeddedObjectMetrics = base.Format(remainingParagraphWidth);
      return new TextEmbeddedObjectMetrics(15.0, embeddedObjectMetrics.Height, embeddedObjectMetrics.Baseline);
    }
  }
}
