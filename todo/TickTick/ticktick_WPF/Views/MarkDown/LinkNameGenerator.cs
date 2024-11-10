// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LinkNameGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class LinkNameGenerator : VisualLineElementGenerator
  {
    private readonly ILinkTextEditor _editor;
    private readonly bool _showUnderLine;

    public LinkNameGenerator(ILinkTextEditor editor, bool showUnderLine = false)
    {
      this._editor = editor;
      this._showUnderLine = showUnderLine;
    }

    private void GetMatch(int startOffset, out int matchOffset)
    {
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      Dictionary<int, LinkInfo> linkNameDict = this._editor.GetLinkNameDict();
      if (linkNameDict.Any<KeyValuePair<int, LinkInfo>>())
      {
        foreach (KeyValuePair<int, LinkInfo> keyValuePair in linkNameDict)
        {
          if (keyValuePair.Key >= startOffset)
          {
            if (keyValuePair.Key <= endOffset)
            {
              try
              {
                Match match = new Regex(ticktick_WPF.Util.Utils.EscapeReg(keyValuePair.Value.Link)).Match(text.Text, text.Offset, text.Count);
                if (match.Success)
                {
                  matchOffset = match.Index - text.Offset + startOffset;
                  return;
                }
              }
              catch (Exception ex)
              {
              }
            }
          }
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
        LinkInfo linkInfo = this._editor.GetLinkNameDict()[offset];
        return (VisualLineElement) new LinkUrlTextElement(this.CurrentContext.VisualLine, linkInfo.Link.Length, linkInfo.Url, this._editor, this._showUnderLine);
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
    }
  }
}
