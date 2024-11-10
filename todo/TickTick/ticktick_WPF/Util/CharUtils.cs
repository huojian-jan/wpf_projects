// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.CharUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util
{
  public class CharUtils
  {
    public static bool IsAsciiSpec(char ch)
    {
      if (ch >= ' ' && ch < 'A' || ch > 'Z' && ch < 'a')
        return true;
      return ch > 'z' && ch < '\u007F';
    }

    public static bool IsAbc(char ch)
    {
      if (ch >= 'a' && ch <= 'z')
        return true;
      return ch >= 'A' && ch <= 'Z';
    }
  }
}
