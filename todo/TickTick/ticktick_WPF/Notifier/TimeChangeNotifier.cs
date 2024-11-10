// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.TimeChangeNotifier
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.Views.Pomo;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public static class TimeChangeNotifier
  {
    private static DateTime _currentDate;
    private static Timer _timer;

    public static event EventHandler<EventArgs> DayChanged;

    public static void BeginTimer()
    {
      TimeChangeNotifier._timer?.Stop();
      TimeChangeNotifier._timer?.Close();
      int interval = (61 - DateTime.Now.Second) * 1000;
      TimeChangeNotifier._currentDate = DateTime.Now.Date;
      TimeChangeNotifier._timer = new Timer((double) interval);
      TimeChangeNotifier._timer.Elapsed += (ElapsedEventHandler) ((sender, args) =>
      {
        DateTime now = DateTime.Now;
        TimeChangeNotifier._timer.Interval = (double) ((61 - now.Second) * 1000);
        Application.Current?.Dispatcher.Invoke((Action) (() =>
        {
          ReminderBase.TryRemind(now);
          if (LocalSettings.Settings.ShowCountDown)
            LocalSettings.Settings.NotifyPropertyChanged("ShowCountDown");
          DataChangedNotifier.NotifyPeriodicCheck();
          TimeChangeNotifier.CheckDateChanged();
        }), DispatcherPriority.Background);
      });
      TimeChangeNotifier._timer.Start();
    }

    private static void CheckDateChanged()
    {
      if (!(TimeChangeNotifier._currentDate.Date != DateTime.Now.Date))
        return;
      TimeChangeNotifier._currentDate = DateTime.Now.Date;
      LocalSettings.Settings.StatisticsModel?.ClearToday();
      TickFocusManager.ReloadStatistics();
      TimeChangeNotifier.OnDayChanged();
    }

    private static void OnDayChanged()
    {
      TaskDefaultDao.InitCache();
      EventHandler<EventArgs> dayChanged = TimeChangeNotifier.DayChanged;
      if (dayChanged == null)
        return;
      dayChanged((object) null, (EventArgs) null);
    }
  }
}
