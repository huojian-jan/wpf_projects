// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.ListElementGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class ListElementGenerator : VisualLineElementGenerator
  {
    private readonly Regex _defaultLinkRegex = new Regex("^\\s{0,40}[-\\*\\+] .*", RegexOptions.Compiled);
    private MarkDownEditor _editor;

    public ListElementGenerator(MarkDownEditor editor) => this._editor = editor;

    private Match GetMatch(int startOffset, out int matchOffset)
    {
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      Match match = this._defaultLinkRegex.Match(text.Text, text.Offset, text.Count);
      matchOffset = match.Success ? match.Index - text.Offset + startOffset : -1;
      return match;
    }

    public override int GetFirstInterestedOffset(int startOffset)
    {
      int matchOffset;
      this.GetMatch(startOffset, out matchOffset);
      return matchOffset;
    }

    public override VisualLineElement ConstructElement(int offset)
    {
      return this.ConstructVisualLineElement(offset);
    }

    private VisualLineElement ConstructVisualLineElement(int offset)
    {
      int matchOffset;
      Match match = this.GetMatch(offset, out matchOffset);
      if (match.Success && matchOffset == offset)
      {
        bool flag = false;
        if (offset == 0)
        {
          flag = true;
        }
        else
        {
          switch (this.CurrentContext.Document.Text[offset - 1])
          {
            case '\n':
            case '\r':
              flag = true;
              break;
          }
        }
        if (flag)
        {
          int length = match.Value.TakeWhile<char>(new Func<char, bool>(char.IsWhiteSpace)).Count<char>();
          return (VisualLineElement) new ListItemElement(length, length + 2, FormattedTextElement.PrepareText(TextFormatter.Create(), string.Empty, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)), this._editor);
        }
      }
      return (VisualLineElement) null;
    }
  }
}
