// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.HideLinkBracketGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class HideLinkBracketGenerator : VisualLineElementGenerator
  {
    private readonly FrameworkElement _element;
    private readonly bool _isLeft;
    private Dictionary<int, LinkInfo> _dict;

    public HideLinkBracketGenerator(
      FrameworkElement element,
      Dictionary<int, LinkInfo> nameDict,
      bool isLeft)
    {
      this._element = element;
      this._isLeft = isLeft;
      this._dict = nameDict;
    }

    private void GetMatch(int startOffset, out int matchOffset)
    {
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      if (this._dict.Any<KeyValuePair<int, LinkInfo>>())
      {
        try
        {
          foreach (KeyValuePair<int, LinkInfo> keyValuePair in this._dict)
          {
            string str1 = ticktick_WPF.Util.Utils.EscapeReg(keyValuePair.Value.Link);
            string str2 = ticktick_WPF.Util.Utils.EscapeReg(keyValuePair.Value.Url);
            Match match = new Regex(this._isLeft ? "\\[" + str1 : "\\]\\(" + str2 + "\\)").Match(text.Text, text.Offset, text.Count);
            if (match.Success)
            {
              matchOffset = match.Index - text.Offset + startOffset;
              return;
            }
          }
        }
        catch (Exception ex)
        {
        }
      }
      matchOffset = -1;
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
        LinkInfo linkInfo = (LinkInfo) null;
        foreach (KeyValuePair<int, LinkInfo> keyValuePair in this._dict)
        {
          if (this._isLeft && keyValuePair.Key - 1 == offset)
          {
            linkInfo = keyValuePair.Value;
            break;
          }
          if (!this._isLeft && offset - keyValuePair.Value.Link.Length == keyValuePair.Key)
          {
            linkInfo = keyValuePair.Value;
            break;
          }
        }
        return (VisualLineElement) new LinkElement(this._isLeft ? 1 : linkInfo?.Url?.Length.GetValueOrDefault() + 3, FormattedTextElement.PrepareText(TextFormatter.Create(), string.Empty, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)));
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
    }
  }
}
