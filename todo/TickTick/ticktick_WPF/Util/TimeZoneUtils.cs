// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TimeZoneUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;
using TickTickUtils;
using TickTickUtils.Lang;
using TimeZoneConverter;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class TimeZoneUtils
  {
    private static ConcurrentDictionary<string, string> _timeZoneNameDict;

    public static TimeData TransferToSelectedTimeZoneTime(TimeData origin)
    {
      if (origin == null)
        return new TimeData();
      if (origin.IsAllDay.GetValueOrDefault())
        return origin;
      if (origin.TimeZone?.TimeZone == null)
      {
        origin.TimeZone = new TimeZoneViewModel(TimeZoneInfo.Local);
        return origin;
      }
      if (origin.TimeZone.TimeZone.Equals(TimeZoneInfo.Local))
        return origin;
      TimeData selectedTimeZoneTime = TimeData.Clone(origin);
      selectedTimeZoneTime.StartDate = TimeZoneUtils.LocalToTargetZoneTime(selectedTimeZoneTime.StartDate, selectedTimeZoneTime.TimeZone.TimeZoneName);
      selectedTimeZoneTime.DueDate = TimeZoneUtils.LocalToTargetZoneTime(selectedTimeZoneTime.DueDate, selectedTimeZoneTime.TimeZone.TimeZoneName);
      return selectedTimeZoneTime;
    }

    public static TimeData TransferToLocalTime(TimeData origin)
    {
      if (origin == null)
        return new TimeData();
      if (origin.IsAllDay.GetValueOrDefault() || origin.TimeZone.TimeZone.Equals(TimeZoneInfo.Local))
        return origin;
      TimeData localTime = TimeData.Clone(origin);
      TimeData timeData1 = localTime;
      DateTime? nullable1;
      DateTime? nullable2;
      if (localTime.StartDate.HasValue)
      {
        nullable1 = localTime.StartDate;
        nullable2 = new DateTime?(TimeZoneUtils.ToLocalTime(nullable1.Value, origin.TimeZone.TimeZoneName));
      }
      else
        nullable2 = localTime.StartDate;
      timeData1.StartDate = nullable2;
      TimeData timeData2 = localTime;
      nullable1 = localTime.DueDate;
      DateTime? nullable3;
      if (nullable1.HasValue)
      {
        nullable1 = localTime.DueDate;
        nullable3 = new DateTime?(TimeZoneUtils.ToLocalTime(nullable1.Value, origin.TimeZone.TimeZoneName));
      }
      else
        nullable3 = localTime.DueDate;
      timeData2.DueDate = nullable3;
      return localTime;
    }

    public static DateTime ToLocalTime(DateTime time, string timeZoneName)
    {
      TimeZoneInfo timeZoneInfo = TimeZoneUtils.GetTimeZoneInfo(timeZoneName);
      try
      {
        return TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(time, DateTimeKind.Unspecified), timeZoneInfo, TimeZoneInfo.Local);
      }
      catch (Exception ex)
      {
        return time;
      }
    }

    public static DateTime LocalToTargetTzTime(DateTime time, string timeZoneName)
    {
      TimeZoneInfo timeZoneInfo = TimeZoneUtils.GetTimeZoneInfo(timeZoneName);
      try
      {
        return TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Local, timeZoneInfo);
      }
      catch (Exception ex)
      {
        return time;
      }
    }

    public static DateTime UtcToTargetTzTime(DateTime time, string toZone)
    {
      if (Utils.IsEmptyDate(time))
        return time;
      time = DateTime.SpecifyKind(time, DateTimeKind.Utc);
      TimeZoneInfo timeZoneInfo = TimeZoneUtils.GetTimeZoneInfo(toZone);
      return TimeZoneInfo.ConvertTime(time, timeZoneInfo);
    }

    public static DateTime ToUtcTime(DateTime time, string fromZone)
    {
      if (Utils.IsEmptyDate(time))
        return time;
      TimeZoneInfo timeZoneInfo = TimeZoneUtils.GetTimeZoneInfo(fromZone);
      return TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(time, DateTimeKind.Unspecified), timeZoneInfo, TimeZoneInfo.Utc);
    }

    public static TimeZoneInfo GetTimeZoneInfo(string name)
    {
      if (string.IsNullOrEmpty(name))
        return TimeZoneInfo.Local;
      try
      {
        return TZConvert.GetTimeZoneInfo(name);
      }
      catch (Exception ex)
      {
        return TimeZoneInfo.Local;
      }
    }

    public static string GetTimeZoneName(TimeZoneInfo tz)
    {
      if (TimeZoneUtils._timeZoneNameDict == null)
      {
        TimeZoneUtils._timeZoneNameDict = new ConcurrentDictionary<string, string>();
        if (!File.Exists(AppPaths.TimeZoneDictPath))
        {
          try
          {
            File.WriteAllText(AppPaths.TimeZoneDictPath, string.Empty);
          }
          catch (AggregateException ex)
          {
          }
        }
        else
        {
          foreach (string readAllLine in File.ReadAllLines(AppPaths.TimeZoneDictPath))
          {
            char[] chArray = new char[1]{ ':' };
            string[] strArray = readAllLine.Split(chArray);
            if (strArray.Length == 2 && !string.IsNullOrEmpty(strArray[0]) && strArray[1] != "Etc/UTC" && strArray[1] != "Asia/Shanghai")
              TimeZoneUtils._timeZoneNameDict[strArray[0]] = strArray[1];
          }
        }
      }
      string timeZoneName;
      if (!string.IsNullOrEmpty(tz.Id) && TimeZoneUtils._timeZoneNameDict.ContainsKey(tz.Id))
      {
        timeZoneName = TimeZoneUtils._timeZoneNameDict[tz.Id];
      }
      else
      {
        try
        {
          timeZoneName = TZConvert.WindowsToIana(tz.Id);
        }
        catch (Exception ex)
        {
          UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
          timeZoneName = "Asia/Shanghai";
        }
        TimeZoneUtils._timeZoneNameDict[tz.Id] = timeZoneName;
        if (timeZoneName != "Asia/Shanghai" && timeZoneName != "Etc/UTC")
          File.AppendAllLines(AppPaths.TimeZoneDictPath, (IEnumerable<string>) new string[1]
          {
            tz.Id + ":" + timeZoneName
          });
      }
      return timeZoneName;
    }

    public static string GetTimeZoneDisplayName(TimeZoneInfo tz)
    {
      return TimeZoneData.GetDisplayNameForTimeZone(tz, App.Ci.ToString());
    }

    public static DateTime ZoneToTargetTzTime(string zone, DateTime time, string targetZone)
    {
      if (zone == targetZone || string.IsNullOrEmpty(zone) || string.IsNullOrEmpty(targetZone))
        return time;
      TimeZoneInfo timeZoneInfo1 = TimeZoneUtils.GetTimeZoneInfo(zone);
      int num1 = !timeZoneInfo1.IsDaylightSavingTime(time) || !timeZoneInfo1.SupportsDaylightSavingTime ? 0 : 1;
      TimeZoneInfo timeZoneInfo2 = TimeZoneUtils.GetTimeZoneInfo(targetZone);
      int num2 = !timeZoneInfo2.IsDaylightSavingTime(time) || !timeZoneInfo2.SupportsDaylightSavingTime ? 0 : 1;
      return time.AddMinutes((timeZoneInfo2.BaseUtcOffset - timeZoneInfo1.BaseUtcOffset).TotalMinutes + (double) (num2 * 60) - (double) (num1 * 60));
    }

    public static DateTime? LocalToTargetZoneTime(DateTime? date, string timeZone)
    {
      if (!date.HasValue || date.Value.Year == 1)
        return date;
      TimeZoneInfo timeZoneInfo = TimeZoneUtils.GetTimeZoneInfo(timeZone);
      return new DateTime?(TimeZoneInfo.ConvertTime(date.Value, timeZoneInfo));
    }

    public static void ClearCache() => TimeZoneUtils._timeZoneNameDict?.Clear();
  }
}
