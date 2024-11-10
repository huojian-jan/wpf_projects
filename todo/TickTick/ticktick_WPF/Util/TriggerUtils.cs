// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TriggerUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class TriggerUtils
  {
    private static ConcurrentDictionary<string, TimeSpan> _triggerSpans = new ConcurrentDictionary<string, TimeSpan>();

    public static TimeSpan ParseTrigger(string trigger)
    {
      if (string.IsNullOrEmpty(trigger))
        return new TimeSpan();
      if (TriggerUtils._triggerSpans.ContainsKey(trigger))
        return TriggerUtils._triggerSpans[trigger];
      TimeSpan trigger1 = TriggerUtils.IsLegacyTrigger(trigger) ? TriggerUtils.LegacyTrigger2TimeSpan(trigger) : TriggerUtils.Trigger2TimeSpan(trigger);
      TriggerUtils._triggerSpans[trigger] = trigger1;
      return trigger1;
    }

    public static string ConvertLegacyTrigger(string trigger)
    {
      try
      {
        if (string.IsNullOrEmpty(trigger))
          return string.Empty;
        if (trigger == "TRIGGER:PT0S")
          return "TRIGGER:P0DT9H0M0S";
        if (TriggerUtils.IsLegacyTrigger(trigger))
        {
          Match match = new Regex("(-)?P((\\d{0,})Y)?((\\d{0,})M)?((\\d{0,})W)?((\\d{0,})D)?T?((\\d{0,})H)?((\\d{0,})M)?((\\d{0,})S)?").Match(trigger);
          if (match.Success)
          {
            int result1;
            int.TryParse(match.Groups[7].ToString(), out result1);
            int result2;
            int.TryParse(match.Groups[9].ToString(), out result2);
            return "TRIGGER:-P" + (result1 * 7 + result2 - 1).ToString() + "DT15H0M0S";
          }
        }
        return trigger;
      }
      catch (Exception ex)
      {
        return trigger;
      }
    }

    private static bool IsLegacyTrigger(string trigger) => !trigger.Contains("0S");

    private static TimeSpan Trigger2TimeSpan(string trigger)
    {
      Match match = new Regex("(-)?P((\\d{0,})Y)?((\\d{0,})M)?((\\d{0,})W)?((\\d{0,})D)?T?((\\d{0,})H)?((\\d{0,})M)?((\\d{0,})S)?").Match(trigger);
      if (!match.Success)
        return TimeSpan.Zero;
      int num = match.Groups[1].ToString() == "-" ? 1 : 0;
      int result1;
      int.TryParse(match.Groups[7].ToString(), out result1);
      int result2;
      int.TryParse(match.Groups[9].ToString(), out result2);
      int result3;
      int.TryParse(match.Groups[11].ToString(), out result3);
      int result4;
      int.TryParse(match.Groups[13].ToString(), out result4);
      int days = result1 * 7 + result2;
      return num == 0 ? new TimeSpan(0, result3 * -1, result4 * -1, 0) : new TimeSpan(days, result3, result4, 0);
    }

    private static TimeSpan LegacyTrigger2TimeSpan(string trigger)
    {
      if (!string.IsNullOrEmpty(trigger) && !(trigger == "TRIGGER:PT0S"))
      {
        int num1;
        int result;
        if ((num1 = trigger.IndexOf("D", StringComparison.Ordinal)) != -1 && num1 > 10 && int.TryParse(trigger.Substring(10, num1 - 10), out result))
          return new TimeSpan(result, 0, 0, 0);
        int num2;
        if ((num2 = trigger.IndexOf("W", StringComparison.Ordinal)) != -1 && num2 > 10 && int.TryParse(trigger.Substring(10, num2 - 10), out result))
          return new TimeSpan(result * 7, 0, 0, 0);
        int num3;
        if ((num3 = trigger.IndexOf("M", StringComparison.Ordinal)) != -1 && num3 > 11 && int.TryParse(trigger.Substring(11, num3 - 11), out result))
          return new TimeSpan(0, result, 0);
        int num4;
        if ((num4 = trigger.IndexOf("H", StringComparison.Ordinal)) != -1 && num4 > 11 && int.TryParse(trigger.Substring(11, num4 - 11), out result))
          return new TimeSpan(result, 0, 0);
      }
      return TimeSpan.Zero;
    }

    public static int TriggerToReminder(string trigger)
    {
      Match match = new Regex("(-)?P((\\d{0,})Y)?((\\d{0,})M)?((\\d{0,})W)?T?((\\d{0,})D)?T?((\\d{0,})H)?((\\d{0,})M)?((\\d{0,})S)?").Match(trigger);
      if (!match.Success)
        return 0;
      int num = 1;
      if (!trigger.Contains("-"))
        num = -1;
      int result1;
      int.TryParse(match.Groups[7].ToString(), out result1);
      int result2;
      int.TryParse(match.Groups[9].ToString(), out result2);
      int result3;
      int.TryParse(match.Groups[11].ToString(), out result3);
      int result4;
      int.TryParse(match.Groups[13].ToString(), out result4);
      return num * (result1 * 10080 + result2 * 1440 + result3 * 60 + result4);
    }

    public static string ReminderToTrigger(int minutes)
    {
      if (minutes == 0)
        return "TRIGGER:PT0S";
      if (minutes % 10080 == 0)
      {
        int num = minutes / 10080;
        return num <= 0 ? string.Format("TRIGGER:PT{0}WT", (object) Math.Abs(num)) : string.Format("TRIGGER:-P{0}WT", (object) num);
      }
      if (minutes % 1440 == 0)
      {
        int num = minutes / 1440;
        return num <= 0 ? string.Format("TRIGGER:P{0}DT", (object) Math.Abs(num)) : string.Format("TRIGGER:-P{0}DT", (object) num);
      }
      if (minutes % 60 == 0)
      {
        int num = minutes / 60;
        return num <= 0 ? string.Format("TRIGGER:PT{0}H", (object) Math.Abs(num)) : string.Format("TRIGGER:-PT{0}H", (object) num);
      }
      return minutes <= 0 ? string.Format("TRIGGER:PT{0}M", (object) Math.Abs(minutes)) : string.Format("TRIGGER:-PT{0}M", (object) minutes);
    }
  }
}
