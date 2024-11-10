// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ProjectGroupBatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sync.Model;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class ProjectGroupBatchHandler : BatchHandler
  {
    public ProjectGroupBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public async Task MergeWithServer(
      Collection<ProjectGroupModel> serverGroups,
      LogModel syncBeanLog)
    {
      ProjectGroupBatchHandler groupBatchHandler = this;
      List<ProjectGroupModel> added = new List<ProjectGroupModel>();
      List<ProjectGroupModel> updated = new List<ProjectGroupModel>();
      syncBeanLog.Log += string.Format("\r\n\tMergeGroup num :{0}", (object) serverGroups?.Count);
      if (serverGroups == null)
      {
        added = (List<ProjectGroupModel>) null;
        updated = (List<ProjectGroupModel>) null;
      }
      else
      {
        Dictionary<string, ProjectGroupModel> localGroups = await ProjectGroupDao.GetLocalSyncedProjectGroupMap();
        Constants.SyncStatus syncStatus1;
        foreach (ProjectGroupModel server in serverGroups)
        {
          if (localGroups.ContainsKey(server.id))
          {
            ProjectGroupModel local = localGroups[server.id];
            localGroups.Remove(server.id);
            if (string.IsNullOrEmpty(local.etag) || !local.etag.Equals(server.etag))
            {
              string syncStatus2 = local.sync_status;
              syncStatus1 = Constants.SyncStatus.SYNC_NEW;
              string str1 = syncStatus1.ToString();
              if (!syncStatus2.Equals(str1))
              {
                ProjectGroupSyncedJsonModel savedJson = await ProjectGroupSyncedJsonDao.GetSavedJson(server.id);
                if (savedJson != null)
                {
                  MergeUtils.Merge(JsonConvert.DeserializeObject<ProjectGroupModel>(savedJson.JsonString), server, local);
                  ProjectGroupModel projectGroupModel = local;
                  syncStatus1 = Constants.SyncStatus.SYNC_UPDATE;
                  string str2 = syncStatus1.ToString();
                  projectGroupModel.sync_status = str2;
                  updated.Add(local);
                }
                else
                {
                  server._Id = local._Id;
                  ProjectGroupModel projectGroupModel = server;
                  syncStatus1 = Constants.SyncStatus.SYNC_DONE;
                  string str3 = syncStatus1.ToString();
                  projectGroupModel.sync_status = str3;
                  server.open = local.open;
                  server.Timeline = local.Timeline ?? new TimelineModel(Constants.SortType.project.ToString());
                  server.Timeline.SortType = server.SyncTimeline?.SortType ?? server.Timeline.SortType;
                  server.Timeline.sortOption = server.SyncTimeline?.SortOption ?? server.Timeline.sortOption;
                  updated.Add(server);
                }
                local = (ProjectGroupModel) null;
              }
            }
          }
          else
          {
            ProjectGroupModel projectGroupModel = server;
            syncStatus1 = Constants.SyncStatus.SYNC_DONE;
            string str = syncStatus1.ToString();
            projectGroupModel.sync_status = str;
            server.userId = int.Parse(groupBatchHandler.userId);
            server.Timeline = server.SyncTimeline == null ? (TimelineModel) null : new TimelineModel(server.SyncTimeline.SortType, server.SyncTimeline.SortOption);
            added.Add(server);
          }
        }
        List<ProjectGroupModel> deleted = localGroups.Values.Where<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (v => v.sync_status == Constants.SyncStatus.SYNC_DONE.ToString())).ToList<ProjectGroupModel>();
        syncBeanLog.Log += string.Format(" a:{0} u:{1} d:{2}", (object) added.Count, (object) updated.Count, (object) deleted.Count);
        if (added.Count > 0 || updated.Count > 0 || deleted.Count > 0)
        {
          await ProjectGroupDao.SaveServerMergeData(added, updated, deleted);
          if (groupBatchHandler.syncResult == null)
            groupBatchHandler.syncResult = new SyncResult();
          groupBatchHandler.syncResult.AddedProjectGroups = added;
          groupBatchHandler.syncResult.UpdatedProjectGroups = updated;
          groupBatchHandler.syncResult.DeletedProjectGroups = deleted;
        }
        localGroups = (Dictionary<string, ProjectGroupModel>) null;
        deleted = (List<ProjectGroupModel>) null;
        added = (List<ProjectGroupModel>) null;
        updated = (List<ProjectGroupModel>) null;
      }
    }

    public static async Task CommitToServer(LogModel logModel)
    {
      List<ProjectGroupModel> postProjectGroup = await ProjectGroupDao.GetNeedPostProjectGroup();
      if (postProjectGroup == null || postProjectGroup.Count <= 0)
        return;
      SyncProjectGroupBean syncBean = ProjectGroupTransfer.DescribeSyncProjectGroupBean(postProjectGroup);
      if (syncBean == null || syncBean.Empty)
        return;
      LogModel logModel1 = logModel;
      logModel1.Log = logModel1.Log + "\r\n" + string.Format("big sync:  update group count : a {0} u {1} d {2}", (object) syncBean.Add.Count, (object) syncBean.Update.Count, (object) syncBean.Deleted.Count);
      BatchUpdateResult batchUpdateResult = await Communicator.BatchUpdateProjectGroup(syncBean);
      if (batchUpdateResult == null)
        return;
      await ProjectGroupBatchHandler.HandleCommitResult(batchUpdateResult.Id2etag, batchUpdateResult.Id2error, logModel);
    }

    private static async Task HandleCommitResult(
      Dictionary<string, string> id2Etag,
      Dictionary<string, string> id2Error,
      LogModel logModel)
    {
      string serverId;
      ProjectGroupModel projectGroup;
      if (id2Etag != null)
      {
        logModel.Log += "Id2etag : ";
        foreach (string key in id2Etag.Keys)
        {
          serverId = key;
          projectGroup = await ProjectGroupDao.GetProjectGroupById(serverId);
          if (projectGroup != null)
          {
            projectGroup.sync_status = Constants.SyncStatus.SYNC_DONE.ToString();
            projectGroup.etag = id2Etag[serverId];
            int num = await App.Connection.UpdateAsync((object) projectGroup);
            CacheManager.UpdateProjectGroup(projectGroup);
            LogModel logModel1 = logModel;
            logModel1.Log = logModel1.Log + serverId + "  ";
          }
          projectGroup = (ProjectGroupModel) null;
          serverId = (string) null;
        }
        await ProjectGroupSyncedJsonDao.BatchDeleteGroups(id2Etag.Keys.ToList<string>());
      }
      if (id2Error == null)
        return;
      serverId = string.Empty;
      foreach (string serverId1 in id2Error.Keys)
      {
        projectGroup = await ProjectGroupDao.GetProjectGroupById(serverId1);
        Constants.SyncStatus syncStatus;
        if (projectGroup != null)
        {
          switch (id2Error[serverId1])
          {
            case "EXISTED":
              ProjectGroupModel projectGroupModel1 = projectGroup;
              syncStatus = Constants.SyncStatus.SYNC_UPDATE;
              string str1 = syncStatus.ToString();
              projectGroupModel1.sync_status = str1;
              int num1 = await App.Connection.UpdateAsync((object) projectGroup);
              break;
            case "NOT_EXISTED":
              ProjectGroupModel projectGroupModel2 = projectGroup;
              string str2;
              if (projectGroup.deleted != 0)
              {
                syncStatus = Constants.SyncStatus.SYNC_DONE;
                str2 = syncStatus.ToString();
              }
              else
              {
                syncStatus = Constants.SyncStatus.SYNC_NEW;
                str2 = syncStatus.ToString();
              }
              projectGroupModel2.sync_status = str2;
              int num2 = await App.Connection.UpdateAsync((object) projectGroup);
              break;
          }
        }
        CacheManager.UpdateProjectGroup(projectGroup);
        serverId = serverId + serverId1 + " : " + id2Error[serverId1];
        projectGroup = (ProjectGroupModel) null;
      }
      LogModel logModel2 = logModel;
      logModel2.Log = logModel2.Log + "   error " + serverId;
      serverId = (string) null;
    }
  }
}
