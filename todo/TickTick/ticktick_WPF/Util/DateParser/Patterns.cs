// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.Patterns
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Text.RegularExpressions;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  internal class Patterns
  {
    public const string GOOD_IRI_CHAR = "a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF";
    public static Regex IP_ADDRESS = new Regex("((25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}|[1-9][0-9]|[1-9])\\.(25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}|[1-9][0-9]|[1-9]|0)\\.(25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}|[1-9][0-9]|[1-9]|0)\\.(25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}|[1-9][0-9]|[0-9]))");
    private const string IRI = "[a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF]([a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF\\-]{0,61}[a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF]){0,1}";
    private const string GOOD_GTLD_CHAR = "a-zA-Z -\uD7FF豈-\uFDCFﷰ-\uFFEF";
    private const string GTLD = "[a-zA-Z -\uD7FF豈-\uFDCFﷰ-\uFFEF]{2,63}";
    private const string HOST_NAME = "([a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF]([a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF\\-]{0,61}[a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF]){0,1}\\.)+[a-zA-Z -\uD7FF豈-\uFDCFﷰ-\uFFEF]{2,63}";
    public static Regex DOMAIN_NAME = new Regex("(([a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF]([a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF\\-]{0,61}[a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF]){0,1}\\.)+[a-zA-Z -\uD7FF豈-\uFDCFﷰ-\uFFEF]{2,63}|" + Patterns.IP_ADDRESS?.ToString() + ")");
    public static Regex WEB_URL = new Regex("((?:(http|https|Http|Https|rtsp|Rtsp):\\/\\/(?:(?:[a-zA-Z0-9\\$\\-\\_\\.\\+\\!\\*\\'\\(\\)\\,\\;\\?\\&\\=]|(?:\\%[a-fA-F0-9]{2})){1,64}(?:\\:(?:[a-zA-Z0-9\\$\\-\\_\\.\\+\\!\\*\\'\\(\\)\\,\\;\\?\\&\\=]|(?:\\%[a-fA-F0-9]{2})){1,25})?\\@)?)?(?:" + Patterns.DOMAIN_NAME?.ToString() + ")(?:\\:\\d{1,5})?)(\\/(?:(?:[a-zA-Z0-9 -\uD7FF豈-\uFDCFﷰ-\uFFEF\\;\\/\\?\\:\\@\\&\\=\\#\\~\\-\\.\\+\\!\\*\\'\\(\\)\\,\\_])|(?:\\%[a-fA-F0-9]{2}))*)?(?:\\b|$)");
  }
}
