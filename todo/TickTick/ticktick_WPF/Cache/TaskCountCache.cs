// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.TaskCountCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.MainListView;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Cache
{
  public static class TaskCountCache
  {
    public static readonly ConcurrentDictionary<string, int> CountData = new ConcurrentDictionary<string, int>();
    public static readonly ConcurrentDictionary<string, DateTime> CountCalculateTimeDict = new ConcurrentDictionary<string, DateTime>();

    public static DateTime LoadTime { get; set; }

    public static void Clear()
    {
      TaskCountCache.CountData.Clear();
      TaskCountCache.CountCalculateTimeDict.Clear();
      TaskCountCache.LoadTime = DateTime.Now.AddDays(-1.0);
    }

    public static int TryGetCount(string key)
    {
      if (LocalSettings.Settings.ExtraSettings.NumDisplayType == 2)
        return 0;
      return TaskCountCache.CountData != null && TaskCountCache.CountData.ContainsKey(key) ? TaskCountCache.CountData[key] : -1;
    }

    public static async Task<int> TryGetCount(ProjectIdentity pId)
    {
      if (LocalSettings.Settings.ExtraSettings.NumDisplayType == 2 || TaskCountCache.CountData == null || pId == null || pId is CompletedProjectIdentity || pId is TrashProjectIdentity || pId is AbandonedProjectIdentity)
        return 0;
      string key = pId.QueryId;
      if (TaskCountCache.CountCalculateTimeDict.ContainsKey(key) && TaskCountCache.CountCalculateTimeDict[key] < TaskCountCache.LoadTime || !TaskCountCache.CountData.ContainsKey(key))
      {
        int countByIdentity = await ProjectAndTaskIdsCache.GetCountByIdentity(pId);
        TaskCountCache.TryAddValue(key, countByIdentity);
        return countByIdentity;
      }
      if (TaskCountCache.CountData.ContainsKey(key))
        return TaskCountCache.CountData[key];
      key = (string) null;
      return 0;
    }

    public static void SetNeedLoad() => TaskCountCache.LoadTime = DateTime.Now;

    public static async Task LoadTodayOrWeekCount()
    {
      LocalSettings settings = LocalSettings.Settings;
      if (settings.SmartListToday != 1)
        TaskCountCache.TryAddValue("_special_id_today", await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) SmartProjectIdentity.BuildSmartProject("_special_id_today")));
      if (settings.SmartList7Day == 1)
      {
        settings = (LocalSettings) null;
      }
      else
      {
        TaskCountCache.TryAddValue("_special_id_week", await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) SmartProjectIdentity.BuildSmartProject("_special_id_week")));
        settings = (LocalSettings) null;
      }
    }

    private static async Task<bool> ReloadSmartListCount()
    {
      LocalSettings settings = LocalSettings.Settings;
      bool changed = false;
      if (settings.SmartListAll != 1)
      {
        AllProjectIdentity allIdentity = new AllProjectIdentity();
        await ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) allIdentity);
        int countByIdentity = await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) allIdentity);
        int num;
        if (!TaskCountCache.CountData.TryGetValue("_special_id_all", out num) || num != countByIdentity)
        {
          changed = true;
          TaskCountCache.TryAddValue("_special_id_all", countByIdentity);
        }
        allIdentity = (AllProjectIdentity) null;
      }
      if (settings.SmartListToday != 1)
      {
        TodayProjectIdentity todIdentity = new TodayProjectIdentity();
        await ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) todIdentity);
        int countByIdentity = await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) todIdentity);
        int num;
        if (!TaskCountCache.CountData.TryGetValue("_special_id_today", out num) || num != countByIdentity)
        {
          changed = true;
          TaskCountCache.TryAddValue("_special_id_today", countByIdentity);
        }
        todIdentity = (TodayProjectIdentity) null;
      }
      if (settings.SmartListTomorrow != 1)
      {
        TomorrowProjectIdentity tomIdentity = new TomorrowProjectIdentity();
        await ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) tomIdentity);
        int countByIdentity = await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) tomIdentity);
        int num;
        if (!TaskCountCache.CountData.TryGetValue("_special_id_tomorrow", out num) || num != countByIdentity)
        {
          changed = true;
          TaskCountCache.TryAddValue("_special_id_tomorrow", countByIdentity);
        }
        tomIdentity = (TomorrowProjectIdentity) null;
      }
      if (settings.SmartList7Day != 1)
      {
        WeekProjectIdentity weekIdentity = new WeekProjectIdentity();
        await ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) weekIdentity);
        int countByIdentity = await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) weekIdentity);
        int num;
        if (!TaskCountCache.CountData.TryGetValue("_special_id_week", out num) || num != countByIdentity)
        {
          changed = true;
          TaskCountCache.TryAddValue("_special_id_week", countByIdentity);
        }
        weekIdentity = (WeekProjectIdentity) null;
      }
      bool flag = changed;
      settings = (LocalSettings) null;
      return flag;
    }

    private static void TryAddValue(string key, int count)
    {
      if (string.IsNullOrEmpty(key))
        return;
      TaskCountCache.CountData[key] = count;
      TaskCountCache.CountCalculateTimeDict[key] = DateTime.Now;
    }

    public static async Task ReloadProjectTaskCount(ProjectIdentity selectedProject)
    {
      if (LocalSettings.Settings.ExtraSettings.NumDisplayType == 2)
        return;
      Task.Run((Func<Task>) (async () =>
      {
        Thread.Sleep(200);
        ProjectAndTaskIdsCache.RemoveEventCache(selectedProject);
        await ProjectAndTaskIdsCache.ResetProjectTaskIds(selectedProject);
        int countByIdentity = await ProjectAndTaskIdsCache.GetCountByIdentity(selectedProject);
        TaskCountCache.TryAddValue(selectedProject.QueryId, countByIdentity);
        ListViewContainer.ResetIdentityCount(selectedProject, countByIdentity);
      }));
    }

    public static void TryReloadGroupAndSmartCount()
    {
      DelayActionHandlerCenter.TryDoAction(nameof (TryReloadGroupAndSmartCount), new EventHandler(TaskCountCache.ReloadGroupAndSmartCount));
    }

    public static void TryReloadSmartCount()
    {
      DelayActionHandlerCenter.TryDoAction(nameof (TryReloadSmartCount), new EventHandler(TaskCountCache.ReloadSmartCount));
    }

    private static async void ReloadSmartCount(object sender, EventArgs eventArgs)
    {
      try
      {
        if (!await TaskCountCache.ReloadSmartListCount())
          return;
        ThreadUtil.DetachedRunOnUiBackThread(new Action(ListViewContainer.ReloadCount));
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
    }

    private static async void ReloadGroupAndSmartCount(object sender, EventArgs eventArgs)
    {
      try
      {
        bool changed = false;
        foreach (ProjectGroupModel projectGroup in CacheManager.GetProjectGroups())
        {
          GroupProjectIdentity groupIdentity = new GroupProjectIdentity(projectGroup);
          await ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) groupIdentity);
          int countByIdentity = await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) groupIdentity);
          if (!TaskCountCache.CountData.ContainsKey(groupIdentity.QueryId) || TaskCountCache.CountData[groupIdentity.QueryId] != countByIdentity)
          {
            changed = true;
            TaskCountCache.TryAddValue(groupIdentity.QueryId, countByIdentity);
          }
          groupIdentity = (GroupProjectIdentity) null;
        }
        if (!(changed | await TaskCountCache.ReloadSmartListCount()))
          return;
        ThreadUtil.DetachedRunOnUiBackThread(new Action(ListViewContainer.ReloadCount));
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
    }
  }
}
