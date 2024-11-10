// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LinkUrlGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Document;
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
  public class LinkUrlGenerator : VisualLineElementGenerator
  {
    private readonly ILinkTextEditor _editor;

    public LinkUrlGenerator(ILinkTextEditor editor) => this._editor = editor;

    private void GetMatch(int startOffset, out int matchOffset)
    {
      DocumentLine lastDocumentLine = this.CurrentContext.VisualLine.LastDocumentLine;
      StringSegment text = this.CurrentContext.GetText(startOffset, lastDocumentLine.EndOffset - startOffset);
      Dictionary<int, LinkInfo> linkNameDict = this._editor.GetLinkNameDict();
      if (linkNameDict.Any<KeyValuePair<int, LinkInfo>>())
      {
        foreach (KeyValuePair<int, LinkInfo> keyValuePair in linkNameDict)
        {
          if (keyValuePair.Key >= lastDocumentLine.Offset)
          {
            if (keyValuePair.Key <= lastDocumentLine.EndOffset)
            {
              try
              {
                Match match = new Regex(ticktick_WPF.Util.Utils.EscapeReg(keyValuePair.Value.Url)).Match(text.Text, text.Offset, text.Count);
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
        LinkInfo linkInfo = this._editor.GetLinkUrlDict()[offset];
        bool dark = ThemeKey.IsDarkTheme(LocalSettings.Settings.ThemeId);
        if (this._editor is FrameworkElement editor)
          dark = (editor.FindResource((object) "IsDarkTheme") as bool?).GetValueOrDefault();
        return (VisualLineElement) new LinkUrlElement(linkInfo.Url.Length, linkInfo.Url, linkInfo.Link, this.CurrentContext.VisualLine, this._editor, FormattedTextElement.PrepareText(TextFormatter.Create(), string.Empty, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)), dark);
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
    }
  }
}
