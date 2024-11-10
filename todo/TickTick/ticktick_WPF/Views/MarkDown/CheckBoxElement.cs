// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.CheckBoxElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  internal sealed class CheckBoxElement : FormattedTextElement
  {
    private readonly ITextRunConstructionContext _context;
    private readonly MarkDownEditor _editor;
    private readonly int _length;
    private readonly int _offset;

    public CheckBoxElement(
      int offset,
      int length,
      TextLine text,
      ITextRunConstructionContext context,
      MarkDownEditor editor)
      : base(text, 6 + length)
    {
      this._offset = offset;
      this._context = context;
      this._length = length;
      this._editor = editor;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return (TextRun) new CheckBoxTextRun((FormattedTextElement) this, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties, this._length, (this._editor.FindResource((object) "IsDarkTheme") as bool?).GetValueOrDefault());
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left)
        return;
      this._editor.UnRegisterCaretChanged();
      if (this._offset + this._length + 3 <= this._context.Document.TextLength - 1)
        this._context.Document.Replace(this._offset + this._length + 3, 1, " ");
      else
        this._editor.Redraw();
      e.Handled = true;
      this._editor.RegisterCaretChanged();
    }

    protected override void OnQueryCursor(QueryCursorEventArgs e)
    {
      e.Handled = true;
      e.Cursor = Cursors.Hand;
    }

    public override bool IsWhitespace(int visualColumn) => visualColumn == 0;

    public override bool IsListStart() => true;
  }
}
