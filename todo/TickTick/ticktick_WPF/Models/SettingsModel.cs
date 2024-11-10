// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SettingsModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;
using System.ComponentModel;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class SettingsModel : BaseModel
  {
    public string UserId { get; set; } = "";

    public double MainWindowHeight { get; set; } = 780.0;

    public double MainWindowWidth { get; set; } = 1140.0;

    public long CheckPoint { get; set; }

    public int SmartListAll { get; set; } = 1;

    public int SmartListToday { get; set; }

    public int SmartListTomorrow { get; set; } = 1;

    public int SmartList7Day { get; set; }

    public int SmartListForMe { get; set; } = 2;

    public int SmartListComplete { get; set; }

    public int SmartListAbandoned { get; set; } = 2;

    public int SmartListTrash { get; set; }

    public string ShortcutOpenOrClose { get; set; } = "Ctrl+Shift+E";

    public string ShortcutAddTask { get; set; } = "Alt+Shift+A";

    public bool IsPro { get; set; }

    public string SortTypeOfAllProject { get; set; } = "dueDate,dueDate";

    public string SortTypeOfInbox { get; set; } = "sortOrder,sortOrder";

    public string SortTypeOfAssignMe { get; set; } = "dueDate,dueDate";

    public string SortTypeOfToday { get; set; } = "dueDate,dueDate";

    public string SortTypeOfTomorrow { get; set; } = "dueDate,dueDate";

    public string SortTypeOfWeek { get; set; } = "dueDate,dueDate";

    public string ClosedSectionStatus { get; set; } = "False";

    public bool MainWindowTopmost { get; set; }

    public int SmartListTag { get; set; }

    public int SmartListInbox { get; set; }

    public int ShowCustomSmartList { get; set; }

    public long SyncPoint { get; set; }

    public string CalendarFilterData { get; set; } = "";

    public string ArrangeTaskFilterData { get; set; } = "";

    public int PomoDuration { get; set; } = 25;

    public int ShortBreakDuration { get; set; } = 5;

    public int LongBreakDuration { get; set; } = 15;

    public int LongBreakEvery { get; set; } = 4;

    public bool ShowDailyPomo { get; set; }

    public bool ShowDailyDuration { get; set; }

    public string PomoShortcut { get; set; } = "Ctrl+Alt+P";

    public int DailyPomoGoals { get; set; } = 4;

    [NotNull]
    [DefaultValue("120")]
    public int DailyFocusDuration { get; set; } = 120;

    public double ProjectPanelWidth { get; set; } = 245.0;

    public int SortOrderOfAll { get; set; }

    public int SortOrderOfToday { get; set; } = 1000;

    public int SortOrderOfTomorrow { get; set; } = 2000;

    public int SortOrderOfWeek { get; set; } = 3000;

    public int SortOrderOfCalendar { get; set; } = 4000;

    public int SortOrderOfInbox { get; set; } = 6000;

    public int SortOrderOfTag { get; set; } = 7000;

    public int SortOrderOfEvent { get; set; } = 8000;

    public int SortOrderOfAssign { get; set; } = 5000;

    public bool ShowReminder { get; set; } = true;

    public string AutoTagUserIds { get; set; } = "";

    public string SelectProjectId { get; set; } = "";

    public bool NeedResetPassword { get; set; }

    public string LockShortcut { get; set; } = "Ctrl+Shift+L";

    public string NeedResetUserId { get; set; } = "";

    public bool NeedShowTutorial { get; set; }

    public bool Maxmized { get; set; }

    public double CheckRemindDate { get; set; }

    public string ShortcutPin { get; set; } = "";

    public bool ShowTimelineTooltip { get; set; } = true;

    public string LastCheckSyncDate { get; set; } = "";

    public string RecentProjects { get; set; } = "";

    public int CollapsedStart { get; set; } = 7;

    public int CollapsedEnd { get; set; } = 21;

    public bool ShowDetails { get; set; }

    public string AllDayCustomRemind { get; set; } = "";

    public string TimeCustomRemind { get; set; } = "";

    public bool ExpandPersonalSection { get; set; } = true;

    public int SmartListSummary { get; set; } = 1;

    [NotNull]
    [DefaultValue("\"Jingle\"")]
    public string CompletionSound { get; set; } = "Jingle";

    public bool UpdateHabit { get; set; }

    public bool EnableRingtone { get; set; } = true;

    public double WindowTop { get; set; } = -1.0;

    public double WindowLeft { get; set; } = -1.0;

    [NotNull]
    [DefaultValue("\"White\"")]
    public string ThemeId { get; set; } = "White";

    public string LastProRemindeTime { get; set; } = "";

    public string CreateStickyShortCut { get; set; } = "";

    public double CalendarHourHeight { get; set; } = 36.0;

    public double CalendarMinMinute { get; set; } = 30.0;

    public int ArrangeDisplayType { get; set; }

    public double WeekAllDayHeight { get; set; }

    public bool ShowArrangeReminder { get; set; } = true;

    [NotNull]
    [DefaultValue("0")]
    public long UpgradeCheckPoint { get; set; }

    public bool ShowReminderInClient { get; set; } = true;

    [NotNull]
    [DefaultValue("0")]
    public bool HabitInToday { get; set; } = true;

    public bool HabitInCal { get; set; } = true;

    public long HabitDefaultOrder { get; set; } = long.MaxValue;

    public bool ShowHabit { get; set; } = true;

    public bool ShowCountDown { get; set; }

    [NotNull]
    [DefaultValue("1")]
    public bool DateParsing { get; set; } = true;

    [NotNull]
    [DefaultValue("1")]
    public bool RemoveTimeText { get; set; } = true;

    public bool KeepTagsInText { get; set; }

    public bool HideComplete { get; set; }

    public bool ShowRepeatCircles { get; set; }

    public bool ShowFocusRecord { get; set; }

    public bool ShowSubtasks { get; set; }

    [NotNull]
    [DefaultValue("\"Sunday\"")]
    public string WeekStartFrom { get; set; } = "Sunday";

    [NotNull]
    [DefaultValue("\"System\"")]
    public string TimeFormat { get; set; } = "System";

    public int PosOfOverdue { get; set; }

    [NotNull]
    [DefaultValue("\"\"")]
    public string NotificationOptions { get; set; } = "";

    [NotNull]
    [DefaultValue("1")]
    public bool EnableHoliday { get; set; } = true;

    [NotNull]
    [DefaultValue("1")]
    public bool EnableLunar { get; set; } = true;

    public bool ShowWeek { get; set; }

    public bool ShowCheckListInCal { get; set; }

    [NotNull]
    [DefaultValue("1")]
    public bool ShowCompletedInCal { get; set; } = true;

    [NotNull]
    [DefaultValue("\"list\"")]
    public string CellColorType { get; set; } = "list";

    public bool EnableTimeZone { get; set; }

    public string StartWeekOfYear { get; set; } = "";

    [NotNull]
    [DefaultValue("1")]
    public double DetailListDivide { get; set; } = 0.8;

    public bool IsNoteEnabled { get; set; }

    public bool ProjectTypeChangeNotified { get; set; }

    public bool DontShowProWindow { get; set; }

    public string PomoLocalSetting { get; set; } = "";

    public string SummaryFilter { get; set; } = "";

    [NotNull]
    [DefaultValue("1")]
    public bool SpellCheckEnable { get; set; } = true;

    public string SpellCheckLanguage { get; set; } = "";

    public string RecentUsedEmojis { get; set; } = "";

    public bool EnableCountDown { get; set; }

    public int ArrangeTaskDateType { get; set; }

    public int MainWindowDisplayModule { get; set; }

    public string ProjectListOpenStatus { get; set; } = "";

    public string CalendarDisplaySettings { get; set; } = "";

    public string ExtraSettings { get; set; } = "";

    public string SmartProjects { get; set; }

    public string ShortcutModel { get; set; }

    public long PreferrenceMTime { get; set; }

    public string UserPreference { get; set; } = "";

    public string CheckedNewFeature { get; set; }

    public string Statistics { get; set; }

    public void CheckNotNullProperty()
    {
      if (this.CellColorType == null)
        this.CellColorType = "list";
      if (this.WeekStartFrom == null)
        this.WeekStartFrom = "Sunday";
      if (this.TimeFormat == null)
        this.TimeFormat = "System";
      if (this.ThemeId == null)
        this.ThemeId = "White";
      if (this.CompletionSound != null)
        return;
      this.CompletionSound = "Jingle";
    }
  }
}
