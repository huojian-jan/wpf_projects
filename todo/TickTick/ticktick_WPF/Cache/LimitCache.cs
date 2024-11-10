// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.LimitCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views;

#nullable disable
namespace ticktick_WPF.Cache
{
  public static class LimitCache
  {
    private static Dictionary<Constants.LimitKind, long> _proLimitData;
    private static Dictionary<Constants.LimitKind, long> _freeLimitData;
    private static Dictionary<Constants.LimitKind, long> _teamLimitData;

    public static long GetFreeLimitByKey(Constants.LimitKind limit)
    {
      Dictionary<Constants.LimitKind, long> freeLimitData = LimitCache._freeLimitData;
      if (freeLimitData == null)
      {
        LimitCache.InitData();
        freeLimitData = LimitCache._freeLimitData;
      }
      long freeLimitByKey;
      if (freeLimitData != null && freeLimitData.TryGetValue(limit, out freeLimitByKey))
        return freeLimitByKey;
      throw new Exception("no limit type existed.");
    }

    public static long GetProLimitByKey(Constants.LimitKind limit)
    {
      Dictionary<Constants.LimitKind, long> proLimitData = LimitCache._proLimitData;
      if (proLimitData == null)
      {
        LimitCache.InitData();
        proLimitData = LimitCache._proLimitData;
      }
      return proLimitData != null && proLimitData.ContainsKey(limit) ? proLimitData[limit] : throw new Exception("no limit type existed.");
    }

    public static long GetLimitByKey(Constants.LimitKind limit)
    {
      Dictionary<Constants.LimitKind, long> dictionary = !LocalSettings.Settings.IsPro ? LimitCache._freeLimitData : (UserManager.IsTeamActive() ? LimitCache._teamLimitData : LimitCache._proLimitData);
      if (dictionary == null)
      {
        LimitCache.InitData();
        dictionary = !LocalSettings.Settings.IsPro ? LimitCache._freeLimitData : (UserManager.IsTeamActive() ? LimitCache._teamLimitData : LimitCache._proLimitData);
      }
      return dictionary != null && dictionary.ContainsKey(limit) ? dictionary[limit] : throw new Exception("no limit type existed.");
    }

    public static async Task InitLimitCache()
    {
      LimitCache.InitData();
      ObservableCollection<LimitsModel> limits = await Communicator.GetLimits();
      if (limits == null || !limits.Any<LimitsModel>())
        return;
      LimitsModel limitsModel1 = limits.FirstOrDefault<LimitsModel>((Func<LimitsModel, bool>) (item => item.type == "FREE"));
      if (limitsModel1 != null)
      {
        LimitCache._freeLimitData[Constants.LimitKind.SubtaskNumber] = (long) limitsModel1.subtaskNumber;
        LimitCache._freeLimitData[Constants.LimitKind.ProjectTaskNumber] = (long) limitsModel1.projectTaskNumber;
        LimitCache._freeLimitData[Constants.LimitKind.ShareUserNumber] = (long) limitsModel1.shareUserNumber;
        LimitCache._freeLimitData[Constants.LimitKind.ProjectNumber] = (long) limitsModel1.projectNumber;
        LimitCache._freeLimitData[Constants.LimitKind.ReminderNumber] = (long) limitsModel1.reminderNumber;
        LimitCache._freeLimitData[Constants.LimitKind.AttachmentSize] = (long) limitsModel1.attachmentSize;
        LimitCache._freeLimitData[Constants.LimitKind.TaskAttachmentNumber] = (long) limitsModel1.taskAttachmentNumber;
        LimitCache._freeLimitData[Constants.LimitKind.DailyUploadNumber] = (long) limitsModel1.dailyUploadNumber;
        LimitCache._freeLimitData[Constants.LimitKind.KanbanNumber] = limitsModel1.kanbanNumber > 0 ? (long) limitsModel1.kanbanNumber : 19L;
        LimitCache._freeLimitData[Constants.LimitKind.HabitNumber] = (long) limitsModel1.habitNumber;
        LimitCache._freeLimitData[Constants.LimitKind.TimerNumber] = (long) limitsModel1.timerNumber;
        LimitCache._freeLimitData[Constants.LimitKind.VisitorNumber] = (long) limitsModel1.visitorNumber;
      }
      LimitsModel limitsModel2 = limits.FirstOrDefault<LimitsModel>((Func<LimitsModel, bool>) (item => item.type == "PRO"));
      if (limitsModel2 != null)
      {
        LimitCache._proLimitData[Constants.LimitKind.SubtaskNumber] = (long) limitsModel2.subtaskNumber;
        LimitCache._proLimitData[Constants.LimitKind.ProjectTaskNumber] = (long) limitsModel2.projectTaskNumber;
        LimitCache._proLimitData[Constants.LimitKind.ShareUserNumber] = (long) limitsModel2.shareUserNumber;
        LimitCache._proLimitData[Constants.LimitKind.ProjectNumber] = (long) limitsModel2.projectNumber;
        LimitCache._proLimitData[Constants.LimitKind.ReminderNumber] = (long) limitsModel2.reminderNumber;
        LimitCache._proLimitData[Constants.LimitKind.AttachmentSize] = (long) limitsModel2.attachmentSize;
        LimitCache._proLimitData[Constants.LimitKind.TaskAttachmentNumber] = (long) limitsModel2.taskAttachmentNumber;
        LimitCache._proLimitData[Constants.LimitKind.DailyUploadNumber] = (long) limitsModel2.dailyUploadNumber;
        LimitCache._proLimitData[Constants.LimitKind.KanbanNumber] = limitsModel2.kanbanNumber > 0 ? (long) limitsModel2.kanbanNumber : 19L;
        LimitCache._proLimitData[Constants.LimitKind.HabitNumber] = (long) limitsModel2.habitNumber;
        LimitCache._proLimitData[Constants.LimitKind.TimerNumber] = (long) limitsModel2.timerNumber;
        LimitCache._proLimitData[Constants.LimitKind.VisitorNumber] = (long) limitsModel2.visitorNumber;
      }
      LimitsModel limitsModel3 = limits.FirstOrDefault<LimitsModel>((Func<LimitsModel, bool>) (item => item.type == "TEAM"));
      if (limitsModel3 == null)
        return;
      LimitCache._teamLimitData[Constants.LimitKind.SubtaskNumber] = (long) limitsModel3.subtaskNumber;
      LimitCache._teamLimitData[Constants.LimitKind.ProjectTaskNumber] = (long) limitsModel3.projectTaskNumber;
      LimitCache._teamLimitData[Constants.LimitKind.ShareUserNumber] = (long) limitsModel3.shareUserNumber;
      LimitCache._teamLimitData[Constants.LimitKind.ProjectNumber] = (long) limitsModel3.projectNumber;
      LimitCache._teamLimitData[Constants.LimitKind.ReminderNumber] = (long) limitsModel3.reminderNumber;
      LimitCache._teamLimitData[Constants.LimitKind.AttachmentSize] = (long) limitsModel3.attachmentSize;
      LimitCache._teamLimitData[Constants.LimitKind.TaskAttachmentNumber] = (long) limitsModel3.taskAttachmentNumber;
      LimitCache._teamLimitData[Constants.LimitKind.DailyUploadNumber] = (long) limitsModel3.dailyUploadNumber;
      LimitCache._teamLimitData[Constants.LimitKind.KanbanNumber] = limitsModel3.kanbanNumber > 0 ? (long) limitsModel3.kanbanNumber : 19L;
      LimitCache._teamLimitData[Constants.LimitKind.HabitNumber] = (long) limitsModel3.habitNumber;
      LimitCache._teamLimitData[Constants.LimitKind.TimerNumber] = (long) limitsModel3.timerNumber;
      LimitCache._teamLimitData[Constants.LimitKind.VisitorNumber] = (long) limitsModel3.visitorNumber;
    }

    private static void InitData()
    {
      LimitCache._freeLimitData = new Dictionary<Constants.LimitKind, long>()
      {
        {
          Constants.LimitKind.SubtaskNumber,
          19L
        },
        {
          Constants.LimitKind.ProjectTaskNumber,
          99L
        },
        {
          Constants.LimitKind.ProjectNumber,
          9L
        },
        {
          Constants.LimitKind.ShareUserNumber,
          2L
        },
        {
          Constants.LimitKind.ReminderNumber,
          2L
        },
        {
          Constants.LimitKind.AttachmentSize,
          10240000L
        },
        {
          Constants.LimitKind.TaskAttachmentNumber,
          20L
        },
        {
          Constants.LimitKind.DailyUploadNumber,
          2L
        },
        {
          Constants.LimitKind.KanbanNumber,
          19L
        },
        {
          Constants.LimitKind.HabitNumber,
          5L
        },
        {
          Constants.LimitKind.TimerNumber,
          3L
        },
        {
          Constants.LimitKind.VisitorNumber,
          1L
        }
      };
      LimitCache._proLimitData = new Dictionary<Constants.LimitKind, long>()
      {
        {
          Constants.LimitKind.SubtaskNumber,
          199L
        },
        {
          Constants.LimitKind.ProjectTaskNumber,
          999L
        },
        {
          Constants.LimitKind.ProjectNumber,
          299L
        },
        {
          Constants.LimitKind.ShareUserNumber,
          29L
        },
        {
          Constants.LimitKind.ReminderNumber,
          5L
        },
        {
          Constants.LimitKind.AttachmentSize,
          20480000L
        },
        {
          Constants.LimitKind.TaskAttachmentNumber,
          20L
        },
        {
          Constants.LimitKind.DailyUploadNumber,
          99L
        },
        {
          Constants.LimitKind.KanbanNumber,
          19L
        },
        {
          Constants.LimitKind.HabitNumber,
          299L
        },
        {
          Constants.LimitKind.TimerNumber,
          49L
        },
        {
          Constants.LimitKind.VisitorNumber,
          29L
        }
      };
      LimitCache._teamLimitData = new Dictionary<Constants.LimitKind, long>()
      {
        {
          Constants.LimitKind.SubtaskNumber,
          199L
        },
        {
          Constants.LimitKind.ProjectTaskNumber,
          999L
        },
        {
          Constants.LimitKind.ProjectNumber,
          499L
        },
        {
          Constants.LimitKind.ShareUserNumber,
          49L
        },
        {
          Constants.LimitKind.ReminderNumber,
          5L
        },
        {
          Constants.LimitKind.AttachmentSize,
          20480000L
        },
        {
          Constants.LimitKind.TaskAttachmentNumber,
          20L
        },
        {
          Constants.LimitKind.DailyUploadNumber,
          99L
        },
        {
          Constants.LimitKind.KanbanNumber,
          19L
        },
        {
          Constants.LimitKind.HabitNumber,
          299L
        },
        {
          Constants.LimitKind.TimerNumber,
          49L
        },
        {
          Constants.LimitKind.VisitorNumber,
          29L
        }
      };
    }

    public static bool CheckCalendarCount(string kind)
    {
      if (kind == "URL")
      {
        if (CacheManager.GetSubscribeCalendars().Count < 5)
          return true;
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("UrlToMany"), MessageBoxButton.OK);
        customerDialog.Owner = (Window) App.Window;
        customerDialog.ShowDialog();
        return false;
      }
      List<BindCalendarAccountModel> calendarAccounts = CacheManager.GetBindCalendarAccounts();
      int num = 0;
      if (!(kind == "CalDAV"))
      {
        if (!(kind == "Google"))
        {
          if (!(kind == "Outlook"))
          {
            if (!(kind == "Exchange"))
            {
              if (kind == "iCloud")
                num = calendarAccounts.Count<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (item => item.Kind == "icloud"));
            }
            else
              num = calendarAccounts.Count<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (item => item.Kind == "exchange"));
          }
          else
            num = calendarAccounts.Count<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (item => item.Site == "outlook"));
        }
        else
          num = calendarAccounts.Count<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (item => item.Site == "google"));
      }
      else
        num = calendarAccounts.Count<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (item => item.Kind == "caldav"));
      if (num < 3)
        return true;
      new CustomerDialog(Utils.GetString("LimitTips"), string.Format(Utils.GetString("CalendarBindExceed"), (object) kind), MessageBoxButton.OK).ShowDialog();
      return false;
    }
  }
}
