// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LinkUrlElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class LinkUrlElement : FormattedTextElement
  {
    private readonly ILinkTextEditor _editor;
    private readonly bool _isTaskLink;
    private readonly VisualLine _line;
    private readonly string _name;
    private readonly string _url;
    private bool _dark;

    public LinkUrlElement(
      int length,
      string url,
      string title,
      VisualLine parentVisualLine,
      ILinkTextEditor editor,
      TextLine text,
      bool dark)
      : base(text, length)
    {
      this._dark = dark;
      this._url = url;
      this._name = title;
      this._line = parentVisualLine;
      this._editor = editor;
      this._isTaskLink = TaskUtils.ParseTaskUrlWithoutTitle(this._url) != null;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return (TextRun) new LinkUrlTextRun((FormattedTextElement) this, this._isTaskLink, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties, this._dark);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left)
        return;
      e.Handled = true;
      this._editor.ShowInsertLink(this._name, this._url, this._line, false);
    }

    protected override void OnQueryCursor(QueryCursorEventArgs e)
    {
      e.Cursor = Cursors.Hand;
      e.Handled = true;
    }
  }
}
