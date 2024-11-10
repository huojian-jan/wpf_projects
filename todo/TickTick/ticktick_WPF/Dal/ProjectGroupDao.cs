// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ProjectGroupDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class ProjectGroupDao : BaseDao<ProjectGroupModel>
  {
    public static async Task TrySaveProjectGroup(
      ProjectGroupModel projectGroup,
      bool isNeedUpdateOpen = false)
    {
      List<ProjectGroupModel> query;
      if (projectGroup.id.Contains("_special_id_closed"))
      {
        query = (List<ProjectGroupModel>) null;
      }
      else
      {
        int userId = Utils.GetCurrentUserIdInt();
        query = await App.Connection.Table<ProjectGroupModel>().Where((Expression<Func<ProjectGroupModel, bool>>) (v => v.userId == userId && v.id.Equals(projectGroup.id))).ToListAsync();
        if (query.Count == 0)
          query = await App.Connection.Table<ProjectGroupModel>().Where((Expression<Func<ProjectGroupModel, bool>>) (v => v._Id.Equals(projectGroup._Id))).ToListAsync();
        if (query.Count != 0)
        {
          await ProjectGroupSyncedJsonDao.TrySaveProjectGroup(query[0].id);
          projectGroup._Id = query[0]._Id;
          projectGroup.etag = query[0].etag;
          if (!isNeedUpdateOpen)
            projectGroup.open = query[0].open;
          projectGroup.name = projectGroup.name.Trim();
          projectGroup.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
          int num = await App.Connection.UpdateAsync((object) projectGroup);
          CacheManager.UpdateProjectGroup(projectGroup);
          query = (List<ProjectGroupModel>) null;
        }
        else
        {
          projectGroup.name = projectGroup.name.Trim();
          projectGroup.userId = userId;
          if (!projectGroup.sortOrder.HasValue)
            projectGroup.sortOrder = new long?(ProjectDao.GetNewProjectOrder(projectGroup.teamId));
          int num = await App.Connection.InsertAsync((object) projectGroup);
          CacheManager.UpdateProjectGroup(projectGroup);
          query = (List<ProjectGroupModel>) null;
        }
      }
    }

    public static async Task SaveProjectGroup(ProjectGroupModel projectGroup, bool needSync = true)
    {
      if (projectGroup == null)
        return;
      if (needSync && projectGroup.sync_status != Constants.SyncStatus.SYNC_NEW.ToString())
        projectGroup.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
      projectGroup.name = projectGroup.name.Trim();
      int num = await App.Connection.UpdateAsync((object) projectGroup);
      CacheManager.UpdateProjectGroup(projectGroup);
    }

    public static async Task<bool> CheckUnSyncItem()
    {
      int userId = Utils.GetCurrentUserIdInt();
      string syncDone = Constants.SyncStatus.SYNC_DONE.ToString();
      return (await App.Connection.Table<ProjectGroupModel>().Where((Expression<Func<ProjectGroupModel, bool>>) (v => v.userId == userId && v.sync_status != default (string) && v.sync_status != "" && v.sync_status != syncDone)).ToListAsync()).Count == 0;
    }

    public static async Task<List<ProjectGroupModel>> GetProjectGroups()
    {
      int userId = Utils.GetCurrentUserIdInt();
      return await App.Connection.Table<ProjectGroupModel>().Where((Expression<Func<ProjectGroupModel, bool>>) (p => p.userId == userId && p.deleted != 1)).ToListAsync() ?? new List<ProjectGroupModel>();
    }

    public static async Task<ProjectGroupModel> GetProjectGroupById(string id)
    {
      int userId = Utils.GetCurrentUserIdInt();
      List<ProjectGroupModel> listAsync = await App.Connection.Table<ProjectGroupModel>().Where((Expression<Func<ProjectGroupModel, bool>>) (v => v.userId == userId && v.id == id)).ToListAsync();
      return listAsync.Count == 0 ? (ProjectGroupModel) null : listAsync[0];
    }

    public static async Task<int> DeleteProjectGroupById(string id)
    {
      int userId = Utils.GetCurrentUserIdInt();
      List<ProjectGroupModel> listAsync = await App.Connection.Table<ProjectGroupModel>().Where((Expression<Func<ProjectGroupModel, bool>>) (v => v.userId == userId && v.id == id)).ToListAsync();
      if (listAsync.Count == 0)
        return -1;
      listAsync[0].deleted = 1;
      CacheManager.DeleteProjectGroup(id);
      return await App.Connection.UpdateAsync((object) listAsync[0]);
    }

    public static async Task<Dictionary<string, ProjectGroupModel>> GetLocalSyncedProjectGroupMap()
    {
      int userId = Utils.GetCurrentUserIdInt();
      Dictionary<string, ProjectGroupModel> groupDict = new Dictionary<string, ProjectGroupModel>();
      List<ProjectGroupModel> listAsync = await App.Connection.Table<ProjectGroupModel>().Where((Expression<Func<ProjectGroupModel, bool>>) (v => v.userId == userId)).ToListAsync();
      if (listAsync.Count > 0)
      {
        foreach (ProjectGroupModel projectGroupModel in listAsync)
        {
          if (!groupDict.ContainsKey(projectGroupModel.id))
            groupDict.Add(projectGroupModel.id, projectGroupModel);
        }
      }
      Dictionary<string, ProjectGroupModel> syncedProjectGroupMap = groupDict;
      groupDict = (Dictionary<string, ProjectGroupModel>) null;
      return syncedProjectGroupMap;
    }

    private static async Task<List<string>> GetProjectGroupIds()
    {
      return await BaseDao<ProjectGroupModel>.GetEntityIds("select id as Id from ProjectGroupModel where userId = '" + Utils.GetCurrentUserIdInt().ToString() + "'");
    }

    public static async Task SaveServerMergeData(
      List<ProjectGroupModel> added,
      List<ProjectGroupModel> updated,
      List<ProjectGroupModel> deleted)
    {
      if (added.Count > 0)
      {
        List<string> projectGroupIds = await ProjectGroupDao.GetProjectGroupIds();
        List<ProjectGroupModel> addlist = new List<ProjectGroupModel>();
        foreach (ProjectGroupModel projectGroupModel in added)
        {
          if (!projectGroupIds.Contains(projectGroupModel.id))
          {
            projectGroupModel.name = projectGroupModel.name.Trim();
            addlist.Add(projectGroupModel);
          }
        }
        if (addlist.Count > 0)
        {
          int num = await App.Connection.InsertAllAsync((IEnumerable) addlist);
          foreach (ProjectGroupModel projectGroup in addlist)
            CacheManager.UpdateProjectGroup(projectGroup);
        }
        addlist = (List<ProjectGroupModel>) null;
      }
      if (updated.Count > 0)
      {
        updated.ForEach((Action<ProjectGroupModel>) (update => update.name = update.name.Trim()));
        await ProjectGroupSyncedJsonDao.BatchDeleteGroups(updated.Select<ProjectGroupModel, string>((Func<ProjectGroupModel, string>) (group => group.id)).ToList<string>());
        int num = await App.Connection.UpdateAllAsync((IEnumerable) updated);
        foreach (ProjectGroupModel projectGroup in updated)
          CacheManager.UpdateProjectGroup(projectGroup);
      }
      if (deleted.Count <= 0)
        return;
      foreach (object obj in deleted)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
      foreach (ProjectGroupModel projectGroup in deleted)
        CacheManager.DeleteProjectGroup(projectGroup);
    }

    public static async Task<List<ProjectGroupModel>> GetNeedPostProjectGroup()
    {
      int userId = Utils.GetCurrentUserIdInt();
      string syncDone = Constants.SyncStatus.SYNC_DONE.ToString();
      return await App.Connection.Table<ProjectGroupModel>().Where((Expression<Func<ProjectGroupModel, bool>>) (v => v.userId == userId && v.sync_status != syncDone)).ToListAsync();
    }

    public static async Task CheckGroupGroupBy(string groupId)
    {
      ProjectGroupModel groupById = CacheManager.GetGroupById(groupId);
      if (groupById == null || !(groupById.SortOption?.groupBy == "assignee") && !(groupById.Timeline?.sortOption?.groupBy == "assignee") || CacheManager.GetProjectsInGroup(groupId).Any<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsShareList())))
        return;
      if (groupById.SortOption?.groupBy == "assignee")
        groupById.SortOption.groupBy = "project";
      if (groupById.Timeline?.sortOption?.groupBy == "assignee")
        groupById.Timeline.sortOption.groupBy = "project";
      await ProjectGroupDao.TrySaveProjectGroup(groupById);
    }
  }
}
