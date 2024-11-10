// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.CalendarConfigHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public static class CalendarConfigHelper
  {
    private static WidgetSettings _settingWindow;

    public static WidgetSettings TryShowSettings(WidgetViewModel model)
    {
      if (CalendarConfigHelper._settingWindow == null)
      {
        CalendarConfigHelper._settingWindow = new WidgetSettings(model, true);
        CalendarConfigHelper._settingWindow.Closed -= new EventHandler(CalendarConfigHelper.OnWindowClosed);
        CalendarConfigHelper._settingWindow.Closed += new EventHandler(CalendarConfigHelper.OnWindowClosed);
      }
      CalendarConfigHelper._settingWindow.Show();
      return CalendarConfigHelper._settingWindow;
    }

    public static WidgetSettings TryRegisterOnReload(WidgetViewModel model)
    {
      if (CalendarConfigHelper._settingWindow == null)
        return (WidgetSettings) null;
      CalendarConfigHelper._settingWindow.DataContext = (object) model;
      return CalendarConfigHelper._settingWindow;
    }

    private static void OnWindowClosed(object sender, EventArgs e)
    {
      CalendarConfigHelper._settingWindow = (WidgetSettings) null;
    }
  }
}
