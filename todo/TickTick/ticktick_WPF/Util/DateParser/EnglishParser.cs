// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.EnglishParser
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
  public class EnglishParser : IParser
  {
    public static readonly bool IsUsOrUkDateFormat = true;

    public DateTime? MatchDate(
      string inputText,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      bool usOrUkDateFormat = EnglishParser.IsUsOrUkDateFormat;
      Match match1 = new Regex("(?i)(\\(|\\b)(3[0-1]|[0-2]?[0-9])([/-])(3[0-1]|[0-2]?[0-9])([/-])?(\\d{4})(\\)|\\b)(\\s)*").Match(inputText);
      int year = baseDate.Year;
      if (match1.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match1.Groups[0].Value);
        string n1;
        string n2;
        if (usOrUkDateFormat)
        {
          n1 = match1.Groups[2].Value.Trim();
          n2 = match1.Groups[4].Value;
        }
        else
        {
          n1 = match1.Groups[4].Value.Trim();
          n2 = match1.Groups[2].Value;
        }
        int month = OldDateParser.TranslateNum(n1);
        int day = OldDateParser.TranslateNum(n2);
        if (!OldDateParser.IsNull(match1.Groups[6].Value) && !OldDateParser.IsNull(match1.Groups[6].Value.Trim()))
        {
          string n3 = match1.Groups[6].Value;
          if (match1.Groups[6].Value.Length == 2)
            n3 = "20" + match1.Groups[6].Value;
          year = OldDateParser.TranslateNum(n3);
        }
        return new DateTime?(new DateTime(year, month, day));
      }
      Match match2 = new Regex("(?i)(\\(|\\b)(\\d{4})([/-])?(1[0-2]|0?[1-9])([/-])(3[0-1]|[0-2]?[0-9])(\\)|\\b)(\\s)*").Match(inputText);
      if (match2.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match2.Groups[0].Value);
        string n4 = match2.Groups[4].Value.Trim();
        string n5 = match2.Groups[6].Value;
        int month = OldDateParser.TranslateNum(n4);
        int day = OldDateParser.TranslateNum(n5);
        string group = match2.Groups[2].Value;
        if (!OldDateParser.IsNull(group))
        {
          string n6 = group;
          if (group.Length == 2)
            n6 = "20" + group;
          year = OldDateParser.TranslateNum(n6);
        }
        return new DateTime?(new DateTime(year, month, day));
      }
      Match match3 = new Regex("(?i)(\\(|\\b)(3[0-1]|[0-2]?[0-9])([/-])(3[0-1]|[0-2]?[0-9])([/-])?(\\d{2})(\\)|\\b)(\\s)*").Match(inputText);
      if (match3.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match3.Groups[0].Value);
        string n7;
        string n8;
        if (usOrUkDateFormat)
        {
          n7 = match3.Groups[2].Value.Trim();
          n8 = match3.Groups[4].Value;
        }
        else
        {
          n7 = match3.Groups[4].Value.Trim();
          n8 = match3.Groups[2].Value;
        }
        int month = OldDateParser.TranslateNum(n7);
        int day = OldDateParser.TranslateNum(n8);
        if (!string.IsNullOrEmpty(match3.Groups[6].Value))
        {
          string n9 = match3.Groups[6].Value;
          if (match3.Groups[6].Value.Length == 2)
            n9 = "20" + match3.Groups[6].Value;
          year = OldDateParser.TranslateNum(n9);
        }
        return new DateTime?(new DateTime(year, month, day));
      }
      Match match4 = new Regex("(?i)(?<![\\d|\\.])(\\(|\\b)(3[0-1]|[0-2]?[0-9])([/-])(3[0-1]|[0-2]?[0-9])([/-])?(\\)|\\b)(?!\\.)(\\s)*").Match(inputText);
      if (!match4.Success)
        return new DateTime?();
      OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match4.Groups[0].Value);
      string n10;
      string n11;
      if (usOrUkDateFormat)
      {
        n10 = match4.Groups[2].Value.Trim();
        n11 = match4.Groups[4].Value;
      }
      else
      {
        n10 = match4.Groups[4].Value.Trim();
        n11 = match4.Groups[2].Value;
      }
      int month1 = OldDateParser.TranslateNum(n10);
      int day1 = OldDateParser.TranslateNum(n11);
      if (!string.IsNullOrEmpty(match4.Groups[6].Value))
      {
        string n12 = match4.Groups[6].Value;
        if (match4.Groups[6].Value.Length == 2)
          n12 = "20" + match4.Groups[6].Value;
        year = OldDateParser.TranslateNum(n12);
      }
      DateTime? nullable1 = new DateTime?(new DateTime(year, month1, day1));
      DateTime? nullable2 = nullable1;
      DateTime dateTime = baseDate;
      if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() < dateTime ? 1 : 0) : 0) != 0)
        nullable1 = new DateTime?(OldDateParser.GetNextDate(month1, true, nullable1.Value));
      return nullable1;
    }

    public DateTime? MatchMonth(
      string inputText,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      Match match1 = new Regex(EnglishParser.GetMonthRecognition()).Match(inputText);
      int year = baseDate.Year;
      if (match1.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match1.Groups[0].Value);
        int enMonthCalendar = EnglishParser.GetEnMonthCalendar(match1.Groups[6].Value);
        bool hasDay = true;
        int day;
        if (!string.IsNullOrEmpty(match1.Groups[3].Value))
          day = OldDateParser.TranslateNum(match1.Groups[3].Value);
        else if (!string.IsNullOrEmpty(match1.Groups[20].Value))
        {
          day = OldDateParser.TranslateNum(match1.Groups[20].Value);
        }
        else
        {
          day = 1;
          hasDay = false;
        }
        if (!string.IsNullOrEmpty(match1.Groups[23].Value))
        {
          string n = match1.Groups[23].Value.Trim();
          int num = OldDateParser.TranslateNum(n);
          if (n.Length <= 3)
            num += 2000;
          year = num;
        }
        bool flag = string.IsNullOrEmpty(match1.Groups[23].Value);
        DateTime date = new DateTime(year, enMonthCalendar, day);
        if (date < baseDate & flag)
          date = OldDateParser.GetNextDate(enMonthCalendar, hasDay, date);
        return new DateTime?(date);
      }
      Match match2 = new Regex("(3[0-1]|[0-2]?[0-9])(st\\.?|ed\\.?|rd\\.?|th\\.?|nd\\.?)").Match(inputText);
      if (!match2.Success)
        return new DateTime?();
      OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match2.Groups[0].Value);
      int month = baseDate.Month;
      int day1 = OldDateParser.TranslateNum(match2.Groups[1].Value);
      DateTime dateTime = new DateTime(year, month, day1);
      if (dateTime < baseDate)
        dateTime = dateTime.AddMonths(1);
      return new DateTime?(dateTime);
    }

    private static string GetMonthRecognition()
    {
      return "(?i)(\\(|\\b)((\\s?(3[0-1]|[0-2]?[0-9]))(st\\.?|ed\\.?|rd\\.?|th\\.?|nd\\.?)?\\s*of\\s*)?(jan(\\.|uary|\\b)|feb(\\.|ruary|\\b)|mar(\\.|ch|\\b)|apr(\\.|il|\\b)|may(\\.|\\b)|jun(\\.|e|\\b)|jul(\\.|y|\\b)|aug(\\.|ust|\\b)|sep(t\\.|\\.|tember|\\b)|oct(\\.|ober|\\b)|nov(\\.|ember|\\b)|dec(\\.|ember|\\b))(\\s?(3[0-1]|[0-2]?[0-9]))?(st\\.?|ed\\.?|rd\\.?|th\\.?|nd\\.?)?,?( *(\\d{4}))?";
    }

    public DateTime? MatchWeekday(
      string inputText,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      Match match = new Regex(EnglishParser.GetDayOfWeekRecog()).Match(inputText);
      if (!match.Success)
        return new DateTime?();
      OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
      DateTime dateTime = baseDate.AddDays((double) ((int) baseDate.DayOfWeek * -1));
      int num = EnglishParser.TranslateDayOfWeek(match.Groups[2].Value);
      if (num != -1)
        dateTime = dateTime.AddDays((double) num);
      if (!OldDateParser.IsNull(match.Groups[1].Value))
      {
        switch (match.Groups[1].Value.ToLower().Trim())
        {
          case "next":
            dateTime = dateTime.AddDays(7.0);
            break;
          case "last":
            dateTime = dateTime.AddDays(-7.0);
            break;
        }
      }
      else if (dateTime < baseDate)
        dateTime = dateTime.AddDays(7.0);
      return new DateTime?(dateTime);
    }

    public DateTime? MatchSpecialDay(
      string inputText,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      string[] specialDayRecognition = EnglishParser.GetSpecialDayRecognition();
      for (int index = 0; index < specialDayRecognition.Length; ++index)
      {
        Match match = new Regex(specialDayRecognition[index]).Match(inputText);
        if (match.Success)
        {
          OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
          switch (index)
          {
            case 0:
              return new DateTime?(baseDate.AddDays(-1.0));
            case 1:
              return new DateTime?(baseDate);
            case 2:
              return new DateTime?(baseDate.AddDays(1.0));
            default:
              goto label_8;
          }
        }
      }
label_8:
      return new DateTime?();
    }

    public TickTickDuration MatchAdvanceTime(string matchStr, ref List<string> recgonizeStrList)
    {
      string str = matchStr;
      string input;
      try
      {
        input = Numerizer.Numerize(str);
      }
      catch (Exception ex)
      {
        input = matchStr;
      }
      Match match = new Regex("(?i)(remind|reminds)\\s+(\\S+ )?(\\d{1,2}|half a|half an|a|an)\\s+(minutes|minute|mins|min|hours|hour|h|days|day|weeks|week)\\s+(early|earlier|before|in advance)").Match(input);
      if (match.Success)
      {
        TickTickDuration duration = OldDateParser.GetDuration(match.Groups[3].Value, match.Groups[4].Value, false);
        if (duration != null)
          return duration;
      }
      if (!new Regex("(?i)(remind|reminds)\\s+(\\S+ ){1,2}(early|earlier)").Match(input).Success)
        return (TickTickDuration) null;
      TickTickDuration tickTickDuration = new TickTickDuration(0, 0, 0, 0, 0, 5, 0);
      tickTickDuration.SetIsPositive(false);
      return tickTickDuration;
    }

    public bool MatchAfterTime(
      string content,
      ref DateTime? baseTime,
      ref List<string> recgonizeStrList)
    {
      string input;
      try
      {
        input = Numerizer.Numerize(content);
      }
      catch (Exception ex)
      {
        input = content;
      }
      Match match1 = new Regex("(?i)(in|after)\\s+(\\d{1,3}|half a|half an|a|an)\\s*(hours|hour|hrs|hr|h)\\s+(\\d{1,3}|a|an)\\s+(minutes|minute|mins|min)").Match(input);
      if (match1.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match1.Groups[0].Value);
        string str1 = match1.Groups[2].Value;
        int num = !OldDateParser.IsAnOrA(str1) ? (!OldDateParser.IsHalf(str1) ? (!OldDateParser.IsNull(str1) ? OldDateParser.TranslateNum(str1) : 0) * 60 : 30) : 60;
        string str2 = match1.Groups[4].Value;
        TickTickDuration duration = OldDateParser.GetDuration(!OldDateParser.IsAnOrA(str2) ? num + (!OldDateParser.IsNull(str2) ? OldDateParser.TranslateNum(str2) : 0) : num + 1, match1.Groups[5].Value, true);
        if (duration != null && baseTime.HasValue)
        {
          baseTime = new DateTime?(OldDateParser.GetDateByDuration(baseTime.Value, duration));
          return true;
        }
      }
      Match match2 = new Regex("(?i)(\\d{1,3}|half a|half an|a|an)\\s*(hours|hour|hrs|hr|h)\\s+(\\d{1,3}|a|an)\\s+(minutes|minute|mins|min)\\s+later").Match(input);
      if (match2.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match2.Groups[0].Value);
        string str3 = match2.Groups[1].Value;
        int num = !OldDateParser.IsAnOrA(str3) ? (!OldDateParser.IsHalf(str3) ? (!OldDateParser.IsNull(str3) ? OldDateParser.TranslateNum(str3) : 0) * 60 : 30) : 60;
        string str4 = match2.Groups[3].Value;
        TickTickDuration duration = OldDateParser.GetDuration(!OldDateParser.IsAnOrA(str4) ? num + (!OldDateParser.IsNull(str4) ? OldDateParser.TranslateNum(str4) : 0) : num + 1, match2.Groups[4].Value, true);
        if (duration != null && baseTime.HasValue)
        {
          baseTime = new DateTime?(OldDateParser.GetDateByDuration(baseTime.Value, duration));
          return true;
        }
      }
      Match match3 = new Regex("(?i)(in|after)\\s+(\\d{1,3}|half a|half an|a|an)\\s*(minutes|minute|mins|min|hours|hour|hrs|hr|h|days|day|d|weeks|week|wk|months|month|years|year|yrs|yr)").Match(input);
      if (match3.Success)
      {
        OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match3.Groups[0].Value);
        TickTickDuration duration = OldDateParser.GetDuration(match3.Groups[2].Value, match3.Groups[3].Value);
        if (duration != null && baseTime.HasValue)
        {
          baseTime = new DateTime?(OldDateParser.GetDateByDuration(baseTime.Value, duration));
          return true;
        }
      }
      Match match4 = new Regex("(?i)(\\d{1,3}|half a|half an|a|an)\\s*(minutes|minute|mins|min|hours|hour|hrs|hr|h|days|day|d|weeks|week|wk|months|month|years|year|yrs|yr)\\s+later").Match(input);
      if (match4.Success)
      {
        TickTickDuration duration = OldDateParser.GetDuration(match4.Groups[1].Value, match4.Groups[2].Value);
        if (duration != null && baseTime.HasValue)
        {
          baseTime = new DateTime?(OldDateParser.GetDateByDuration(baseTime.Value, duration));
          return true;
        }
      }
      return false;
    }

    public bool MatchSpecialTime(
      string inputText,
      ref DateTime? parseDate,
      DateTime baseTime,
      ref List<string> recgonizeStrList)
    {
      bool flag = !parseDate.HasValue;
      Dictionary<string, int> enSpecialHourMap = EnglishParser.GetEnSpecialHourMap();
      foreach (string key in enSpecialHourMap.Keys)
      {
        Match match = new Regex(key).Match(inputText);
        if (match.Success)
        {
          OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
          int num = enSpecialHourMap[key];
          if (!parseDate.HasValue)
            parseDate = new DateTime?(baseTime.Date);
          parseDate = new DateTime?(DateUtils.SetHourAndMinuteOnly(parseDate.Value, 0, 0));
          parseDate = new DateTime?(parseDate.Value.AddHours((double) num));
          if (flag && parseDate.Value <= baseTime)
            parseDate = new DateTime?(parseDate.Value.AddDays(1.0));
          return true;
        }
      }
      return false;
    }

    private static Dictionary<string, int> GetEnSpecialHourMap()
    {
      return new Dictionary<string, int>()
      {
        {
          "(?i)\\bbreakfast\\b",
          8
        },
        {
          "(?i)\\blunch\\b",
          12
        },
        {
          "(?i)\\bsupper\\b",
          18
        },
        {
          "(?i)\\bbrunch\\b",
          10
        },
        {
          "(?i)\\bmorning\\b",
          9
        },
        {
          "(?i)\\bafternoon\\b",
          13
        },
        {
          "(?i)\\bevening\\b",
          17
        },
        {
          "(?i)\\bnight\\b",
          20
        },
        {
          "(?i)\\btonight\\b",
          21
        },
        {
          "(?i)\\bmidnight\\b",
          24
        },
        {
          "(?i)\\bnoon\\b",
          12
        }
      };
    }

    public bool MatchTime(
      string inputText,
      ref DateTime? parseDate,
      DateTime baseTime,
      ref List<string> recgonizeStrList)
    {
      bool flag1 = false;
      if (!parseDate.HasValue)
      {
        flag1 = true;
        parseDate = new DateTime?(new DateTime(baseTime.Year, baseTime.Month, baseTime.Day, baseTime.Hour, baseTime.Minute, baseTime.Second));
      }
      foreach (string pattern in EnglishParser.GetEnTimePattern())
      {
        Match match = new Regex(pattern).Match(inputText);
        if (match.Success)
        {
          OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
          int num = EnglishParser.MatchEnglishNum(match.Groups[2].Value);
          int minutes = 0;
          if (num <= 12 && !OldDateParser.IsNull(match.Groups[4].Value))
            num %= 12;
          bool flag2 = false;
          if (!string.IsNullOrEmpty(match.Groups[3].Value))
            minutes = OldDateParser.TranslateNum(match.Groups[3].Value);
          if (num <= 12)
          {
            if (!string.IsNullOrEmpty(match.Groups[4].Value))
            {
              switch (EnglishParser.AmpmToNumber(match.Groups[4].Value))
              {
                case 0:
                  if (num == 12)
                  {
                    num = 0;
                    break;
                  }
                  break;
                case 1:
                  if (num != 12)
                  {
                    num += 12;
                    break;
                  }
                  break;
              }
              flag2 = true;
            }
            else
            {
              if (new Regex("(?i)(\\bmorning|\\bmidnight)()").Match(inputText).Success)
                flag2 = true;
              if (new Regex("(?i)(\\bafternoon|\\bevening|\\bnight|\\btonight|\\bnoon)").Match(inputText).Success)
              {
                num += 12;
                flag2 = true;
              }
            }
          }
          parseDate = new DateTime?(DateUtils.SetHourAndMinuteOnly(parseDate.Value, 0, minutes));
          parseDate = new DateTime?(parseDate.Value.AddHours((double) (num % 24)));
          if (flag1)
          {
            if (num <= 12 && OldDateParser.IsNull(match.Groups[4].Value) && !flag2)
            {
              DateTime? nullable1 = parseDate;
              DateTime dateTime1 = baseTime;
              DateTime dateTime2;
              if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() <= dateTime1 ? 1 : 0) : 0) != 0)
              {
                ref DateTime? local = ref parseDate;
                dateTime2 = parseDate.Value;
                DateTime? nullable2 = new DateTime?(dateTime2.AddHours(num > 0 ? 12.0 : 24.0));
                local = nullable2;
              }
              DateTime? nullable3 = parseDate;
              dateTime2 = baseTime;
              if ((nullable3.HasValue ? (nullable3.GetValueOrDefault() <= dateTime2 ? 1 : 0) : 0) != 0)
              {
                ref DateTime? local = ref parseDate;
                dateTime2 = parseDate.Value;
                DateTime? nullable4 = new DateTime?(dateTime2.AddHours(12.0));
                local = nullable4;
              }
            }
            else if (EnglishParser.MatchEnglishNum(match.Groups[2].Value) != 0)
            {
              DateTime? nullable = parseDate;
              DateTime dateTime = baseTime;
              if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= dateTime ? 1 : 0) : 0) != 0)
                parseDate = new DateTime?(parseDate.Value.AddDays(1.0));
            }
          }
          return true;
        }
      }
      parseDate = new DateTime?();
      return false;
    }

    private static int AmpmToNumber(string amPmString)
    {
      int number = 1;
      if (amPmString == null)
        return number;
      string group = amPmString.ToLower().Trim();
      if (!OldDateParser.IsNull(group) && group.Contains("a"))
        number = 0;
      if (!OldDateParser.IsNull(group) && group.Contains("p"))
        number = 1;
      return number;
    }

    public RecurrencePattern MatchRepeat(
      string content,
      ref DateTime? parseDate,
      DateTime baseDate,
      ref List<string> recgonizeStrList)
    {
      RecurrencePattern recurrencePattern = new RecurrencePattern();
      Dictionary<string, FrequencyType> dictionary1 = new Dictionary<string, FrequencyType>()
      {
        {
          "(?i)\\b(every|each) ?\\w{0,6} days?\\b",
          FrequencyType.Daily
        },
        {
          "(?i)\\b(every|each) ?\\w{0,6} ?nights?\\b",
          FrequencyType.Daily
        },
        {
          "(?i)\\b(every|each) ?\\w{0,6} ?mornings?\\b",
          FrequencyType.Daily
        },
        {
          "(?i)\\b(every|each) ?\\w{0,6} ?evenings?\\b",
          FrequencyType.Daily
        },
        {
          "(?i)\\b(every|each) ?\\w{0,6} ?afternoons?\\b",
          FrequencyType.Daily
        },
        {
          "(?i)\\b(every|each) \\w{0,6} ?weeks?\\b",
          FrequencyType.Weekly
        },
        {
          "(?i)\\b(every|each) \\w{0,6} ?weekend(s)?\\b",
          FrequencyType.Weekly
        },
        {
          "(?i)\\b(every|each) \\w{0,6} ?weekday(s)?\\b",
          FrequencyType.Weekly
        },
        {
          "(?i)\\b(every|each) \\w{0,6} ?(mon|tues|tue|wednes|wed|thurs|thu|fri|satur|sat|sun)(day)?(s)?\\b",
          FrequencyType.Weekly
        },
        {
          "(?i)\\b(\\d{1,2})?(st|nd|rd|ed|th|)?(last)? ?(day of)? ?(every|each) \\w{0,6} ?months?\\b",
          FrequencyType.Monthly
        },
        {
          "(?i)\\b(every|each) \\w{0,6} ?years?\\b",
          FrequencyType.Yearly
        }
      };
      Dictionary<string, FrequencyType> dictionary2 = new Dictionary<string, FrequencyType>()
      {
        {
          "(?i)\\bdaily\\b",
          FrequencyType.Daily
        },
        {
          "(?i)\\beveryday\\b",
          FrequencyType.Daily
        },
        {
          "(?i)\\bweekly\\b",
          FrequencyType.Weekly
        },
        {
          "(?i)\\bmonthly\\b",
          FrequencyType.Monthly
        },
        {
          "(?i)\\byearly\\b",
          FrequencyType.Yearly
        }
      };
      foreach (string key in dictionary1.Keys)
      {
        Match match = new Regex(key).Match(content);
        if (match.Success)
        {
          OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
          int year = baseDate.Year;
          int month = baseDate.Month;
          FrequencyType frequencyType = dictionary1[key];
          recurrencePattern = new RecurrencePattern()
          {
            Frequency = frequencyType,
            Interval = EnglishParser.FindInterval(content)
          };
          switch (frequencyType)
          {
            case FrequencyType.Weekly:
              if (match.Groups.Count > 2)
              {
                int num = 0;
                string group = match.Groups[2].Value;
                if (!OldDateParser.IsNull(group) && group.Contains("mon"))
                  num = 1;
                else if (!OldDateParser.IsNull(group) && group.Contains("tue"))
                  num = 2;
                else if (!OldDateParser.IsNull(group) && group.Contains("wed"))
                  num = 3;
                else if (!OldDateParser.IsNull(group) && group.Contains("thu"))
                  num = 4;
                else if (!OldDateParser.IsNull(group) && group.Contains("fri"))
                  num = 5;
                else if (!OldDateParser.IsNull(group) && group.Contains("sat"))
                  num = 6;
                else if (!OldDateParser.IsNull(group) && group.Contains("sun"))
                  num = 0;
                DateTime dateTime1 = baseDate.AddDays((double) ((int) baseDate.DayOfWeek * -1));
                parseDate = new DateTime?(dateTime1.AddDays((double) num));
                DateTime? nullable = parseDate;
                DateTime dateTime2 = baseDate;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) != 0)
                  parseDate = new DateTime?(parseDate.Value.AddDays(7.0));
                recurrencePattern.ByWeekNo = new List<int>()
                {
                  num
                };
                return recurrencePattern;
              }
              break;
            case FrequencyType.Monthly:
              if (!OldDateParser.IsNull(match.Groups[3].Value))
              {
                recurrencePattern.ByMonthDay = new List<int>()
                {
                  -1
                };
                parseDate = new DateTime?(new DateTime(year, month, DateTime.DaysInMonth(year, month)));
              }
              else if (!OldDateParser.IsNull(match.Groups[1].Value))
              {
                int day = OldDateParser.TranslateNum(match.Groups[1].Value);
                recurrencePattern.ByMonthDay = new List<int>()
                {
                  day
                };
                parseDate = new DateTime?(new DateTime(year, month, day));
                DateTime? nullable = parseDate;
                DateTime dateTime = baseDate;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() < dateTime ? 1 : 0) : 0) != 0)
                  parseDate = new DateTime?(parseDate.Value.AddMonths(1));
              }
              recurrencePattern.Frequency = FrequencyType.Monthly;
              break;
          }
          return (RecurrencePattern) null;
        }
      }
      foreach (string key in dictionary2.Keys)
      {
        Match match = new Regex(key).Match(content);
        if (match.Success)
        {
          OldDateParser.AddMatchString((ICollection<string>) recgonizeStrList, match.Groups[0].Value);
          FrequencyType frequencyType = dictionary2[key];
          recurrencePattern.Frequency = frequencyType;
          recurrencePattern.Interval = 1;
          return recurrencePattern;
        }
      }
      return recurrencePattern;
    }

    private static int FindInterval(string inputText)
    {
      Dictionary<string, int> dictionary = new Dictionary<string, int>();
      string[] strArray = new string[12]
      {
        "one",
        "two",
        "three",
        "four",
        "five",
        "six",
        "seven",
        "eight",
        "nine",
        "ten",
        "eleven",
        "twelve"
      };
      for (int index = 0; index < strArray.Length; ++index)
      {
        dictionary.Add(strArray[index], index + 1);
        dictionary.Add((index + 1).ToString(), index + 1);
      }
      dictionary.Add("other", 2);
      Regex regex = new Regex("(?i)\\bevery (\\w*)\\b");
      int interval = 1;
      string input = inputText;
      Match match = regex.Match(input);
      if (match.Success && !string.IsNullOrEmpty(match.Groups[1].Value))
      {
        string str = match.Groups[1].Value;
        if (dictionary.ContainsKey(str))
        {
          interval = dictionary[str];
        }
        else
        {
          try
          {
            interval = int.Parse(str);
          }
          catch (Exception ex)
          {
          }
        }
      }
      return interval;
    }

    private static IEnumerable<string> GetEnTimePattern()
    {
      return (IEnumerable<string>) new string[5]
      {
        "(?i)\\s*(\\b)([0-2]?\\d)(?::([0-5]\\d))? ?([ap][\\.|m]m?\\.?)(\\s)*",
        "(?i)\\b(([0-2]?[0-9])[:|：]([0-5][0-9]))(\\b)(\\s)*",
        "(?i)\\b(([0-2]?\\d)() ?o'? ?clock) ?([ap]\\.?m\\.?)?\\b(\\s)*",
        "(?i)(\\bat)\\s{1,}([01]?\\d)()(())($|\\s)*",
        "(?i)(\\b)(one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)( ?)([ap]\\.?m?\\.?)?\\b(\\s)*"
      };
    }

    private static string[] GetSpecialDayRecognition()
    {
      return new string[3]
      {
        "(?i)(\\(|\\b)yesterday(\\)|\\b)(\\s)*",
        "(?i)(\\(|\\b)tod(ay|\\)|\\b|\\.)(\\s)*",
        "(?i)(\\(|\\b)tom(orrow|\\)|\\b|\\.)(\\s)*"
      };
    }

    private static int MatchEnglishNum(string s)
    {
      Dictionary<string, int> dictionary = new Dictionary<string, int>()
      {
        {
          "one",
          1
        },
        {
          "two",
          2
        },
        {
          "three",
          3
        },
        {
          "four",
          4
        },
        {
          "five",
          5
        },
        {
          "six",
          6
        },
        {
          "seven",
          7
        },
        {
          "eight",
          8
        },
        {
          "nine",
          9
        },
        {
          "ten",
          10
        },
        {
          "eleven",
          11
        },
        {
          "twelve",
          12
        }
      };
      string lower = s.Trim().ToLower();
      return dictionary.ContainsKey(lower) ? dictionary[lower] : OldDateParser.TranslateNum(s);
    }

    private static int TranslateDayOfWeek(string day)
    {
      Utils.TupleList<string, int> tupleList = new Utils.TupleList<string, int>()
      {
        {
          "mon",
          1
        },
        {
          "tue",
          2
        },
        {
          "wed",
          3
        },
        {
          "thu",
          4
        },
        {
          "fri",
          5
        },
        {
          "sat",
          6
        },
        {
          "sun",
          7
        }
      };
      string lower = day.Trim().ToLower();
      if (!string.IsNullOrEmpty(lower))
      {
        foreach (Tuple<string, int> tuple in (List<Tuple<string, int>>) tupleList)
        {
          if (lower.Contains(tuple.Item1))
            return tuple.Item2;
        }
      }
      return -1;
    }

    private static string GetDayOfWeekRecog()
    {
      return "(?i)(^|\\b|next |last |this )(sun|mon|tue|wed|thu|fri|sat)((day|sday|nesday|rsday|urday)(\\)|\\b)|(\\)|\\.|(\\b)))(\\s)*";
    }

    private static int GetEnMonthCalendar(string month)
    {
      Utils.TupleList<string, int> tupleList = new Utils.TupleList<string, int>()
      {
        {
          "jan",
          1
        },
        {
          "feb",
          2
        },
        {
          "mar",
          3
        },
        {
          "apr",
          4
        },
        {
          "may",
          5
        },
        {
          "jun",
          6
        },
        {
          "jul",
          7
        },
        {
          "aug",
          8
        },
        {
          "sep",
          9
        },
        {
          "oct",
          10
        },
        {
          "nov",
          11
        },
        {
          "dec",
          12
        }
      };
      string lower = month.Trim().ToLower();
      if (!string.IsNullOrEmpty(lower))
      {
        foreach (Tuple<string, int> tuple in (List<Tuple<string, int>>) tupleList)
        {
          if (lower.Contains(tuple.Item1))
            return tuple.Item2;
        }
      }
      return -1;
    }
  }
}
