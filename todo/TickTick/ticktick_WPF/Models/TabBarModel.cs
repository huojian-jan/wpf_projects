// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TabBarModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TabBarModel
  {
    public string name { get; set; }

    public string status { get; set; }

    public long sortOrder { get; set; }

    public static List<TabBarModel> InitTabBars()
    {
      return new List<TabBarModel>()
      {
        new TabBarModel()
        {
          name = "TASK",
          sortOrder = 0L,
          status = "active"
        },
        new TabBarModel()
        {
          name = "CALENDAR",
          sortOrder = 1L,
          status = "active"
        },
        new TabBarModel()
        {
          name = "MATRIX",
          sortOrder = 2L,
          status = TabBarModel.GetMatrixInitStatus()
        },
        new TabBarModel()
        {
          name = "POMO",
          sortOrder = 3L,
          status = LocalSettings.Settings.PomoLocalSetting.PomoTimer ? "active" : "inactive"
        },
        new TabBarModel()
        {
          name = "HABIT",
          sortOrder = 4L,
          status = LocalSettings.Settings.SettingsModel.ShowHabit ? "active" : "inactive"
        },
        new TabBarModel()
        {
          name = "SEARCH",
          sortOrder = 5L,
          status = "active"
        }
      };
    }

    private static string GetMatrixInitStatus()
    {
      string a = LocalSettings.Settings.UserPreference?.desktop_conf?.tabs?.Contains("matrix").GetValueOrDefault() ? "active" : "inactive";
      if (string.Equals(a, "inactive") && ABTestManager.IsTabWithMatrix())
        a = "active";
      return a;
    }

    public static bool IsActive(string name, List<TabBarModel> bars)
    {
      name = name.ToLower(CultureInfo.InvariantCulture);
      if (bars == null || bars.Count == 0)
        bars = TabBarModel.InitTabBars();
      TabBarModel tabBarModel = bars.FirstOrDefault<TabBarModel>((Func<TabBarModel, bool>) (t => t.name.ToLower(CultureInfo.InvariantCulture) == name));
      return tabBarModel != null && tabBarModel.status == "active";
    }

    public static long GetTabBarSort(string name, List<TabBarModel> bars)
    {
      name = name.ToLower(CultureInfo.InvariantCulture);
      if (bars == null || bars.Count == 0)
        bars = TabBarModel.InitTabBars();
      TabBarModel tabBarModel = bars.FirstOrDefault<TabBarModel>((Func<TabBarModel, bool>) (t => t.name.ToLower(CultureInfo.InvariantCulture) == name));
      return tabBarModel != null ? tabBarModel.sortOrder : bars.Max<TabBarModel>((Func<TabBarModel, long>) (m => m.sortOrder)) + 1L;
    }
  }
}
