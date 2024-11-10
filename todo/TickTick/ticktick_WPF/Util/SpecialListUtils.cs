// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.SpecialListUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Media;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class SpecialListUtils
  {
    public const string SpecialListId = "_special_id_";
    public const string SpecialListTodaySid = "_special_id_today";
    public const string SpecialListTomorrowSid = "_special_id_tomorrow";
    public const string SpecialListWeekSid = "_special_id_week";
    public const string SpecialListTagSid = "_special_id_tag";
    public const string SpecialListEventSid = "_special_id_event";
    public const string SpecialListAssignedSid = "_special_id_assigned";
    public const string SpecialListAllSid = "_special_id_all";
    public const string SpecialListCompletedSid = "_special_id_completed";
    public const string SpecialListAbandonedSid = "_special_id_abandoned";
    public const string SpecialListTrashSid = "_special_id_trash";
    public const string SpecialListClosedGroupSid = "_special_id_closed";
    public const string SpecialListGroupAllSid = "_special_id_group_all";
    public const string SpecialListSearchSid = "_special_id_search";
    public const string SpecialListSummarySid = "_special_id_summary";
    public const string SpecialListDate = "_special_id_date";
    public const string TodaySid = "today";
    public const string TomorrowSid = "tomorrow";
    public const string WeekSid = "week";
    public const string AssignedSid = "assigned";
    public const string TagSid = "tag";
    public const string EventSid = "event";
    public const string AllSid = "all";
    public const string CompletedSid = "completed";
    public const string AbandonedSid = "abandoned";
    public const string TrashSid = "trash";
    public const string ClosedGroupSid = "closed";
    public const string GroupAllSid = "group_all";
    public const string SearchSid = "search";
    public const string SummarySid = "summary";
    public const string DateSid = "date";

    public static bool IsDefaultSmartList(string id)
    {
      return SpecialListUtils.IsAllProject(id) || SpecialListUtils.IsTodayProject(id) || SpecialListUtils.IsTomorrowProject(id) || SpecialListUtils.IsWeekProject(id) || SpecialListUtils.IsAssignToMeProject(id) || SpecialListUtils.IsCompleteProject(id) || SpecialListUtils.IsTrashProject(id);
    }

    public static bool IsSmartProject(string id) => id.Contains("_special_id_");

    public static bool IsAllProject(string id) => "_special_id_all" == id;

    public static bool IsTodayProject(string id) => "_special_id_today" == id;

    private static bool IsTomorrowProject(string id) => "_special_id_tomorrow" == id;

    private static bool IsWeekProject(string id) => "_special_id_week" == id;

    public static bool IsAssignToMeProject(string id) => "_special_id_assigned" == id;

    public static bool IsAbandonedProject(string id) => "_special_id_abandoned" == id;

    public static bool IsCompleteProject(string id) => "_special_id_completed" == id;

    public static bool IsTrashProject(string id) => "_special_id_trash" == id;

    public static bool IsSummaryProject(string sid) => sid == "_special_id_summary";

    public static bool IsInboxProject(string itemId) => itemId == Utils.GetInboxId();

    public static bool IsTargetSmartList(SmartListType smartType, string itemId)
    {
      switch (smartType)
      {
        case SmartListType.All:
          return SpecialListUtils.IsAllProject(itemId);
        case SmartListType.Today:
          return SpecialListUtils.IsTodayProject(itemId);
        case SmartListType.Tomorrow:
          return SpecialListUtils.IsTomorrowProject(itemId);
        case SmartListType.Week:
          return SpecialListUtils.IsWeekProject(itemId);
        case SmartListType.Assign:
          return SpecialListUtils.IsAssignToMeProject(itemId);
        case SmartListType.Completed:
          return SpecialListUtils.IsCompleteProject(itemId);
        case SmartListType.Trash:
          return SpecialListUtils.IsTrashProject(itemId);
        case SmartListType.Summary:
          return SpecialListUtils.IsSummaryProject(itemId);
        case SmartListType.Inbox:
          return SpecialListUtils.IsInboxProject(itemId);
        case SmartListType.Abandoned:
          return SpecialListUtils.IsAbandonedProject(itemId);
        default:
          return false;
      }
    }

    public static SmartListType GetSmartTypeById(string id)
    {
      if (id != null)
      {
        switch (id.Length)
        {
          case 15:
            if (id == "_special_id_all")
              return SmartListType.All;
            break;
          case 16:
            if (id == "_special_id_week")
              return SmartListType.Week;
            break;
          case 17:
            switch (id[13])
            {
              case 'o':
                if (id == "_special_id_today")
                  return SmartListType.Today;
                break;
              case 'r':
                if (id == "_special_id_trash")
                  return SmartListType.Trash;
                break;
            }
            break;
          case 19:
            if (id == "_special_id_summary")
              return SmartListType.Summary;
            break;
          case 20:
            switch (id[12])
            {
              case 'a':
                if (id == "_special_id_assigned")
                  return SmartListType.Assign;
                break;
              case 't':
                if (id == "_special_id_tomorrow")
                  return SmartListType.Tomorrow;
                break;
            }
            break;
          case 21:
            switch (id[12])
            {
              case 'a':
                if (id == "_special_id_abandoned")
                  return SmartListType.Abandoned;
                break;
              case 'c':
                if (id == "_special_id_completed")
                  return SmartListType.Completed;
                break;
            }
            break;
        }
      }
      return SmartListType.Inbox;
    }

    public static Geometry GetIconBySmartType(SmartListType type)
    {
      switch (type)
      {
        case SmartListType.All:
          return Utils.GetIconData("IcAllProject");
        case SmartListType.Today:
          return Utils.GetIcon("CalDayIcon" + DateTime.Today.Day.ToString());
        case SmartListType.Tomorrow:
          return Utils.GetIconData("IcTomorrowProject");
        case SmartListType.Week:
          return Utils.GetIcon("CalWeekIcon" + DateTime.Today.DayOfWeek.ToString().Substring(0, 2));
        case SmartListType.Assign:
          return Utils.GetIconData("IcAssignToMe");
        case SmartListType.Completed:
          return Utils.GetIconData("IcCompletedProject");
        case SmartListType.Trash:
          return Utils.GetIconData("IcTrashProject");
        case SmartListType.Summary:
          return Utils.GetIconData("IcSummaryProject");
        case SmartListType.Inbox:
          return Utils.GetIconData("IcInboxProject");
        case SmartListType.Abandoned:
          return Utils.GetIconData("IcAbandonedProject");
        case SmartListType.Tag:
          return Utils.GetIconData("IcTagLine");
        case SmartListType.Filter:
          return Utils.GetIconData("IcFilterProject");
        default:
          return (Geometry) null;
      }
    }
  }
}
