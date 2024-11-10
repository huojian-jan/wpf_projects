// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LinkBracketTextElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class LinkBracketTextElement : VisualLineText
  {
    private ILinkTextEditor _editor;

    public LinkBracketTextElement(VisualLine parentVisualLine, ILinkTextEditor editor, int length = 1)
      : base(parentVisualLine, length)
    {
      this._editor = editor;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      this.TextRunProperties.SetForegroundBrush(this._editor.GetBracketColor());
      return base.CreateTextRun(startVisualColumn, context);
    }
  }
}
