// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetConfigHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public static class WidgetConfigHelper
  {
    private static readonly Dictionary<string, WidgetSettings> SettingWindows = new Dictionary<string, WidgetSettings>();

    public static WidgetSettings TryGetSettings(WidgetViewModel model)
    {
      WidgetSettings settings;
      WidgetConfigHelper.SettingWindows.TryGetValue(model.Id, out settings);
      return settings;
    }

    public static WidgetSettings TryShowSettings(WidgetViewModel model)
    {
      WidgetSettings widgetSettings1;
      WidgetConfigHelper.SettingWindows.TryGetValue(model.Id, out widgetSettings1);
      if (widgetSettings1 == null)
      {
        WidgetSettings widgetSettings2 = new WidgetSettings(model, model.IsSingleMode);
        widgetSettings2.Closed += new EventHandler(WidgetConfigHelper.OnWindowClosed);
        WidgetConfigHelper.SettingWindows.Add(model.Id, widgetSettings2);
        widgetSettings2.Show();
        return widgetSettings2;
      }
      widgetSettings1.Rebind(model);
      return widgetSettings1;
    }

    private static void OnWindowClosed(object sender, EventArgs e)
    {
      WidgetSettings widgetSettings = (WidgetSettings) sender;
      WidgetConfigHelper.SettingWindows.Remove(widgetSettings.WidgetId);
    }
  }
}
