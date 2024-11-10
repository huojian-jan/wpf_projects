// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.EmojiGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class EmojiGenerator : VisualLineElementGenerator
  {
    private readonly IEmojiRender _editor;

    public EmojiGenerator(IEmojiRender editor) => this._editor = editor;

    private void GetMatch(int startOffset, out int matchOffset)
    {
      DocumentLine lastDocumentLine = this.CurrentContext.VisualLine.LastDocumentLine;
      if (lastDocumentLine == null)
      {
        matchOffset = -1;
      }
      else
      {
        Dictionary<int, string> emojiDict = this._editor.GetEmojiDict();
        if (emojiDict.Any<KeyValuePair<int, string>>())
        {
          foreach (KeyValuePair<int, string> keyValuePair in emojiDict)
          {
            if (keyValuePair.Value != null && keyValuePair.Key >= startOffset && lastDocumentLine.EndOffset >= keyValuePair.Key + keyValuePair.Value.Length && keyValuePair.Value.Length > 0)
            {
              matchOffset = keyValuePair.Key;
              return;
            }
          }
        }
        matchOffset = -1;
      }
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
        if (this._editor.GetEmojiDict().ContainsKey(offset))
        {
          string emoji = this._editor.GetEmojiDict()[offset];
          DocumentLine lastDocumentLine = this.CurrentContext.VisualLine.LastDocumentLine;
          return offset + emoji.Length > lastDocumentLine.EndOffset ? (VisualLineElement) null : (VisualLineElement) new EmojiElement(emoji, FormattedTextElement.PrepareText(TextFormatter.Create(), string.Empty, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)));
        }
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
      return (VisualLineElement) null;
    }
  }
}
