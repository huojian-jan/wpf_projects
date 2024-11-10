// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.StringPool
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Concurrent;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class StringPool
  {
    private static ConcurrentDictionary<string, string> _stringPool = new ConcurrentDictionary<string, string>();

    public static string GetOrCreate(string str)
    {
      if (string.IsNullOrEmpty(str))
        return str;
      string str1;
      if (!StringPool._stringPool.TryGetValue(str, out str1))
      {
        StringPool._stringPool[str] = str;
        str1 = str;
      }
      return str1;
    }
  }
}
