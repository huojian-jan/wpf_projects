// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.IParser
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public interface IParser
  {
    RecurrencePattern MatchRepeat(
      string matchStr,
      ref DateTime? date,
      DateTime baseDate,
      ref List<string> recgonizeStrList);

    bool MatchAfterTime(string matchStr, ref DateTime? baseTime, ref List<string> recgonizeStrList);

    TickTickDuration MatchAdvanceTime(string matchStr, ref List<string> recgonizeStrList);

    DateTime? MatchWeekday(string text, DateTime baseDate, ref List<string> recgonizeStrList);

    DateTime? MatchDate(string text, DateTime baseDate, ref List<string> recgonizeStrList);

    DateTime? MatchMonth(string text, DateTime baseDate, ref List<string> recgonizeStrList);

    DateTime? MatchSpecialDay(string text, DateTime baseDate, ref List<string> recgonizeStrList);

    bool MatchTime(
      string text,
      ref DateTime? parseDate,
      DateTime baseDate,
      ref List<string> recgonizeStrList);

    bool MatchSpecialTime(
      string text,
      ref DateTime? parseDate,
      DateTime baseDate,
      ref List<string> recgonizeStrList);
  }
}
