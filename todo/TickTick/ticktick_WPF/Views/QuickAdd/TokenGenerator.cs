// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.TokenGenerator
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
  public class TokenGenerator : VisualLineElementGenerator
  {
    private static Regex _defaultLinkRegex;
    private readonly List<QuickAddToken> _tokens = new List<QuickAddToken>();
    private readonly List<string> _removeTokens = new List<string>();
    private readonly QuickAddText _quickAddText;
    public static List<string> PriorityText = new List<string>()
    {
      "!" + ticktick_WPF.Util.Utils.GetPriorityName(0),
      "!" + ticktick_WPF.Util.Utils.GetPriorityName(1),
      "!" + ticktick_WPF.Util.Utils.GetPriorityName(3),
      "!" + ticktick_WPF.Util.Utils.GetPriorityName(5),
      "！" + ticktick_WPF.Util.Utils.GetPriorityName(0),
      "！" + ticktick_WPF.Util.Utils.GetPriorityName(1),
      "！" + ticktick_WPF.Util.Utils.GetPriorityName(3),
      "！" + ticktick_WPF.Util.Utils.GetPriorityName(5),
      "!4",
      "!3",
      "!2",
      "!1",
      "！4",
      "！3",
      "！2",
      "！1"
    };

    public Regex PriorityRegex { get; }

    public TokenGenerator(QuickAddText quickAddText)
    {
      this._quickAddText = quickAddText;
      string seed = "";
      string str = TokenGenerator.PriorityText.Aggregate<string, string>(seed, (Func<string, string, string>) ((current, p) => current + "(" + p + " )|"));
      this.PriorityRegex = new Regex(str.Substring(0, str.Length - 1));
    }

    public event EventHandler<QuickAddToken> TokenRemoved;

    public QuickAddToken TryAddToken(QuickAddToken token)
    {
      QuickAddToken quickAddToken = (QuickAddToken) null;
      if (token.Exclusive)
      {
        quickAddToken = this._tokens.FirstOrDefault<QuickAddToken>((Func<QuickAddToken, bool>) (t => t.TokenType == token.TokenType));
        this._tokens.RemoveAll((Predicate<QuickAddToken>) (item => item.TokenType == token.TokenType));
      }
      if (!this._tokens.Exists((Predicate<QuickAddToken>) (item => item.TokenType == token.TokenType && item.Value == token.Value)))
      {
        this._tokens.Add(token);
        this._tokens.Sort((Comparison<QuickAddToken>) ((a, b) => b.Value.Length.CompareTo(a.Value.Length)));
      }
      return quickAddToken;
    }

    public bool TryAddTagToken(QuickAddToken token)
    {
      if ((this._tokens.Exists((Predicate<QuickAddToken>) (item => item.TokenType == token.TokenType && string.Equals(item.Value, token.Value, StringComparison.CurrentCultureIgnoreCase))) ? 1 : (this._removeTokens.Exists((Predicate<string>) (item => string.Equals(item, token.Value, StringComparison.CurrentCultureIgnoreCase))) ? 1 : 0)) != 0)
        return false;
      this._tokens.Add(token);
      this._tokens.Sort((Comparison<QuickAddToken>) ((a, b) => b.Value.Length.CompareTo(a.Value.Length)));
      return true;
    }

    public bool AddTokenCheckExist(QuickAddToken token)
    {
      if ((this._tokens.Exists((Predicate<QuickAddToken>) (item => item.TokenType == token.TokenType && string.Equals(item.Value, token.Value, StringComparison.CurrentCultureIgnoreCase))) ? 1 : (this._removeTokens.Exists((Predicate<string>) (item => string.Equals(item, token.Value, StringComparison.CurrentCultureIgnoreCase))) ? 1 : 0)) != 0)
        return false;
      this._tokens.Add(token);
      return true;
    }

    public IEnumerable<QuickAddToken> GetTokens() => (IEnumerable<QuickAddToken>) this._tokens;

    public List<QuickAddToken> GetTokensByType(TokenType type)
    {
      return this._tokens.Where<QuickAddToken>((Func<QuickAddToken, bool>) (t => t.TokenType == type)).ToList<QuickAddToken>();
    }

    public void RemoveToken(QuickAddToken token, bool notify = true, bool removeTemp = true)
    {
      if (removeTemp)
        this._removeTokens.Add(token.Value);
      this._tokens.Remove(token);
      if (!notify)
        return;
      EventHandler<QuickAddToken> tokenRemoved = this.TokenRemoved;
      if (tokenRemoved == null)
        return;
      tokenRemoved((object) this, token);
    }

    public void CheckInvalidTokens(string content)
    {
      if (!this._tokens.Any<QuickAddToken>())
        return;
      foreach (QuickAddToken e in this._tokens.Where<QuickAddToken>((Func<QuickAddToken, bool>) (token => !content.Contains(token.Value))))
      {
        EventHandler<QuickAddToken> tokenRemoved = this.TokenRemoved;
        if (tokenRemoved != null)
          tokenRemoved((object) this, e);
      }
      this._tokens.RemoveAll((Predicate<QuickAddToken>) (token => !content.Contains(token.Value)));
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
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      StringSegment text = this.CurrentContext.GetText(startOffset, endOffset - startOffset);
      matchOffset = -1;
      string str = string.Empty;
      if (this._tokens.Any<QuickAddToken>())
      {
        str = this._tokens.Aggregate<QuickAddToken, string>(str, (Func<string, QuickAddToken, string>) ((current, token) => current + "(" + token.Value.Replace("^", "\\^").Replace("+", "\\+").Replace("*", "\\*").Replace(")", "\\)").Replace("(", "\\(") + ")|"));
        if (str.EndsWith("|"))
          str = str.Substring(0, str.Length - 1);
      }
      if (string.IsNullOrEmpty(text.Text) || string.IsNullOrEmpty(str))
        return (System.Text.RegularExpressions.Match) null;
      Regex regex = new Regex(str);
      int beginning = text.Offset;
      int length = text.Count;
      while (beginning < text.Text.Length)
      {
        System.Text.RegularExpressions.Match match = regex.Match(text.Text, beginning, length);
        if (!match.Success)
          return (System.Text.RegularExpressions.Match) null;
        string input = text.Text.Substring(0, startOffset);
        MatchCollection matchCollection = regex.Matches(input);
        bool flag = true;
        if (matchCollection.Count > 0)
        {
          for (int i = 0; i < matchCollection.Count; ++i)
          {
            if (matchCollection[i].Value == match.Value)
            {
              beginning = match.Index + match.Value.Length;
              length = text.Text.Length - (match.Index + match.Value.Length);
              flag = false;
              break;
            }
          }
        }
        if (flag)
        {
          matchOffset = match.Index - text.Offset + startOffset;
          return match;
        }
      }
      return (System.Text.RegularExpressions.Match) null;
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
        if (match.Success)
        {
          QuickAddToken token = this._tokens.FirstOrDefault<QuickAddToken>((Func<QuickAddToken, bool>) (val => val.Value == match.Value));
          if (token != null)
          {
            string color = this._quickAddText.GetColor();
            if (token.TokenType == TokenType.Priority)
              color = ticktick_WPF.Util.Utils.GetPriorityColor(TokenGenerator.GetPriorityByText(match.Value)).Color.ToString();
            return (VisualLineElement) new TokenElement(FormattedTextElement.PrepareText(TextFormatter.Create(), match.Value, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)), token, this, this._quickAddText.GetFontSize() - 1.0, match.Value.Length, color);
          }
        }
        return (VisualLineElement) null;
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
    }

    public void Reset() => this._tokens.Clear();

    public List<string> GetSelectedTags()
    {
      return this._tokens.Where<QuickAddToken>((Func<QuickAddToken, bool>) (token => token.TokenType == TokenType.Tag)).Select<QuickAddToken, string>((Func<QuickAddToken, string>) (token => token.Value.ToLower().Substring(1))).ToList<string>();
    }

    public List<QuickAddToken> RemoveTokenByType(TokenType tokenType)
    {
      List<QuickAddToken> list = this._tokens.Where<QuickAddToken>((Func<QuickAddToken, bool>) (item => item.TokenType == tokenType)).ToList<QuickAddToken>();
      this._tokens.RemoveAll((Predicate<QuickAddToken>) (item => item.TokenType == tokenType));
      return list;
    }

    public bool ExistToken(QuickAddToken token)
    {
      return this._tokens.Any<QuickAddToken>((Func<QuickAddToken, bool>) (t => t.TokenType == token.TokenType && t.Value == token.Value));
    }

    public bool ExistToken(TokenType type)
    {
      return this._tokens.Any<QuickAddToken>((Func<QuickAddToken, bool>) (t => t.TokenType == type));
    }

    public bool ExistRemoveToken(string token)
    {
      return this._removeTokens.Any<string>((Func<string, bool>) (t => string.Equals(t, token, StringComparison.CurrentCultureIgnoreCase)));
    }

    public List<QuickAddToken> GetTagTokens()
    {
      return this._tokens.Where<QuickAddToken>((Func<QuickAddToken, bool>) (token => token.TokenType == TokenType.Tag)).ToList<QuickAddToken>();
    }

    public void HandleRemainText(List<string> remains)
    {
      if (remains == null)
        return;
      foreach (QuickAddToken e in this._tokens.Where<QuickAddToken>((Func<QuickAddToken, bool>) (item => !remains.Any<string>((Func<string, bool>) (text => string.Equals(text, item.Value, StringComparison.CurrentCultureIgnoreCase))))).ToList<QuickAddToken>())
      {
        if (e.TokenType == TokenType.Tag)
        {
          this._tokens.Remove(e);
          EventHandler<QuickAddToken> tokenRemoved = this.TokenRemoved;
          if (tokenRemoved != null)
            tokenRemoved((object) this, e);
        }
      }
      this._removeTokens.RemoveAll((Predicate<string>) (item => remains.Count == 0 || !remains.Any<string>((Func<string, bool>) (text => string.Equals(text, item, StringComparison.CurrentCultureIgnoreCase)))));
    }

    public static int GetPriorityByText(string pri)
    {
      int num1 = TokenGenerator.PriorityText.IndexOf(pri);
      if (num1 < 0)
        return 0;
      int num2 = num1 % 4;
      return num2 != 0 ? 2 * num2 - 1 : 0;
    }
  }
}
