// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.CheckElementGenerator
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
  public class CheckElementGenerator : VisualLineElementGenerator
  {
    private static readonly Regex DefaultLinkRegex = new Regex("^\\s{0,40}- \\[[x|X]\\] .*");
    private readonly MarkDownEditor _editor;

    public CheckElementGenerator(MarkDownEditor editor) => this._editor = editor;

    private Match GetMatch(int startOffset, out int matchOffset)
    {
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      Match match = CheckElementGenerator.DefaultLinkRegex.Match(text.Text, text.Offset, text.Count);
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
          int length = match.Value.IndexOf("-", StringComparison.Ordinal);
          return (VisualLineElement) new CheckBoxElement(offset, length, FormattedTextElement.PrepareText(TextFormatter.Create(), "", (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)), this.CurrentContext, this._editor);
        }
      }
      return (VisualLineElement) null;
    }
  }
}
