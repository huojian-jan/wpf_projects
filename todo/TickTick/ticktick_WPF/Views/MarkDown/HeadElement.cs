// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.HeadElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class HeadElement : FormattedTextElement
  {
    private readonly int _flagLength;

    public HeadElement(int flagLength, TextLine text)
      : base(text, flagLength)
    {
      this._flagLength = flagLength;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return (TextRun) new HeadTextRun((FormattedTextElement) this, this._flagLength, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties);
    }
  }
}
