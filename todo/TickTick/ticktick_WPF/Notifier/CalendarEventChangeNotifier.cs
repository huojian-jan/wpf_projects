// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.CalendarEventChangeNotifier
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public static class CalendarEventChangeNotifier
  {
    public static event EventHandler<TextExtra> TitleChanged;

    public static event EventHandler<TextExtra> SummaryChanged;

    public static event EventHandler<string> Deleted;

    public static event EventHandler<string> Restored;

    public static event EventHandler<CalendarEventModel> Changed;

    public static event EventHandler RemoteChanged;

    public static void NotifyTitleChanged(string eventId, string text)
    {
      EventHandler<TextExtra> titleChanged = CalendarEventChangeNotifier.TitleChanged;
      if (titleChanged == null)
        return;
      titleChanged((object) null, new TextExtra()
      {
        Id = eventId,
        Text = text
      });
    }

    public static void NotifySummaryChanged(string eventId, string text)
    {
      EventHandler<TextExtra> summaryChanged = CalendarEventChangeNotifier.SummaryChanged;
      if (summaryChanged == null)
        return;
      summaryChanged((object) null, new TextExtra()
      {
        Id = eventId,
        Text = text
      });
    }

    public static void NotifyEventDeleted(string eventId)
    {
      EventHandler<string> deleted = CalendarEventChangeNotifier.Deleted;
      if (deleted == null)
        return;
      deleted((object) null, eventId);
    }

    public static void NotifyEventChanged(CalendarEventModel model)
    {
      EventHandler<CalendarEventModel> changed = CalendarEventChangeNotifier.Changed;
      if (changed == null)
        return;
      changed((object) null, model);
    }

    public static void NotifyEventRestored(string eventId)
    {
      EventHandler<string> restored = CalendarEventChangeNotifier.Restored;
      if (restored == null)
        return;
      restored((object) null, eventId);
    }

    public static void NotifyRemoteChanged()
    {
      EventHandler remoteChanged = CalendarEventChangeNotifier.RemoteChanged;
      if (remoteChanged != null)
        remoteChanged((object) null, (EventArgs) null);
      if (ABTestManager.IsNewRemindCalculate())
        EventReminderCalculator.InitAllEventsReminderTimes();
      else
        ReminderCalculator.AssembleReminders();
    }
  }
}
