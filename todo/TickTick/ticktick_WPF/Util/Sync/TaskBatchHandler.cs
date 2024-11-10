// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.TaskBatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util.Sync.Model;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class TaskBatchHandler : BatchHandler
  {
    public TaskBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public async Task MergeWithServer(SyncTaskBean syncTaskBean, int type, LogModel logModel)
    {
      TaskBatchHandler taskBatchHandler = this;
      TaskSyncModel taskSyncModel = await TaskSyncCollector.CollectTaskSyncBean(syncTaskBean, type);
      if (taskBatchHandler.syncResult == null)
        taskBatchHandler.syncResult = new SyncResult();
      if (syncTaskBean != null && taskSyncModel != null)
      {
        taskBatchHandler.syncResult.AddedTasks = taskSyncModel.TaskSyncBean.Added;
        taskBatchHandler.syncResult.UpdatedTasks = taskSyncModel.TaskSyncBean.Updated;
        taskBatchHandler.syncResult.UpdatedTasks.AddRange((IEnumerable<TaskModel>) taskSyncModel.TaskSyncBean.Updating);
        taskBatchHandler.syncResult.DeletedTasks = taskSyncModel.TaskSyncBean.DeletedInTrash.Select<TaskModel, TaskProject>((Func<TaskModel, TaskProject>) (task => new TaskProject()
        {
          projectId = task.projectId,
          taskId = task.id
        })).ToList<TaskProject>();
      }
      await TaskBatchHandler.BatchSaveTaskSyncModel(taskSyncModel);
      if (syncTaskBean == null)
        taskSyncModel = (TaskSyncModel) null;
      else if (taskSyncModel == null)
      {
        taskSyncModel = (TaskSyncModel) null;
      }
      else
      {
        logModel.Log += string.Format("\r\n\tpull task a:{0} u:{1} d:{2} result a:{3} ud:{4} ug:{5} d:{6} df:{7}", (object) syncTaskBean.Add.Count, (object) syncTaskBean.Update.Count, (object) syncTaskBean.Delete.Count, (object) taskSyncModel.TaskSyncBean.Added.Count, (object) taskSyncModel.TaskSyncBean.Updated.Count, (object) taskSyncModel.TaskSyncBean.Updating.Count, (object) taskSyncModel.TaskSyncBean.DeletedInTrash.Count, (object) taskSyncModel.TaskSyncBean.DeletedForever.Count);
        logModel.Log += string.Format("  |  pull attachment a:{0} ud:{1} d:{2}", (object) taskSyncModel.AttachmentSyncBean.Added.Count, (object) taskSyncModel.AttachmentSyncBean.Updated.Count, (object) taskSyncModel.AttachmentSyncBean.Deleted.Count);
        taskSyncModel = (TaskSyncModel) null;
      }
    }

    private static async Task BatchSaveTaskSyncModel(TaskSyncModel taskSyncModel)
    {
      await TaskBatchHandler.SaveTaskSyncBean(taskSyncModel.TaskSyncBean);
      await TaskBatchHandler.SaveTaskOtherEntity(taskSyncModel);
      await TaskSyncedJsonDao.SaveTaskSyncedJsons(taskSyncModel.TaskSyncedJsonBean);
    }

    private static async Task SaveTaskOtherEntity(TaskSyncModel taskSyncModel)
    {
      if (taskSyncModel.AttachmentSyncBean.Empty)
        return;
      await AttachmentDao.SaveServerMergeToDb(taskSyncModel.AttachmentSyncBean);
    }

    private static async Task SaveTaskSyncBean(TaskSyncBean taskSyncBean)
    {
      if (taskSyncBean.DeletedInTrash.Count > 0)
      {
        foreach (TaskModel taskModel in taskSyncBean.DeletedInTrash)
          await TaskDao.DeleteTaskToTrash(taskModel.id);
      }
      if (taskSyncBean.DeletedForever.Count > 0)
      {
        foreach (TaskModel taskModel in taskSyncBean.DeletedForever)
          await TaskDao.DeleteTaskForever(taskModel.id);
      }
      if (taskSyncBean.Added.Count > 0)
        await TaskDao.BatchCreateTaskFromRemote(taskSyncBean.Added, false);
      if (taskSyncBean.Updated.Count > 0 || taskSyncBean.Updating.Count > 0)
        await TaskDao.BatchUpdateTasksFromRemote(taskSyncBean);
      TaskChangeNotifier.OnServerTaskChanged(taskSyncBean.GetIds());
    }

    public async Task<List<SyncStatusModel>> GetNeedPostCreatedTasks()
    {
      return await TaskDao.GetNeedPostTasks(4);
    }

    public async Task<List<SyncStatusModel>> GetNeedClearTrash()
    {
      return await TaskDao.GetNeedPostTasks(8);
    }

    public static async Task HandleCommitResult(
      SyncTaskBean syncTaskBean,
      BatchUpdateResult result,
      LogModel logModel)
    {
      bool emptyExisted = false;
      LogModel logModel1 = logModel;
      logModel1.Log = logModel1.Log + " etag : " + result?.Id2etag?.Count.ToString();
      if (result == null)
      {
        logModel.Log += " result null";
      }
      else
      {
        string log;
        if (result.Id2error != null && result.Id2error.Count > 0)
        {
          log = string.Empty;
          foreach (KeyValuePair<string, string> keyValuePair in result.Id2error)
          {
            KeyValuePair<string, string> error = keyValuePair;
            switch (error.Value)
            {
              case "EXISTED":
                int num1 = await SyncStatusDao.DeleteSyncStatus(error.Key, 4, false) ? 1 : 0;
                await SyncStatusDao.AddModifySyncStatus(error.Key);
                break;
              case "UNKNOWN":
                int num2 = await SyncStatusDao.DeleteSyncStatus(error.Key, 6) ? 1 : 0;
                int num3 = await SyncStatusDao.DeleteSyncStatus(error.Key, 7) ? 1 : 0;
                break;
            }
            log = log + " " + error.Key + ":" + error.Value + " ";
            error = new KeyValuePair<string, string>();
          }
          LogModel logModel2 = logModel;
          logModel2.Log = logModel2.Log + " error : " + log;
          log = (string) null;
        }
        List<string> entityIdsByType1 = await SyncStatusDao.GetEntityIdsByType(4);
        if (entityIdsByType1 != null && entityIdsByType1.Count > 0)
        {
          logModel.Log += " create : ";
          foreach (string str in entityIdsByType1)
          {
            log = str;
            if (!string.IsNullOrEmpty(log))
            {
              TaskBatchHandler.UpdateEtag(result, log);
              if (result.Id2etag != null && result.Id2etag.Count != 0 && result.Id2etag.ContainsKey(log))
              {
                if (await SyncStatusDao.DeleteSyncStatus(log, 4, false))
                  await SyncStatusDao.AddSyncStatus(log, 0);
                LogModel logModel3 = logModel;
                logModel3.Log = logModel3.Log + log + ",";
              }
            }
            else
              emptyExisted = true;
            log = (string) null;
          }
        }
        List<string> entityIdsByType2 = await SyncStatusDao.GetEntityIdsByType(5);
        if (entityIdsByType2 != null && entityIdsByType2.Count > 0)
        {
          foreach (string str in entityIdsByType2)
          {
            if (!string.IsNullOrEmpty(str))
            {
              TaskBatchHandler.UpdateEtag(result, str);
              if (result.Id2error == null || result.Id2error.Count == 0 || !result.Id2error.Keys.Contains<string>(str))
              {
                int num = await SyncStatusDao.DeleteSyncStatus(str, 5) ? 1 : 0;
              }
            }
          }
        }
        List<string> entityIdsByType3 = await SyncStatusDao.GetEntityIdsByType(6);
        if (entityIdsByType3 != null && entityIdsByType3.Count > 0)
        {
          foreach (string str in entityIdsByType3)
          {
            if (!string.IsNullOrEmpty(str))
            {
              TaskBatchHandler.UpdateEtag(result, str);
              if (result.Id2etag != null && result.Id2etag.Count != 0 && result.Id2etag.Keys.Contains<string>(str))
              {
                int num = await SyncStatusDao.DeleteSyncStatus(str, 6) ? 1 : 0;
              }
            }
          }
        }
        List<string> entityIdsByType4 = await SyncStatusDao.GetEntityIdsByType(0);
        if (entityIdsByType4 != null && entityIdsByType4.Count > 0)
        {
          logModel.Log += " update : ";
          foreach (string str in entityIdsByType4)
          {
            if (str != null)
            {
              TaskBatchHandler.UpdateEtag(result, str);
              if (result.Id2etag != null && result.Id2etag.Count != 0 && result.Id2etag.ContainsKey(str))
              {
                LogModel logModel4 = logModel;
                logModel4.Log = logModel4.Log + str + ",";
                int num = await SyncStatusDao.DeleteSyncStatus(str, 0) ? 1 : 0;
              }
            }
            else
              emptyExisted = true;
          }
        }
        if (result.Id2etag != null && syncTaskBean != null && result.Id2etag.Count > 0)
        {
          TaskSyncedJsonBean bean = new TaskSyncedJsonBean();
          if (syncTaskBean.Add != null && syncTaskBean.Add.Count > 0)
          {
            foreach (TaskModel taskModel in syncTaskBean.Add)
              bean.Added.Add(taskModel);
          }
          if (syncTaskBean.Update != null && syncTaskBean.Update.Count > 0)
          {
            foreach (TaskModel taskModel in syncTaskBean.Update)
              bean.Updated.Add(taskModel);
          }
          await TaskSyncedJsonDao.SaveTaskSyncedJsons(bean);
        }
        if (!emptyExisted)
          return;
        await SyncStatusDao.DeleteEmptySyncStatus();
      }
    }

    private static async void UpdateEtag(BatchUpdateResult result, string taskSid)
    {
      if (string.IsNullOrWhiteSpace(taskSid) || result?.Id2etag == null || !result.Id2etag.ContainsKey(taskSid))
        return;
      TaskModel taskById = await TaskDao.GetTaskById(taskSid);
      if (taskById == null)
        return;
      taskById.etag = result.Id2etag[taskSid];
      await TaskDao.UpdateTask(taskById);
    }

    public async Task<List<SyncStatusModel>> GetNeedPostDeletedTasks()
    {
      return await TaskDao.GetNeedPostTasks(5);
    }

    public async Task<List<SyncStatusModel>> GetNeedPostDeleteForeverTasks()
    {
      return await TaskDao.GetNeedPostTasks(6);
    }

    public async Task<List<SyncStatusModel>> GetLocalContentUpdatedTasks()
    {
      return await TaskDao.GetNeedPostTasks(0);
    }

    public async Task MergeTasksOfOpenedProjects(
      Dictionary<string, ObservableCollection<TaskModel>> serverTasksDict)
    {
      ObservableCollection<TaskModel> tasksInProjectIds = await TaskDao.GetTasksInProjectIds((IEnumerable<string>) serverTasksDict.Keys.ToList<string>());
      Dictionary<string, TaskModel> id2Tasks = new Dictionary<string, TaskModel>();
      foreach (TaskModel taskModel in (Collection<TaskModel>) tasksInProjectIds)
        id2Tasks[taskModel.id] = taskModel;
      List<TaskModel> addOrUpdateServerTasks = new List<TaskModel>();
      foreach (ObservableCollection<TaskModel> observableCollection in serverTasksDict.Values)
      {
        if (observableCollection != null)
        {
          foreach (TaskModel taskModel1 in (Collection<TaskModel>) observableCollection)
          {
            if (id2Tasks.ContainsKey(taskModel1.id))
            {
              TaskModel taskModel2 = id2Tasks[taskModel1.id];
              if (taskModel2 != null)
              {
                id2Tasks.Remove(taskModel1.id);
                if (!(taskModel1.etag == taskModel2.etag))
                  addOrUpdateServerTasks.Add(taskModel1);
              }
            }
            else
              addOrUpdateServerTasks.Add(taskModel1);
          }
        }
      }
      foreach (string key in id2Tasks.Keys)
      {
        string taskId = key;
        TaskModel taskModel = id2Tasks[taskId];
        if (taskModel == null || taskModel.status == 0)
        {
          Dictionary<int, SyncStatusModel> syncStatusMap = await SyncStatusDao.GetSyncStatusMap(taskId);
          if (syncStatusMap == null || !syncStatusMap.ContainsKey(4) && !syncStatusMap.ContainsKey(2) && !syncStatusMap.ContainsKey(7))
            TaskDao.DeleteTaskInDb(taskId);
          taskId = (string) null;
        }
      }
      TaskSyncModel taskSyncModel = new TaskSyncModel();
      await TaskSyncCollector.MergeUpdatedTasksFromServer(taskSyncModel, addOrUpdateServerTasks, 1);
      await TaskBatchHandler.BatchSaveTaskSyncModel(taskSyncModel);
      if (taskSyncModel.TaskSyncBean.Empty)
      {
        id2Tasks = (Dictionary<string, TaskModel>) null;
        addOrUpdateServerTasks = (List<TaskModel>) null;
        taskSyncModel = (TaskSyncModel) null;
      }
      else
      {
        TaskChangeNotifier.NotifyTaskBatchChanged(taskSyncModel.TaskSyncBean.GetIds());
        id2Tasks = (Dictionary<string, TaskModel>) null;
        addOrUpdateServerTasks = (List<TaskModel>) null;
        taskSyncModel = (TaskSyncModel) null;
      }
    }

    public static async Task HandleRepeatCompletedTasks(SyncTaskBean syncBean)
    {
      if (syncBean == null)
        return;
      List<TaskModel> source = new List<TaskModel>();
      if (syncBean.Add != null && syncBean.Add.Any<TaskModel>())
        source.AddRange((IEnumerable<TaskModel>) syncBean.Add);
      if (syncBean.Update != null && syncBean.Update.Any<TaskModel>())
        source.AddRange((IEnumerable<TaskModel>) syncBean.Update);
      if (!source.Any<TaskModel>())
        return;
      List<TaskModel> remoteCompletedTasks = source.Where<TaskModel>((Func<TaskModel, bool>) (task => task.status != 0 && !string.IsNullOrEmpty(task.repeatTaskId) && task.id != null && task.id != task.repeatTaskId)).ToList<TaskModel>();
      if (remoteCompletedTasks.Any<TaskModel>())
      {
        List<TaskModel> localCompletedTasks = await TaskDao.GetUnSyncCompletedTasks();
        if (localCompletedTasks != null && localCompletedTasks.Any<TaskModel>())
        {
          List<TaskModel> duplicatedTasks = remoteCompletedTasks.Select<TaskModel, TaskModel>((Func<TaskModel, TaskModel>) (serverTask => localCompletedTasks.FirstOrDefault<TaskModel>((Func<TaskModel, bool>) (localTask =>
          {
            if (localTask.repeatTaskId != null && serverTask.repeatTaskId != null && localTask.repeatTaskId == serverTask.repeatTaskId)
            {
              DateTime? startDate = localTask.startDate;
              if (startDate.HasValue)
              {
                startDate = serverTask.startDate;
                if (startDate.HasValue)
                {
                  startDate = localTask.startDate;
                  DateTime dateTime1 = startDate.Value;
                  startDate = serverTask.startDate;
                  DateTime dateTime2 = startDate.Value;
                  return dateTime1 == dateTime2;
                }
              }
            }
            return false;
          })))).Where<TaskModel>((Func<TaskModel, bool>) (duplicated => duplicated != null)).ToList<TaskModel>();
          if (duplicatedTasks.Any<TaskModel>())
          {
            await SyncStatusDao.BatchRemoveAddSyncStatus(duplicatedTasks.Select<TaskModel, string>((Func<TaskModel, string>) (task => task.id)).ToList<string>());
            foreach (TaskModel taskModel in duplicatedTasks)
              await TaskDao.DeleteTaskForever(taskModel.id);
          }
          duplicatedTasks = (List<TaskModel>) null;
        }
      }
      remoteCompletedTasks = (List<TaskModel>) null;
    }

    public static async void HandleSetParentResult(Dictionary<string, string> resultId2Etag)
    {
    }
  }
}
