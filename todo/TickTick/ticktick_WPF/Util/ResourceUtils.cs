// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ResourceUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ResourceUtils
  {
    private static List<double> _columnWidths = new List<double>()
    {
      242.0,
      282.0,
      342.0
    };

    public static void SetKanbanColumnWidth()
    {
      double num = 282.0;
      int kbSize = LocalSettings.Settings.ExtraSettings.KbSize;
      if (kbSize < 3 && kbSize >= 0)
        num = ResourceUtils._columnWidths[kbSize];
      if (Application.Current == null)
        return;
      Application.Current.Resources[(object) "KanbanColumnWidth"] = (object) num;
    }

    public static double GetDoubleResource(string key, double defaultVal = 0.0)
    {
      return Application.Current.TryFindResource((object) key) is double resource ? resource : defaultVal;
    }
  }
}
