// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.UriUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Util
{
  public class UriUtils
  {
    public static string AddAttribute(string url, string key, string val)
    {
      if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
        return url;
      int startIndex1 = url.IndexOf("?", StringComparison.Ordinal);
      int startIndex2 = url.IndexOf("#", StringComparison.Ordinal);
      string str1 = startIndex1 >= 0 || startIndex2 >= 0 ? url.Substring(0, startIndex1 >= 0 ? startIndex1 : startIndex2) : url;
      string str2 = startIndex1 >= 0 ? url.Substring(startIndex1, (startIndex2 >= 0 ? startIndex2 : url.Length) - startIndex1) : string.Empty;
      string str3 = startIndex2 >= 0 ? url.Substring(startIndex2, url.Length - startIndex2) : string.Empty;
      string str4 = string.IsNullOrEmpty(str2) ? "?" : (str2.EndsWith("?") ? "" : "&");
      string str5 = str2 + str4 + Utils.UrlEncode0(key) + "=" + Utils.UrlEncode0(val);
      return str1 + str5 + str3;
    }

    public static string AddLangAttribute(string url)
    {
      return UriUtils.AddAttribute(url, "lang", Utils.GetLanguage());
    }
  }
}
