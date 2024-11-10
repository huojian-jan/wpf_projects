// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.NumberedListItemElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  internal sealed class NumberedListItemElement : FormattedTextElement
  {
    private readonly int _length;
    private readonly string _numberText;
    private SolidColorBrush _brush;

    public NumberedListItemElement(
      string numberText,
      int length,
      TextLine text,
      SolidColorBrush brush)
      : base(text, numberText.Length)
    {
      this._brush = brush;
      this._numberText = numberText;
      this._length = length;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return (TextRun) new NumberedListItemTextRun((FormattedTextElement) this, this._numberText, this._length, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties, this._brush);
    }

    public override bool IsWhitespace(int visualColumn) => visualColumn == 0;

    public override bool IsListStart() => true;
  }
}
