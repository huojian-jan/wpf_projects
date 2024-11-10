// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.DateTokenElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public sealed class DateTokenElement : FormattedTextElement
  {
    private readonly double _fontSize;
    private readonly DateTokenGenerator _generator;
    private readonly string _token;

    public DateTokenElement(
      TextLine text,
      string token,
      DateTokenGenerator generator,
      double fontSize,
      int length)
      : base(text, length)
    {
      this._token = token;
      this._generator = generator;
      this._fontSize = fontSize;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return (TextRun) new DateTokenTextRun((FormattedTextElement) this, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties, this._token, this._fontSize, this._generator.GetColor());
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
      this._generator?.AddIgnoreToken(this._token);
    }
  }
}
