// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.NameUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Text.RegularExpressions;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class NameUtils
  {
    public static bool IsValidName(string exp, bool checkSpace = true)
    {
      return checkSpace ? !new Regex("\"| |#|:|\\*|\\?|>|<|\\||\\/|\\\\").IsMatch(exp) : !new Regex("\"|#|:|\\*|\\?|>|<|\\||\\/|\\\\").IsMatch(exp);
    }

    public static bool IsValidNameNoCheckSharp(string exp, bool checkSpace = true)
    {
      return checkSpace ? !new Regex("\"| |:|\\*|\\?|>|<|\\||\\/|\\\\").IsMatch(exp) : !new Regex("\"|:|\\*|\\?|>|<|\\||\\/|\\\\").IsMatch(exp);
    }

    public static bool IsValidColumnName(string exp)
    {
      return !new Regex("\"|#|:|\\*|\\?|>|<|\\||\\/|\\\\").IsMatch(exp);
    }
  }
}
