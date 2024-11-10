// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.ListItemElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  internal sealed class ListItemElement : FormattedTextElement
  {
    private readonly int _length;
    private MarkDownEditor _editor;

    public ListItemElement(int length, int count, TextLine text, MarkDownEditor editor)
      : base(text, count)
    {
      this._length = length;
      this._editor = editor;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return (TextRun) new ListItemTextRun((FormattedTextElement) this, this._length, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties, this._editor);
    }

    public override bool IsWhitespace(int visualColumn) => visualColumn == 0;

    public override bool IsListStart() => true;
  }
}
