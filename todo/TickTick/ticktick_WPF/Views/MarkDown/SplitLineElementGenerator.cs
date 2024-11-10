// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.SplitLineElementGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System.Text.RegularExpressions;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class SplitLineElementGenerator : VisualLineElementGenerator
  {
    private static readonly Regex DefaultLinkRegex = new Regex("^[-|\\*|_|\\+]{3,}$");
    private readonly TextEditor _editor;

    public SplitLineElementGenerator(TextEditor editBox) => this._editor = editBox;

    private Match GetMatch(int startOffset, out int matchOffset, out int length)
    {
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      if (text.Text != "****")
      {
        Match match = SplitLineElementGenerator.DefaultLinkRegex.Match(text.Text, text.Offset, text.Count);
        matchOffset = match.Success ? match.Index - text.Offset + startOffset : -1;
        length = match.Success ? match.Length : -1;
        return match;
      }
      matchOffset = -1;
      length = 0;
      return SplitLineElementGenerator.DefaultLinkRegex.Match(string.Empty);
    }

    public override int GetFirstInterestedOffset(int startOffset)
    {
      int matchOffset;
      this.GetMatch(startOffset, out matchOffset, out int _);
      return matchOffset;
    }

    public override VisualLineElement ConstructElement(int offset)
    {
      int matchOffset;
      int length;
      return this.GetMatch(offset, out matchOffset, out length).Success && matchOffset == offset ? (VisualLineElement) new LineElement(length, this._editor, FormattedTextElement.PrepareText(TextFormatter.Create(), string.Empty, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties))) : (VisualLineElement) null;
    }
  }
}
