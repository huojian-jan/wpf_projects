// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.IndentListElementGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Text.RegularExpressions;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class IndentListElementGenerator : VisualLineElementGenerator
  {
    private readonly Regex _defaultLinkRegex = new Regex("^\\t\\*\\ .*");

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
      try
      {
        int matchOffset;
        return this.GetMatch(offset, out matchOffset).Success && matchOffset == offset ? (VisualLineElement) new IndentListItemElement(FormattedTextElement.PrepareText(TextFormatter.Create(), "", (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties))) : (VisualLineElement) null;
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
    }
  }
}
