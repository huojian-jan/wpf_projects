// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.CalendarDisplay
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF
{
  public class CalendarDisplay
  {
    public int Year { get; set; }

    public int Month { get; set; }

    public int Day { get; set; }

    public int Era { get; set; }

    public bool LearMonth { get; set; }

    public bool LearYear { get; set; }

    public string CalendarType { get; set; }

    public string DisplayText()
    {
      try
      {
        return this.DoDisplayText();
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
        return string.Empty;
      }
    }

    public string DoDisplayText() => this.Day != 1 ? this.GetDayName() : this.GetMonthName();

    private string GetDayName()
    {
      string[] strArray;
      return this.CalendarType.Equals("shaka") || Utils.IsEn() && (this.CalendarType.Equals("hebcal") || this.CalendarType.Equals("hijri")) || !CalendarConverter.CalendarDayNames.TryGetValue(this.CalendarType, out strArray) ? this.Day.ToString() : strArray[this.Day - 1] ?? "";
    }

    private string GetKoreanLunarMonthName(IReadOnlyList<string> monthNames)
    {
      if (CalendarConverter.KoreanLunarLeapYearMonth.Keys.Contains<int>(this.Year))
      {
        int num = CalendarConverter.KoreanLunarLeapYearMonth[this.Year];
        if (this.LearMonth && this.Month == num)
          return "윤" + this.GetNonLeapMonthName(monthNames, this.Month - 2);
        if (this.Month > num)
          return monthNames[this.Month - 2] ?? "";
      }
      return this.GetNonLeapMonthName(monthNames);
    }

    private string GetHebcalMonthName(IReadOnlyList<string> monthNames)
    {
      if (this.LearYear)
      {
        if (this.Month == 6)
          return !Utils.IsEn() ? "אֲדָר א" : "Ada Ⅰ";
        if (this.LearMonth && this.Month == 7)
          return !Utils.IsEn() ? "אֲדָר ב'" : "Ada Ⅱ";
        if (this.Month >= 8)
          return monthNames[this.Month - 2] ?? "";
      }
      return this.GetNonLeapMonthName(monthNames);
    }

    private string GetNonLeapMonthName(IReadOnlyList<string> monthNames, int monthIndex = -1)
    {
      if (monthIndex == -1)
        monthIndex = this.Month - 1;
      return monthIndex < 0 || monthIndex >= monthNames.Count ? string.Empty : monthNames[monthIndex] ?? "";
    }

    private string GetMonthName()
    {
      string[] monthNames;
      if (!(Utils.IsEn() ? CalendarConverter.CalendarMonthNamesEn : CalendarConverter.CalendarMonthNames).TryGetValue(this.CalendarType, out monthNames))
        return string.Empty;
      if (this.CalendarType.Equals("hebcal"))
        return this.GetHebcalMonthName((IReadOnlyList<string>) monthNames);
      if (this.CalendarType.Equals("korean-lunar"))
        return this.GetKoreanLunarMonthName((IReadOnlyList<string>) monthNames);
      int index = this.Month - 1;
      return index < 0 || index >= monthNames.Length ? string.Empty : monthNames[index] ?? "";
    }

    public CalendarDisplay(DateTime date, string calendarType)
    {
      if (calendarType == "shaka")
      {
        Tuple<int, int, int> tuple = CalendarDisplay.ConvertIndianDate(date);
        this.Year = tuple.Item1;
        this.Month = tuple.Item2;
        this.Day = tuple.Item3;
        this.CalendarType = calendarType;
      }
      else
      {
        Calendar calendar = CalendarConverter.GetCalendar(calendarType);
        this.Year = calendar.GetYear(date);
        this.Month = calendar.GetMonth(date);
        this.Day = calendar.GetDayOfMonth(date);
        this.Era = calendar.GetEra(date);
        this.LearMonth = calendar.IsLeapMonth(this.Year, this.Month, this.Era);
        this.LearYear = calendar.IsLeapYear(this.Year);
        this.CalendarType = calendarType;
      }
    }

    public static Tuple<int, int, int> ConvertIndianDate(DateTime date)
    {
      GregorianCalendar gregorianCalendar = new GregorianCalendar();
      int year = gregorianCalendar.GetYear(date);
      int month = gregorianCalendar.GetMonth(date);
      int dayOfMonth = gregorianCalendar.GetDayOfMonth(date);
      bool flag = gregorianCalendar.IsLeapYear(year);
      int num1 = flag ? 21 : 22;
      int num2 = gregorianCalendar.GetYear(date) - 78;
      int num3;
      int num4;
      if (month == 12 && dayOfMonth >= 22)
      {
        num3 = 10;
        num4 = dayOfMonth - 21;
      }
      else if (month == 12)
      {
        num3 = 9;
        num4 = dayOfMonth + 9;
      }
      else if (month == 11 && dayOfMonth >= 22)
      {
        num3 = 9;
        num4 = dayOfMonth - 21;
      }
      else if (month == 11)
      {
        num3 = 8;
        num4 = dayOfMonth + 9;
      }
      else if (month == 10 && dayOfMonth >= 23)
      {
        num3 = 8;
        num4 = dayOfMonth - 22;
      }
      else if (month == 10)
      {
        num3 = 7;
        num4 = dayOfMonth + 8;
      }
      else if (month == 9 && dayOfMonth >= 23)
      {
        num3 = 7;
        num4 = dayOfMonth - 22;
      }
      else if (month == 9)
      {
        num3 = 6;
        num4 = dayOfMonth + 9;
      }
      else if (month == 8 && dayOfMonth >= 23)
      {
        num3 = 6;
        num4 = dayOfMonth - 22;
      }
      else if (month == 8)
      {
        num3 = 5;
        num4 = dayOfMonth + 9;
      }
      else if (month == 7 && dayOfMonth >= 23)
      {
        num3 = 5;
        num4 = dayOfMonth - 22;
      }
      else if (month == 7)
      {
        num3 = 4;
        num4 = dayOfMonth + 9;
      }
      else if (month == 6 && dayOfMonth >= 22)
      {
        num3 = 4;
        num4 = dayOfMonth - 21;
      }
      else if (month == 6)
      {
        num3 = 3;
        num4 = dayOfMonth + 10;
      }
      else if (month == 5 && dayOfMonth >= 22)
      {
        num3 = 3;
        num4 = dayOfMonth - 21;
      }
      else if (month == 5)
      {
        num3 = 2;
        num4 = dayOfMonth + 10;
      }
      else if (month == 4 && dayOfMonth >= 21)
      {
        num3 = 2;
        num4 = dayOfMonth - 20;
      }
      else if (month == 4)
      {
        num3 = 1;
        num4 = dayOfMonth + (flag ? 11 : 10);
      }
      else if (month == 3 && dayOfMonth >= num1)
      {
        num3 = 1;
        num4 = dayOfMonth - num1 + 1;
      }
      else if (month == 3)
      {
        --num2;
        num3 = 12;
        num4 = dayOfMonth + (flag ? 10 : 9);
      }
      else if (month == 2 && dayOfMonth >= 20)
      {
        --num2;
        num3 = 12;
        num4 = dayOfMonth - 19;
      }
      else if (month == 2)
      {
        --num2;
        num3 = 11;
        num4 = dayOfMonth + 11;
      }
      else if (month == 1 && dayOfMonth >= 21)
      {
        --num2;
        num3 = 11;
        num4 = dayOfMonth - 20;
      }
      else
      {
        --num2;
        num3 = 10;
        num4 = dayOfMonth + 10;
      }
      return new Tuple<int, int, int>(num2, num3, num4);
    }
  }
}
