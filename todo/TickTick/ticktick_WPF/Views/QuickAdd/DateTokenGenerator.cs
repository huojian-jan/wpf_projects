// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.DateTokenGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class DateTokenGenerator : VisualLineElementGenerator
  {
    private readonly List<string> _ignoreTokens = new List<string>();
    private readonly IDateParseBox _parseTextBox;
    private readonly List<string> _tokens = new List<string>();
    private List<string> _parsedToken = new List<string>();

    public DateTokenGenerator(IDateParseBox parseTextBox) => this._parseTextBox = parseTextBox;

    public List<string> GetValidTokens()
    {
      return this._tokens.Except<string>((IEnumerable<string>) this._ignoreTokens).ToList<string>();
    }

    public void AddTokens(IEnumerable<string> tokens, string content)
    {
      List<string> parsedTokens = tokens.ToList<string>();
      if (parsedTokens.Count == this._parsedToken.Count && this._parsedToken.All<string>((Func<string, bool>) (t => parsedTokens.Contains(t))))
        return;
      this._tokens.Clear();
      this._parsedToken = parsedTokens;
      if (content == null)
        return;
      string[] strArray = new string[content.Length];
      foreach (string str in this._parsedToken)
      {
        int index = content.IndexOf(str, StringComparison.Ordinal);
        if (index >= 0 && (strArray[index] == null || strArray[index].Length < str.Length))
          strArray[index] = str;
      }
      int startIndex = 0;
      while (startIndex < strArray.Length)
      {
        string str1 = strArray[startIndex];
        if (string.IsNullOrEmpty(str1))
        {
          ++startIndex;
        }
        else
        {
          int index = startIndex + 1;
          while (index < strArray.Length && index <= startIndex + str1.Length)
          {
            string str2 = strArray[index];
            if (string.IsNullOrEmpty(str2))
            {
              ++index;
            }
            else
            {
              if (index + str2.Length <= content.Length)
                str1 = content.Substring(startIndex, index + str2.Length - startIndex);
              ++index;
            }
          }
          startIndex = index;
          this._tokens.Add(str1.Trim());
        }
      }
    }

    public void ClearTokens()
    {
      this._tokens.Clear();
      this._parsedToken.Clear();
    }

    public void Reset()
    {
      this._ignoreTokens.Clear();
      this._tokens.Clear();
      this._parsedToken.Clear();
    }

    public void AddIgnoreToken(string token)
    {
      if (this._ignoreTokens != null && !this._ignoreTokens.Contains(token))
        this._ignoreTokens.Add(token);
      this._parseTextBox?.ForceRender();
    }

    private System.Text.RegularExpressions.Match GetMatch(int startOffset, out int matchOffset)
    {
      try
      {
        return this.Match(startOffset, out matchOffset);
      }
      catch (Exception ex)
      {
        matchOffset = -1;
        return (System.Text.RegularExpressions.Match) null;
      }
    }

    private System.Text.RegularExpressions.Match Match(int startOffset, out int matchOffset)
    {
      if (!this._parseTextBox.CanParseDate)
      {
        matchOffset = -1;
        return (System.Text.RegularExpressions.Match) null;
      }
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text1 = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      matchOffset = -1;
      string str = string.Empty;
      List<string> list = this._tokens.Except<string>((IEnumerable<string>) this._ignoreTokens).ToList<string>();
      if (list.Any<string>())
      {
        str = list.Aggregate<string, string>(str, (Func<string, string, string>) ((current, token) => current + "(" + token.Replace(")", "\\)").Replace("(", "\\)") + ")|"));
        if (str.EndsWith("|"))
          str = str.Substring(0, str.Length - 1);
      }
      if (string.IsNullOrEmpty(text1.Text) || string.IsNullOrEmpty(str))
        return (System.Text.RegularExpressions.Match) null;
      System.Text.RegularExpressions.Match match = new Regex(str).Match(text1.Text, text1.Offset, text1.Count);
      if (!match.Success)
        return (System.Text.RegularExpressions.Match) null;
      string text2 = text1.Text.Substring(0, startOffset);
      if (text2.Contains(match.Value))
      {
        this._parseTextBox?.RemoveTokenText(ref text2);
        if (text2.Contains(match.Value))
          return (System.Text.RegularExpressions.Match) null;
      }
      matchOffset = match.Index - text1.Offset + startOffset;
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
        System.Text.RegularExpressions.Match match = this.GetMatch(offset, out int _);
        return (VisualLineElement) new DateTokenElement(FormattedTextElement.PrepareText(TextFormatter.Create(), match.Value, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)), match.Value, this, this._parseTextBox.GetFontSize() - 1.0, match.Value.Length);
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
    }

    public List<string> GetIgnoreTokens() => this._ignoreTokens;

    public void CheckIgnoreToken(string editBoxText)
    {
      foreach (string str in this._ignoreTokens.ToList<string>())
      {
        if (!editBoxText.Contains(str))
          this._ignoreTokens.Remove(str);
      }
    }

    public void SetIgnoreToken(List<string> tokens)
    {
      this._ignoreTokens.Clear();
      this._ignoreTokens.AddRange((IEnumerable<string>) tokens);
    }

    public string GetColor() => this._parseTextBox.GetColor();
  }
}
