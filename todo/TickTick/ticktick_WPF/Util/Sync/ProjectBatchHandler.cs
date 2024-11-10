// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ProjectBatchHandler
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
using ticktick_WPF.Util.Twitter;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class ProjectBatchHandler : BatchHandler
  {
    public ProjectBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public async Task MergeWithServer(Collection<ProjectModel> serverProjects, LogModel logModel)
    {
      ProjectBatchHandler projectBatchHandler1 = this;
      List<ProjectModel> added = new List<ProjectModel>();
      List<ProjectModel> updated = new List<ProjectModel>();
      logModel.Log += string.Format("  |  MergeProjects num :{0}", (object) serverProjects?.Count);
      if (serverProjects == null)
      {
        added = (List<ProjectModel>) null;
        updated = (List<ProjectModel>) null;
      }
      else
      {
        Dictionary<string, ProjectModel> localMap = await ProjectDao.GetLocalSyncedProjectMap(projectBatchHandler1.userId);
        List<ProjectSyncedJsonModel> projectSyncedModels = await ProjectSyncJsonDao.GetProjectSyncedModels();
        foreach (ProjectModel serverProject in serverProjects)
        {
          ProjectBatchHandler projectBatchHandler = projectBatchHandler1;
          ProjectModel delta = serverProject;
          if (!LocalSettings.Settings.IsNoteEnabled && delta.kind == Constants.ProjectKind.NOTE.ToString())
          {
            LocalSettings.Settings.IsNoteEnabled = true;
            LocalSettings.Settings.Save();
          }
          if (delta.userid == null)
            delta.userid = projectBatchHandler1.userId;
          if (localMap.ContainsKey(delta.id))
          {
            ProjectModel revised = localMap[delta.id];
            localMap.Remove(delta.id);
            if ((revised.etag == null || delta.etag == null || !revised.etag.Equals(delta.etag)) && !revised.delete_status)
            {
              if (revised.sync_status == null)
                revised.sync_status = Constants.SyncStatus.SYNC_DONE.ToString();
              if (revised.sync_status.Equals(Constants.SyncStatus.SYNC_UPDATE.ToString()) || revised.sync_status.Equals(Constants.SyncStatus.SYNC_INIT.ToString()))
              {
                ProjectModel original = revised;
                ProjectSyncedJsonModel projectSyncedJsonModel = projectSyncedModels != null ? projectSyncedModels.FirstOrDefault<ProjectSyncedJsonModel>((Func<ProjectSyncedJsonModel, bool>) (p => p.UserId == projectBatchHandler.userId && p.ProjectId == delta.id)) : (ProjectSyncedJsonModel) null;
                if (projectSyncedJsonModel != null)
                  original = JsonConvert.DeserializeObject<ProjectModel>(projectSyncedJsonModel.JsonString);
                MergeUtils.Merge(original, delta, revised);
                revised.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
                updated.Add(revised);
              }
              else if (revised.sync_status.Equals(Constants.SyncStatus.SYNC_DONE.ToString()))
              {
                delta._Id = revised._Id;
                delta.sync_status = Constants.SyncStatus.SYNC_DONE.ToString();
                delta.ShowAddColumn = revised.ShowAddColumn;
                delta.Timeline = revised.Timeline ?? new TimelineModel();
                delta.Timeline.SortType = delta.SyncTimeline?.SortType ?? delta.Timeline.SortType;
                delta.Timeline.sortOption = delta.SyncTimeline?.SortOption ?? delta.Timeline.sortOption;
                updated.Add(delta);
              }
            }
          }
          else
          {
            delta.sync_status = Constants.SyncStatus.SYNC_DONE.ToString();
            delta.userid = projectBatchHandler1.userId;
            delta.Timeline = delta.SyncTimeline == null ? (TimelineModel) null : new TimelineModel(delta.SyncTimeline.SortType, delta.SyncTimeline.SortOption);
            added.Add(delta);
          }
        }
        List<ProjectModel> deleted = localMap.Values.Where<ProjectModel>((Func<ProjectModel, bool>) (project => project.sync_status == Constants.SyncStatus.SYNC_DONE.ToString() && !project.delete_status)).ToList<ProjectModel>();
        LogModel logModel1 = logModel;
        logModel1.Log = logModel1.Log + " a:" + added.Select<ProjectModel, string>((Func<ProjectModel, string>) (a => a.id)).Join<string>(",") + " u:" + updated.Select<ProjectModel, string>((Func<ProjectModel, string>) (a => a.id)).Join<string>(",") + " d:" + deleted.Select<ProjectModel, string>((Func<ProjectModel, string>) (a => a.id)).Join<string>(",");
        if (added.Count > 0 || updated.Count > 0 || deleted.Count > 0)
        {
          await ProjectDao.SaveServerMergeData(added, updated, deleted);
          if (projectBatchHandler1.syncResult == null)
            projectBatchHandler1.syncResult = new SyncResult();
          projectBatchHandler1.syncResult.AddedProjects = added;
          projectBatchHandler1.syncResult.UpdatedProjects = updated;
          projectBatchHandler1.syncResult.DeletedProjects = deleted;
        }
        localMap = (Dictionary<string, ProjectModel>) null;
        deleted = (List<ProjectModel>) null;
        added = (List<ProjectModel>) null;
        updated = (List<ProjectModel>) null;
      }
    }

    public static async Task CommitToServer(bool delete, LogModel logModel)
    {
      Dictionary<string, string> needPostProject = await ProjectDao.GetNeedPostProject();
      if (needPostProject == null || needPostProject.Count <= 0)
        return;
      SyncProjectBean syncBean = await ProjectTransfer.DescribeSyncProjectBean(needPostProject);
      if (!syncBean.Empty)
      {
        if (delete)
        {
          syncBean.Update.Clear();
          syncBean.Add.Clear();
          LogModel logModel1 = logModel;
          logModel1.Log = logModel1.Log + "\r\n" + string.Format("big sync: delete project count : {0}", (object) syncBean.Deleted.Count);
        }
        else
        {
          syncBean.Deleted.Clear();
          LogModel logModel2 = logModel;
          logModel2.Log = logModel2.Log + "\r\n" + string.Format("big sync: update project count : a {0} u {1}", (object) syncBean.Add.Count, (object) syncBean.Update.Count);
        }
      }
      if (!syncBean.Empty)
      {
        BatchUpdateResult batchUpdateResult = await Communicator.BatchUpdateProject(syncBean);
        if (batchUpdateResult != null)
          await ProjectBatchHandler.HandleCommitResult(batchUpdateResult.Id2etag, batchUpdateResult.Id2error, syncBean, logModel);
      }
      syncBean = (SyncProjectBean) null;
    }

    private static async Task HandleCommitResult(
      Dictionary<string, string> id2Etag,
      Dictionary<string, string> id2Error,
      SyncProjectBean bean,
      LogModel logModel)
    {
      if (id2Etag != null && id2Etag.Any<KeyValuePair<string, string>>())
      {
        List<ProjectModel> projects = bean.Update;
        projects.AddRange((IEnumerable<ProjectModel>) bean.Add);
        logModel.Log += "Id2etag : ";
        foreach (string key in id2Etag.Keys)
        {
          string serverId = key;
          ProjectModel projectById = await ProjectDao.GetProjectById(serverId);
          if (projectById != null)
          {
            ProjectModel projectModel = projects.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == serverId));
            if (projectModel != null)
            {
              DateTime? modifiedTime1 = projectModel.modifiedTime;
              DateTime? modifiedTime2 = projectById.modifiedTime;
              if ((modifiedTime1.HasValue & modifiedTime2.HasValue ? (modifiedTime1.GetValueOrDefault() >= modifiedTime2.GetValueOrDefault() ? 1 : 0) : 0) == 0)
              {
                projectById.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
                goto label_9;
              }
            }
            projectById.sync_status = Constants.SyncStatus.SYNC_DONE.ToString();
label_9:
            projectById.etag = id2Etag[serverId];
          }
          CacheManager.UpdateProject(projectById, false);
          int num = await App.Connection.UpdateAsync((object) projectById);
          LogModel logModel1 = logModel;
          logModel1.Log = logModel1.Log + serverId + "  ";
        }
        await ProjectSyncJsonDao.BatchDeleteProjects(id2Etag.Keys.ToList<string>());
        projects = (List<ProjectModel>) null;
      }
      if (id2Error != null && id2Error.Count > 0)
      {
        string log = string.Empty;
        foreach (string serverId in id2Error.Keys)
        {
          ProjectModel project = await ProjectDao.GetProjectById(serverId);
          if (project != null)
          {
            Constants.SyncStatus syncStatus;
            switch (id2Error[serverId])
            {
              case "EXISTED":
                ProjectModel projectModel1 = project;
                syncStatus = Constants.SyncStatus.SYNC_UPDATE;
                string status1 = syncStatus.ToString();
                projectModel1.SetSyncStatus(status1);
                int num1 = await App.Connection.UpdateAsync((object) project);
                break;
              case "NO_TEAM_PERMISSION":
                ProjectModel projectModel2 = project;
                syncStatus = Constants.SyncStatus.SYNC_DONE;
                string status2 = syncStatus.ToString();
                projectModel2.SetSyncStatus(status2);
                int num2 = await App.Connection.UpdateAsync((object) project);
                break;
              case "EXCEED_QUOTA":
                ProjectModel projectModel3 = project;
                syncStatus = Constants.SyncStatus.SYNC_ERROR_UP_LIMIT;
                string status3 = syncStatus.ToString();
                projectModel3.SetSyncStatus(status3);
                int num3 = await App.Connection.UpdateAsync((object) project);
                break;
              case "NO_PROJECT_PERMISSION":
                ProjectModel projectModel4 = project;
                syncStatus = Constants.SyncStatus.SYNC_DONE;
                string status4 = syncStatus.ToString();
                projectModel4.SetSyncStatus(status4);
                int num4 = await App.Connection.UpdateAsync((object) project);
                break;
              case "UNKNOWN":
                ProjectModel projectModel5 = project;
                syncStatus = Constants.SyncStatus.SYNC_DONE;
                string status5 = syncStatus.ToString();
                projectModel5.SetSyncStatus(status5);
                int num5 = await App.Connection.UpdateAsync((object) project);
                break;
            }
            CacheManager.UpdateProject(project, false);
          }
          log = log + " " + serverId + " : " + id2Error[serverId] + " ";
          project = (ProjectModel) null;
        }
        LogModel logModel2 = logModel;
        logModel2.Log = logModel2.Log + "error " + log;
        log = (string) null;
      }
      if (bean.Deleted == null || bean.Deleted.Count <= 0)
        return;
      foreach (string id in bean.Deleted)
      {
        ProjectModel projectById = await ProjectDao.GetProjectById(id);
        if (projectById != null)
          App.Connection.DeleteAsync((object) projectById);
      }
      await ProjectSyncJsonDao.BatchDeleteProjects(bean.Deleted.ToList<string>());
    }
  }
}
