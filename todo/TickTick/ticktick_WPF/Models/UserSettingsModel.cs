// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.UserSettingsModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;
using System.Collections.Generic;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class UserSettingsModel : ErrorModel
  {
    public CalendarViewConf calendarViewConf;
    public string shortcutItemConfPc;
    public List<string> defaultADReminders;
    public int defaultDueDate;
    public int defaultPriority;
    public List<string> defaultReminds;
    public int defaultTimeDuration;
    public string defaultTimeMode;
    public string defaultProjectId;
    public List<string> defaultTags;
    public int defaultToAdd;
    public bool holidayEnabled;
    public string inboxColor;
    public bool isTimeZoneOptionEnabled;
    public string locale;
    public bool lunarEnabled;
    public bool nlpEnabled;
    public string[] notificationOptions;
    public int posOfOverdue;
    public bool removeDate;
    public bool removeTag;
    public bool showChecklist;
    public bool showCompleted;
    public bool showFutureTask;
    public bool showMeridiem;
    public string startDayOfWeek;
    public TabBarConf[] tabBars;
    public bool templateEnabled;
    public string timeZone;
    public bool weekNumbersEnabled;
    public string startWeekOfYear;
    public bool enableCountdown;
    public SmartProjectConf[] smartProjects;

    [Ignore]
    public bool nlpenabled { get; set; }
  }
}
