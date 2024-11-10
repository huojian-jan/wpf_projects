// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Resource.TimeZoneData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ticktick_WPF.Dal;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Resource
{
  public static class TimeZoneData
  {
    public static Dictionary<string, Dictionary<string, string>> DisplayNames = new Dictionary<string, Dictionary<string, string>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);

    static TimeZoneData()
    {
      try
      {
        string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (directoryName != null)
        {
          string path = Path.Combine(directoryName, "TimeZoneDisplayName.txt");
          if (File.Exists(path))
            TimeZoneData.DisplayNames = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(path));
        }
      }
      catch (Exception ex)
      {
      }
      TimeZoneData.LocalTimeZoneModel = new TimeZoneViewModel(TimeZoneInfo.Local);
      UtilLog.Info("LocalTimeZone:" + TimeZoneData.LocalTimeZoneModel.DisplayName + "   " + TimeZoneData.LocalTimeZoneModel.TimeZoneName);
    }

    public static TimeZoneViewModel LocalTimeZoneModel { get; set; }

    public static bool EnableTz => UserDao.IsPro();

    public static string GetDisplayNameForTimeZone(TimeZoneInfo tz, string ci)
    {
      ci = ci?.Replace("-", "_");
      if (!string.IsNullOrEmpty(ci) && TimeZoneData.DisplayNames.ContainsKey(ci))
      {
        Dictionary<string, string> displayName = TimeZoneData.DisplayNames[ci];
        if (tz != null && displayName.ContainsKey(tz.Id))
          return displayName[tz.Id];
      }
      return tz?.DisplayName;
    }
  }
}
