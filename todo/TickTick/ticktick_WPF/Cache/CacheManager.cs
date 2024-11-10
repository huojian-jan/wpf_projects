// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.CacheManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Search;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Cache
{
  public static class CacheManager
  {
    private static readonly Dictionary<string, ICacheable> Cacheables = new Dictionary<string, ICacheable>();
    public static bool IsInit = false;

    public static event EventHandler CacheInited;

    public static async Task Init()
    {
      await CacheManager.InitData();
      await CacheManager.WarmUp();
    }

    private static async Task InitData()
    {
      CacheManager.Cacheables.Clear();
      CacheManager.Cacheables["syncTask"] = (ICacheable) new TaskSyncCache();
      CacheManager.Cacheables["tag"] = (ICacheable) new TagCache();
      CacheManager.Cacheables["project"] = (ICacheable) new ProjectCache();
      CacheManager.Cacheables["projectGroup"] = (ICacheable) new ProjectGroupCache();
      CacheManager.Cacheables["calendarProfile"] = (ICacheable) new CalendarProfileCache();
      CacheManager.Cacheables["bindCalendarAccount"] = (ICacheable) new BindCalendarAccountCache();
      CacheManager.Cacheables["bindCalendars"] = (ICacheable) new BindCalendarsCache();
      CacheManager.Cacheables["filter"] = (ICacheable) new FilterCache();
      CacheManager.Cacheables["team"] = (ICacheable) new TeamCache();
      CacheManager.Cacheables["sectionStatus"] = (ICacheable) new SectionStatusCache();
      await Task.WhenAll((IEnumerable<Task>) new List<Task>()
      {
        CacheManager.Cacheables["project"].Load(),
        CacheManager.Cacheables["projectGroup"].Load(),
        CacheManager.Cacheables["team"].Load()
      });
    }

    private static async Task WarmUp()
    {
      try
      {
        await Task.WhenAll((IEnumerable<Task>) new List<Task>()
        {
          CacheManager.HandleCaches((Func<ICacheable, Task>) (cache => cache.Load())),
          (Task) HolidayManager.GetRecentHolidays(),
          TaskCompletionRateDao.WarmUp(),
          ArchiveCourseDao.CheckOldArchiveCourse()
        });
        await TaskViewModelHelper.Init();
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
      CacheManager.IsInit = true;
    }

    public static async Task Clear()
    {
      await CacheManager.HandleCaches((Func<ICacheable, Task>) (cache => cache.Clear()));
      TaskDefaultDao.ClearCache();
    }

    private static async Task HandleCaches(Func<ICacheable, Task> process)
    {
      if (CacheManager.Cacheables == null || CacheManager.Cacheables.Count <= 0)
        return;
      List<KeyValuePair<string, ICacheable>> list = CacheManager.Cacheables.ToList<KeyValuePair<string, ICacheable>>();
      List<Task> taskList = new List<Task>();
      foreach (KeyValuePair<string, ICacheable> keyValuePair in list)
      {
        if (keyValuePair.Key != "project" && keyValuePair.Key != "projectGroup")
          taskList.Add(process(keyValuePair.Value));
      }
      await Task.WhenAll((IEnumerable<Task>) taskList);
    }

    public static void DeleteTag(string tag)
    {
      CacheManager.DeleteEntity<TagModel>(nameof (tag), tag);
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        SearchProjectHelper.DeleteModels(tag);
        ProjectAndTaskIdsCache.RemoveIdsModelByKey(tag);
      }));
    }

    public static void UpdateTag(TagModel tag, bool onlyUpdate = false)
    {
      CacheManager.UpdateEntity<TagModel>(nameof (tag), tag, (Func<TagModel, string>) (p => p.name));
      if (onlyUpdate)
        return;
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        SearchProjectHelper.UpdateModels(tag);
        DataChangedNotifier.NotifyTagChanged(tag);
      }));
    }

    public static void DeleteProjectGroup(string groupId)
    {
      CacheManager.DeleteEntity<ProjectGroupModel>("projectGroup", groupId);
    }

    public static void DeleteProjectGroup(ProjectGroupModel projectGroup)
    {
      CacheManager.DeleteEntity<ProjectGroupModel>(nameof (projectGroup), projectGroup, (Func<ProjectGroupModel, string>) (p => p.id));
      DataChangedNotifier.NotifyGroupChanged(projectGroup);
    }

    public static void UpdateProjectGroup(ProjectGroupModel projectGroup)
    {
      CacheManager.UpdateEntity<ProjectGroupModel>(nameof (projectGroup), projectGroup, (Func<ProjectGroupModel, string>) (p => p.id));
      DataChangedNotifier.NotifyGroupChanged(projectGroup);
    }

    public static void DeleteFilter(string id)
    {
      CacheManager.DeleteEntity<FilterModel>("filter", id);
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        SearchProjectHelper.DeleteFilter(id);
        DataChangedNotifier.NotifyFilterChanged(new FilterChangeArgs()
        {
          deleteId = id
        });
      }));
    }

    public static void UpdateFilter(FilterModel filter)
    {
      FilterModel filterById = CacheManager.GetFilterById(filter?.id);
      bool ruleChanged = filter?.rule != filterById?.rule;
      CacheManager.UpdateEntity<FilterModel>(nameof (filter), filter, (Func<FilterModel, string>) (p => p.id));
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        SearchProjectHelper.UpdateModels(filter);
        if (ruleChanged)
          TaskCountCache.ReloadProjectTaskCount((ProjectIdentity) new FilterProjectIdentity(filter));
        DataChangedNotifier.NotifyFilterChanged(new FilterChangeArgs()
        {
          Filter = filter,
          RuleChanged = ruleChanged
        });
      }));
    }

    public static void UpdateTeam(TeamModel team)
    {
      CacheManager.UpdateEntity<TeamModel>(nameof (team), team, (Func<TeamModel, string>) (p => p.id));
      DataChangedNotifier.NotifyTeamChanged();
    }

    public static void DeleteTeam(TeamModel team)
    {
      CacheManager.DeleteEntity<TeamModel>(nameof (team), team, (Func<TeamModel, string>) (p => p.id));
      DataChangedNotifier.NotifyTeamChanged();
    }

    public static void ClearCalendarProfile()
    {
      CacheManager.GetCacheable("calendarProfile")?.Clear();
      DataChangedNotifier.NotifyCalendarChanged();
    }

    public static void DeleteBindAccount(BindCalendarAccountModel model)
    {
      CacheManager.DeleteEntity<BindCalendarAccountModel>("bindCalendarAccount", model, (Func<BindCalendarAccountModel, string>) (p => p.Id));
      DataChangedNotifier.NotifyCalendarChanged();
    }

    public static void DeleteBindCalendar(BindCalendarModel model)
    {
      CacheManager.DeleteEntity<BindCalendarModel>("bindCalendars", model, (Func<BindCalendarModel, string>) (p => p.Id));
    }

    public static void UpdateBindCalendar(BindCalendarModel model)
    {
      CacheManager.UpdateEntity<BindCalendarModel>("bindCalendars", model, (Func<BindCalendarModel, string>) (p => p.Id));
      DataChangedNotifier.NotifyCalendarChanged();
    }

    public static void UpdateBindAccount(BindCalendarAccountModel account)
    {
      CacheManager.UpdateEntity<BindCalendarAccountModel>("bindCalendarAccount", account, (Func<BindCalendarAccountModel, string>) (p => p.Id));
      DataChangedNotifier.NotifyCalendarChanged();
    }

    public static void DeleteCalendarProfile(CalendarSubscribeProfileModel profile)
    {
      CacheManager.DeleteEntity<CalendarSubscribeProfileModel>("calendarProfile", profile, (Func<CalendarSubscribeProfileModel, string>) (p => p.Id));
      DataChangedNotifier.NotifyCalendarChanged();
    }

    public static void UpdateCalendarProfile(CalendarSubscribeProfileModel profile)
    {
      CacheManager.UpdateEntity<CalendarSubscribeProfileModel>("calendarProfile", profile, (Func<CalendarSubscribeProfileModel, string>) (p => p.Id));
      DataChangedNotifier.NotifyCalendarChanged();
    }

    public static void DeleteProject(ProjectModel project)
    {
      CacheManager.DeleteEntity<ProjectModel>(nameof (project), project, (Func<ProjectModel, string>) (p => p.id));
      ThreadUtil.DetachedRunOnUiBackThread((Action) (() =>
      {
        DataChangedNotifier.NotifyProjectChanged();
        SearchProjectHelper.DeleteModels(project);
        TaskViewModelHelper.OnProjectDeleted(project);
        TaskCountCache.TryReloadGroupAndSmartCount();
      }));
    }

    public static void UpdateProject(ProjectModel project, bool notify = true)
    {
      CacheManager.UpdateEntity<ProjectModel>(nameof (project), project, (Func<ProjectModel, string>) (p => p.id));
      ThreadUtil.DetachedRunOnUiBackThread((Action) (() =>
      {
        NotificationHelper.OnProjectChanged(project);
        if (notify)
          DataChangedNotifier.NotifyProjectChanged();
        SearchProjectHelper.UpdateModels(project);
        TaskViewModelHelper.OnProjectChanged(project);
        TaskCountCache.TryReloadGroupAndSmartCount();
      }));
    }

    public static void UpdateSyncTask(TaskSyncedJsonModel json)
    {
      CacheManager.UpdateEntity<TaskSyncedJsonModel>("syncTask", json, (Func<TaskSyncedJsonModel, string>) (p => p.taskSid));
    }

    public static void DeleteSyncTask(TaskSyncedJsonModel json)
    {
      CacheManager.DeleteEntity<TaskSyncedJsonModel>("syncTask", json, (Func<TaskSyncedJsonModel, string>) (p => p.taskSid));
    }

    public static void AddSectionStatus(SectionStatusModel model)
    {
      CacheManager.UpdateEntity<SectionStatusModel>("sectionStatus", model, (Func<SectionStatusModel, string>) (p => p.Identity + "|" + p.Name));
    }

    public static void DeleteSectionStatus(SectionStatusModel model)
    {
      CacheManager.DeleteEntity<SectionStatusModel>("sectionStatus", model, (Func<SectionStatusModel, string>) (p => p.Identity + "|" + p.Name));
    }

    private static void UpdateEntity<T>(string key, T data, Func<T, string> getId)
    {
      ICacheable cacheable = CacheManager.GetCacheable(key);
      if (cacheable == null || !(cacheable is CacheBase<T> cacheBase) || string.IsNullOrEmpty(getId(data)))
        return;
      cacheBase.Update(getId(data), data);
    }

    private static void DeleteEntity<T>(string key, string id)
    {
      ICacheable cacheable = CacheManager.GetCacheable(key);
      if (cacheable == null || !(cacheable is CacheBase<T> cacheBase))
        return;
      cacheBase.Delete(id);
    }

    private static void DeleteEntity<T>(string key, T data, Func<T, string> getId)
    {
      ICacheable cacheable = CacheManager.GetCacheable(key);
      if (cacheable == null || !(cacheable is CacheBase<T> cacheBase))
        return;
      cacheBase.Delete(getId(data));
    }

    public static List<ProjectModel> GetProjectsWithoutInbox()
    {
      return CacheManager.GetDataSafely<ProjectModel>("project").Where<ProjectModel>((Func<ProjectModel, bool>) (p => !p.Isinbox)).ToList<ProjectModel>();
    }

    public static List<ProjectModel> GetOpenedProjects()
    {
      return CacheManager.GetDataSafely<ProjectModel>("project").Where<ProjectModel>((Func<ProjectModel, bool>) (p =>
      {
        if (!p.closed.HasValue)
          return true;
        return p.closed.HasValue && !p.closed.Value;
      })).ToList<ProjectModel>();
    }

    public static List<ProjectModel> GetProjects()
    {
      return CacheManager.GetDataSafely<ProjectModel>("project");
    }

    public static List<ProjectModel> GetEnableProjects()
    {
      return CacheManager.GetDataSafely<ProjectModel>("project").Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsEnable())).ToList<ProjectModel>();
    }

    public static Dictionary<string, long> GetProjectSortOrders()
    {
      Dictionary<string, long> projectSortOrders1 = new Dictionary<string, long>();
      List<ProjectModel> projects = CacheManager.GetProjects();
      List<ProjectGroupModel> projectGroups = CacheManager.GetProjectGroups();
      List<TeamModel> source = CacheManager.GetTeams();
      List<CacheManager.ProjectSortable> projectSortableList = new List<CacheManager.ProjectSortable>();
      if (source == null)
        source = new List<TeamModel>();
      foreach (ProjectModel projectModel in projects)
      {
        ProjectModel project = projectModel;
        TeamModel teamModel = source.FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (t => t.id == project.teamId));
        long teamOrder = teamModel != null ? (long) source.IndexOf(teamModel) : long.MaxValue;
        CacheManager.ProjectSortable projectSortable = new CacheManager.ProjectSortable(project.id, project.sortOrder, -1L, teamOrder, project.name);
        ProjectGroupModel projectGroupModel = projectGroups.FirstOrDefault<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (p => p.id == project.groupId));
        if (projectGroupModel != null)
          projectSortable.GroupOrder = projectGroupModel.sortOrder.GetValueOrDefault();
        projectSortableList.Add(projectSortable);
      }
      projectSortableList.Sort((Comparison<CacheManager.ProjectSortable>) ((left, right) =>
      {
        if (left.TeamOrder != right.TeamOrder)
          return left.TeamOrder.CompareTo(right.TeamOrder);
        int projectSortOrders2 = (left.GroupOrder == -1L ? left.ProjectOrder : left.GroupOrder).CompareTo(right.GroupOrder == -1L ? right.ProjectOrder : right.GroupOrder);
        if (projectSortOrders2 == 0)
          projectSortOrders2 = left.ProjectOrder.CompareTo(right.ProjectOrder);
        return projectSortOrders2;
      }));
      for (int index = 0; index < projectSortableList.Count; ++index)
      {
        CacheManager.ProjectSortable projectSortable = projectSortableList[index];
        if (!projectSortOrders1.ContainsKey(projectSortable.ProjectId))
          projectSortOrders1.Add(projectSortable.ProjectId, (long) index);
      }
      return projectSortOrders1;
    }

    public static List<SectionStatusModel> GetSectionStatus()
    {
      return CacheManager.GetDataSafely<SectionStatusModel>("sectionStatus");
    }

    public static List<ProjectGroupModel> GetProjectGroups()
    {
      return CacheManager.GetDataSafely<ProjectGroupModel>("projectGroup");
    }

    public static List<FilterModel> GetFilters()
    {
      return CacheManager.GetDataSafely<FilterModel>("filter");
    }

    public static List<TeamModel> GetTeams()
    {
      return CacheManager.GetDataSafely<TeamModel>("team").OrderByDescending<TeamModel, DateTime>((Func<TeamModel, DateTime>) (team => team.joinedTime)).ToList<TeamModel>();
    }

    public static List<BindCalendarModel> GetBindCalendars()
    {
      return CacheManager.GetDataSafely<BindCalendarModel>("bindCalendars");
    }

    public static List<BindCalendarAccountModel> GetBindCalendarAccounts()
    {
      return CacheManager.GetDataSafely<BindCalendarAccountModel>("bindCalendarAccount").ToList<BindCalendarAccountModel>();
    }

    public static List<CalendarSubscribeProfileModel> GetSubscribeCalendars()
    {
      return CacheManager.GetDataSafely<CalendarSubscribeProfileModel>("calendarProfile");
    }

    public static List<TagModel> GetTags() => CacheManager.GetDataSafely<TagModel>("tag");

    private static List<T> GetDataSafely<T>(string key)
    {
      ICacheable cacheable = CacheManager.GetCacheable(key);
      return cacheable != null && cacheable is CacheBase<T> cacheBase ? cacheBase.GetData().Values.ToList<T>() : new List<T>();
    }

    private static ConcurrentDictionary<string, T> GetDictSafely<T>(string key)
    {
      ICacheable cacheable = CacheManager.GetCacheable(key);
      return cacheable != null && cacheable is CacheBase<T> cacheBase ? cacheBase.GetData() : new ConcurrentDictionary<string, T>();
    }

    public static ProjectModel GetDefaultProjectInGroup(string groupId)
    {
      return CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.groupId == groupId && p.IsProjectPermit())).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).FirstOrDefault<ProjectModel>();
    }

    public static List<ProjectModel> GetProjectsInGroup(string groupId)
    {
      return CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.groupId == groupId && p.IsValid())).ToList<ProjectModel>();
    }

    private static ICacheable GetCacheable(string key)
    {
      return CacheManager.Cacheables != null && CacheManager.Cacheables.Count > 0 && CacheManager.Cacheables.ContainsKey(key) ? CacheManager.Cacheables[key] : (ICacheable) null;
    }

    public static List<string> GetExpiredProjectIds()
    {
      List<string> expiredTeams = CacheManager.GetTeams().Where<TeamModel>((Func<TeamModel, bool>) (t => t.expired)).Select<TeamModel, string>((Func<TeamModel, string>) (team => team.id)).ToList<string>();
      if (expiredTeams.Any<string>())
      {
        List<string> list = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => expiredTeams.Contains(p.teamId))).Select<ProjectModel, string>((Func<ProjectModel, string>) (project => project.id)).ToList<string>();
        if (list.Any<string>())
          return list;
      }
      return new List<string>();
    }

    public static bool IsShareListUser()
    {
      return CacheManager.GetProjects().Any<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsShareList()));
    }

    public static ConcurrentDictionary<string, ProjectModel> GetProjectDict()
    {
      return CacheManager.GetDictSafely<ProjectModel>("project");
    }

    public static ProjectModel GetProjectById(string projectId)
    {
      ProjectModel projectModel;
      return !string.IsNullOrEmpty(projectId) && CacheManager.GetProjectDict().TryGetValue(projectId, out projectModel) ? projectModel : (ProjectModel) null;
    }

    public static ConcurrentDictionary<string, ProjectGroupModel> GetGroupDict()
    {
      return CacheManager.GetDictSafely<ProjectGroupModel>("projectGroup");
    }

    public static ProjectGroupModel GetGroupById(string id)
    {
      ProjectGroupModel projectGroupModel;
      return !string.IsNullOrEmpty(id) && CacheManager.GetGroupDict().TryGetValue(id, out projectGroupModel) ? projectGroupModel : (ProjectGroupModel) null;
    }

    public static ConcurrentDictionary<string, TagModel> GetTagDict()
    {
      return CacheManager.GetDictSafely<TagModel>("tag");
    }

    public static TagModel GetTagByName(string name)
    {
      ConcurrentDictionary<string, TagModel> tagDict = CacheManager.GetTagDict();
      return !string.IsNullOrEmpty(name) && tagDict.ContainsKey(name) ? tagDict[name] : (TagModel) null;
    }

    public static ConcurrentDictionary<string, FilterModel> GetFilterDict()
    {
      return CacheManager.GetDictSafely<FilterModel>("filter");
    }

    public static FilterModel GetFilterById(string filterId)
    {
      ConcurrentDictionary<string, FilterModel> filterDict = CacheManager.GetFilterDict();
      return !string.IsNullOrEmpty(filterId) && filterDict.ContainsKey(filterId) ? filterDict[filterId] : (FilterModel) null;
    }

    public static ConcurrentDictionary<string, CalendarSubscribeProfileModel> GetSubscribeDict()
    {
      return CacheManager.GetDictSafely<CalendarSubscribeProfileModel>("calendarProfile");
    }

    public static ConcurrentDictionary<string, BindCalendarAccountModel> GetAccountCalDict()
    {
      return CacheManager.GetDictSafely<BindCalendarAccountModel>("bindCalendarAccount");
    }

    public static ConcurrentDictionary<string, BindCalendarModel> GetBindCalendarDict()
    {
      return CacheManager.GetDictSafely<BindCalendarModel>("bindCalendars");
    }

    public static ConcurrentDictionary<string, TeamModel> GetTeamDict()
    {
      return CacheManager.GetDictSafely<TeamModel>("team");
    }

    public static BindCalendarAccountModel GetAccountCalById(string accountId)
    {
      ConcurrentDictionary<string, BindCalendarAccountModel> accountCalDict = CacheManager.GetAccountCalDict();
      return !string.IsNullOrEmpty(accountId) && accountCalDict.ContainsKey(accountId) ? accountCalDict[accountId] : (BindCalendarAccountModel) null;
    }

    public static bool CheckProjectValid(string projectId, bool inAll = false)
    {
      ProjectModel projectById = CacheManager.GetProjectById(projectId);
      if (projectById == null || !projectById.IsValid())
        return false;
      return !inAll || projectById.inAll;
    }

    public static bool CheckProjectEnable(string projectId, bool inAll)
    {
      ProjectModel projectById = CacheManager.GetProjectById(projectId);
      if (projectById == null || !projectById.IsEnable())
        return false;
      return !inAll || projectById.inAll;
    }

    public static List<ProjectModel> GetProjectInTeams(string teamId)
    {
      if (teamId == "c1a7e08345e444dea187e21a692f0d7a")
        teamId = (string) null;
      return CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p =>
      {
        if (p.Isinbox)
          return false;
        return string.IsNullOrEmpty(teamId) ? string.IsNullOrEmpty(p.teamId) : p.teamId == teamId;
      })).ToList<ProjectModel>();
    }

    public static TagModel GetPrimaryTag(string[] tags)
    {
      if (tags != null && tags.Length != 0)
      {
        List<TagModel> tags1 = CacheManager.GetTags(tags);
        if (tags1.Count > 0)
          return tags1.OrderBy<TagModel, long>((Func<TagModel, long>) (it => it.sortOrder)).FirstOrDefault<TagModel>();
      }
      return (TagModel) null;
    }

    public static List<TagModel> GetTags(string[] tags)
    {
      List<TagModel> tags1 = new List<TagModel>();
      ConcurrentDictionary<string, TagModel> tagDict = CacheManager.GetTagDict();
      if (tags != null && tags.Length != 0)
      {
        foreach (string tag in tags)
        {
          TagModel tagModel;
          if (!string.IsNullOrEmpty(tag) && tagDict.TryGetValue(tag, out tagModel))
            tags1.Add(tagModel);
        }
      }
      return tags1;
    }

    public static CalendarSubscribeProfileModel GetSubscribeCalById(string calendarId)
    {
      ConcurrentDictionary<string, CalendarSubscribeProfileModel> subscribeDict = CacheManager.GetSubscribeDict();
      CalendarSubscribeProfileModel subscribeProfileModel;
      return !string.IsNullOrEmpty(calendarId) && subscribeDict.TryGetValue(calendarId, out subscribeProfileModel) ? subscribeProfileModel : (CalendarSubscribeProfileModel) null;
    }

    public static BindCalendarModel GetBindCalendarById(string calendarId)
    {
      ConcurrentDictionary<string, BindCalendarModel> bindCalendarDict = CacheManager.GetBindCalendarDict();
      BindCalendarModel bindCalendarModel;
      return !string.IsNullOrEmpty(calendarId) && bindCalendarDict.TryGetValue(calendarId, out bindCalendarModel) ? bindCalendarModel : (BindCalendarModel) null;
    }

    public static TeamModel GetTeamById(string teamId)
    {
      ConcurrentDictionary<string, TeamModel> teamDict = CacheManager.GetTeamDict();
      TeamModel teamModel;
      return !string.IsNullOrEmpty(teamId) && teamDict.TryGetValue(teamId, out teamModel) ? teamModel : (TeamModel) null;
    }

    public static TeamModel GetTeam()
    {
      return CacheManager.GetTeams().FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (t => !t.expired));
    }

    private struct ProjectSortable
    {
      public string ProjectId;
      public long ProjectOrder;
      public long GroupOrder;
      public long TeamOrder;
      public string Name;

      public ProjectSortable(
        string projectId,
        long projectOrder,
        long groupOrder,
        long teamOrder,
        string name)
      {
        this.ProjectId = projectId;
        this.ProjectOrder = projectOrder;
        this.GroupOrder = groupOrder;
        this.TeamOrder = teamOrder;
        this.Name = name;
      }
    }

    private static class CacheKey
    {
      public const string Tag = "tag";
      public const string Project = "project";
      public const string ProjectGroup = "projectGroup";
      public const string Filter = "filter";
      public const string SyncTask = "syncTask";
      public const string CalendarProfile = "calendarProfile";
      public const string BindCalendarAccount = "bindCalendarAccount";
      public const string BindCalendars = "bindCalendars";
      public const string Team = "team";
      public const string SectionStatus = "sectionStatus";
    }
  }
}
