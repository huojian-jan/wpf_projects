// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.TickTickDuration
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Text;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public class TickTickDuration
  {
    public static string OnTime = "TRIGGER:PT0S";
    private bool _isPositive;
    private int _years;
    private int _months;
    private int _weeks;
    private int _days;
    private int _hours;
    private int _minutes;
    private int _seconds;

    public int GetYears() => this._years;

    public void SetYears(int years) => this._years = years;

    public int GetMonths() => this._months;

    public void SetMonths(int months) => this._months = months;

    public int GetWeeks() => this._weeks;

    public void SetWeeks(int weeks) => this._weeks = weeks;

    public int GetDays() => this._days;

    public void SetDays(int days) => this._days = days;

    public int GetHours() => this._hours;

    public void SetHours(int hours) => this._hours = hours;

    public int GetMinutes() => this._minutes;

    public void SetMinutes(int minutes) => this._minutes = minutes;

    public int GetSeconds() => this._seconds;

    public void SetSeconds(int seconds) => this._seconds = seconds;

    public bool IsPositive() => this._isPositive;

    public void SetIsPositive(bool isPositive) => this._isPositive = isPositive;

    public TickTickDuration(
      int years,
      int months,
      int weeks,
      int days,
      int hours,
      int minutes,
      int seconds)
    {
      this._years = years;
      this._months = months;
      this._weeks = weeks;
      this._days = days;
      this._hours = hours;
      this._minutes = minutes;
      this._seconds = seconds;
    }

    public TickTickDuration(bool isPositive, int days, int hours, int minutes)
    {
      this._isPositive = isPositive;
      this._days = days;
      this._hours = hours;
      this._minutes = minutes;
    }

    public TickTickDuration(bool isPositive, int weeks, int days)
    {
      this._isPositive = isPositive;
      this._weeks = weeks;
      this._days = days;
    }

    public static TickTickDuration Build(string lexicalRepresentation)
    {
      try
      {
        return string.IsNullOrWhiteSpace(lexicalRepresentation) ? (TickTickDuration) null : new TickTickDuration(lexicalRepresentation);
      }
      catch (Exception ex)
      {
      }
      return TickTickDuration.BuildOntimeDuration();
    }

    private static TickTickDuration BuildOntimeDuration() => new TickTickDuration(true, 0, 0, 0);

    private TickTickDuration(string lexicalRepresentation)
    {
      string whole = lexicalRepresentation.Replace("TRIGGER:", "");
      int[] idx = new int[1];
      int length = whole.Length;
      bool flag1 = false;
      idx[0] = 0;
      bool flag2;
      if (length != idx[0] && whole[idx[0]] == '-')
      {
        ++idx[0];
        flag2 = false;
      }
      else
        flag2 = true;
      if (length != idx[0])
      {
        int num1 = (int) whole[idx[0]++];
      }
      int len1 = 0;
      string[] parts1 = new string[4];
      int[] partsIndex1 = new int[4];
      for (; length != idx[0] && TickTickDuration.IsDigit(whole[idx[0]]) && len1 < 4; parts1[len1++] = TickTickDuration.ParsePiece(whole, (IList<int>) idx))
        partsIndex1[len1] = idx[0];
      if (length != idx[0] && whole[idx[0]++] == 'T')
        flag1 = true;
      int len2 = 0;
      string[] parts2 = new string[3];
      int[] partsIndex2 = new int[3];
      for (; length != idx[0] && TickTickDuration.IsDigitOrPeriod(whole[idx[0]]) && len2 < 3; parts2[len2++] = TickTickDuration.ParsePiece(whole, (IList<int>) idx))
        partsIndex2[len2] = idx[0];
      if (flag1)
        ;
      int num2 = idx[0];
      if (len1 == 0)
        ;
      TickTickDuration.OrganizeParts((IList<string>) parts1, (IList<int>) partsIndex1, len1, "YMWD");
      TickTickDuration.OrganizeParts((IList<string>) parts2, (IList<int>) partsIndex2, len2, "HMS");
      this._years = TickTickDuration.ParseInteger(parts1[0]);
      this._months = TickTickDuration.ParseInteger(parts1[1]);
      this._weeks = TickTickDuration.ParseInteger(parts1[2]);
      this._days = TickTickDuration.ParseInteger(parts1[3]);
      this._hours = TickTickDuration.ParseInteger(parts2[0]);
      this._minutes = TickTickDuration.ParseInteger(parts2[1]);
      this._seconds = TickTickDuration.ParseInteger(parts2[2]);
      this._isPositive = flag2;
    }

    private static string ParsePiece(string whole, IList<int> idx)
    {
      int startIndex = idx[0];
      while (idx[0] < whole.Length && TickTickDuration.IsDigitOrPeriod(whole[idx[0]]))
        ++idx[0];
      int num = idx[0];
      int length = whole.Length;
      ++idx[0];
      return whole.Substring(startIndex, idx[0]);
    }

    private static void OrganizeParts(
      IList<string> parts,
      IList<int> partsIndex,
      int len,
      string tokens)
    {
      int index1 = tokens.Length;
      for (int index2 = len - 1; index2 >= 0; --index2)
      {
        int num = tokens.LastIndexOf(parts[index2][parts[index2].Length - 1], index1 - 1);
        for (int index3 = num + 1; index3 < index1; ++index3)
          parts[index3] = (string) null;
        index1 = num;
        parts[index1] = parts[index2];
        partsIndex[index1] = partsIndex[index2];
      }
      for (int index4 = index1 - 1; index4 >= 0; --index4)
        parts[index4] = (string) null;
    }

    private static bool IsDigit(char ch) => '0' <= ch && ch <= '9';

    private static bool IsDigitOrPeriod(char ch) => TickTickDuration.IsDigit(ch) || ch == '.';

    private static int ParseInteger(string part)
    {
      if (part == null)
        return 0;
      part = part.Substring(0, part.Length - 1);
      return int.Parse(part);
    }

    public override bool Equals(object obj)
    {
      return obj is TickTickDuration tickTickDuration && this._isPositive == tickTickDuration.IsPositive() && this._years == tickTickDuration.GetYears() && this._months == tickTickDuration.GetMonths() && this._weeks == tickTickDuration.GetWeeks() && this._days == tickTickDuration.GetDays() && this._days == tickTickDuration.GetDays() && this._hours == tickTickDuration.GetHours() && this._minutes == tickTickDuration.GetMinutes();
    }

    public override string ToString()
    {
      bool flag1 = TickTickDuration.HasValue(this._years);
      bool flag2 = TickTickDuration.HasValue(this._months);
      bool flag3 = TickTickDuration.HasValue(this._weeks);
      bool flag4 = TickTickDuration.HasValue(this._days);
      bool flag5 = TickTickDuration.HasValue(this._hours);
      bool flag6 = TickTickDuration.HasValue(this._minutes);
      bool flag7 = TickTickDuration.HasValue(this._seconds);
      if (!flag1 && !flag2 && !flag3 && !flag4 && !flag5 && !flag6 && !flag7)
        return TickTickDuration.OnTime;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("TRIGGER:");
      if (!this._isPositive)
        stringBuilder.Append('-');
      stringBuilder.Append('P');
      if (flag1)
        stringBuilder.Append(this._years.ToString() + "Y");
      if (flag2)
        stringBuilder.Append(this._months.ToString() + "M");
      if (flag3)
        stringBuilder.Append(this._weeks.ToString() + "W");
      if (flag4)
        stringBuilder.Append(this._days.ToString() + "D");
      if (flag5 | flag6 | flag7)
      {
        stringBuilder.Append('T');
        if (flag5)
          stringBuilder.Append(this._hours.ToString() + "H");
        if (flag6)
          stringBuilder.Append(this._minutes.ToString() + "M");
        if (flag7)
          stringBuilder.Append(this._seconds.ToString() + "S");
      }
      return stringBuilder.ToString();
    }

    private static bool HasValue(int value) => value != 0;

    public string ToReminderText() => this.ToReminderText(false);

    public string ToReminderText(bool isAllday) => this.ToReminderText(isAllday, true);

    public string ToReminderText(bool isAllday, bool showUnit)
    {
      string str = "";
      if (isAllday & showUnit)
        str = "（%1$s）";
      if (TickTickDuration.HasValue(this._weeks))
        return " 提前 %d 周";
      if (TickTickDuration.HasValue(this._days))
        return this._days > 0 ? " 提前 %d 天" : "当天";
      if (TickTickDuration.HasValue(this._hours))
        return " 提前 %d 小时";
      if (TickTickDuration.HasValue(this._minutes))
        return " 提前 %d 分钟";
      return !isAllday ? "准时" : "当天" + str;
    }
  }
}
