// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.SettingsMenuHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Twitter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class SettingsMenuHelper
  {
    public static bool IsNewFeature(SettingsType type)
    {
      if (type != SettingsType.Matrix)
        return false;
      bool flag = !LocalSettings.Settings.CheckedNewFeature.Contains(SettingsType.Matrix.ToString()) && DateTime.Today < new DateTime(2022, 5, 5);
      if (flag && LocalSettings.Settings.ShowMatrix)
      {
        flag = false;
        SettingsMenuHelper.CheckNewFeature(SettingsType.Matrix);
      }
      return flag;
    }

    public static void CheckNewFeature(SettingsType type)
    {
      List<string> list = ((IEnumerable<string>) LocalSettings.Settings.CheckedNewFeature.Split(';')).ToList<string>();
      list.Remove("");
      if (!list.Contains(type.ToString()))
        list.Add(type.ToString());
      LocalSettings.Settings.CheckedNewFeature = list.Join<string>(";");
    }
  }
}
