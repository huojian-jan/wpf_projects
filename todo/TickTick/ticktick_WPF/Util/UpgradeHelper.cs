// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.UpgradeHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Summary;
using ticktick_WPF.Views.Tag;
using TickTickDao;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class UpgradeHelper
  {
    private static readonly List<long> CheckPoints = new List<long>()
    {
      1678896000000L,
      1678904400000L,
      1679000000000L,
      1687654800000L,
      1690005000000L,
      1690010000000L,
      1690020000000L,
      1690030000000L,
      1690040000000L
    };

    public static async Task CheckUpgradePoint()
    {
      int num = LocalSettings.Settings.UpgradeCheckPoint == 0L ? 1 : 0;
      long checkPoint = LocalSettings.Settings.UpgradeCheckPoint;
      if (num == 0)
      {
        if (checkPoint < 1687654800000L)
          App.CheckTwitterLogin(false);
        if (!Utils.IsNetworkAvailable())
          return;
        if (checkPoint < 1690040000000L)
        {
          UserPreferenceModel remotePreference = await SettingsHelper.PullRemotePreference(new long?(0L));
          if (checkPoint < 1690040000000L)
          {
            try
            {
              if (Utils.IsTickPackage())
              {
                if (Utils.IsZhCn())
                {
                  if (LocalSettings.Settings.EnableLunar)
                  {
                    if (LocalSettings.Settings.UserPreference != null)
                    {
                      if (remotePreference != null)
                      {
                        UtilLog.Info("migrate lunar calendar");
                        UserPreferenceModel userPreference = LocalSettings.Settings.UserPreference;
                        AlternativeCalendar alternativeCalendar = new AlternativeCalendar();
                        alternativeCalendar.calendar = "lunar";
                        alternativeCalendar.mtime = Utils.GetNowTimeStampInMills();
                        userPreference.alternativeCalendar = alternativeCalendar;
                        LocalSettings.Settings.UserPreference.mtime = Utils.GetNowTimeStampInMills();
                        LocalSettings.Settings.Save();
                        await SettingsHelper.PushLocalPreference();
                      }
                    }
                  }
                }
              }
            }
            catch (Exception ex)
            {
              UtilLog.Error(ex);
            }
          }
          if (checkPoint < 1690030000000L)
          {
            try
            {
              if (LocalSettings.Settings.UserPreference != null && remotePreference != null)
                LocalSettings.Settings.UserPreference.summaryTemplates = remotePreference.summaryTemplates;
              SummaryFilterViewModel.TryInitDefaultTemplate();
            }
            catch (Exception ex)
            {
              UtilLog.Error(ex);
            }
          }
          if (checkPoint < 1666627400000L && LocalSettings.Settings.UserPreference != null && remotePreference != null)
            LocalSettings.Settings.UserPreference.TimeTable = remotePreference.TimeTable;
          if (checkPoint < 1690010000000L && LocalSettings.Settings.UserPreference != null && remotePreference != null)
            LocalSettings.Settings.UserPreference.RecentlyColors = remotePreference.RecentlyColors;
          if (checkPoint < 1636646430000L)
          {
            if (remotePreference?.matrix == null)
              return;
            if (remotePreference.matrix.IsEmpty())
              return;
            if (MatrixManager.CheckNeedUpdateDefaultRule(LocalSettings.Settings.MatrixModel))
              await MatrixManager.ResetMatrixRule(false);
          }
          if (checkPoint < 1690005000000L && LocalSettings.Settings.UserPreference != null && remotePreference != null)
            LocalSettings.Settings.UserPreference.FocusConfig = remotePreference.FocusConfig;
          remotePreference = (UserPreferenceModel) null;
        }
        if (checkPoint < 1672000100000L)
          UpgradeHelper.UpdateSkipHabit();
      }
      else
        LocalSettings.Settings.ExtraSettings.ShowParentInCalArrange = true;
      if (checkPoint < 1690020000000L)
        Task.Run((Func<Task>) (async () =>
        {
          SyncBean syncBean = await Communicator.BatchCheck(Math.Min(Math.Min(1678896000000L, 1678904400000L), Math.Min(1687654800000L, 1690020000000L)));
          if (checkPoint < 1678896000000L)
            await UpgradeHelper.UpdateProjectAndTeams(syncBean);
          if (checkPoint < 1678904400000L)
          {
            await UpgradeHelper.UpdateSortOptions(syncBean);
            UpgradeHelper.UpdateTaskSortOrders();
            UpgradeHelper.UpdateSmartSortOptions();
            SyncManager.TryDelaySync();
          }
          if (checkPoint < 1687654800000L)
            await UpgradeHelper.UpdateKanbanView(syncBean);
          if (checkPoint >= 1690020000000L)
          {
            syncBean = (SyncBean) null;
          }
          else
          {
            await UpgradeHelper.UpdateTagType(syncBean);
            syncBean = (SyncBean) null;
          }
        }));
      if (checkPoint < 1679000000000L)
        UpgradeHelper.UpdateNewTaskSortOrders();
      LocalSettings.Settings.UpgradeCheckPoint = UpgradeHelper.CheckPoints.LastOrDefault<long>();
      await LocalSettings.Settings.Save();
    }

    private static async Task UpdateTagType(SyncBean syncBean)
    {
      List<TagModel> allTags = await TagDao.GetAllTags();
      SyncBean syncBean1 = syncBean;
      Dictionary<string, TagModel> dictionary;
      if (syncBean1 == null)
      {
        dictionary = (Dictionary<string, TagModel>) null;
      }
      else
      {
        List<TagModel> tags = syncBean1.tags;
        dictionary = tags != null ? tags.ToDictionaryEx<TagModel, string, TagModel>((Func<TagModel, string>) (t => t.name), (Func<TagModel, TagModel>) (t => t)) : (Dictionary<string, TagModel>) null;
      }
      Dictionary<string, TagModel> serverTag = dictionary;
      foreach (TagModel tag in allTags)
      {
        if (serverTag != null && serverTag.ContainsKey(tag.name))
        {
          TagModel tagModel = serverTag[tag.name];
          if (tagModel != null && tagModel.type != tag.type)
          {
            tag.type = tagModel.type;
            int num = await BaseDao<TagModel>.UpdateAsync(tag);
            CacheManager.UpdateTag(tag, true);
          }
        }
      }
      serverTag = (Dictionary<string, TagModel>) null;
    }

    private static async Task UpdateKanbanView(SyncBean syncBean)
    {
      List<TagModel> allTags = await TagDao.GetAllTags();
      SyncBean syncBean1 = syncBean;
      Dictionary<string, TagModel> dictionary;
      if (syncBean1 == null)
      {
        dictionary = (Dictionary<string, TagModel>) null;
      }
      else
      {
        List<TagModel> tags = syncBean1.tags;
        dictionary = tags != null ? tags.ToDictionaryEx<TagModel, string, TagModel>((Func<TagModel, string>) (t => t.name), (Func<TagModel, TagModel>) (t => t)) : (Dictionary<string, TagModel>) null;
      }
      Dictionary<string, TagModel> serverTag = dictionary;
      foreach (TagModel tag in allTags)
      {
        if (serverTag != null && serverTag.ContainsKey(tag.name))
        {
          TagModel tagModel = serverTag[tag.name];
          if (tagModel != null && tagModel.viewMode != tag.viewMode)
          {
            tag.viewMode = tagModel.viewMode;
            int num = await BaseDao<TagModel>.UpdateAsync(tag);
            CacheManager.UpdateTag(tag, true);
          }
        }
      }
      serverTag = (Dictionary<string, TagModel>) null;
    }

    private static async Task UpdateNewTaskSortOrders()
    {
      List<SyncSortOrderModel> version2Models = await SyncSortOrderDao.GetVersion2Models();
      List<SyncSortOrderModel> models = new List<SyncSortOrderModel>();
      List<SyncSortOrderModel> add = new List<SyncSortOrderModel>();
      Regex regex = new Regex("taskBy#(.+)_(.+)");
      Dictionary<string, long> tagSortDict = TagDataHelper.GetTagSortDict();
      foreach (SyncSortOrderModel syncSortOrderModel in version2Models.Where<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (model => !model.SortOrderType.Contains("|"))))
      {
        Match match = regex.Match(syncSortOrderModel.SortOrderType);
        if (match.Success)
        {
          string groupBy = match.Groups[1].Value;
          string str1 = match.Groups[2].Value;
          string str2 = syncSortOrderModel.GroupId;
          if (str2 == "today" || str2 == "tomorrow" || str2 == "week" || str2 == "n7ds")
            str2 = "all";
          if (str2 == "assigned")
            str2 = "assignee";
          TaskBaseViewModel task = (TaskBaseViewModel) null;
          if (syncSortOrderModel.Type == 1)
            task = TaskCache.GetTaskById(syncSortOrderModel.EntityId);
          else if (syncSortOrderModel.Type == 2)
            task = TaskDetailItemCache.GetCheckItemById(syncSortOrderModel.EntityId);
          string groupNameByTask = UpgradeHelper.GetGroupNameByTask(task, tagSortDict, groupBy, str2 == "all");
          if (groupNameByTask != null)
          {
            string type = "taskBy#" + groupBy + "|" + groupNameByTask + "_" + str1;
            add.Add(new SyncSortOrderModel(type)
            {
              Type = syncSortOrderModel.Type,
              SyncStatus = 1,
              SortOrder = syncSortOrderModel.SortOrder,
              EntityId = syncSortOrderModel.EntityId,
              GroupId = str2,
              UserId = syncSortOrderModel.UserId
            });
          }
          else
            models.Add(syncSortOrderModel);
        }
      }
      foreach (SyncSortOrderModel syncSortOrderModel in models)
      {
        syncSortOrderModel.SyncStatus = 1;
        syncSortOrderModel.Deleted = 1;
      }
      int num1 = await SyncSortOrderDao.UpdateAllAsync(models);
      int num2 = await SyncSortOrderDao.InsertAllAsync(add);
      TaskSortOrderService.FetchAllAsync();
      add = (List<SyncSortOrderModel>) null;
    }

    private static string GetGroupNameByTask(
      TaskBaseViewModel task,
      Dictionary<string, long> sortedTagDict,
      string groupBy,
      bool inAll)
    {
      switch (groupBy)
      {
        case "none":
          return "none";
        case "sortOrder":
          return !string.IsNullOrEmpty(task?.ColumnId) ? task.ColumnId : "none";
        case "dueDate":
          if (task == null)
            return DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) DateTimeFormatInfo.InvariantInfo);
          DateTime? startDate = task.StartDate;
          if (!startDate.HasValue)
            return "noDate";
          startDate = task.StartDate;
          DateTime today = DateTime.Today;
          if ((startDate.HasValue ? (startDate.GetValueOrDefault() >= today ? 1 : 0) : 0) != 0)
          {
            startDate = task.StartDate;
            DateTime dateTime = DateTime.Today.AddDays(inAll ? 7.0 : 2.0);
            if ((startDate.HasValue ? (startDate.GetValueOrDefault() < dateTime ? 1 : 0) : 0) != 0)
            {
              startDate = task.StartDate;
              return startDate.Value.ToString("yyyyMMdd", (IFormatProvider) DateTimeFormatInfo.InvariantInfo);
            }
          }
          return (string) null;
        case "priority":
          return task != null ? task.Priority.ToString() ?? "" : "0";
        case "tag":
          if (task == null)
            return "noTag";
          string primaryTag = TagDataHelper.GetPrimaryTag(sortedTagDict, (IList<string>) ((IEnumerable<string>) task.Tags).ToList<string>(), (ICollection<string>) null);
          return !string.IsNullOrEmpty(primaryTag) ? primaryTag : "noTag";
        case "assignee":
          return task != null && !string.IsNullOrEmpty(task.Assignee) ? task.Assignee : "-1";
        default:
          return (string) null;
      }
    }

    private static async Task UpdateTaskSortOrders()
    {
      string keyId = "taskBy#dueDate|{0}_dueDate";
      Dictionary<string, Dictionary<string, Dictionary<string, SyncSortOrderModel>>> dict = new Dictionary<string, Dictionary<string, Dictionary<string, SyncSortOrderModel>>>();
      foreach (TaskSortOrderInDateModel allSort in await TaskSortOrderInDateDao.GetAllSorts())
      {
        if (allSort.projectid != null && allSort.taskid != null)
        {
          string str = string.Format(keyId, (object) allSort.date);
          if (!dict.ContainsKey(str))
            dict[str] = new Dictionary<string, Dictionary<string, SyncSortOrderModel>>();
          Dictionary<string, Dictionary<string, SyncSortOrderModel>> dictionary = dict[str];
          string projectid = allSort.projectid;
          if (!dictionary.ContainsKey(projectid))
            dictionary[projectid] = new Dictionary<string, SyncSortOrderModel>();
          dictionary[projectid][allSort.taskid] = SyncSortOrderModel.Build(str, allSort);
        }
      }
      await TaskSortOrderService.SaveInits(dict);
      keyId = "taskBy#project|{0}_dueDate";
      dict.Clear();
      foreach (TaskSortOrderInProjectModel localSortOrder in await ProjectSortOrderDao.GetLocalSortOrders())
      {
        if (localSortOrder.ProjectId != null && localSortOrder.EntityId != null)
        {
          string str = string.Format(keyId, (object) localSortOrder.ProjectId);
          if (!dict.ContainsKey(str))
            dict[str] = new Dictionary<string, Dictionary<string, SyncSortOrderModel>>();
          Dictionary<string, Dictionary<string, SyncSortOrderModel>> dictionary = dict[str];
          if (!dictionary.ContainsKey(localSortOrder.ProjectId))
            dictionary[localSortOrder.ProjectId] = new Dictionary<string, SyncSortOrderModel>();
          dictionary[localSortOrder.ProjectId][localSortOrder.EntityId] = SyncSortOrderModel.Build(str, localSortOrder);
        }
      }
      await TaskSortOrderService.SaveInits(dict);
      keyId = "taskBy#priority|{0}_dueDate";
      dict.Clear();
      foreach (TaskSortOrderInPriorityModel allSortOrder in await TaskSortOrderInPriorityDao.GetAllSortOrders())
      {
        if (allSortOrder.CatId != null && allSortOrder.EntityId != null)
        {
          string str = string.Format(keyId, (object) allSortOrder.Priority);
          if (!dict.ContainsKey(str))
            dict[str] = new Dictionary<string, Dictionary<string, SyncSortOrderModel>>();
          Dictionary<string, Dictionary<string, SyncSortOrderModel>> dictionary = dict[str];
          string catId = allSortOrder.CatId;
          if (!dictionary.ContainsKey(catId))
            dictionary[catId] = new Dictionary<string, SyncSortOrderModel>();
          dictionary[catId][allSortOrder.EntityId] = SyncSortOrderModel.Build(str, allSortOrder);
        }
      }
      await TaskSortOrderService.SaveInits(dict);
      keyId = "taskBy#tag|{0}_dueDate";
      dict.Clear();
      foreach (SyncSortOrderModel model in await SyncSortOrderDao.SetAllTaskOrderInTag())
      {
        if (model.GroupId != null && model.EntityId != null)
        {
          string str = string.Format(keyId, (object) model.SortOrderType.Replace("taskByTag_", ""));
          if (!dict.ContainsKey(str))
            dict[str] = new Dictionary<string, Dictionary<string, SyncSortOrderModel>>();
          Dictionary<string, Dictionary<string, SyncSortOrderModel>> dictionary = dict[str];
          string groupId = model.GroupId;
          if (!dictionary.ContainsKey(groupId))
            dictionary[groupId] = new Dictionary<string, SyncSortOrderModel>();
          dictionary[groupId][model.EntityId] = SyncSortOrderModel.Build(str, model);
        }
      }
      await TaskSortOrderService.SaveInits(dict);
      TaskSortOrderService.FetchAllAsync();
      keyId = (string) null;
      dict = (Dictionary<string, Dictionary<string, Dictionary<string, SyncSortOrderModel>>>) null;
    }

    private static async Task UpdateSmartSortOptions()
    {
      UserPreferenceModel userPreferenceModel = await SettingsHelper.PullRemotePreference(new long?(0L));
      if (userPreferenceModel?.matrix != null)
      {
        List<QuadrantModel> quadrants = userPreferenceModel.matrix.quadrants;
        if ((quadrants != null ? (quadrants.Any<QuadrantModel>((Func<QuadrantModel, bool>) (q => q.SortOption != null)) ? 1 : 0) : 0) != 0)
        {
          LocalSettings.Settings.UserPreference.matrix = userPreferenceModel.matrix;
          goto label_5;
        }
      }
      LocalSettings.Settings.InitMatrixSortOption();
label_5:
      if (userPreferenceModel?.SmartProjectsOption != null)
        LocalSettings.Settings.UserPreference.SmartProjectsOption = userPreferenceModel.SmartProjectsOption;
      else
        LocalSettings.Settings.InitSmartProjectsOption();
    }

    private static async Task UpdateSortOptions(SyncBean syncBean)
    {
      ObservableCollection<ProjectModel> allProjects = await ProjectDao.GetAllProjects();
      SyncBean syncBean1 = syncBean;
      Dictionary<string, ProjectModel> dictionary1;
      if (syncBean1 == null)
      {
        dictionary1 = (Dictionary<string, ProjectModel>) null;
      }
      else
      {
        ObservableCollection<ProjectModel> projectProfiles = syncBean1.projectProfiles;
        dictionary1 = projectProfiles != null ? projectProfiles.ToDictionaryEx<ProjectModel, string, ProjectModel>((Func<ProjectModel, string>) (p => p.id), (Func<ProjectModel, ProjectModel>) (p => p)) : (Dictionary<string, ProjectModel>) null;
      }
      Dictionary<string, ProjectModel> serverProj = dictionary1;
      foreach (ProjectModel project in (Collection<ProjectModel>) allProjects)
      {
        if (serverProj != null && serverProj.ContainsKey(project.id))
        {
          ProjectModel projectModel = serverProj[project.id];
          if (projectModel.SortOption != null)
          {
            project.SortOption = projectModel.SortOption;
            if (project.SyncTimeline != null && projectModel.SyncTimeline?.SortOption != null)
              project.SyncTimeline.SortOption = projectModel.SyncTimeline.SortOption;
            if (project.Timeline != null && projectModel.SyncTimeline?.SortOption != null)
              project.Timeline.sortOption = projectModel.SyncTimeline.SortOption;
            int num = await BaseDao<ProjectModel>.UpdateAsync(project);
            CacheManager.UpdateProject(project, false);
            continue;
          }
        }
        if (!project.Isinbox && project.SortOption == null)
        {
          project.SortOption = project.GetSortOption();
          project.SyncTimeline = new TimelineSyncModel(project.SyncTimeline.SortType);
          project.Timeline.sortOption = project.SyncTimeline.SortOption;
          project.sync_status = Constants.SyncStatus.SYNC_INIT.ToString();
          int num = await BaseDao<ProjectModel>.UpdateAsync(project);
          CacheManager.UpdateProject(project, false);
        }
      }
      List<TagModel> allTags = await TagDao.GetAllTags();
      SyncBean syncBean2 = syncBean;
      Dictionary<string, TagModel> dictionary2;
      if (syncBean2 == null)
      {
        dictionary2 = (Dictionary<string, TagModel>) null;
      }
      else
      {
        List<TagModel> tags = syncBean2.tags;
        dictionary2 = tags != null ? tags.ToDictionaryEx<TagModel, string, TagModel>((Func<TagModel, string>) (t => t.name), (Func<TagModel, TagModel>) (t => t)) : (Dictionary<string, TagModel>) null;
      }
      Dictionary<string, TagModel> serverTag = dictionary2;
      foreach (TagModel tag in allTags)
      {
        if (serverTag != null && serverTag.ContainsKey(tag.name))
        {
          TagModel tagModel = serverTag[tag.name];
          if (tagModel.SortOption != null)
          {
            tag.SortOption = tagModel.SortOption;
            int num = await BaseDao<TagModel>.UpdateAsync(tag);
            CacheManager.UpdateTag(tag, true);
            continue;
          }
        }
        if (tag.SortOption == null)
        {
          tag.SortOption = tag.GetSortOption();
          if (tag.status == 2)
            tag.status = 3;
          int num = await BaseDao<TagModel>.UpdateAsync(tag);
          CacheManager.UpdateTag(tag, true);
        }
      }
      List<FilterModel> allFilters = await FilterDao.GetAllFilters();
      SyncBean syncBean3 = syncBean;
      Dictionary<string, FilterModel> dictionary3;
      if (syncBean3 == null)
      {
        dictionary3 = (Dictionary<string, FilterModel>) null;
      }
      else
      {
        ObservableCollection<FilterModel> filters = syncBean3.filters;
        dictionary3 = filters != null ? filters.ToDictionaryEx<FilterModel, string, FilterModel>((Func<FilterModel, string>) (m => m.id), (Func<FilterModel, FilterModel>) (t => t)) : (Dictionary<string, FilterModel>) null;
      }
      Dictionary<string, FilterModel> serverFilter = dictionary3;
      foreach (FilterModel filter in allFilters)
      {
        if (serverFilter != null && serverFilter.ContainsKey(filter.id))
        {
          FilterModel filterModel = serverFilter[filter.id];
          if (filterModel.SortOption != null)
          {
            filter.SortOption = filterModel.SortOption;
            if (filter.SyncTimeline != null && filterModel.SyncTimeline?.SortOption != null)
              filter.SyncTimeline.SortOption = filterModel.SyncTimeline.SortOption;
            if (filter.Timeline != null && filterModel.SyncTimeline?.SortOption != null)
              filter.Timeline.sortOption = filterModel.SyncTimeline.SortOption;
            int num = await BaseDao<FilterModel>.UpdateAsync(filter);
            CacheManager.UpdateFilter(filter);
            continue;
          }
        }
        if (filter.SortOption == null)
        {
          filter.SortOption = filter.GetSortOption();
          if (filter.syncStatus == 2)
            filter.syncStatus = 3;
          filter.SyncTimeline = new TimelineSyncModel(filter.SyncTimeline.SortType);
          filter.Timeline.sortOption = filter.SyncTimeline.SortOption;
          int num = await BaseDao<FilterModel>.UpdateAsync(filter);
          CacheManager.UpdateFilter(filter);
        }
      }
      Dictionary<string, ProjectGroupModel>.ValueCollection values = (await ProjectGroupDao.GetLocalSyncedProjectGroupMap()).Values;
      SyncBean syncBean4 = syncBean;
      Dictionary<string, ProjectGroupModel> dictionary4;
      if (syncBean4 == null)
      {
        dictionary4 = (Dictionary<string, ProjectGroupModel>) null;
      }
      else
      {
        ObservableCollection<ProjectGroupModel> projectGroups = syncBean4.projectGroups;
        dictionary4 = projectGroups != null ? projectGroups.ToDictionaryEx<ProjectGroupModel, string, ProjectGroupModel>((Func<ProjectGroupModel, string>) (g => g.id), (Func<ProjectGroupModel, ProjectGroupModel>) (g => g)) : (Dictionary<string, ProjectGroupModel>) null;
      }
      Dictionary<string, ProjectGroupModel> serverGroup = dictionary4;
      foreach (ProjectGroupModel group in values)
      {
        if (serverGroup != null && serverGroup.ContainsKey(group.id))
        {
          ProjectGroupModel projectGroupModel = serverGroup[group.id];
          if (projectGroupModel.SortOption != null)
          {
            group.SortOption = projectGroupModel.SortOption;
            if (group.SyncTimeline != null && projectGroupModel.SyncTimeline?.SortOption != null)
              group.SyncTimeline.SortOption = projectGroupModel.SyncTimeline.SortOption;
            if (group.Timeline != null && projectGroupModel.SyncTimeline?.SortOption != null)
              group.Timeline.sortOption = projectGroupModel.SyncTimeline.SortOption;
            int num = await BaseDao<ProjectGroupModel>.UpdateAsync(group);
            CacheManager.UpdateProjectGroup(group);
            continue;
          }
        }
        if (group.SortOption == null)
        {
          group.SortOption = group.GetSortOption();
          string syncStatus1 = group.sync_status;
          Constants.SyncStatus syncStatus2 = Constants.SyncStatus.SYNC_DONE;
          string str1 = syncStatus2.ToString();
          if (syncStatus1 == str1)
          {
            ProjectGroupModel projectGroupModel = group;
            syncStatus2 = Constants.SyncStatus.SYNC_INIT;
            string str2 = syncStatus2.ToString();
            projectGroupModel.sync_status = str2;
          }
          group.SyncTimeline = new TimelineSyncModel(group.SyncTimeline.SortType);
          group.Timeline.sortOption = group.SyncTimeline.SortOption;
          int num = await BaseDao<ProjectGroupModel>.UpdateAsync(group);
          CacheManager.UpdateProjectGroup(group);
        }
      }
      serverProj = (Dictionary<string, ProjectModel>) null;
      serverTag = (Dictionary<string, TagModel>) null;
      serverFilter = (Dictionary<string, FilterModel>) null;
      serverGroup = (Dictionary<string, ProjectGroupModel>) null;
    }

    private static async Task UpdateProjectAndTeams(SyncBean syncBean)
    {
      if (syncBean?.projectProfiles != null)
      {
        foreach (ProjectModel project in (Collection<ProjectModel>) syncBean.projectProfiles)
        {
          ProjectModel projectById = await ProjectDao.GetProjectById(project.id);
          if (projectById != null)
          {
            projectById.openToTeam = project.openToTeam;
            projectById.needAudit = project.needAudit;
            projectById.teamMemberPermission = project.teamMemberPermission;
            CacheManager.UpdateProject(projectById, false);
            BaseDao<ProjectModel>.UpdateAsync(projectById);
          }
        }
      }
      List<TeamModel> remoteTeams = await Communicator.GetAllTeams();
      if (remoteTeams == null)
      {
        remoteTeams = (List<TeamModel>) null;
      }
      else
      {
        List<TeamModel> allTeams = await TeamDao.GetAllTeams();
        using (List<TeamModel>.Enumerator enumerator = remoteTeams.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            TeamModel team = enumerator.Current;
            TeamModel team1 = allTeams.FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (l => l.id == team.id));
            if (team1 != null)
            {
              team1.needAuditCode = team.needAuditCode;
              team1.needAuditUrl = team.needAuditUrl;
              team1.industry = team.industry;
              team1.scale = team.scale;
              team1.logo = team.logo;
              team1.teamPro = team.teamPro;
              CacheManager.UpdateTeam(team1);
              TeamDao.UpdateTeam(team1);
            }
          }
          remoteTeams = (List<TeamModel>) null;
        }
      }
    }

    private static async Task UpdateSkipHabit()
    {
      List<HabitModel> remotes = await HabitService.PullRemoteHabits(false) ?? new List<HabitModel>();
      List<HabitModel> localHabits = await HabitDao.GetAllHabits() ?? new List<HabitModel>();
      foreach (HabitModel habitModel1 in localHabits)
      {
        HabitModel habit = habitModel1;
        HabitModel habitModel2 = remotes.FirstOrDefault<HabitModel>((Func<HabitModel, bool>) (h => h.Id == habit.Id));
        if (habitModel2 != null)
          habit.ExDates = habitModel2.ExDates;
      }
      List<SkipHabitModel> skipHabitModelList = await HabitDao.GetAllSkipHabitModels() ?? new List<SkipHabitModel>();
      if (skipHabitModelList.Count > 0)
      {
        Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
        foreach (SkipHabitModel skipHabitModel in skipHabitModelList)
        {
          DateTime result;
          if (!string.IsNullOrEmpty(skipHabitModel.HabitId) && DateTime.TryParseExact(skipHabitModel.Stamp, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result) && result > DateTime.Today.AddDays(-30.0))
          {
            if (dictionary.ContainsKey(skipHabitModel.HabitId))
              dictionary[skipHabitModel.HabitId].Add(skipHabitModel.Stamp);
            else
              dictionary[skipHabitModel.HabitId] = new List<string>()
              {
                skipHabitModel.Stamp
              };
          }
        }
        foreach (string str in dictionary.Keys.ToList<string>())
        {
          string id = str;
          HabitModel habitModel = localHabits.FirstOrDefault<HabitModel>((Func<HabitModel, bool>) (h => h.Id == id));
          if (habitModel != null)
          {
            List<string> list = dictionary[habitModel.Id];
            if (habitModel.ExDates != null)
            {
              list.AddRange((IEnumerable<string>) habitModel.ExDates);
              list = list.Distinct<string>().ToList<string>();
            }
            habitModel.ExDates = list.ToArray();
            habitModel.SyncStatus = habitModel.SyncStatus != 0 ? 1 : 0;
          }
        }
      }
      int num = await HabitDao.UpdateAllAsync(localHabits);
      remotes = (List<HabitModel>) null;
      localHabits = (List<HabitModel>) null;
    }
  }
}
