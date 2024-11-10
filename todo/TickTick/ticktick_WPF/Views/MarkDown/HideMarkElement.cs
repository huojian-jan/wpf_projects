// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.HideMarkElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class HideMarkElement : FormattedTextElement
  {
    private double _width;

    public HideMarkElement(int length, double width, TextLine text)
      : base(text, length)
    {
      this._width = width;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return (TextRun) new HideMarkTextRun(this._width, (FormattedTextElement) this, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties);
    }
  }
}
