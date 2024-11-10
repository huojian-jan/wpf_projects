// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ProjectDataProvider
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public static class ProjectDataProvider
  {
    public static List<PtfType> SortedPtfTypes = new List<PtfType>()
    {
      PtfType.Project,
      PtfType.Tag,
      PtfType.Filter,
      PtfType.Subscribe
    };

    public static async Task<List<ProjectItemViewModel>> GetProjectData()
    {
      return await ProjectDataProvider.AddPtf(await ProjectDataProvider.AddSpecialProjects());
    }

    private static async Task<List<ProjectItemViewModel>> AddSpecialProjects()
    {
      List<ProjectItemViewModel> specialProjects = new List<ProjectItemViewModel>();
      await ProjectDataProvider.TryAddSpecialProject("SmartListAll", (SmartProject) new AllProject(), "_special_id_all", (ICollection<ProjectItemViewModel>) specialProjects);
      await ProjectDataProvider.TryAddSpecialProject("SmartListToday", (SmartProject) new TodayProject(), "_special_id_today", (ICollection<ProjectItemViewModel>) specialProjects);
      await ProjectDataProvider.TryAddSpecialProject("SmartListTomorrow", (SmartProject) new TomorrowProject(), "_special_id_tomorrow", (ICollection<ProjectItemViewModel>) specialProjects);
      await ProjectDataProvider.TryAddSpecialProject("SmartList7Day", (SmartProject) new WeekProject(), "_special_id_week", (ICollection<ProjectItemViewModel>) specialProjects);
      await ProjectDataProvider.TryAddSpecialProject("SmartListForMe", (SmartProject) new AssignProject(), "_special_id_assigned", (ICollection<ProjectItemViewModel>) specialProjects);
      InboxProject project = new InboxProject();
      await ProjectDataProvider.TryAddInboxProject("SmartListInbox", (SmartProject) project, project.Id, (ICollection<ProjectItemViewModel>) specialProjects);
      ProjectDataProvider.TryAddSpecialProject("SmartListSummary", (SmartProject) new SummaryProject(), "", (ICollection<ProjectItemViewModel>) specialProjects);
      if (specialProjects.Count > 0)
      {
        List<ProjectItemViewModel> projectItemViewModelList = specialProjects;
        LineViewModel lineViewModel = new LineViewModel();
        lineViewModel.SortOrder = 8001L;
        projectItemViewModelList.Add((ProjectItemViewModel) lineViewModel);
      }
      List<ProjectItemViewModel> projectItemViewModelList1 = specialProjects;
      PtfAllViewModel ptfAllViewModel1 = new PtfAllViewModel(PtfType.Project);
      ptfAllViewModel1.SortOrder = (long) (8002 + ProjectDataProvider.SortedPtfTypes.IndexOf(PtfType.Project));
      projectItemViewModelList1.Add((ProjectItemViewModel) ptfAllViewModel1);
      if (LocalSettings.Settings.SmartListTag != 1)
      {
        List<ProjectItemViewModel> projectItemViewModelList2 = specialProjects;
        PtfAllViewModel ptfAllViewModel2 = new PtfAllViewModel(PtfType.Tag);
        ptfAllViewModel2.SortOrder = (long) (8002 + ProjectDataProvider.SortedPtfTypes.IndexOf(PtfType.Tag));
        projectItemViewModelList2.Add((ProjectItemViewModel) ptfAllViewModel2);
      }
      if (LocalSettings.Settings.ShowCustomSmartList != 1)
      {
        List<ProjectItemViewModel> projectItemViewModelList3 = specialProjects;
        PtfAllViewModel ptfAllViewModel3 = new PtfAllViewModel(PtfType.Filter);
        ptfAllViewModel3.SortOrder = (long) (8002 + ProjectDataProvider.SortedPtfTypes.IndexOf(PtfType.Filter));
        projectItemViewModelList3.Add((ProjectItemViewModel) ptfAllViewModel3);
      }
      ProjectDataProvider.TryAddCalendars((ICollection<ProjectItemViewModel>) specialProjects, 8002 + ProjectDataProvider.SortedPtfTypes.IndexOf(PtfType.Subscribe));
      if (LocalSettings.Settings.SmartListTrash == 0 || LocalSettings.Settings.SmartListComplete == 0 || LocalSettings.Settings.SmartListSummary == 0)
      {
        LineViewModel lineViewModel = new LineViewModel();
        lineViewModel.SortOrder = 10000L;
        specialProjects.Add((ProjectItemViewModel) lineViewModel);
      }
      ProjectDataProvider.TryAddSpecialProject("SmartListComplete", (SmartProject) new CompletedProject(), "", (ICollection<ProjectItemViewModel>) specialProjects);
      ProjectDataProvider.TryAddSpecialProject("SmartListAbandoned", (SmartProject) new AbandonedProject(), "_special_id_abandoned", (ICollection<ProjectItemViewModel>) specialProjects);
      ProjectDataProvider.TryAddSpecialProject("SmartListTrash", (SmartProject) new TrashProject(), "", (ICollection<ProjectItemViewModel>) specialProjects);
      List<ProjectItemViewModel> list = specialProjects.OrderBy<ProjectItemViewModel, long>((Func<ProjectItemViewModel, long>) (p => p.SortOrder)).ToList<ProjectItemViewModel>();
      specialProjects = (List<ProjectItemViewModel>) null;
      return list;
    }

    public static void ReSortPtfTypes()
    {
      List<SmartProjectConf> smartProjects = LocalSettings.Settings.SmartProjects;
      Dictionary<PtfType, long> sortOrders = smartProjects != null ? smartProjects.Where<SmartProjectConf>((Func<SmartProjectConf, bool>) (p => SmartProjectConf.ProjectSectionName.Contains(p.name))).ToDictionary<SmartProjectConf, PtfType, long>((Func<SmartProjectConf, PtfType>) (p => GetPtfTypeByName(p.name)), (Func<SmartProjectConf, long>) (p => p.order ?? 3L)) : (Dictionary<PtfType, long>) null;
      if (sortOrders == null)
        return;
      ProjectDataProvider.SortedPtfTypes.Sort((Comparison<PtfType>) ((a, b) =>
      {
        if (sortOrders.ContainsKey(a) && sortOrders.ContainsKey(b))
          return sortOrders[a].CompareTo(sortOrders[b]);
        return !sortOrders.ContainsKey(a) && sortOrders.ContainsKey(b) ? 1 : 0;
      }));

      static PtfType GetPtfTypeByName(string name)
      {
        switch (name)
        {
          case "project":
            return PtfType.Project;
          case "tag":
            return PtfType.Tag;
          case "filter":
            return PtfType.Filter;
          case "subscribedCalendar":
            return PtfType.Subscribe;
          default:
            return PtfType.Project;
        }
      }
    }

    public static void SetNewSortOrders(IEnumerable<PtfAllViewModel> ptfs)
    {
      long num = 0;
      List<SmartProjectConf> smartProjects = LocalSettings.Settings.SmartProjects;
      List<SmartProjectConf> list = smartProjects != null ? smartProjects.Where<SmartProjectConf>((Func<SmartProjectConf, bool>) (p => SmartProjectConf.ProjectSectionName.Contains(p.name))).ToList<SmartProjectConf>() : (List<SmartProjectConf>) null;
      foreach (PtfAllViewModel ptf in ptfs)
      {
        string name = GetNameByPtfType(ptf.Type);
        SmartProjectConf smartProjectConf = list != null ? list.FirstOrDefault<SmartProjectConf>((Func<SmartProjectConf, bool>) (p => p.name == name)) : (SmartProjectConf) null;
        if (smartProjectConf == null)
          LocalSettings.Settings.AddSmartProjectConf(new SmartProjectConf()
          {
            name = name,
            order = new long?(num)
          });
        else
          smartProjectConf.order = new long?(num);
        ++num;
      }
      ProjectDataProvider.ReSortPtfTypes();
      SettingsHelper.PushLocalSettings();

      static string GetNameByPtfType(PtfType type)
      {
        switch (type)
        {
          case PtfType.Project:
            return "project";
          case PtfType.Tag:
            return "tag";
          case PtfType.Filter:
            return "filter";
          case PtfType.Subscribe:
            return "subscribedCalendar";
          default:
            return "project";
        }
      }
    }

    private static void TryAddCalendars(
      ICollection<ProjectItemViewModel> projects,
      int startSortOrder)
    {
      List<BindCalendarAccountModel> calendarAccounts = CacheManager.GetBindCalendarAccounts();
      List<BindCalendarModel> bindCalendars = CacheManager.GetBindCalendars();
      Func<BindCalendarAccountModel, bool> predicate = (Func<BindCalendarAccountModel, bool>) (a => bindCalendars.Any<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.AccountId == a.Id && cal.Show != "hidden")));
      List<BindCalendarAccountModel> list1 = calendarAccounts.Where<BindCalendarAccountModel>(predicate).ToList<BindCalendarAccountModel>();
      List<CalendarSubscribeProfileModel> list2 = CacheManager.GetSubscribeCalendars().Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (profile => profile.Show != "hidden")).ToList<CalendarSubscribeProfileModel>();
      if (!list1.Any<BindCalendarAccountModel>() && !list2.Any<CalendarSubscribeProfileModel>())
        return;
      ICollection<ProjectItemViewModel> projectItemViewModels = projects;
      PtfAllViewModel ptfAllViewModel = new PtfAllViewModel(PtfType.Subscribe);
      ptfAllViewModel.SortOrder = (long) startSortOrder;
      projectItemViewModels.Add((ProjectItemViewModel) ptfAllViewModel);
    }

    public static void AddSubscribeCalendar(IList<ProjectItemViewModel> itemList, int i)
    {
      List<BindCalendarAccountModel> calendarAccounts = CacheManager.GetBindCalendarAccounts();
      List<BindCalendarModel> bindCalendars = CacheManager.GetBindCalendars();
      calendarAccounts.Sort((Comparison<BindCalendarAccountModel>) ((a, b) =>
      {
        int accountType1 = SubscribeCalendarHelper.GetAccountType(a);
        int accountType2 = SubscribeCalendarHelper.GetAccountType(b);
        return accountType1 == accountType2 && a.CreatedTime.HasValue ? a.CreatedTime.Value.CompareTo((object) b.CreatedTime) : accountType1.CompareTo(accountType2);
      }));
      List<BindCalendarAccountModel> list1 = calendarAccounts.Where<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (a =>
      {
        List<BindCalendarModel> list2 = bindCalendars.Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.AccountId == a.Id)).ToList<BindCalendarModel>();
        return !list2.Any<BindCalendarModel>() || list2.Any<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Show != "hidden"));
      })).ToList<BindCalendarAccountModel>();
      List<CalendarSubscribeProfileModel> list3 = CacheManager.GetSubscribeCalendars().Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (profile => profile.Show != "hidden")).ToList<CalendarSubscribeProfileModel>();
      list3.Sort((Comparison<CalendarSubscribeProfileModel>) ((a, b) =>
      {
        DateTime? createdTime = a.CreatedTime;
        ref DateTime? local = ref createdTime;
        return !local.HasValue ? -1 : local.GetValueOrDefault().CompareTo((object) b.CreatedTime);
      }));
      foreach (BindCalendarAccountModel account in list1)
        itemList.Insert(i++, (ProjectItemViewModel) new BindCalendarAccountProjectViewModel(account));
      foreach (CalendarSubscribeProfileModel profile in list3)
        itemList.Insert(i++, (ProjectItemViewModel) new SubscribeCalendarProjectViewModel(profile));
    }

    public static void AddNormalProjects(IList<ProjectItemViewModel> itemList, int insertIndex)
    {
      List<ProjectItemViewModel> list = ProjectDataProvider.AssembleProjects().ToList<ProjectItemViewModel>();
      if (list.Any<ProjectItemViewModel>())
      {
        foreach (ProjectItemViewModel projectItemViewModel in list)
          itemList.Insert(insertIndex++, projectItemViewModel);
      }
      else
      {
        IList<ProjectItemViewModel> projectItemViewModelList = itemList;
        int index = insertIndex;
        EmptyProjectViewModel projectViewModel = new EmptyProjectViewModel();
        projectViewModel.TeamId = string.Empty;
        projectViewModel.Title = Utils.GetString("NoProjectHint");
        projectItemViewModelList.Insert(index, (ProjectItemViewModel) projectViewModel);
      }
    }

    private static Dictionary<string, List<ProjectModel>> GroupClosedProject(
      List<ProjectModel> projects)
    {
      Dictionary<string, List<ProjectModel>> dictionary = new Dictionary<string, List<ProjectModel>>();
      if (projects != null && projects.Any<ProjectModel>())
      {
        foreach (string str in projects.Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.teamId)).Distinct<string>().ToList<string>())
        {
          string teamId = str;
          string key = teamId;
          if (string.IsNullOrEmpty(key))
            key = "c1a7e08345e444dea187e21a692f0d7a";
          List<ProjectModel> list = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.teamId == teamId)).ToList<ProjectModel>();
          list.Sort((Comparison<ProjectModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
          if (!dictionary.ContainsKey(key))
            dictionary.Add(key, list);
          else
            dictionary[key].AddRange((IEnumerable<ProjectModel>) list);
        }
      }
      return dictionary;
    }

    private static List<ClosedProjectGroupViewModel> GetClosedGroups(
      Dictionary<string, List<ProjectModel>> groups)
    {
      List<ClosedProjectGroupViewModel> closedGroups = new List<ClosedProjectGroupViewModel>();
      foreach (KeyValuePair<string, List<ProjectModel>> group in groups)
      {
        ProjectGroupModel itemGroup = new ProjectGroupModel()
        {
          id = "_special_id_closed" + group.Key,
          name = Utils.GetString("ArchivedLists"),
          teamId = group.Key,
          open = LocalSettings.Settings.ClosedSectionStatus == "True"
        };
        List<ProjectItemViewModel> projectItemViewModelList = new List<ProjectItemViewModel>();
        foreach (ProjectModel project in group.Value)
        {
          project.groupId = itemGroup.id;
          projectItemViewModelList.Add((ProjectItemViewModel) new SubProjectViewModel(project));
        }
        ClosedProjectGroupViewModel projectGroupViewModel1 = new ClosedProjectGroupViewModel(itemGroup);
        projectGroupViewModel1.TeamId = group.Key;
        projectGroupViewModel1.Children = projectItemViewModelList;
        ClosedProjectGroupViewModel projectGroupViewModel2 = projectGroupViewModel1;
        closedGroups.Add(projectGroupViewModel2);
      }
      return closedGroups;
    }

    private static IEnumerable<ProjectItemViewModel> AssembleProjects()
    {
      List<ProjectModel> projects = CacheManager.GetProjectsWithoutInbox().Where<ProjectModel>((Func<ProjectModel, bool>) (p => !p.closed.HasValue || !p.closed.Value)).ToList<ProjectModel>();
      List<ProjectGroupModel> projectGroups = CacheManager.GetProjectGroups();
      List<string> groupsId = projectGroups != null ? projectGroups.Select<ProjectGroupModel, string>((Func<ProjectGroupModel, string>) (g => g.id)).ToList<string>() : (List<string>) null;
      List<TeamModel> teams = CacheManager.GetTeams();
      if (!UserManager.IsTeamUser())
        teams = new List<TeamModel>();
      List<ClosedProjectGroupViewModel> closedGroups = ProjectDataProvider.GetClosedGroups(ProjectDataProvider.GroupClosedProject(CacheManager.GetProjectsWithoutInbox().Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.closed.HasValue && p.closed.Value)).ToList<ProjectModel>()));
      List<ProjectModel> orphanProjects = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p =>
      {
        if (string.IsNullOrEmpty(p.groupId) || p.groupId.ToString().ToUpper() == "NONE")
          return true;
        return groupsId != null && !groupsId.Contains(p.groupId);
      })).ToList<ProjectModel>();
      List<ProjectGroupViewModel> groupModels = projectGroups != null ? projectGroups.Select<ProjectGroupModel, ProjectGroupViewModel>((Func<ProjectGroupModel, ProjectGroupViewModel>) (group =>
      {
        return new ProjectGroupViewModel(group)
        {
          Children = new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.groupId == group.id)).Select<ProjectModel, SubProjectViewModel>((Func<ProjectModel, SubProjectViewModel>) (p => new SubProjectViewModel(p))).ToList<SubProjectViewModel>())
        };
      })).ToList<ProjectGroupViewModel>() : (List<ProjectGroupViewModel>) null;
      if (groupModels != null && groupModels.Any<ProjectGroupViewModel>())
      {
        foreach (ProjectGroupViewModel projectGroupViewModel in groupModels)
        {
          if (projectGroupViewModel.Children != null && projectGroupViewModel.Children.Count > 1)
            projectGroupViewModel.Children = projectGroupViewModel.Children.OrderBy<ProjectItemViewModel, long>((Func<ProjectItemViewModel, long>) (p => p.SortOrder)).ThenByDescending<ProjectItemViewModel, string>((Func<ProjectItemViewModel, string>) (p => p.Title)).ToList<ProjectItemViewModel>();
          projectGroupViewModel.Children?.Add((ProjectItemViewModel) new EmptySubViewModel(projectGroupViewModel.ProjectGroup));
        }
      }
      else
        groupModels = new List<ProjectGroupViewModel>();
      TeamGroupViewModel teamGroupViewModel1 = new TeamGroupViewModel(new TeamModel()
      {
        id = "c1a7e08345e444dea187e21a692f0d7a",
        name = Utils.GetString("Personal"),
        open = LocalSettings.Settings.ExpandPersonalSection
      });
      teamGroupViewModel1.Id = "c1a7e08345e444dea187e21a692f0d7a";
      teamGroupViewModel1.Children = new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) groupModels.Where<ProjectGroupViewModel>((Func<ProjectGroupViewModel, bool>) (group => string.IsNullOrEmpty(group.TeamId)))).Union<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) orphanProjects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => string.IsNullOrEmpty(p.teamId) || teams.All<TeamModel>((Func<TeamModel, bool>) (t => t.id != p.teamId)))).Select<ProjectModel, NormalProjectViewModel>((Func<ProjectModel, NormalProjectViewModel>) (p => new NormalProjectViewModel(p)))))).OrderBy<ProjectItemViewModel, long>((Func<ProjectItemViewModel, long>) (p => p.SortOrder)).ThenByDescending<ProjectItemViewModel, string>((Func<ProjectItemViewModel, string>) (p => p.Title)).ToList<ProjectItemViewModel>();
      TeamGroupViewModel teamGroupViewModel2 = teamGroupViewModel1;
      if (!UserDao.IsPro())
      {
        int num = CacheManager.GetProjectsWithoutInbox().Count<ProjectModel>((Func<ProjectModel, bool>) (p => !p.delete_status));
        if (num >= 8)
          teamGroupViewModel2.NotifyMessage = Utils.GetString("ListCountUsed") + num.ToString() + "/9";
      }
      ClosedProjectGroupViewModel projectGroupViewModel1 = closedGroups.FirstOrDefault<ClosedProjectGroupViewModel>((Func<ClosedProjectGroupViewModel, bool>) (group => group.TeamId == "c1a7e08345e444dea187e21a692f0d7a"));
      if (projectGroupViewModel1 != null)
        teamGroupViewModel2.Children.Add((ProjectItemViewModel) projectGroupViewModel1);
      if (teams.Count == 0)
      {
        teamGroupViewModel2.Open = true;
        List<ProjectItemViewModel> source = ProjectDataProvider.ExpandItems(new List<TeamGroupViewModel>()
        {
          teamGroupViewModel2
        });
        if (source.Any<ProjectItemViewModel>())
        {
          source.RemoveAt(0);
          return (IEnumerable<ProjectItemViewModel>) source;
        }
      }
      List<TeamModel> list1 = teams.Where<TeamModel>((Func<TeamModel, bool>) (team => !team.expired)).ToList<TeamModel>();
      list1.Sort((Comparison<TeamModel>) ((a, b) => b.joinedTime.CompareTo(a.joinedTime)));
      List<TeamGroupViewModel> list2 = list1.Select<TeamModel, TeamGroupViewModel>((Func<TeamModel, TeamGroupViewModel>) (team =>
      {
        return new TeamGroupViewModel(team)
        {
          Id = team.id,
          Open = team.open,
          Children = new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) groupModels.Where<ProjectGroupViewModel>((Func<ProjectGroupViewModel, bool>) (group => group.TeamId == team.id))).Union<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) orphanProjects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.teamId == team.id)).Select<ProjectModel, NormalProjectViewModel>((Func<ProjectModel, NormalProjectViewModel>) (p => new NormalProjectViewModel(p)))))).OrderBy<ProjectItemViewModel, long>((Func<ProjectItemViewModel, long>) (p => p.SortOrder)).ThenByDescending<ProjectItemViewModel, string>((Func<ProjectItemViewModel, string>) (p => p.Title)).Union<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) closedGroups.Where<ClosedProjectGroupViewModel>((Func<ClosedProjectGroupViewModel, bool>) (group => group.TeamId == team.id))))).ToList<ProjectItemViewModel>()
        };
      })).ToList<TeamGroupViewModel>();
      list2.Add(teamGroupViewModel2);
      List<TeamModel> list3 = teams.Where<TeamModel>((Func<TeamModel, bool>) (team => team.expired)).ToList<TeamModel>();
      list3.Sort((Comparison<TeamModel>) ((a, b) => b.expiredDate.CompareTo(a.expiredDate)));
      list2.AddRange((IEnumerable<TeamGroupViewModel>) list3.Select<TeamModel, TeamGroupViewModel>((Func<TeamModel, TeamGroupViewModel>) (team =>
      {
        return new TeamGroupViewModel(team)
        {
          Id = team.id,
          Open = team.open,
          Children = new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) groupModels.Where<ProjectGroupViewModel>((Func<ProjectGroupViewModel, bool>) (group => group.TeamId == team.id))).Union<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) orphanProjects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.teamId == team.id)).Select<ProjectModel, NormalProjectViewModel>((Func<ProjectModel, NormalProjectViewModel>) (p => new NormalProjectViewModel(p)))))).OrderBy<ProjectItemViewModel, long>((Func<ProjectItemViewModel, long>) (p => p.SortOrder)).ThenByDescending<ProjectItemViewModel, string>((Func<ProjectItemViewModel, string>) (p => p.Title)).Union<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) new List<ProjectItemViewModel>((IEnumerable<ProjectItemViewModel>) closedGroups.Where<ClosedProjectGroupViewModel>((Func<ClosedProjectGroupViewModel, bool>) (group => group.TeamId == team.id))))).ToList<ProjectItemViewModel>()
        };
      })).ToList<TeamGroupViewModel>());
      foreach (TeamGroupViewModel model in list2)
      {
        ProjectDataProvider.SetTeamId((ProjectItemViewModel) model, model.Id);
        if (!model.Children.Any<ProjectItemViewModel>())
        {
          List<ProjectItemViewModel> children = model.Children;
          EmptyProjectViewModel projectViewModel = new EmptyProjectViewModel();
          projectViewModel.TeamId = model.Team.id;
          projectViewModel.Title = Utils.GetString(model.TeamId == "c1a7e08345e444dea187e21a692f0d7a" ? "NoProjectHint" : "NoTeamProjectHint");
          children.Add((ProjectItemViewModel) projectViewModel);
        }
      }
      return (IEnumerable<ProjectItemViewModel>) ProjectDataProvider.ExpandItems(list2);
    }

    private static void SetTeamId(ProjectItemViewModel model, string teamId)
    {
      if (model == null)
        return;
      model.TeamId = teamId;
      if (model.Children == null)
        return;
      foreach (ProjectItemViewModel child in model.Children)
        ProjectDataProvider.SetTeamId(child, teamId);
    }

    private static List<ProjectItemViewModel> ExpandItems(List<TeamGroupViewModel> items)
    {
      List<ProjectItemViewModel> models = new List<ProjectItemViewModel>();
      if (items != null && items.Any<TeamGroupViewModel>())
      {
        foreach (ProjectItemViewModel projectItemViewModel in items)
          ProjectDataProvider.AddItems(projectItemViewModel, models);
      }
      return models;
    }

    private static void AddItems(ProjectItemViewModel item, List<ProjectItemViewModel> models)
    {
      models.Add(item);
      if (item.Children == null || !item.Children.Any<ProjectItemViewModel>() || !item.Open)
        return;
      foreach (ProjectItemViewModel child in item.Children)
        ProjectDataProvider.AddItems(child, models);
    }

    private static async Task TryAddSpecialProject(
      string index,
      SmartProject project,
      string smartListId,
      ICollection<ProjectItemViewModel> projectList)
    {
      switch ((ShowStatus) LocalSettings.Settings[index])
      {
        case ShowStatus.Hide:
          return;
        case ShowStatus.Auto:
          SmartProjectIdentity identity = SmartProjectIdentity.BuildSmartProject(smartListId);
          if (identity is AbandonedProjectIdentity)
          {
            if (TaskViewModelHelper.GetClosedModels(new DateTime?(), DateTime.Now, (List<string>) null, 1, closedType: -1).Count <= 0)
              return;
            break;
          }
          if (await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) identity, true) <= 0)
            return;
          break;
      }
      int count = TaskCountCache.TryGetCount(smartListId);
      ICollection<ProjectItemViewModel> projectItemViewModels = projectList;
      SmartProjectViewModel projectViewModel = new SmartProjectViewModel(project);
      projectViewModel.Count = count;
      projectItemViewModels.Add((ProjectItemViewModel) projectViewModel);
    }

    private static async Task TryAddInboxProject(
      string index,
      SmartProject project,
      string inboxId,
      ICollection<ProjectItemViewModel> projectList)
    {
      switch ((ShowStatus) LocalSettings.Settings[index])
      {
        case ShowStatus.Hide:
          return;
        case ShowStatus.Auto:
          if (await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) ProjectIdentity.CreateInboxProject(), true) <= 0)
            return;
          break;
      }
      int count = TaskCountCache.TryGetCount(inboxId);
      ICollection<ProjectItemViewModel> projectItemViewModels = projectList;
      InboxProjectViewModel projectViewModel = new InboxProjectViewModel(project);
      projectViewModel.Count = count;
      projectItemViewModels.Add((ProjectItemViewModel) projectViewModel);
    }

    public static async Task<List<ProjectItemViewModel>> AddPtf(List<ProjectItemViewModel> itemList)
    {
      ProjectItemViewModel projectItemViewModel = itemList.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p is PtfAllViewModel ptfAllViewModel1 && ptfAllViewModel1.IsProject));
      int num1 = itemList.IndexOf(projectItemViewModel);
      if (num1 >= 0 && CacheManager.GetTeams().Count > 0)
      {
        itemList.RemoveAt(num1);
        ProjectDataProvider.AddNormalProjects((IList<ProjectItemViewModel>) itemList, num1);
      }
      else if (projectItemViewModel != null && num1 >= 0)
      {
        if (!UserDao.IsPro())
        {
          int num2 = CacheManager.GetProjectsWithoutInbox().Count<ProjectModel>((Func<ProjectModel, bool>) (p => !p.delete_status));
          if (num2 >= 8)
            projectItemViewModel.NotifyMessage = Utils.GetString("ListCountUsed") + num2.ToString() + "/9";
        }
        if (LocalSettings.Settings.AllProjectOpened)
          ProjectDataProvider.AddNormalProjects((IList<ProjectItemViewModel>) itemList, num1 + 1);
      }
      int num3 = itemList.IndexOf(itemList.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p is PtfAllViewModel ptfAllViewModel2 && ptfAllViewModel2.IsTag)));
      if (num3 >= 0 && LocalSettings.Settings.AllTagOpened)
        await ProjectDataProvider.AddTagProject((IList<ProjectItemViewModel>) itemList, num3 + 1);
      int num4 = itemList.IndexOf(itemList.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p is PtfAllViewModel ptfAllViewModel3 && ptfAllViewModel3.IsFilter)));
      if (num4 >= 0 && LocalSettings.Settings.AllFilterOpened)
        ProjectDataProvider.AddFilterProject((IList<ProjectItemViewModel>) itemList, num4 + 1);
      int num5 = itemList.IndexOf(itemList.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p is PtfAllViewModel ptfAllViewModel4 && ptfAllViewModel4.IsSubscribe)));
      if (num5 >= 0 && LocalSettings.Settings.AllSubscribeOpened)
        ProjectDataProvider.AddSubscribeCalendar((IList<ProjectItemViewModel>) itemList, num5 + 1);
      ProjectDataProvider.AssembleTaskCount((IEnumerable<ProjectItemViewModel>) itemList);
      return itemList;
    }

    private static void AssembleTaskCount(IEnumerable<ProjectItemViewModel> models)
    {
      foreach (ProjectItemViewModel model in models)
        model.LoadCount();
    }

    public static bool IsTagCategory(ProjectItemViewModel model)
    {
      return model is TagProjectViewModel || model is EmptySubViewModel emptySubViewModel && emptySubViewModel.IsEmptyTag || model is EmptyTagViewModel || model is ShareTagAllViewModel;
    }

    public static bool IsShareTagCategory(ProjectItemViewModel model)
    {
      switch (model)
      {
        case TagProjectViewModel _ when model.InSubSection:
          return true;
        case EmptySubViewModel emptySubViewModel when emptySubViewModel.IsEmptyTag:
          return model.InSubSection;
        default:
          return false;
      }
    }

    public static bool IsFilterCategory(ProjectItemViewModel model)
    {
      return model is FilterProjectViewModel || model is EmptyFilterViewModel;
    }

    public static bool IsSubscribeCategory(ProjectItemViewModel model)
    {
      return model is SubscribeCalendarProjectViewModel || model is BindCalendarAccountProjectViewModel;
    }

    public static bool IsProjectCategory(ProjectItemViewModel model)
    {
      switch (model)
      {
        case NormalProjectViewModel _:
        case ProjectGroupViewModel _:
        case EmptyProjectViewModel projectViewModel when projectViewModel.Type == PtfType.Project:
          return true;
        case EmptySubViewModel emptySubViewModel:
          return !emptySubViewModel.IsEmptyTag;
        default:
          return false;
      }
    }

    public static async Task AddTagProject(IList<ProjectItemViewModel> itemList, int insertIndex)
    {
      List<TagModel> tags = TagDataHelper.GetTags();
      // ISSUE: explicit non-virtual call
      if (tags != null && __nonvirtual (tags.Count) > 0)
      {
        List<TagModel> sortTags = new List<TagModel>();
        HashSet<string> tagNames = new HashSet<string>(tags.Select<TagModel, string>((Func<TagModel, string>) (t => t.name)));
        foreach (TagModel tag in tags)
        {
          if (tag.parent == tag.name)
            tag.parent = (string) null;
          if (LocalSettings.Settings.SmartListTag == 2)
          {
            if (await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) new TagProjectIdentity(tag), true) <= 0)
              continue;
          }
          sortTags.Add(tag);
        }
        if (sortTags.Count > 0)
        {
          List<TagModel> list1 = sortTags.Where<TagModel>((Func<TagModel, bool>) (tag => string.IsNullOrEmpty(tag.parent) || !tagNames.Contains(tag.parent))).ToList<TagModel>();
          List<TagModel> list2 = list1.Where<TagModel>((Func<TagModel, bool>) (t => t.type != 2)).ToList<TagModel>();
          List<TagModel> list3 = list1.Where<TagModel>((Func<TagModel, bool>) (t => t.type == 2)).ToList<TagModel>();
          if (list2.Count == 0)
          {
            IList<ProjectItemViewModel> projectItemViewModelList = itemList;
            int index = insertIndex++;
            EmptyTagViewModel emptyTagViewModel = new EmptyTagViewModel();
            emptyTagViewModel.TeamId = string.Empty;
            emptyTagViewModel.Title = Utils.GetString("NoTagHint");
            projectItemViewModelList.Insert(index, (ProjectItemViewModel) emptyTagViewModel);
          }
          foreach (TagModel tagModel in list2)
          {
            TagModel tag = tagModel;
            TagProjectViewModel projectViewModel = new TagProjectViewModel(tag);
            itemList.Insert(insertIndex++, (ProjectItemViewModel) projectViewModel);
            if (projectViewModel.IsParent)
            {
              List<TagModel> list4 = sortTags.Where<TagModel>((Func<TagModel, bool>) (t => t.parent == tag.name)).ToList<TagModel>();
              projectViewModel.Children = list4.Select<TagModel, ProjectItemViewModel>((Func<TagModel, ProjectItemViewModel>) (t => (ProjectItemViewModel) new TagProjectViewModel(t))).ToList<ProjectItemViewModel>();
              projectViewModel.Children.Add((ProjectItemViewModel) new EmptySubViewModel(tag));
              if (!tag.collapsed)
              {
                foreach (ProjectItemViewModel child in projectViewModel.Children)
                  itemList.Insert(insertIndex++, child);
              }
            }
          }
          if (list3.Count > 0)
          {
            itemList.Insert(insertIndex++, (ProjectItemViewModel) new ShareTagAllViewModel());
            foreach (TagModel tagModel in list3)
            {
              TagModel tag = tagModel;
              TagProjectViewModel projectViewModel1 = new TagProjectViewModel(tag);
              projectViewModel1.InSubSection = true;
              TagProjectViewModel projectViewModel2 = projectViewModel1;
              itemList.Insert(insertIndex++, (ProjectItemViewModel) projectViewModel2);
              if (projectViewModel2.IsParent)
              {
                List<TagModel> list5 = sortTags.Where<TagModel>((Func<TagModel, bool>) (t => t.parent == tag.name)).ToList<TagModel>();
                projectViewModel2.Children = list5.Select<TagModel, ProjectItemViewModel>((Func<TagModel, ProjectItemViewModel>) (t =>
                {
                  return (ProjectItemViewModel) new TagProjectViewModel(t)
                  {
                    InSubSection = true
                  };
                })).ToList<ProjectItemViewModel>();
                List<ProjectItemViewModel> children = projectViewModel2.Children;
                EmptySubViewModel emptySubViewModel = new EmptySubViewModel(tag);
                emptySubViewModel.InSubSection = true;
                children.Add((ProjectItemViewModel) emptySubViewModel);
                if (!tag.collapsed)
                {
                  foreach (ProjectItemViewModel child in projectViewModel2.Children)
                    itemList.Insert(insertIndex++, child);
                }
              }
            }
          }
        }
        sortTags = (List<TagModel>) null;
      }
      else
      {
        IList<ProjectItemViewModel> projectItemViewModelList = itemList;
        int index = insertIndex;
        EmptyTagViewModel emptyTagViewModel = new EmptyTagViewModel();
        emptyTagViewModel.TeamId = string.Empty;
        emptyTagViewModel.Title = Utils.GetString("NoTagHint");
        projectItemViewModelList.Insert(index, (ProjectItemViewModel) emptyTagViewModel);
      }
    }

    public static async Task AddShareTagProject(
      IList<ProjectItemViewModel> itemList,
      int insertIndex)
    {
      List<TagModel> tags = TagDataHelper.GetTags();
      List<TagModel> list1 = tags != null ? tags.Where<TagModel>((Func<TagModel, bool>) (t => t.type == 2)).ToList<TagModel>() : (List<TagModel>) null;
      // ISSUE: explicit non-virtual call
      if (list1 != null && __nonvirtual (list1.Count) > 0)
      {
        List<TagModel> sortTags = new List<TagModel>();
        HashSet<string> tagNames = new HashSet<string>(list1.Select<TagModel, string>((Func<TagModel, string>) (t => t.name)));
        foreach (TagModel tag in list1)
        {
          if (tag.parent == tag.name)
            tag.parent = (string) null;
          if (LocalSettings.Settings.SmartListTag == 2)
          {
            if (await ProjectAndTaskIdsCache.GetCountByIdentity((ProjectIdentity) new TagProjectIdentity(tag), true) <= 0)
              continue;
          }
          sortTags.Add(tag);
        }
        if (sortTags.Count > 0)
        {
          foreach (TagModel tagModel in sortTags.Where<TagModel>((Func<TagModel, bool>) (tag => string.IsNullOrEmpty(tag.parent) || !tagNames.Contains(tag.parent))).ToList<TagModel>())
          {
            TagModel tag = tagModel;
            TagProjectViewModel projectViewModel1 = new TagProjectViewModel(tag);
            projectViewModel1.InSubSection = true;
            TagProjectViewModel projectViewModel2 = projectViewModel1;
            itemList.Insert(insertIndex++, (ProjectItemViewModel) projectViewModel2);
            if (projectViewModel2.IsParent)
            {
              List<TagModel> list2 = sortTags.Where<TagModel>((Func<TagModel, bool>) (t => t.parent == tag.name)).ToList<TagModel>();
              projectViewModel2.Children = list2.Select<TagModel, ProjectItemViewModel>((Func<TagModel, ProjectItemViewModel>) (t =>
              {
                return (ProjectItemViewModel) new TagProjectViewModel(t)
                {
                  InSubSection = true
                };
              })).ToList<ProjectItemViewModel>();
              List<ProjectItemViewModel> children = projectViewModel2.Children;
              EmptySubViewModel emptySubViewModel = new EmptySubViewModel(tag);
              emptySubViewModel.InSubSection = true;
              children.Add((ProjectItemViewModel) emptySubViewModel);
              if (!tag.collapsed)
              {
                foreach (ProjectItemViewModel child in projectViewModel2.Children)
                  itemList.Insert(insertIndex++, child);
              }
            }
          }
        }
        sortTags = (List<TagModel>) null;
      }
      else
      {
        IList<ProjectItemViewModel> projectItemViewModelList = itemList;
        int index = insertIndex;
        EmptyTagViewModel emptyTagViewModel = new EmptyTagViewModel();
        emptyTagViewModel.TeamId = string.Empty;
        emptyTagViewModel.Title = Utils.GetString("NoTagHint");
        projectItemViewModelList.Insert(index, (ProjectItemViewModel) emptyTagViewModel);
      }
    }

    public static void AddFilterProject(IList<ProjectItemViewModel> itemList, int insertIndex)
    {
      List<FilterModel> list = CacheManager.GetFilters().Where<FilterModel>((Func<FilterModel, bool>) (f => f.deleted != 1)).OrderBy<FilterModel, long>((Func<FilterModel, long>) (f => f.sortOrder)).ToList<FilterModel>();
      if (list.Any<FilterModel>())
      {
        foreach (FilterModel filter in list)
        {
          FilterProjectViewModel projectViewModel = new FilterProjectViewModel(filter);
          itemList.Insert(insertIndex++, (ProjectItemViewModel) projectViewModel);
        }
      }
      else
      {
        IList<ProjectItemViewModel> projectItemViewModelList = itemList;
        int index = insertIndex;
        EmptyFilterViewModel emptyFilterViewModel = new EmptyFilterViewModel();
        emptyFilterViewModel.TeamId = string.Empty;
        emptyFilterViewModel.Title = Utils.GetString("NoFilterHint");
        projectItemViewModelList.Insert(index, (ProjectItemViewModel) emptyFilterViewModel);
      }
    }
  }
}
