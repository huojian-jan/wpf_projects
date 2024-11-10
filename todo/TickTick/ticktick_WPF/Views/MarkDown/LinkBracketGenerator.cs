// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LinkBracketGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class LinkBracketGenerator : VisualLineElementGenerator
  {
    private readonly ILinkTextEditor _editor;
    private readonly bool _isLeft;
    private readonly bool _isLink;

    public LinkBracketGenerator(ILinkTextEditor editor, bool isLeft, bool isLink)
    {
      this._editor = editor;
      this._isLeft = isLeft;
      this._isLink = isLink;
    }

    private void GetMatch(int startOffset, out int matchOffset)
    {
      DocumentLine lastDocumentLine = this.CurrentContext.VisualLine.LastDocumentLine;
      this.CurrentContext.GetText(startOffset, lastDocumentLine.EndOffset - startOffset);
      Dictionary<int, LinkInfo> linkNameDict = this._editor.GetLinkNameDict();
      if (linkNameDict.Any<KeyValuePair<int, LinkInfo>>())
      {
        try
        {
          foreach (KeyValuePair<int, LinkInfo> keyValuePair in linkNameDict)
          {
            if (keyValuePair.Key >= lastDocumentLine.Offset && keyValuePair.Key <= lastDocumentLine.EndOffset)
            {
              matchOffset = !this._isLink ? (this._isLeft ? keyValuePair.Key + keyValuePair.Value.Link.Length + 1 : keyValuePair.Key + keyValuePair.Value.Link.Length + 2 + keyValuePair.Value.Url.Length) : (this._isLeft ? keyValuePair.Key - 1 : keyValuePair.Key + keyValuePair.Value.Link.Length);
              if (matchOffset >= startOffset && matchOffset < lastDocumentLine.EndOffset)
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
        return offset >= this.CurrentContext.VisualLine.Document.Text.Length ? (VisualLineElement) null : (VisualLineElement) new LinkBracketTextElement(this.CurrentContext.VisualLine, this._editor);
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
    }
  }
}
