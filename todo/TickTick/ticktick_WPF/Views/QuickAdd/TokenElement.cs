// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.TokenElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public sealed class TokenElement : FormattedTextElement
  {
    private readonly double _fontSize;
    private readonly TokenGenerator _generator;
    private readonly QuickAddToken _token;
    private string _color;

    public TokenElement(
      TextLine text,
      QuickAddToken token,
      TokenGenerator generator,
      double fontSize,
      int length,
      string color)
      : base(text, length)
    {
      this._token = token;
      this._color = color;
      this._generator = generator;
      this._fontSize = fontSize;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return (TextRun) new TagTokenRun((FormattedTextElement) this, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties, this._token, this._fontSize, this._color);
    }

    protected override void OnQueryCursor(QueryCursorEventArgs e)
    {
      e.Cursor = Cursors.Hand;
      e.Handled = true;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left || e.Handled)
        return;
      e.Handled = true;
      this._generator?.RemoveToken(this._token);
    }
  }
}
