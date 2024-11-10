// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.NumberedListElementGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class NumberedListElementGenerator : VisualLineElementGenerator
  {
    public static readonly Regex OrderedListPattern = new Regex("^\\s{0,40}[ ]{0,3}(\\d+)\\.(?=[ ]{1,3}\\s*)", RegexOptions.Compiled);
    private MarkDownEditor _editor;

    public NumberedListElementGenerator(MarkDownEditor markDownEditor)
    {
      this._editor = markDownEditor;
    }

    private Match GetMatch(int startOffset, out int matchOffset, out string numberText)
    {
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      Match match = NumberedListElementGenerator.OrderedListPattern.Match(text.Text, 0, Math.Min(text.Text.Length, 50));
      if (match.Success && match.Index + this.CurrentContext.VisualLine.StartOffset >= startOffset)
      {
        matchOffset = match.Index + this.CurrentContext.VisualLine.StartOffset;
        numberText = match.Value + " ";
      }
      else
      {
        matchOffset = -1;
        numberText = string.Empty;
      }
      return match;
    }

    public override int GetFirstInterestedOffset(int startOffset)
    {
      int matchOffset;
      this.GetMatch(startOffset, out matchOffset, out string _);
      return matchOffset;
    }

    public override VisualLineElement ConstructElement(int offset)
    {
      try
      {
        int matchOffset;
        string numberText;
        Match match = this.GetMatch(offset, out matchOffset, out numberText);
        if (!match.Success || matchOffset != offset)
          return (VisualLineElement) null;
        int length = match.Value.TakeWhile<char>(new Func<char, bool>(char.IsWhiteSpace)).Count<char>();
        return (VisualLineElement) new NumberedListItemElement(numberText, length, FormattedTextElement.PrepareText(TextFormatter.Create(), string.Empty, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)), ThemeUtil.GetColor("BaseColorOpacity60", (FrameworkElement) this._editor));
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
    }
  }
}
