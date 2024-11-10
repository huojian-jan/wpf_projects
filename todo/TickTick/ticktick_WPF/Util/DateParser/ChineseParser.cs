// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.ChineseParser
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net;
using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public class ChineseParser : IParser
  {
    public RecurrencePattern MatchRepeat(
      string matchStr,
      ref DateTime? parseDate,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      RecurrencePattern recurrencePattern = new RecurrencePattern();
      Match match1 = new Regex("每(\\d{1,2})?[年|个](\\d{1,2}|" + ChineseParser.GetChinaNumPattern(12) + ")[/|\\-|－|月|\\s](\\d{1,2}|" + ChineseParser.GetChinaNumPattern(31) + ")[日|号]").Match(matchStr);
      if (match1.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match1.Groups[0].Value);
        recurrencePattern.Interval = OldDateParser.IsNull(match1.Groups[1].Value) ? 1 : OldDateParser.TranslateNum(match1.Groups[1].Value);
        recurrencePattern.Frequency = FrequencyType.Yearly;
        int year = baseDate.Year;
        int month = OldDateParser.TranslateNum(match1.Groups[2].Value);
        int day = OldDateParser.TranslateNum(match1.Groups[3].Value);
        parseDate = new DateTime?(new DateTime(year, month, day));
        DateTime? nullable = parseDate;
        DateTime date = baseDate.Date;
        if ((nullable.HasValue ? (nullable.GetValueOrDefault() < date ? 1 : 0) : 0) != 0)
          parseDate = new DateTime?(OldDateParser.GetNextDate(month, true, parseDate.Value));
        return recurrencePattern;
      }
      Match match2 = new Regex("每(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(12) + "|两)?(个)?(日|天|礼拜|星期|周|月|年)?(工作日|末)?(最后一天|第)?(\\d{1,2}|天|日|" + ChineseParser.GetChinaNumPattern(31) + ")?(天|号|月)?").Match(matchStr);
      if (match2.Success)
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match2.Groups[0].Value);
      if (match2.Success && !OldDateParser.IsNull(match2.Groups[2].Value) && !OldDateParser.IsNull(match2.Groups[3].Value))
      {
        recurrencePattern.Interval = 1;
        recurrencePattern.Frequency = FrequencyType.Yearly;
      }
      else if (match2.Success && !OldDateParser.IsNull(match2.Groups[3].Value))
      {
        recurrencePattern.Interval = !OldDateParser.IsNull(match2.Groups[1].Value) ? OldDateParser.TranslateNum(match2.Groups[1].Value) : 1;
        string str = match2.Groups[3].Value;
        if (str != null)
        {
          switch (str.Length)
          {
            case 1:
              switch (str[0])
              {
                case '周':
                  break;
                case '天':
                case '日':
                  recurrencePattern.Frequency = FrequencyType.Daily;
                  parseDate = new DateTime?(new DateTime(baseDate.Year, baseDate.Month, baseDate.Day));
                  goto label_33;
                case '年':
                  recurrencePattern.Frequency = FrequencyType.Yearly;
                  if (!OldDateParser.IsNull(match2.Groups[6].Value) && !OldDateParser.IsNull(match2.Groups[7].Value) && "月" == match2.Groups[7].Value)
                  {
                    int year = baseDate.Year;
                    int month = OldDateParser.TranslateNum(match2.Groups[6].Value);
                    parseDate = new DateTime?(new DateTime(year, month, baseDate.Day));
                    if (parseDate.Value.Date < baseDate.Date)
                    {
                      parseDate = new DateTime?(OldDateParser.GetNextDate(month, false, parseDate.Value));
                      goto label_33;
                    }
                    else
                      goto label_33;
                  }
                  else
                  {
                    parseDate = new DateTime?(DateTime.Today.Date);
                    goto label_33;
                  }
                case '月':
                  recurrencePattern.Frequency = FrequencyType.Monthly;
                  int year1 = baseDate.Year;
                  int month1 = baseDate.Month;
                  if ("最后一天" == match2.Groups[5].Value)
                  {
                    recurrencePattern.ByMonthDay = new List<int>()
                    {
                      -1
                    };
                    int day = DateTime.DaysInMonth(year1, month1);
                    parseDate = new DateTime?(new DateTime(year1, month1, day));
                    goto label_33;
                  }
                  else if (!OldDateParser.IsNull(match2.Groups[6].Value))
                  {
                    recurrencePattern.ByMonthDay = new List<int>()
                    {
                      OldDateParser.TranslateNum(match2.Groups[6].Value)
                    };
                    int day = OldDateParser.TranslateNum(match2.Groups[6].Value);
                    parseDate = new DateTime?(new DateTime(year1, month1, day));
                    if (parseDate.Value.Date < baseDate.Date)
                    {
                      parseDate = new DateTime?(parseDate.Value.AddMonths(1));
                      goto label_33;
                    }
                    else
                      goto label_33;
                  }
                  else
                  {
                    recurrencePattern.ByMonthDay = new List<int>()
                    {
                      baseDate.Day
                    };
                    parseDate = new DateTime?(new DateTime(year1, month1, baseDate.Day));
                    if (parseDate.Value.Date < baseDate.Date)
                    {
                      parseDate = new DateTime?(parseDate.Value.AddMonths(1));
                      goto label_33;
                    }
                    else
                      goto label_33;
                  }
                default:
                  goto label_33;
              }
              break;
            case 2:
              switch (str[0])
              {
                case '星':
                  if (str == "星期")
                    break;
                  goto label_33;
                case '礼':
                  if (str == "礼拜")
                    break;
                  goto label_33;
                default:
                  goto label_33;
              }
              break;
            default:
              goto label_33;
          }
          recurrencePattern.Frequency = FrequencyType.Weekly;
          if (!OldDateParser.IsNull(match2.Groups[6].Value))
          {
            int num = OldDateParser.TranslateNum(match2.Groups[6].Value);
            if (num > 0 && num < 8)
              recurrencePattern.ByWeekNo = new List<int>()
              {
                num
              };
            DateTime dateTime = baseDate.AddDays((double) ((int) baseDate.DayOfWeek * -1));
            parseDate = new DateTime?(dateTime.AddDays((double) num));
            if (parseDate.Value.Date < baseDate.Date)
              parseDate = new DateTime?(parseDate.Value.AddDays(7.0));
          }
          else
            parseDate = new DateTime?(new DateTime(baseDate.Year, baseDate.Month, baseDate.Day));
        }
      }
label_33:
      return recurrencePattern;
    }

    public TickTickDuration MatchAdvanceTime(string matchStr, ref List<string> recgonizeStrList)
    {
      Match match1 = new Regex("(提前|提早)(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(60) + "|半|两)(分钟|小时|天|周|星期|礼拜)").Match(matchStr);
      if (match1.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match1.Groups[0].Value);
        return OldDateParser.GetDuration(match1.Groups[2].Value, match1.Groups[3].Value, false);
      }
      Match match2 = new Regex("(提前|提早)(提醒我)").Match(matchStr);
      if (!match2.Success)
        return (TickTickDuration) null;
      OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match2.Groups[0].Value);
      TickTickDuration tickTickDuration = new TickTickDuration(0, 0, 0, 0, 0, 5, 0);
      tickTickDuration.SetIsPositive(false);
      return tickTickDuration;
    }

    public bool MatchAfterTime(
      string matchStr,
      ref DateTime? baseTime,
      ref List<string> recgonizeStrList)
    {
      bool flag = false;
      Match match1 = new Regex("(\\d{1,3}|两|" + ChineseParser.GetChinaNumPattern(60) + ")(个)?(小时)(\\d{1,3}|两|" + ChineseParser.GetChinaNumPattern(60) + ")(分钟|分)(后|之后|以后)").Match(matchStr);
      if (match1.Success)
      {
        flag = true;
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match1.Groups[0].Value);
        string str1 = match1.Groups[1].Value;
        int num1 = !OldDateParser.IsAnOrA(str1) ? (!OldDateParser.IsNull(str1) ? OldDateParser.TranslateNum(str1) : 0) : 1;
        string str2 = match1.Groups[4].Value;
        int num2 = !OldDateParser.IsAnOrA(str2) ? (!OldDateParser.IsNull(str2) ? OldDateParser.TranslateNum(str2) : 0) : 1;
        TickTickDuration duration = OldDateParser.GetDuration(num2 < 60 ? num1 * 60 + num2 : num2, match1.Groups[5].Value, true);
        if (duration != null && baseTime.HasValue)
          baseTime = new DateTime?(OldDateParser.GetDateByDuration(baseTime.Value, duration));
      }
      if (!flag)
      {
        Match match2 = new Regex("(\\d{1,3}|两|" + ChineseParser.GetChinaNumPattern(60) + ")?(个)?(半)(个)?(小时)(后|之后|以后)").Match(matchStr);
        if (match2.Success)
        {
          OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match2.Groups[0].Value);
          flag = true;
          string str = match2.Groups[1].Value;
          TickTickDuration duration = OldDateParser.GetDuration((!OldDateParser.IsAnOrA(str) ? (!OldDateParser.IsNull(str) ? OldDateParser.TranslateNum(str) : 0) : 1) * 60 + 30, "分钟", true);
          if (duration != null && baseTime.HasValue)
            baseTime = new DateTime?(OldDateParser.GetDateByDuration(baseTime.Value, duration));
        }
      }
      if (!flag)
      {
        Match match3 = new Regex("(\\d{1,3}|两|" + ChineseParser.GetChinaNumPattern(60) + ")(个)?(分钟|分|小时|天|周|星期|礼拜|月|年)(后|之后|以后)").Match(matchStr);
        if (match3.Success)
        {
          OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match3.Groups[0].Value);
          flag = true;
          TickTickDuration duration = OldDateParser.GetDuration(match3.Groups[1].Value, match3.Groups[3].Value);
          if (duration != null && baseTime.HasValue)
            baseTime = new DateTime?(OldDateParser.GetDateByDuration(baseTime.Value, duration));
        }
      }
      return flag;
    }

    public bool MatchTime(
      string text,
      ref DateTime? parseDate,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      IEnumerable<string> matchTimePattern = ChineseParser.GetMatchTimePattern();
      string amFlag = ChineseParser.GetAmFlag();
      string pmFlag = ChineseParser.GetPmFlag();
      bool flag1 = false;
      foreach (string pattern in matchTimePattern)
      {
        Match match1 = new Regex(pattern).Match(text);
        if (match1.Success)
        {
          OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match1.Groups[0].Value);
          int num1 = OldDateParser.MatchNum(match1.Groups[3].Value);
          int num2 = num1;
          int minutes = !string.IsNullOrEmpty(match1.Groups[5].Value) ? OldDateParser.MatchNum(match1.Groups[5].Value) : 0;
          if (num1 <= 12)
          {
            Match match2 = new Regex(amFlag).Match(text);
            Match match3 = new Regex(pmFlag).Match(text);
            if (match2.Success)
            {
              flag1 = true;
              if (text.Contains("中午"))
              {
                if (num1 < 3)
                  num2 += 12;
              }
              else if (text.Contains("凌晨") && num1 == 12)
                num2 = 0;
            }
            else if (match3.Success)
            {
              flag1 = true;
              num2 += 12;
              if (match3.Groups[0].Value.Contains("明"))
                num2 += 24;
            }
          }
          bool flag2 = true;
          if (!parseDate.HasValue)
          {
            flag2 = false;
            parseDate = new DateTime?(baseDate.Date);
          }
          parseDate = new DateTime?(DateUtils.SetHourAndMinuteOnly(parseDate.Value, 0, minutes));
          parseDate = new DateTime?(parseDate.Value.AddHours((double) num2));
          if (!flag2)
          {
            DateTime? nullable = parseDate;
            DateTime dateTime1 = baseDate;
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= dateTime1 ? 1 : 0) : 0) != 0)
            {
              parseDate = new DateTime?(flag1 || num2 > 12 ? parseDate.Value.AddDays(1.0) : parseDate.Value.AddHours(12.0));
              nullable = parseDate;
              DateTime dateTime2 = baseDate;
              if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= dateTime2 ? 1 : 0) : 0) != 0)
                parseDate = new DateTime?(parseDate.Value.AddHours(12.0));
            }
            if (num2 == 0 && !flag1)
            {
              parseDate = new DateTime?(DateUtils.SetHourAndMinuteOnly(parseDate.Value, 0, minutes));
              nullable = parseDate;
              DateTime dateTime3 = baseDate;
              if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= dateTime3 ? 1 : 0) : 0) != 0)
                parseDate = new DateTime?(parseDate.Value.AddDays(1.0));
            }
          }
          else if (text.Contains("今天") && num2 < 12)
          {
            DateTime? nullable = parseDate;
            DateTime dateTime = baseDate;
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() < dateTime ? 1 : 0) : 0) != 0)
              parseDate = new DateTime?(parseDate.Value.AddHours(12.0));
          }
          return true;
        }
      }
      return false;
    }

    public bool MatchSpecialTime(
      string matchStr,
      ref DateTime? parseDate,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      bool flag = !parseDate.HasValue;
      Match match = new Regex("(早上|上午|中午|下午|傍晚|晚上|凌晨|午夜|半夜|明晚|今晚|明早)").Match(matchStr);
      if (!match.Success)
        return false;
      OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
      Dictionary<string, int> specialHourMap = ChineseParser.GetSpecialHourMap();
      string key = match.Groups[0].Value;
      if (!parseDate.HasValue)
        parseDate = new DateTime?(baseDate.Date);
      parseDate = new DateTime?(DateUtils.SetHourAndMinuteOnly(parseDate.Value, 0, 0));
      ref DateTime? local1 = ref parseDate;
      DateTime dateTime = parseDate.Value;
      DateTime? nullable1 = new DateTime?(dateTime.AddHours((double) specialHourMap[key]));
      local1 = nullable1;
      if (parseDate.Value <= baseDate & flag)
      {
        ref DateTime? local2 = ref parseDate;
        dateTime = parseDate.Value;
        DateTime? nullable2 = new DateTime?(dateTime.AddDays(1.0));
        local2 = nullable2;
      }
      return true;
    }

    private static Dictionary<string, int> GetSpecialHourMap()
    {
      return new Dictionary<string, int>()
      {
        {
          "早上",
          7
        },
        {
          "上午",
          9
        },
        {
          "中午",
          12
        },
        {
          "下午",
          13
        },
        {
          "傍晚",
          17
        },
        {
          "晚上",
          20
        },
        {
          "今晚",
          20
        },
        {
          "凌晨",
          24
        },
        {
          "午夜",
          24
        },
        {
          "半夜",
          26
        },
        {
          "明早",
          31
        },
        {
          "明晚",
          44
        }
      };
    }

    private static IEnumerable<string> GetMatchTimePattern()
    {
      string amFlag = ChineseParser.GetAmFlag();
      string pmFlag = ChineseParser.GetPmFlag();
      return (IEnumerable<string>) new string[4]
      {
        amFlag + "?" + pmFlag + "?\\s?(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(24) + "|零|两)\\s*点\\s*((\\d{1,2}|半|1?3?一?三?刻|零?两|" + ChineseParser.GetChinaNumPattern(60) + ")\\s*(分)?)?" + amFlag + "?" + pmFlag + "?",
        amFlag + "?" + pmFlag + "?\\s?(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(24) + "|零|两)\\s*点钟\\s*((\\d{1,2}|零?两|" + ChineseParser.GetChinaNumPattern(60) + ")\\s*(分)?)?" + amFlag + "?" + pmFlag + "?",
        amFlag + "?" + pmFlag + "?\\s?(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(24) + "|零|两)\\s*[点|:|：]\\s*((\\d{1,2}|半|1?3?一?三?刻)())?" + amFlag + "?" + pmFlag + "?",
        amFlag + "?" + pmFlag + "?\\s?(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(24) + "|零|两)\\s*点钟\\s*((\\d{1,2}|半|1?3?一?三?刻)())?" + amFlag + "?" + pmFlag + "?"
      };
    }

    private static string GetPmFlag() => "(下午|傍晚|晚上|明晚|今晚)";

    private static string GetAmFlag() => "(早上|上午|中午|凌晨|午夜|半夜|明早)";

    public DateTime? MatchSpecialDay(
      string text,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      Match match = new Regex(ChineseParser.GetSpecialDateRecognition()).Match(text);
      if (match.Success)
      {
        string key = match.Groups[0].Value;
        if (ChineseParser.GetSpecialDateMaps().ContainsKey(key))
        {
          OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
          int specialDateMap = ChineseParser.GetSpecialDateMaps()[key];
          return new DateTime?(baseDate.AddDays((double) specialDateMap));
        }
      }
      return new DateTime?();
    }

    private static string GetSpecialDateRecognition() => "(大前天|前天|昨天|今天|明天|后天|大后天)";

    private static Dictionary<string, int> GetSpecialDateMaps()
    {
      return new Dictionary<string, int>()
      {
        {
          "大前天",
          -3
        },
        {
          "前天",
          -2
        },
        {
          "昨天",
          -1
        },
        {
          "今天",
          0
        },
        {
          "明天",
          1
        },
        {
          "后天",
          2
        },
        {
          "大后天",
          3
        }
      };
    }

    public DateTime? MatchWeekday(
      string text,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      Match match = new Regex(ChineseParser.GetWeekDayRecognition()).Match(text);
      if (match.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
        string str1 = match.Groups[2].Value;
        string str2 = match.Groups[4].Value;
        string str3 = match.Groups[1].Value;
        if (!string.IsNullOrEmpty(str2))
        {
          int num = OldDateParser.MatchNum(str2);
          if (str1.Contains("下"))
            num += 7;
          DateTime dateTime = baseDate.Date.AddDays((double) ((int) baseDate.DayOfWeek * -1)).AddDays((double) num);
          if (dateTime < baseDate.Date && string.IsNullOrEmpty(str3))
            dateTime = dateTime.AddDays(7.0);
          return new DateTime?(dateTime);
        }
      }
      return new DateTime?();
    }

    private static string GetWeekDayRecognition()
    {
      return "(本|这|这个)?(下个|下)?(星期|礼拜|周)(一|二|三|四|五|六|日|天|1|2|3|4|5|6|7)";
    }

    public DateTime? MatchMonth(string text, DateTime baseDate, ref List<string> recgonizeStrList)
    {
      return new DateTime?();
    }

    public DateTime? MatchDate(string text, DateTime baseDate, ref List<string> recgonizeStrList)
    {
      DateTime? matchDateResult1 = ChineseParser.GetMatchDateResult(ChineseParser.GetChineseMonthDayYearPattern(), text, 5, 1, 3, 4, 2, baseDate, ref recgonizeStrList);
      if (matchDateResult1.HasValue)
        return matchDateResult1;
      foreach (string pattern in ChineseParser.GetChineseDatePattern())
      {
        DateTime? matchDateResult2 = ChineseParser.GetMatchDateResult(pattern, text, 1, 3, 5, 2, 4, baseDate, ref recgonizeStrList);
        if (matchDateResult2.HasValue)
          return matchDateResult2;
      }
      return new DateTime?();
    }

    private static DateTime? GetMatchDateResult(
      string pattern,
      string matchStr,
      int yearGroupIndex,
      int monthGroupIndex,
      int dayOfMonthGroupIndex,
      int yearUnitGroupIndex,
      int monthUnitGroupIndex,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      Match match = new Regex(pattern).Match(matchStr);
      if (match.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
        string s = match.Groups[yearGroupIndex].Value;
        string str1 = match.Groups[monthGroupIndex].Value;
        string str2 = match.Groups[dayOfMonthGroupIndex].Value;
        string str3 = match.Groups[yearUnitGroupIndex].Value;
        string str4 = match.Groups[monthUnitGroupIndex].Value;
        if ("年" != str3 && "月" != str4 && !string.IsNullOrEmpty(str3) && str3 != str4)
        {
          str2 = str1;
          str1 = s;
          s = "";
        }
        if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(str1))
        {
          int year = int.Parse(s);
          int num1 = OldDateParser.MatchNum(str1);
          int num2 = OldDateParser.MatchNum(str2);
          int month = num1;
          int day = num2;
          return new DateTime?(new DateTime(year, month, day));
        }
        if (!string.IsNullOrEmpty(str1))
        {
          int year = baseDate.Year;
          int month = OldDateParser.MatchNum(str1);
          int day = string.IsNullOrEmpty(str2) ? 1 : OldDateParser.MatchNum(str2);
          bool hasDay = !string.IsNullOrEmpty(str2);
          DateTime date = new DateTime(year, month, day);
          if (date.Date < baseDate.Date)
            date = OldDateParser.GetNextDate(month, hasDay, date);
          return new DateTime?(date);
        }
        if (!string.IsNullOrEmpty(str2))
        {
          int year = baseDate.Year;
          int month = baseDate.Month;
          int day = OldDateParser.MatchNum(str2);
          DateTime date = new DateTime(year, month, day);
          if (date.Date < baseDate.Date)
            date = string.IsNullOrEmpty(str1) ? date.AddMonths(1) : OldDateParser.GetNextDate(month, true, date);
          return new DateTime?(date);
        }
      }
      return new DateTime?();
    }

    private static string GetChineseMonthDayYearPattern()
    {
      return "(?i)(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(12) + ")([\\/|\\-|\\－|月|\\s])(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(31) + "|上旬|中旬|下旬)?[日|号]?([\\/|\\-|\\－|年|\\s])(\\d{4})";
    }

    private static IEnumerable<string> GetChineseDatePattern()
    {
      return (IEnumerable<string>) new string[3]
      {
        "(?i)(\\d{2,4})([\\/|\\-|\\－|年|\\s])(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(12) + ")([\\/|\\-|\\－|月|\\s])(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(31) + "|上旬|中旬|下旬)?[日|号]?",
        "(?i)()()(?<![\\d|\\.])(\\d{1,2}|下|下个|" + ChineseParser.GetChinaNumPattern(12) + ")([\\/|\\-|\\－|月])(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(31) + "|上旬|中旬|下旬)?[日|号]?",
        "(?i)()()()()(\\d{1,2}|" + ChineseParser.GetChinaNumPattern(31) + "|上旬|中旬|下旬)[日|号]"
      };
    }

    public static string GetChinaNumPattern(int num)
    {
      Dictionary<int, string> dictionary = new Dictionary<int, string>()
      {
        {
          6,
          "零?六|零?五|零?四|零?三|零?二|零?一"
        }
      };
      dictionary.Add(12, "十二|十一|十|零?九|零?八|零?七|" + dictionary[6]);
      dictionary.Add(24, "二十四|二十三|二十二|二十一|二十|十九|十八|十七|十六|十五|十四|十三|" + dictionary[12]);
      dictionary.Add(31, "三十一|三十|二十九|二十八|二十七|二十六|二十五|" + dictionary[24]);
      dictionary.Add(60, "六十|五十九|五十八|五十七|五十六|五十五|五十四|五十三|五十二|五十一|五十|四十九|四十八|四十七|四十六|四十五|四十四|四十三|四十二|四十一|四十|三十九|三十八|三十七|三十六|三十五|三十四|三十三|三十二|" + dictionary[31]);
      return !dictionary.ContainsKey(num) ? dictionary[60] : dictionary[num];
    }
  }
}
