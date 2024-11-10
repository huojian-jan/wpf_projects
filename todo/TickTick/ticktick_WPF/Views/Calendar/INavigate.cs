// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.INavigate
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public interface INavigate
  {
    DateTime Today();

    DateTime Next();

    DateTime Last();

    DateTime GoTo(DateTime dateTime, bool checkStart = true, bool scrollToNow = false);

    (DateTime, DateTime) GetTimeSpan();

    void Reload(bool force = false, int delay = 50, bool setWeekend = false);

    void ShowAddWindow(DateTime dateTime);

    List<CalendarDisplayModel> ExistTasks(HashSet<string> taskIds);

    void RemoveItem(CalendarDisplayViewModel model);
  }
}
