// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LineElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class LineElement : FormattedTextElement
  {
    private readonly TextEditor _editor;

    public LineElement(int length, TextEditor editor, TextLine text)
      : base(text, length)
    {
      this._editor = editor;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return (TextRun) new LineTextRun((FormattedTextElement) this, this._editor, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties);
    }
  }
}
