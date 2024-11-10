// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Twitter.TaskTitleUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.Twitter
{
  public static class TaskTitleUtils
  {
    public static string GetLastMatchName(
      ref string title,
      List<string> tokens,
      string identifier,
      string identifierExtra)
    {
      int index = -1;
      return TaskTitleUtils.GetLastMatchName(ref title, tokens, identifier, identifierExtra, ref index);
    }

    public static string GetLastMatchName(
      string title,
      List<string> tokens,
      string identifier,
      string identifierExtra,
      ref int index)
    {
      return TaskTitleUtils.GetLastMatchName(ref title, tokens, identifier, identifierExtra, ref index);
    }

    private static string GetLastMatchName(
      ref string title,
      List<string> tokens,
      string identifier,
      string identifierExtra,
      ref int index)
    {
      string lastMatchName = (string) null;
      title = title.Trim();
      foreach (string token in tokens)
      {
        int startIndex1 = title.LastIndexOf(" " + identifier + token, StringComparison.Ordinal);
        int num1 = title.LastIndexOf(identifier + token + " ", StringComparison.Ordinal);
        if (startIndex1 != -1 || num1 != -1)
        {
          if (num1 == startIndex1 + 1 && startIndex1 > index)
          {
            index = startIndex1;
            lastMatchName = token;
          }
          if (startIndex1 == -1 && num1 == 0 && index <= 0)
          {
            index = 0;
            lastMatchName = token;
          }
          if (startIndex1 == title.Length - token.Length - 2)
          {
            title = title.Remove(startIndex1, token.Length + 2);
            index = title.Length;
            return token;
          }
        }
        if (identifierExtra != identifier)
        {
          int startIndex2 = title.LastIndexOf(" " + identifierExtra + token, StringComparison.Ordinal);
          int num2 = title.LastIndexOf(identifierExtra + token + " ", StringComparison.Ordinal);
          if (startIndex2 != -1 || num2 != -1)
          {
            if (num2 == startIndex2 + 1 && startIndex2 > index)
            {
              index = startIndex2;
              lastMatchName = token;
            }
            if (startIndex2 == -1 && num2 == 0 && index <= 0)
            {
              index = 0;
              lastMatchName = token;
            }
            if (startIndex2 == title.Length - token.Length - 2)
            {
              title = title.Remove(startIndex2, token.Length + 2);
              index = title.Length;
              return token;
            }
          }
        }
        if (title == identifier + token || title == identifierExtra + token)
        {
          title = string.Empty;
          index = title.Length;
          return token;
        }
      }
      if (lastMatchName != null)
        title = title.Remove(index, lastMatchName.Length + (index > 0 ? 3 : 2)).Insert(index, " ");
      return lastMatchName;
    }
  }
}
