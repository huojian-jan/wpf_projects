// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.QuoteElementGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System.Text.RegularExpressions;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class QuoteElementGenerator : VisualLineElementGenerator
  {
    private readonly Regex _defaultLinkRegex = new Regex("^> .*");

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
      int matchOffset;
      if (this.GetMatch(offset, out matchOffset).Success && matchOffset == offset)
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
          return (VisualLineElement) new QuoteItemElement(FormattedTextElement.PrepareText(TextFormatter.Create(), string.Empty, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)));
      }
      return (VisualLineElement) null;
    }
  }
}
