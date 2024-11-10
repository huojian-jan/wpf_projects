// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.GlobalEventManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public static class GlobalEventManager
  {
    public static event EventHandler<double> PomoWindowOpacityChanged;

    public static event EventHandler<string> PomoWindowThemeChanged;

    public static event EventHandler PomoDailyGoalChanged;

    public static event EventHandler TimeFormatChanged;

    public static event EventHandler ReloadCalendar;

    public static event EventHandler ModifyRecurrenceCompleted;

    public static event EventHandler<RemindMessage> RemindHandled;

    public static event EventHandler ArrangeFilterTypeChanged;

    public static event EventHandler ArrangeTaskSortTypeChanged;

    public static event EventHandler QuadrantSortChanged;

    public static event EventHandler StickyOpacityChanged;

    public static event EventHandler WidgetOpenChanged;

    public static event EventHandler<string> TimelineSetChanged;

    public static void NotifyWidgetOpenChanged()
    {
      EventHandler widgetOpenChanged = GlobalEventManager.WidgetOpenChanged;
      if (widgetOpenChanged == null)
        return;
      widgetOpenChanged((object) null, (EventArgs) null);
    }

    public static void NotifyPomoOpacityChanged(double opacity)
    {
      EventHandler<double> windowOpacityChanged = GlobalEventManager.PomoWindowOpacityChanged;
      if (windowOpacityChanged == null)
        return;
      windowOpacityChanged((object) null, opacity);
    }

    public static void NotifyPomoThemeChanged(string theme)
    {
      EventHandler<string> windowThemeChanged = GlobalEventManager.PomoWindowThemeChanged;
      if (windowThemeChanged == null)
        return;
      windowThemeChanged((object) null, theme);
    }

    public static void NotifyPomoDailyGoalChanged()
    {
      EventHandler dailyGoalChanged = GlobalEventManager.PomoDailyGoalChanged;
      if (dailyGoalChanged == null)
        return;
      dailyGoalChanged((object) null, (EventArgs) null);
    }

    public static void NotifyTimeFormatChanged()
    {
      EventHandler timeFormatChanged = GlobalEventManager.TimeFormatChanged;
      if (timeFormatChanged == null)
        return;
      timeFormatChanged((object) null, (EventArgs) null);
    }

    public static void NotifyRemindHandled(RemindMessage remindMessage)
    {
      EventHandler<RemindMessage> remindHandled = GlobalEventManager.RemindHandled;
      if (remindHandled == null)
        return;
      remindHandled((object) null, remindMessage);
    }

    public static void NotifyReloadCalendar()
    {
      EventHandler reloadCalendar = GlobalEventManager.ReloadCalendar;
      if (reloadCalendar == null)
        return;
      reloadCalendar((object) null, (EventArgs) null);
    }

    public static void NotifyModifyRecurrenceCompleted()
    {
      EventHandler recurrenceCompleted = GlobalEventManager.ModifyRecurrenceCompleted;
      if (recurrenceCompleted == null)
        return;
      recurrenceCompleted((object) null, (EventArgs) null);
    }

    public static void NotifyArrangeTaskDateTypeChanged()
    {
      EventHandler filterTypeChanged = GlobalEventManager.ArrangeFilterTypeChanged;
      if (filterTypeChanged == null)
        return;
      filterTypeChanged((object) null, (EventArgs) null);
    }

    public static void NotifyArrangeDisplayTypeChanged()
    {
      EventHandler taskSortTypeChanged = GlobalEventManager.ArrangeTaskSortTypeChanged;
      if (taskSortTypeChanged == null)
        return;
      taskSortTypeChanged((object) null, (EventArgs) null);
    }

    public static void NotifyQuadrantSortChanged(object sender)
    {
      EventHandler quadrantSortChanged = GlobalEventManager.QuadrantSortChanged;
      if (quadrantSortChanged == null)
        return;
      quadrantSortChanged(sender, (EventArgs) null);
    }

    internal static void OnStickyOpacityChanged()
    {
      EventHandler stickyOpacityChanged = GlobalEventManager.StickyOpacityChanged;
      if (stickyOpacityChanged == null)
        return;
      stickyOpacityChanged((object) null, (EventArgs) null);
    }

    public static void NotifyTimelineSetChanged(string name)
    {
      EventHandler<string> timelineSetChanged = GlobalEventManager.TimelineSetChanged;
      if (timelineSetChanged == null)
        return;
      timelineSetChanged((object) null, name);
    }
  }
}
