// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.DataChangedNotifier
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.Views.Kanban;
using ticktick_WPF.Views.MarkDown.Colorizer;
using ticktick_WPF.Views.Pomo;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public static class DataChangedNotifier
  {
    public static event EventHandler ProjectChanged;

    public static event EventHandler<ProjectIdentity> ViewModeChanged;

    public static event EventHandler<string> ProjectColumnChanged;

    public static event EventHandler<ProjectGroupModel> ProjectGroupChanged;

    public static event EventHandler<FilterChangeArgs> FilterChanged;

    public static event EventHandler TeamChanged;

    public static event EventHandler WeekStartFromChanged;

    public static event EventHandler YearStartFromChanged;

    public static event EventHandler<SyncResult> SyncDone;

    public static event EventHandler PeriodicCheck;

    public static event EventHandler<bool> CalendarConfigChanged;

    public static event EventHandler CalendarChanged;

    public static event EventHandler EventArchivedChanged;

    public static event EventHandler CalendarProjectFilterChanged;

    public static event EventHandler TimeFormatChanged;

    public static event EventHandler<string> PomoChanged;

    public static event EventHandler TaskDefaultChanged;

    public static event EventHandler TagDeleted;

    public static event EventHandler<TagModel> TagChanged;

    public static event EventHandler TagTypeChanged;

    public static event EventHandler ThemeModeChanged;

    public static event EventHandler IsDarkChanged;

    public static event EventHandler MainWindowHidden;

    public static event EventHandler HideCompleteChanged;

    public static event EventHandler<HabitCheckInModel> HabitCheckInChanged;

    public static event EventHandler<string> HabitCyclesChanged;

    public static event EventHandler<string> HabitArchived;

    public static event EventHandler HabitLogChanged;

    public static event EventHandler HabitsChanged;

    public static event EventHandler<string> HabitSkip;

    public static event EventHandler HabitSectionChanged;

    public static event EventHandler HabitsSyncDone;

    public static event EventHandler TimelineSettingsChanged;

    public static event EventHandler ScheduleChanged;

    public static event EventHandler<int> MatrixQuadrantChanged;

    public static event EventHandler ShowCompleteLineChanged;

    public static event EventHandler ProjectPinRemoteChanged;

    public static event EventHandler<(string, SortOption)> ListSectionOpenChanged;

    public static event EventHandler<string> SortOptionChanged;

    public static event EventHandler FontFamilyChanged;

    public static event EventHandler TemplateChanged;

    public static void NotifyTemplateChanged()
    {
      EventHandler templateChanged = DataChangedNotifier.TemplateChanged;
      if (templateChanged == null)
        return;
      templateChanged((object) null, (EventArgs) null);
    }

    public static void NotifyHabitsSyncDone()
    {
      EventHandler habitsSyncDone = DataChangedNotifier.HabitsSyncDone;
      if (habitsSyncDone == null)
        return;
      habitsSyncDone((object) null, (EventArgs) null);
    }

    internal static void OnScheduleChanged()
    {
      EventHandler scheduleChanged = DataChangedNotifier.ScheduleChanged;
      if (scheduleChanged == null)
        return;
      scheduleChanged((object) null, (EventArgs) null);
    }

    public static void NotifyHabitsChanged()
    {
      EventHandler habitsChanged = DataChangedNotifier.HabitsChanged;
      if (habitsChanged == null)
        return;
      habitsChanged((object) null, (EventArgs) null);
    }

    public static void NotifyHabitSkip(string habitId)
    {
      EventHandler<string> habitSkip = DataChangedNotifier.HabitSkip;
      if (habitSkip == null)
        return;
      habitSkip((object) null, habitId);
    }

    public static void NotifyHabitSectionChanged()
    {
      EventHandler habitSectionChanged = DataChangedNotifier.HabitSectionChanged;
      if (habitSectionChanged == null)
        return;
      habitSectionChanged((object) null, (EventArgs) null);
    }

    public static void NotifyHabitCyclesChanged(string str)
    {
      EventHandler<string> habitCyclesChanged = DataChangedNotifier.HabitCyclesChanged;
      if (habitCyclesChanged == null)
        return;
      habitCyclesChanged((object) null, str);
    }

    public static void NotifyHabitCheckInChanged(HabitCheckInModel checkIn, object sender = null)
    {
      EventHandler<HabitCheckInModel> habitCheckInChanged = DataChangedNotifier.HabitCheckInChanged;
      if (habitCheckInChanged != null)
        habitCheckInChanged(sender, checkIn);
      if (!(checkIn.CheckinStamp == DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)))
        return;
      if (TickFocusManager.IsFocusing(checkIn.HabitId) && checkIn.CheckStatus != 0)
        FocusTimer.TryStartWithId("");
      if (ABTestManager.IsNewRemindCalculate())
        HabitReminderCalculator.RecalHabitReminder(checkIn.HabitId);
      else
        ReminderCalculator.AssembleReminders();
    }

    public static void NotifyTagDeleted()
    {
      EventHandler tagDeleted = DataChangedNotifier.TagDeleted;
      if (tagDeleted == null)
        return;
      tagDeleted((object) null, (EventArgs) null);
    }

    public static void NotifyTagChanged(TagModel model, string originName = null)
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler<TagModel> tagChanged = DataChangedNotifier.TagChanged;
        if (tagChanged == null)
          return;
        tagChanged((object) originName, model);
      }));
    }

    public static void NotifyTagTypeChanged()
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler tagTypeChanged = DataChangedNotifier.TagTypeChanged;
        if (tagTypeChanged == null)
          return;
        tagTypeChanged((object) null, (EventArgs) null);
      }));
    }

    public static void NotifyProjectChanged()
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler projectChanged = DataChangedNotifier.ProjectChanged;
        if (projectChanged != null)
          projectChanged((object) null, (EventArgs) null);
        EventHandler taskDefaultChanged = DataChangedNotifier.TaskDefaultChanged;
        if (taskDefaultChanged == null)
          return;
        taskDefaultChanged((object) null, (EventArgs) null);
      }));
    }

    public static void NotifyGroupChanged(ProjectGroupModel group)
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler<ProjectGroupModel> projectGroupChanged = DataChangedNotifier.ProjectGroupChanged;
        if (projectGroupChanged == null)
          return;
        projectGroupChanged((object) null, group);
      }));
    }

    public static void NotifyFilterChanged(FilterChangeArgs args)
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler<FilterChangeArgs> filterChanged = DataChangedNotifier.FilterChanged;
        if (filterChanged == null)
          return;
        filterChanged((object) null, args);
      }));
    }

    public static void NotifyTeamChanged()
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler teamChanged = DataChangedNotifier.TeamChanged;
        if (teamChanged == null)
          return;
        teamChanged((object) null, (EventArgs) null);
      }));
    }

    public static void NotifyWeekStartFromChanged()
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        ScheduleService.CourseDisplayModelDict.Clear();
        EventHandler startFromChanged = DataChangedNotifier.WeekStartFromChanged;
        if (startFromChanged == null)
          return;
        startFromChanged((object) null, (EventArgs) null);
      }));
    }

    public static void NotifyYearStartFromChanged()
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler startFromChanged = DataChangedNotifier.YearStartFromChanged;
        if (startFromChanged == null)
          return;
        startFromChanged((object) null, (EventArgs) null);
      }));
    }

    public static void NotifyAutoSyncDone(SyncResult result)
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler<SyncResult> syncDone = DataChangedNotifier.SyncDone;
        if (syncDone == null)
          return;
        syncDone((object) null, result);
      }));
    }

    public static void NotifyPeriodicCheck()
    {
      EventHandler periodicCheck = DataChangedNotifier.PeriodicCheck;
      if (periodicCheck == null)
        return;
      periodicCheck((object) null, (EventArgs) null);
    }

    public static void NotifyCalendarConfigChanged(bool colorChanged)
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler<bool> calendarConfigChanged = DataChangedNotifier.CalendarConfigChanged;
        if (calendarConfigChanged == null)
          return;
        calendarConfigChanged((object) null, colorChanged);
      }));
    }

    public static void NotifyCalendarProjectFilterChanged(object sender)
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler projectFilterChanged = DataChangedNotifier.CalendarProjectFilterChanged;
        if (projectFilterChanged == null)
          return;
        projectFilterChanged(sender, (EventArgs) null);
      }));
    }

    public static void NotifyCalendarChanged()
    {
      DelayActionHandlerCenter.TryDoAction(nameof (NotifyCalendarChanged), (EventHandler) ((sender, args) => Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler calendarChanged = DataChangedNotifier.CalendarChanged;
        if (calendarChanged == null)
          return;
        calendarChanged((object) null, (EventArgs) null);
      }))), 1000);
    }

    public static void NotifyEventArchivedChanged()
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler eventArchivedChanged = DataChangedNotifier.EventArchivedChanged;
        if (eventArchivedChanged == null)
          return;
        eventArchivedChanged((object) null, (EventArgs) null);
      }));
    }

    public static void NotifyPomoChanged(string taskId)
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler<string> pomoChanged = DataChangedNotifier.PomoChanged;
        if (pomoChanged == null)
          return;
        pomoChanged((object) null, taskId);
      }));
    }

    public static void NotifyTaskDefaultChanged()
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        EventHandler taskDefaultChanged = DataChangedNotifier.TaskDefaultChanged;
        if (taskDefaultChanged == null)
          return;
        taskDefaultChanged((object) null, (EventArgs) null);
      }));
    }

    public static void NotifyTimeFormatChanged()
    {
      EventHandler timeFormatChanged = DataChangedNotifier.TimeFormatChanged;
      if (timeFormatChanged == null)
        return;
      timeFormatChanged((object) null, (EventArgs) null);
    }

    public static void NotifyThemeModeChanged()
    {
      EventHandler themeModeChanged = DataChangedNotifier.ThemeModeChanged;
      if (themeModeChanged != null)
        themeModeChanged((object) null, (EventArgs) null);
      KanbanColumnView.SetEmptyBrushColor();
    }

    public static void NotifyIsDarkChanged()
    {
      DeleteLineColorizer.OnThemeChanged();
      EventHandler isDarkChanged = DataChangedNotifier.IsDarkChanged;
      if (isDarkChanged == null)
        return;
      isDarkChanged((object) null, (EventArgs) null);
    }

    public static void NotifyMainWindowHidden()
    {
      EventHandler mainWindowHidden = DataChangedNotifier.MainWindowHidden;
      if (mainWindowHidden == null)
        return;
      mainWindowHidden((object) null, (EventArgs) null);
    }

    public static void NotifyHideCompleteChanged()
    {
      EventHandler hideCompleteChanged = DataChangedNotifier.HideCompleteChanged;
      if (hideCompleteChanged == null)
        return;
      hideCompleteChanged((object) null, (EventArgs) null);
    }

    public static void NotifyColumnChanged(string projectId)
    {
      ThreadUtil.DetachedRunOnUiBackThread((Action) (() =>
      {
        EventHandler<string> projectColumnChanged = DataChangedNotifier.ProjectColumnChanged;
        if (projectColumnChanged == null)
          return;
        projectColumnChanged((object) null, projectId);
      }));
    }

    public static void NotifyMatrixQuadrantChanged(int level)
    {
      EventHandler<int> matrixQuadrantChanged = DataChangedNotifier.MatrixQuadrantChanged;
      if (matrixQuadrantChanged == null)
        return;
      matrixQuadrantChanged((object) null, level);
    }

    public static void NotifyTimelineSettingsChanged()
    {
      ThreadUtil.DetachedRunOnUiBackThread((Action) (() =>
      {
        EventHandler timelineSettingsChanged = DataChangedNotifier.TimelineSettingsChanged;
        if (timelineSettingsChanged == null)
          return;
        timelineSettingsChanged((object) null, (EventArgs) null);
      }));
    }

    public static void NotifyShowCompleteLineChanged()
    {
      EventHandler completeLineChanged = DataChangedNotifier.ShowCompleteLineChanged;
      if (completeLineChanged == null)
        return;
      completeLineChanged((object) null, (EventArgs) null);
    }

    public static void NotifyProjectViewModeChanged(ProjectIdentity identity)
    {
      EventHandler<ProjectIdentity> viewModeChanged = DataChangedNotifier.ViewModeChanged;
      if (viewModeChanged == null)
        return;
      viewModeChanged((object) null, identity);
    }

    public static void NotifyProjectPinRemoteChanged()
    {
      EventHandler pinRemoteChanged = DataChangedNotifier.ProjectPinRemoteChanged;
      if (pinRemoteChanged == null)
        return;
      pinRemoteChanged((object) null, (EventArgs) null);
    }

    public static void NotifyHabitArchived(string habitId)
    {
      EventHandler<string> habitArchived = DataChangedNotifier.HabitArchived;
      if (habitArchived == null)
        return;
      habitArchived((object) null, habitId);
    }

    public static void NotifyHabitLogChanged()
    {
      EventHandler habitLogChanged = DataChangedNotifier.HabitLogChanged;
      if (habitLogChanged == null)
        return;
      habitLogChanged((object) null, (EventArgs) null);
    }

    public static async Task NotifyListSectionOpenChanged(
      object sender,
      (string, SortOption) idToSort)
    {
      await Task.Delay(100);
      EventHandler<(string, SortOption)> sectionOpenChanged = DataChangedNotifier.ListSectionOpenChanged;
      if (sectionOpenChanged == null)
        return;
      sectionOpenChanged(sender, idToSort);
    }

    public static void NotifyFontFamilyChanged()
    {
      EventHandler fontFamilyChanged = DataChangedNotifier.FontFamilyChanged;
      if (fontFamilyChanged == null)
        return;
      fontFamilyChanged((object) null, (EventArgs) null);
    }

    public static void NotifySortOptionChanged(string catId)
    {
      EventHandler<string> sortOptionChanged = DataChangedNotifier.SortOptionChanged;
      if (sortOptionChanged == null)
        return;
      sortOptionChanged((object) null, catId);
    }
  }
}
