// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.MathUtil
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Util
{
  public class MathUtil
  {
    public static bool DoubleGe(double left, double right, double precision)
    {
      return Math.Abs(left - right) < precision || left > right;
    }

    public static bool DoubleGt(double left, double right, double precision)
    {
      return Math.Abs(left - right) >= precision && left > right;
    }

    public static bool DoubleLe(double left, double right, double precision)
    {
      return Math.Abs(left - right) < precision || left < right;
    }

    public static bool DoubleLt(double left, double right, double precision)
    {
      return Math.Abs(left - right) >= precision && left < right;
    }

    public static bool DoubleEq(double left, double right, double precision)
    {
      return Math.Abs(left - right) < precision;
    }

    public static bool TryParseString2Double(string str, out double db)
    {
      return double.TryParse(str, out db);
    }

    public static long LongAvg(long left, long right) => left / 2L + right / 2L;

    public static bool Between(double val, double d1, double d2, bool checkEqual = true)
    {
      double num1 = Math.Max(d1, d2);
      double num2 = Math.Min(d1, d2);
      return checkEqual ? val >= num2 && val <= num1 : val > num2 && val < num1;
    }

    public static bool Between(int val, int d1, int d2, bool checkEqual = true)
    {
      int num1 = Math.Max(d1, d2);
      int num2 = Math.Min(d1, d2);
      return checkEqual ? val >= num2 && val <= num1 : val > num2 && val < num1;
    }
  }
}
