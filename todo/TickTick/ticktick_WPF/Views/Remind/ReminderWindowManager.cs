// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Remind.ReminderWindowManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Pomo;

#nullable disable
namespace ticktick_WPF.Views.Remind
{
  public class ReminderWindowManager
  {
    private static List<IRemindPop> _reminderWindowSortLists = new List<IRemindPop>();
    private static List<IRemindPop> _reminderWindowLists = new List<IRemindPop>();

    public static async Task TryAddNewWindow(ReminderModel reminder)
    {
      ReminderWindowManager._reminderWindowLists.FirstOrDefault<IRemindPop>((Func<IRemindPop, bool>) (window => window.IsSameReminder(reminder)))?.TryClose();
      if (reminder.Type == 4)
        ReminderWindowManager.AddHabitWindow(reminder);
      else
        ReminderWindowManager.AddWindow(reminder);
    }

    private static async void AddHabitWindow(ReminderModel reminder)
    {
      HabitReminderWindow habitReminderWindow = new HabitReminderWindow();
      habitReminderWindow.Closed += new EventHandler(ReminderWindowManager.OnPopupClosed);
      ReminderWindowManager._reminderWindowSortLists.Add((IRemindPop) habitReminderWindow);
      ReminderWindowManager._reminderWindowLists.Add((IRemindPop) habitReminderWindow);
      habitReminderWindow.InitData(reminder);
      habitReminderWindow.ShowWindow();
      ReminderWindowManager.SetReminderWindowsStyle();
    }

    private static void AddWindow(ReminderModel reminder)
    {
      ReminderPopup reminderPopup = new ReminderPopup(reminder);
      reminderPopup.Closed += new EventHandler(ReminderWindowManager.OnPopupClosed);
      ReminderWindowManager._reminderWindowSortLists.Add((IRemindPop) reminderPopup);
      ReminderWindowManager._reminderWindowLists.Add((IRemindPop) reminderPopup);
      reminderPopup.ShowWindow();
      ReminderWindowManager.SetReminderWindowsStyle();
    }

    public static void AddPomoReminder(bool relax, bool auto)
    {
      PomoReminderWindow pomoReminderWindow = new PomoReminderWindow();
      pomoReminderWindow.Closed += new EventHandler(ReminderWindowManager.OnPopupClosed);
      pomoReminderWindow.IsRelax = relax;
      pomoReminderWindow.IsAutomatic = auto;
      pomoReminderWindow.SetIcon();
      ReminderWindowManager.CloseAllPomoReminders();
      ReminderWindowManager._reminderWindowSortLists.Add((IRemindPop) pomoReminderWindow);
      ReminderWindowManager._reminderWindowLists.Add((IRemindPop) pomoReminderWindow);
      ReminderWindowManager.SetReminderWindowsStyle();
      pomoReminderWindow.ShowWindow();
    }

    public static void OnWindowMoved(IRemindPop rem)
    {
      if (!ReminderWindowManager._reminderWindowSortLists.Contains(rem))
        return;
      ReminderWindowManager._reminderWindowSortLists.Remove(rem);
      ReminderWindowManager.SetReminderWindowsStyle();
    }

    private static void OnPopupClosed(object sender, EventArgs e)
    {
      if (sender is IRemindPop remindPop)
      {
        ReminderWindowManager._reminderWindowSortLists.Remove(remindPop);
        ReminderWindowManager._reminderWindowLists.Remove(remindPop);
      }
      ReminderWindowManager.SetReminderWindowsStyle();
    }

    private static void SetReminderWindowsStyle()
    {
      List<IRemindPop> list = ReminderWindowManager._reminderWindowSortLists.ToList<IRemindPop>();
      for (int index = 0; index < list.Count; ++index)
      {
        if (list.Count - index <= 3)
          list[index].SetDisplayStyle(list.Count - index - 1);
        else
          list[index].TryHide();
      }
    }

    public static void CloseAllPomoReminders()
    {
      foreach (IRemindPop remindPop in ReminderWindowManager._reminderWindowLists.ToList<IRemindPop>())
      {
        if (remindPop is PomoReminderWindow pomoReminderWindow)
          pomoReminderWindow.TryClose();
      }
    }
  }
}
