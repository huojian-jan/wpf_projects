// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.StringExtensions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public static class StringExtensions
  {
    private static Regex _numRegex = new Regex("(^0x[\\da-fA-F]+$|^([+-]?(?:\\d+(?:\\.\\d*)?|\\.\\d+)(?:[eE][+-]?\\d+)?(?!\\.\\d+)(?=\\D|\\s|$))|\\d+)");
    private static Regex _zeroRegex = new Regex("^0+");

    public static List<TitleChunk> CompareNumber(this string text)
    {
      text = text.Trim();
      MatchCollection matchCollection = StringExtensions._numRegex.Matches(text);
      List<TitleChunk> titleChunkList = new List<TitleChunk>();
      int startIndex = 0;
      if (matchCollection.Count > 0)
      {
        for (int i = 0; i < matchCollection.Count; ++i)
        {
          Match match = matchCollection[i];
          if (match.Index > startIndex)
          {
            string text1 = text.Substring(startIndex, match.Index - startIndex);
            titleChunkList.Add(new TitleChunk(text1, false, 0));
          }
          string text2 = StringExtensions._zeroRegex.Replace(match.Value, "");
          titleChunkList.Add(new TitleChunk(text2, true, match.Length));
          startIndex = match.Index + match.Length;
        }
        if (text.Length > startIndex)
        {
          string text3 = text.Substring(startIndex, text.Length - startIndex);
          titleChunkList.Add(new TitleChunk(text3, false, 0));
        }
      }
      else
        titleChunkList.Add(new TitleChunk(text, false, 0));
      return titleChunkList;
    }

    public static List<string> SplitByStr(this string text, string sep)
    {
      if (string.IsNullOrEmpty(text))
        return new List<string>() { text };
      List<string> stringList = new List<string>();
      do
      {
        int length = text.IndexOf(sep, StringComparison.Ordinal);
        if (length >= 0)
        {
          stringList.Add(text.Substring(0, length));
          text = length + sep.Length < text.Length ? text.Substring(length + sep.Length) : (string) null;
        }
        else
        {
          stringList.Add(text);
          text = (string) null;
        }
      }
      while (!string.IsNullOrEmpty(text));
      return stringList;
    }

    public static string UpCaseFirst(this string text)
    {
      return string.IsNullOrEmpty(text) ? text : text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
    }

    public static string PaddingWith(this string text, string quote)
    {
      if (!text.Contains("\r\n"))
        return quote + text;
      string[] strArray = text.Replace("\r\n", "\n").Split('\n');
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < strArray.Length; ++index)
      {
        string str = strArray[index];
        if (!string.IsNullOrEmpty(str))
        {
          stringBuilder.Append(quote + str);
          if (index < strArray.Length - 1)
            stringBuilder.Append("\r\n");
        }
      }
      return stringBuilder.ToString();
    }

    public static string UnPaddingWith(this string text, string quote)
    {
      if (text.Contains("\r\n"))
      {
        string[] strArray = text.Replace("\r\n", "\n").Split('\n');
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < strArray.Length; ++index)
        {
          string str = strArray[index];
          if (!string.IsNullOrEmpty(str))
          {
            stringBuilder.Append(str.TrimStart(quote.ToCharArray()));
            if (index < strArray.Length - 1)
              stringBuilder.Append("\r\n");
          }
        }
        return stringBuilder.ToString();
      }
      return quote != null ? text.TrimStart(quote.ToCharArray()) : text;
    }

    public static string SurroundWith(this string text, string quote)
    {
      if (text.Contains("\r\n"))
        text = text.Replace("\r\n", "\n");
      if (!text.Contains("\n"))
        return quote + text.Trim() + quote;
      string[] strArray = text.Replace("\r\n", "\n").Split('\n');
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < strArray.Length; ++index)
      {
        string str = strArray[index];
        if (!string.IsNullOrEmpty(str))
        {
          stringBuilder.Append(quote + str.Trim() + quote);
          if (index < strArray.Length - 1)
            stringBuilder.Append("\n");
        }
        else
          stringBuilder.Append("\n");
      }
      return stringBuilder.ToString();
    }

    public static string UnSurroundWith(this string text, string quote)
    {
      if (string.IsNullOrEmpty(quote))
        return text;
      if (text.Contains("\r\n"))
        text = text.Replace("\r\n", "\n");
      if (text.Contains("\n"))
      {
        string[] strArray = text.Split('\n');
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < strArray.Length; ++index)
        {
          string str = strArray[index];
          if (!string.IsNullOrEmpty(str))
          {
            stringBuilder.Append(str.Trim(quote.ToCharArray()));
            if (index < strArray.Length - 1)
              stringBuilder.Append("\n");
          }
          else
            stringBuilder.Append("\n");
        }
        return stringBuilder.ToString();
      }
      if (text.StartsWith(quote) && text.EndsWith(quote))
      {
        text = text.Substring(quote.Length);
        if (text.Length >= quote.Length)
          text = text.Substring(0, text.Length - quote.Length);
      }
      return text;
    }

    public static string ReplaceSmartChars(this string smart)
    {
      return smart.Replace('–', '-').Replace('—', '-').Replace('―', '-').Replace('‗', '_').Replace('‘', '\'').Replace('’', '\'').Replace('‚', ',').Replace('‛', '\'').Replace('“', '"').Replace('”', '"').Replace('„', '"').Replace("…", "...").Replace('′', '\'').Replace('″', '"');
    }

    public static int WordCount(this string text)
    {
      return text != null ? Regex.Matches(text, "[\\S]+").Count : 0;
    }

    public static string AddOffsetToFileName(this string file, int offset)
    {
      return string.Format("{0}|{1}", (object) file.StripOffsetFromFileName(), (object) offset);
    }

    public static string ReplaceDate(this string text)
    {
      return new Regex("\\$DATE(?:\\(\"(.+)\"\\))?\\$").Replace(text, (MatchEvaluator) (match => DateTime.Now.ToString(match?.Groups[1].Value)));
    }

    public static string StripOffsetFromFileName(this string file)
    {
      if (string.IsNullOrWhiteSpace(file))
        return file;
      int length = file.IndexOf('|');
      return length < 0 ? file : file.Substring(0, length);
    }

    public static string ToLowerInvariant(this string text)
    {
      return text.ToLower(CultureInfo.InvariantCulture);
    }

    public static string ToUpperInvariant(this string text)
    {
      return text.ToUpper(CultureInfo.InvariantCulture);
    }

    public static string ToSlug(this string value, bool toLower = false)
    {
      if (value == null)
        return "";
      string str1 = value.Normalize(NormalizationForm.FormKD);
      int length = str1.Length;
      bool flag = false;
      StringBuilder stringBuilder = new StringBuilder(length);
      for (int index = 0; index < length; ++index)
      {
        char c = str1[index];
        if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9')
        {
          if (flag)
          {
            stringBuilder.Append('-');
            flag = false;
          }
          stringBuilder.Append(c);
        }
        else if (c >= 'A' && c <= 'Z')
        {
          if (flag)
          {
            stringBuilder.Append('-');
            flag = false;
          }
          if (toLower)
            stringBuilder.Append((char) ((uint) c | 32U));
          else
            stringBuilder.Append(c);
        }
        else if (c == ' ' || c == ',' || c == '.' || c == '/' || c == '\\' || c == '-' || c == '_' || c == '=')
        {
          if (!flag && stringBuilder.Length > 0)
            flag = true;
        }
        else
        {
          string str2 = StringExtensions.ConvertEdgeCases(c, toLower);
          if (str2 != null)
          {
            if (flag)
            {
              stringBuilder.Append('-');
              flag = false;
            }
            stringBuilder.Append(str2);
          }
        }
        if (stringBuilder.Length == 80)
          break;
      }
      return stringBuilder.ToString();
    }

    private static string ConvertEdgeCases(char c, bool toLower)
    {
      string str = (string) null;
      switch (c)
      {
        case 'Þ':
          str = "th";
          break;
        case 'ß':
          str = "ss";
          break;
        case 'ø':
          str = "o";
          break;
        case 'đ':
          str = "d";
          break;
        case 'ı':
          str = "i";
          break;
        case 'Ł':
          str = toLower ? "l" : "L";
          break;
        case 'ł':
          str = "l";
          break;
      }
      return str;
    }
  }
}
