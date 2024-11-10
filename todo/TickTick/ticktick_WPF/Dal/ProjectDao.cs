// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ProjectDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.NewUser;
using TickTickDao;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class ProjectDao : BaseDao<ProjectModel>
  {
    private static async Task<List<string>> GetProjectIds()
    {
      return await BaseDao<ProjectModel>.GetEntityIds("select id as Id from ProjectModel where userId = '" + Utils.GetCurrentUserIdInt().ToString() + "'");
    }

    public static async Task<ObservableCollection<ProjectModel>> GetAllProjects(
      bool withInbox = true,
      bool withClosed = true)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return new ObservableCollection<ProjectModel>(await App.Connection.Table<ProjectModel>().Where((Expression<Func<ProjectModel, bool>>) (v => v.userid == userId && v.delete_status == false && (withInbox || v.Isinbox == false) && (withClosed || v.closed == new bool?() || v.closed == (bool?) false))).OrderBy<long>((Expression<Func<ProjectModel, long>>) (p => p.sortOrder)).ToListAsync());
    }

    public static async Task<ObservableCollection<ProjectModel>> GetProjectsWithoutClosed(
      bool withInbox = true)
    {
      return await ProjectDao.GetAllProjects(withInbox, false);
    }

    public static async Task<ObservableCollection<ProjectModel>> GetProjectsInGroup(string groupId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return new ObservableCollection<ProjectModel>(await App.Connection.Table<ProjectModel>().Where((Expression<Func<ProjectModel, bool>>) (v => v.userid == userId && v.groupId.Equals(groupId) && v.delete_status == false)).OrderBy<long>((Expression<Func<ProjectModel, long>>) (v => v.sortOrder)).ToListAsync());
    }

    public static async Task<ProjectModel> GetMinOrderProjectInGroup(string groupId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<ProjectModel> listAsync = await App.Connection.Table<ProjectModel>().Where((Expression<Func<ProjectModel, bool>>) (v => v.userid == userId && v.groupId.Equals(groupId) && v.delete_status == false)).ToListAsync();
      List<ProjectModel> list = listAsync != null ? listAsync.Where<ProjectModel>((Func<ProjectModel, bool>) (v => v.IsEnable())).ToList<ProjectModel>() : (List<ProjectModel>) null;
      return list != null ? list.OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (v => v.sortOrder)).FirstOrDefault<ProjectModel>() : (ProjectModel) null;
    }

    public static async Task<bool> CheckUnSyncProjects()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      string syncDone = Constants.SyncStatus.SYNC_DONE.ToString();
      return (await App.Connection.Table<ProjectModel>().Where((Expression<Func<ProjectModel, bool>>) (v => v.userid == userId && v.sync_status != default (string) && v.sync_status != "" && v.sync_status != syncDone)).ToListAsync()).Count == 0;
    }

    public static async Task CreateProject(ProjectModel project, bool notify = true)
    {
      if (string.IsNullOrEmpty(project.userid))
        project.userid = Utils.GetCurrentUserIdInt().ToString();
      int num = await App.Connection.InsertAsync((object) project);
      CacheManager.UpdateProject(project, notify);
    }

    public static async Task DeleteProjectById(string projectId)
    {
      ProjectModel project = await ProjectDao.GetProjectById(projectId);
      if (project == null)
      {
        project = (ProjectModel) null;
      }
      else
      {
        if (project.IsNew())
        {
          int num1 = await App.Connection.DeleteAsync((object) project);
        }
        else
        {
          project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
          project.delete_status = true;
          int num2 = await App.Connection.UpdateAsync((object) project);
        }
        List<TaskModel> tasksInProjectAsync = await TaskDao.GetTasksInProjectAsync(projectId);
        if (project.IsShareList())
        {
          foreach (TaskModel taskModel in tasksInProjectAsync)
          {
            App.Connection.DeleteAsync((object) taskModel);
            TaskCache.DeleteTask(taskModel.id);
          }
        }
        else
          TaskService.BatchDeleteTasks(tasksInProjectAsync, false);
        CacheManager.DeleteProject(project);
        List<SyncStatusModel> moveColumn = await SyncStatusDao.GetSyncStatusByType(32);
        List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(project.id);
        (columnsByProjectId != null ? columnsByProjectId.Where<ColumnModel>((Func<ColumnModel, bool>) (c => moveColumn.Any<SyncStatusModel>((Func<SyncStatusModel, bool>) (m => m.EntityId == c.id)))).ToList<ColumnModel>() : (List<ColumnModel>) null)?.ForEach((Action<ColumnModel>) (c => BaseDao<ColumnModel>.DeleteAsync(c)));
        project = (ProjectModel) null;
      }
    }

    public static async Task<int> TryUpdateProject(ProjectModel project)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<ProjectModel> listAsync = await App.Connection.Table<ProjectModel>().Where((Expression<Func<ProjectModel, bool>>) (v => v.id.Equals(project.id) && v.userid == userId)).ToListAsync();
      if (listAsync.Count == 0)
        listAsync = await App.Connection.Table<ProjectModel>().Where((Expression<Func<ProjectModel, bool>>) (v => v._Id.Equals(project._Id) && v.userid == userId)).ToListAsync();
      if (listAsync.Count != 0)
      {
        ProjectModel projectModel = listAsync[0];
        Constants.SyncStatus syncStatus1;
        if (!(projectModel.sync_status == Constants.SyncStatus.SYNC_NEW.ToString()))
        {
          string syncStatus2 = projectModel.sync_status;
          syncStatus1 = Constants.SyncStatus.SYNC_ERROR_UP_LIMIT;
          string str = syncStatus1.ToString();
          if (!(syncStatus2 == str))
            goto label_9;
        }
        string syncStatus3 = project.sync_status;
        syncStatus1 = Constants.SyncStatus.SYNC_UPDATE;
        string str1 = syncStatus1.ToString();
        if (syncStatus3 == str1)
          project.sync_status = projectModel.sync_status;
label_9:
        project._Id = listAsync[0]._Id;
        project.name = project.name.Trim();
        project.modifiedTime = new DateTime?(DateTime.Now);
        CacheManager.UpdateProject(project);
        await ProjectSyncJsonDao.TryAddProjectJson(project.id);
        int num = await App.Connection.UpdateAsync((object) project);
        return project._Id;
      }
      int num1 = await App.Connection.InsertAsync((object) project);
      CacheManager.UpdateProject(project);
      return project._Id;
    }

    public static async Task<Dictionary<string, ProjectModel>> GetLocalSyncedProjectMap(
      string userId)
    {
      Dictionary<string, ProjectModel> projectDict = new Dictionary<string, ProjectModel>();
      List<ProjectModel> listAsync = await App.Connection.Table<ProjectModel>().Where((Expression<Func<ProjectModel, bool>>) (v => v.userid.Equals(userId))).ToListAsync();
      if (listAsync.Count > 0)
      {
        foreach (ProjectModel projectModel in listAsync)
        {
          if (!projectDict.ContainsKey(projectModel.id) && !projectModel.id.StartsWith("inbox"))
            projectDict.Add(projectModel.id, projectModel);
        }
      }
      Dictionary<string, ProjectModel> syncedProjectMap = projectDict;
      projectDict = (Dictionary<string, ProjectModel>) null;
      return syncedProjectMap;
    }

    public static async Task SaveServerMergeData(
      List<ProjectModel> added,
      List<ProjectModel> updated,
      List<ProjectModel> deleted)
    {
      if (added.Count > 0 && added.Count > 0)
      {
        List<string> projectIds = await ProjectDao.GetProjectIds();
        List<ProjectModel> items = new List<ProjectModel>();
        foreach (ProjectModel projectModel in added)
        {
          projectModel.name = projectModel.name.Trim();
          if (!projectIds.Contains(projectModel.id))
            items.Add(projectModel);
        }
        if (items.Count > 0)
        {
          int num = await App.Connection.InsertAllAsync((IEnumerable) items);
          foreach (ProjectModel project in added)
          {
            CacheManager.UpdateProject(project, false);
            if (project.IsValid() && project.IsShareList())
              AvatarHelper.GetProjectUsersAsync(project.id);
          }
        }
      }
      if (updated.Count > 0)
      {
        updated.ForEach((Action<ProjectModel>) (project => project.name = project.name.Trim()));
        int num = await App.Connection.UpdateAllAsync((IEnumerable) updated);
        foreach (ProjectModel project in updated)
          CacheManager.UpdateProject(project, false);
        await ProjectSyncJsonDao.BatchDeleteProjects(updated);
      }
      if (deleted.Count > 0)
      {
        List<SyncStatusModel> moveColumn = await SyncStatusDao.GetSyncStatusByType(32);
        SyncStatusModel m1;
        foreach (SyncStatusModel syncStatusModel in moveColumn)
        {
          m1 = syncStatusModel;
          ColumnModel column = await ColumnDao.GetColumnById(m1.EntityId);
          if (deleted.Any<ProjectModel>((Func<ProjectModel, bool>) (d => d.id == column.projectId)))
          {
            column.projectId = m1.MoveFromId;
            if (deleted.Any<ProjectModel>((Func<ProjectModel, bool>) (d => d.id == column.projectId)))
            {
              int num1 = await BaseDao<ColumnModel>.DeleteAsync(column);
            }
            else
              await ColumnDao.UpdateColumn(column);
            int num2 = await BaseDao<SyncStatusModel>.DeleteAsync(m1);
          }
          m1 = (SyncStatusModel) null;
        }
        foreach (SyncStatusModel syncStatusModel in await SyncStatusDao.GetSyncStatusByType(2))
        {
          m1 = syncStatusModel;
          TaskModel task = await TaskDao.GetTaskById(m1.EntityId);
          if (deleted.Any<ProjectModel>((Func<ProjectModel, bool>) (d => d.id == task.projectId)))
          {
            task.projectId = m1.MoveFromId;
            if (deleted.Any<ProjectModel>((Func<ProjectModel, bool>) (d => d.id == task.projectId)))
            {
              await TaskDao.DeleteTaskInDb(task.id);
              UtilLog.Info("SaveServerMergeData.TaskDeleted " + task.id);
            }
            else
              await TaskService.UpdateTaskProject(task);
            int num = await BaseDao<SyncStatusModel>.DeleteAsync(m1);
          }
          m1 = (SyncStatusModel) null;
        }
        foreach (ProjectModel projectModel in deleted)
        {
          ProjectModel project = projectModel;
          if (!project.id.StartsWith("inbox"))
          {
            TaskStickyWindow.TryCloseInProject(project.id);
            int num = await App.Connection.DeleteAsync((object) project);
            CacheManager.DeleteProject(project);
            List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(project.id);
            (columnsByProjectId != null ? columnsByProjectId.Where<ColumnModel>((Func<ColumnModel, bool>) (c => moveColumn.Any<SyncStatusModel>((Func<SyncStatusModel, bool>) (m => m.EntityId != c.id)))).ToList<ColumnModel>() : (List<ColumnModel>) null)?.ForEach((Action<ColumnModel>) (c => BaseDao<ColumnModel>.DeleteAsync(c)));
            foreach (TaskModel taskModel in await TaskDao.GetTasksInProjectAsync(project.id))
            {
              if (project.IsShareList() && !project.isOwner)
                App.Connection.DeleteAsync((object) taskModel);
              else
                TaskDao.DeleteTaskToTrash(taskModel.id);
            }
            project = (ProjectModel) null;
          }
        }
      }
      DataChangedNotifier.NotifyProjectChanged();
    }

    public static async Task<Dictionary<string, string>> GetNeedPostProject()
    {
      List<ProjectSyncStatusModel> source = await App.Connection.QueryAsync<ProjectSyncStatusModel>("select id as ProjectId, sync_status as SyncStatus from ProjectModel where sync_status is not null and sync_status <> 'SYNC_DONE' and userId = '" + Utils.GetCurrentUserIdInt().ToString() + "'");
      Dictionary<string, string> dict = new Dictionary<string, string>();
      if (source != null && source.Count > 0)
      {
        foreach (ProjectSyncStatusModel projectSyncStatusModel in source.Where<ProjectSyncStatusModel>((Func<ProjectSyncStatusModel, bool>) (statusModel => !dict.ContainsKey(statusModel.ProjectId))))
          dict.Add(projectSyncStatusModel.ProjectId, projectSyncStatusModel.SyncStatus);
      }
      return dict;
    }

    public static async Task<HashSet<string>> GetProjectIdSet()
    {
      return await BaseDao<ProjectModel>.GetEntityIdSet("select id as Id from ProjectModel where userId = '" + LocalSettings.Settings.LoginUserId + "'");
    }

    public static async Task<ProjectModel> GetProjectById(string id)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<ProjectModel>().Where((Expression<Func<ProjectModel, bool>>) (p => p.userid == userId && p.id == id)).FirstOrDefaultAsync();
    }

    public static List<ProjectModel> GetProjectsInIdsOrGroupIds(
      List<string> ids,
      List<string> groupIds)
    {
      return CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p =>
      {
        if (ids != null && ids.Contains(p.id))
          return true;
        return groupIds != null && groupIds.Contains(p.groupId);
      })).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).ToList<ProjectModel>();
    }

    public static long GetNewProjectOrder(string teamId)
    {
      List<long> list1 = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => !p.Isinbox && p.IsValid() && p.teamId == teamId)).Select<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).ToList<long>();
      List<long> list2 = CacheManager.GetProjectGroups().Where<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (g => g.teamId == teamId)).Select<ProjectGroupModel, long>((Func<ProjectGroupModel, long>) (g => g.sortOrder.GetValueOrDefault())).ToList<long>();
      list1.AddRange((IEnumerable<long>) list2);
      if (list1.Count <= 0)
        return 0;
      list1.Sort();
      return list1[0] - 268435456L;
    }

    public static async Task SaveInboxColor(string color)
    {
      ProjectModel inbox = await ProjectDao.GetProjectById(Utils.GetInboxId());
      if (inbox == null)
        inbox = (ProjectModel) null;
      else if (!(inbox.color != color))
      {
        inbox = (ProjectModel) null;
      }
      else
      {
        inbox.color = color;
        int num = await App.Connection.UpdateAsync((object) inbox);
        CacheManager.UpdateProject(inbox);
        inbox = (ProjectModel) null;
      }
    }

    public static async Task SetProjectDefaultColumn(string projectId, string columnId)
    {
      List<TaskModel> tasksInProjectAsync = await TaskDao.GetTasksInProjectAsync(projectId);
      if (!tasksInProjectAsync.Any<TaskModel>())
        ;
      else
      {
        List<TaskModel> uncompletedTasks = tasksInProjectAsync.Where<TaskModel>((Func<TaskModel, bool>) (task => task.status == 0 && task.deleted == 0 && string.IsNullOrEmpty(task.columnId))).ToList<TaskModel>();
        if (uncompletedTasks.Any<TaskModel>())
        {
          UtilLog.Info(string.Format("SetProjectDefaultColumn {0} columnId {1} count {2}", (object) projectId, (object) columnId, (object) uncompletedTasks.Count));
          uncompletedTasks.ForEach((Action<TaskModel>) (task => task.columnId = columnId));
          int num = await App.Connection.UpdateAllAsync((IEnumerable) tasksInProjectAsync);
          await SyncStatusDao.BatchAddModifySyncStatus((IEnumerable<string>) uncompletedTasks.Select<TaskModel, string>((Func<TaskModel, string>) (task => task.id)).ToList<string>());
        }
        uncompletedTasks = (List<TaskModel>) null;
      }
    }

    public static async Task<int> GetTaskOverLimitProjectNum()
    {
      string loginUserId = LocalSettings.Settings.LoginUserId;
      List<int> source = await App.Connection.QueryAsync<int>("select count(id) from (select t.id,t.projectId from TaskModel t, ProjectModel p where t.userId = '" + loginUserId + "' and p.userId = '" + loginUserId + "' and t.projectId = p.id and (p.closed is null or p.closed = false)) group by projectId");
      // ISSUE: explicit non-virtual call
      return source == null || __nonvirtual (source.Count) <= 0 ? 0 : source.Where<int>((Func<int, bool>) (num => num > 99)).ToList<int>().Count;
    }

    public static bool IsValidProject(string projectId)
    {
      if (projectId == "#alllists")
        return true;
      ProjectModel projectById = CacheManager.GetProjectById(projectId);
      return projectById != null && projectById.IsValid();
    }

    public static async Task<List<string>> GetProjectIdByGroupId(string groupId)
    {
      return await BaseDao<ProjectModel>.GetEntityIds("select id from projectModel where groupId = " + groupId);
    }

    public static async Task UpdateInboxSortType(SortOption sortOption)
    {
      ProjectModel projectById = CacheManager.GetProjectById(Utils.GetInboxId());
      if (projectById == null)
        return;
      projectById.SortOption = sortOption;
      ProjectDao.TryUpdateProject(projectById);
    }

    internal static async Task SwitchViewModel(ProjectModel project, string viewMode)
    {
      string syncStatus1 = project.sync_status;
      Constants.SyncStatus syncStatus2 = Constants.SyncStatus.SYNC_NEW;
      string str1 = syncStatus2.ToString();
      if (syncStatus1 != str1)
      {
        ProjectModel projectModel = project;
        syncStatus2 = Constants.SyncStatus.SYNC_UPDATE;
        string str2 = syncStatus2.ToString();
        projectModel.sync_status = str2;
      }
      project.viewMode = viewMode;
      switch (viewMode)
      {
        case "kanban":
          if (project.SortOption?.groupBy == "none")
          {
            project.SortOption.groupBy = "sortOrder";
            project.sortType = "sortOrder";
            break;
          }
          break;
        case "timeline":
          TimelineModel.CheckTimelineEmpty((ITimeline) project, Constants.SortType.sortOrder);
          break;
      }
      int num = await ProjectDao.TryUpdateProject(project);
      SyncManager.Sync();
    }

    public static ProjectModel GetAssignProjectInGroup(string groupId, string assignee)
    {
      List<ProjectModel> list = CacheManager.GetProjectsInGroup(groupId).Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsEnable())).ToList<ProjectModel>();
      list.Sort((Comparison<ProjectModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
      foreach (ProjectModel assignProjectInGroup in list)
      {
        if (AvatarHelper.GetUserByIdAndProjectId(assignee, assignProjectInGroup.id) != null)
          return assignProjectInGroup;
      }
      return (ProjectModel) null;
    }

    public static async Task SaveProjectName(string projectId, string name)
    {
      ProjectModel projectById = CacheManager.GetProjectById(projectId);
      if (projectById == null)
        return;
      projectById.name = name;
      projectById.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
      int num = await ProjectDao.TryUpdateProject(projectById);
      SyncManager.Sync();
    }

    internal static async Task InitProjects(List<string> projects)
    {
      if (projects == null || projects.Count == 0)
        return;
      long order = CacheManager.GetProjects().Max<ProjectModel>((Func<ProjectModel, long>) (p => p.sortOrder));
      foreach (string project in projects)
      {
        order += 268435456L;
        string str;
        await ProjectDao.CreateProject(new ProjectModel()
        {
          id = Utils.GetGuid(),
          name = (NewUserGuide.DefaultProjects.TryGetValue(project, out str) ? str : "") + project,
          sortType = Constants.SortType.sortOrder.ToString(),
          SortOption = new SortOption()
          {
            groupBy = Constants.SortType.sortOrder.ToString(),
            orderBy = Constants.SortType.sortOrder.ToString()
          },
          viewMode = "list",
          userid = LocalSettings.Settings.LoginUserId,
          sync_status = Constants.SyncStatus.SYNC_NEW.ToString(),
          kind = Constants.ProjectKind.TASK.ToString(),
          sortOrder = order
        }, false);
        DataChangedNotifier.NotifyProjectChanged();
        ListViewContainer.ReloadProjectData();
        SyncManager.TryDelaySync();
      }
    }

    public static async Task<string> PullGuideProject(string abTest)
    {
      GuideProjectModel gProject = await Communicator.GetGuideProject("newuser3_202403" + abTest);
      if (gProject == null)
        return (string) null;
      ProjectModel project = gProject.ToProject();
      List<ColumnModel> columns = gProject.ToColumns();
      List<TaskModel> tasks = gProject.ToTasks();
      int num = await BaseDao<ColumnModel>.InsertAllAsync(columns);
      await ProjectDao.CreateProject(project);
      await TaskDao.BatchInsertTasksAndItems(tasks);
      return gProject.id;
    }
  }
}
