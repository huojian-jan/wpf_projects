// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.CustomLinkElementGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class CustomLinkElementGenerator : VisualLineElementGenerator
  {
    private readonly Regex _linkRegex;
    private ILinkTextEditor _editor;
    private bool _ignoreUrlDict;

    public CustomLinkElementGenerator(ILinkTextEditor editor, bool ignoreUrlDict = false)
    {
      this._editor = editor;
      this._linkRegex = new Regex("(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]?\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]\\.[^\\s]{2,})");
      this._ignoreUrlDict = ignoreUrlDict;
    }

    private Match GetMatch(int startOffset, out int matchOffset)
    {
      if (this._editor == null)
      {
        matchOffset = -1;
        return (Match) null;
      }
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      string input = text.Text;
      Dictionary<int, LinkInfo> linkUrlDict = this._ignoreUrlDict ? (Dictionary<int, LinkInfo>) null : this._editor.GetLinkUrlDict();
      if (linkUrlDict != null && linkUrlDict.Any<KeyValuePair<int, LinkInfo>>())
      {
        foreach (KeyValuePair<int, LinkInfo> keyValuePair in linkUrlDict)
        {
          int startIndex = keyValuePair.Key - (endOffset - input.Length);
          if (keyValuePair.Key >= startOffset && startIndex + keyValuePair.Value.Url.Length <= input.Length)
            input = input.Remove(startIndex, keyValuePair.Value.Url.Length).Insert(startIndex, CustomLinkElementGenerator.BuildEmptyString(keyValuePair.Value.Url.Length));
        }
      }
      Match match = this._linkRegex.Match(input, text.Offset, text.Count);
      matchOffset = match.Success ? match.Index - text.Offset + startOffset : -1;
      return match;
    }

    private static string BuildEmptyString(int length)
    {
      StringBuilder stringBuilder = new StringBuilder(length);
      for (int index = 0; index < length; ++index)
        stringBuilder.Append(" ");
      return stringBuilder.ToString();
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
      return match.Success && matchOffset == offset ? this.ConstructElementFromMatch((Capture) match) : (VisualLineElement) null;
    }

    private VisualLineElement ConstructElementFromMatch(Capture m)
    {
      string uri;
      Uri uriFromMatch = CustomLinkElementGenerator.GetUriFromMatch(m, out uri);
      if (!(uriFromMatch == (Uri) null))
      {
        if (this._editor != null)
        {
          try
          {
            LineLinkText lineLinkText = new LineLinkText(this.CurrentContext.VisualLine, uri, this._editor);
            lineLinkText.NavigateUri = uriFromMatch;
            lineLinkText.RequireControlModifierForClick = false;
            return (VisualLineElement) lineLinkText;
          }
          catch (Exception ex)
          {
            return (VisualLineElement) null;
          }
        }
      }
      return (VisualLineElement) null;
    }

    private static Uri GetUriFromMatch(Capture match, out string uri)
    {
      string uriString = match.Value.TrimEnd("!\"#$%&'()*+,-.@:;<=>[\\]^_`{|}~".ToCharArray());
      uri = uriString;
      if (uriString.StartsWith("www.", StringComparison.Ordinal))
        uriString = "http://" + uriString;
      return !Uri.IsWellFormedUriString(uriString, UriKind.Absolute) ? (Uri) null : new Uri(uriString);
    }
  }
}
