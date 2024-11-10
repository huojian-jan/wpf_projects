// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.SmartProjectService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Service
{
  public class SmartProjectService
  {
    public const string InboxId = "inbox";
    public const string AllId = "all";
    public const string TodayId = "today";
    public const string TomorrowId = "tomorrow";
    public const string AssignToMeId = "assignToMe";
    public const string WeekId = "week";

    public static void SaveSmartProjectViewMode(string identityId, string viewMode)
    {
      string nameByIdentityId = SmartProjectService.GetSmartNameByIdentityId(identityId);
      if (string.IsNullOrEmpty(nameByIdentityId))
        return;
      LocalSettings.Settings.SaveSmartViewMode(nameByIdentityId, viewMode);
    }

    public static string GetSmartProjectViewMode(string identityId)
    {
      string nameByIdentityId = SmartProjectService.GetSmartNameByIdentityId(identityId);
      if (nameByIdentityId == "assignToMe")
        return "list";
      string str = (string) null;
      if (LocalSettings.Settings.UserPreference?.SmartProjectsOption != null)
        str = LocalSettings.Settings.UserPreference.SmartProjectsOption.GetViewModeByName(nameByIdentityId);
      return !string.IsNullOrEmpty(str) ? str : "list";
    }

    private static string GetSmartNameByIdentityId(string identityId)
    {
      if (identityId.StartsWith("inbox"))
        return "inbox";
      switch (identityId)
      {
        case "_special_id_all":
          return "all";
        case "_special_id_today":
          return "today";
        case "_special_id_tomorrow":
          return "tomorrow";
        case "_special_id_week":
          return "week";
        case "_special_id_assigned":
          return "assignToMe";
        default:
          return (string) null;
      }
    }

    public static SortOption GetSmartProjectSortOption(string name, bool inKanban)
    {
      SortOption sortOption = (SortOption) null;
      if (LocalSettings.Settings.UserPreference?.SmartProjectsOption == null)
        LocalSettings.Settings.InitSmartProjectsOption(false);
      if (LocalSettings.Settings.UserPreference?.SmartProjectsOption != null)
        sortOption = LocalSettings.Settings.UserPreference.SmartProjectsOption.GetSortOptionByName(name);
      SortOption projectSortOption = sortOption;
      if (projectSortOption != null)
        return projectSortOption;
      return new SortOption()
      {
        groupBy = "dueDate",
        orderBy = "dueDate"
      };
    }

    private static SortOption GetOldSortOptionBySmartId(string name, bool inKanban = false)
    {
      switch (name)
      {
        case "inbox":
          return SortOption.GetOptionInSmartString(LocalSettings.Settings.SortTypeOfInbox);
        case "all":
          return SortOption.GetOptionInSmartString(LocalSettings.Settings.SortTypeOfAllProject);
        case "today":
          return SortOption.GetOptionInSmartString(LocalSettings.Settings.SortTypeOfToday);
        case "tomorrow":
          return SortOption.GetOptionInSmartString(LocalSettings.Settings.SortTypeOfTomorrow);
        case "assignToMe":
          return SortOption.GetOptionInSmartString(LocalSettings.Settings.SortTypeOfAssignMe);
        case "week":
          return SortOption.GetOptionInSmartString(LocalSettings.Settings.SortTypeOfWeek);
        default:
          return new SortOption()
          {
            groupBy = "dueDate",
            orderBy = "dueDate"
          };
      }
    }

    public static List<SmartProjectOption> InitSmartProjectOptions()
    {
      return new List<SmartProjectOption>()
      {
        new SmartProjectOption()
        {
          Name = "inbox",
          ViewMode = "list",
          SortOption = SmartProjectService.GetOldSortOptionBySmartId("inbox")
        },
        new SmartProjectOption()
        {
          Name = "all",
          ViewMode = "list",
          SortOption = SmartProjectService.GetOldSortOptionBySmartId("all")
        },
        new SmartProjectOption()
        {
          Name = "today",
          ViewMode = "list",
          SortOption = SmartProjectService.GetOldSortOptionBySmartId("today")
        },
        new SmartProjectOption()
        {
          Name = "tomorrow",
          ViewMode = "list",
          SortOption = SmartProjectService.GetOldSortOptionBySmartId("tomorrow")
        },
        new SmartProjectOption()
        {
          Name = "assignToMe",
          ViewMode = "list",
          SortOption = SmartProjectService.GetOldSortOptionBySmartId("assignToMe")
        },
        new SmartProjectOption()
        {
          Name = "week",
          ViewMode = "list",
          SortOption = SmartProjectService.GetOldSortOptionBySmartId("week")
        }
      };
    }

    public static TimelineModel GetSmartProjectTimeline(string identityId)
    {
      string nameByIdentityId = SmartProjectService.GetSmartNameByIdentityId(identityId);
      TimelineModel timelineModel = (TimelineModel) null;
      if (LocalSettings.Settings.UserPreference?.SmartProjectsOption != null)
        timelineModel = LocalSettings.Settings.UserPreference.SmartProjectsOption.GetTimeline(nameByIdentityId);
      TimelineModel smartProjectTimeline = timelineModel;
      if (smartProjectTimeline != null)
        return smartProjectTimeline;
      return !(nameByIdentityId == "inbox") ? new TimelineModel("project") : new TimelineModel("sortOrder");
    }

    public static void SaveSmartProjectTimeline(string identityId, TimelineModel timeline)
    {
      string nameByIdentityId = SmartProjectService.GetSmartNameByIdentityId(identityId);
      if (string.IsNullOrEmpty(nameByIdentityId))
        return;
      LocalSettings.Settings.SaveSmartProjectTimeline(nameByIdentityId, timeline);
    }
  }
}
