// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.SyncService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.SyncServices;
using ticktick_WPF.Util.Attachment;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Undo;
using TickTickModels;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class SyncService
  {
    private static readonly SemaphoreLocker SyncLock = new SemaphoreLocker();
    public static bool NeedRecommit;
    private readonly SyncResult _syncResult = new SyncResult();
    private EventBatchHandler _eventBatchHandler;
    private FilterBatchHandler _filterBatchHandler;
    private ProjectBatchHandler _projectBatchHandler;
    private ProjectGroupBatchHandler _projectGroupBatchHandler;
    private TaskOrderBatchHandler _sortOrderBatchHandler;
    private SyncOrderBatchHandler _syncOrderBatchHandler;
    private TagBatchHandler _tagBatchHandler;
    private TaskBatchHandler _taskBatchHandler;
    private bool _needSync;
    private LogModel _syncLog;
    private bool _pullingOthers;
    private static readonly BlockingList<string> TaskPullingProjectIds = new BlockingList<string>();

    public event EventHandler<SyncResult> SyncFinished;

    public event EventHandler<SyncResult> SyncStart;

    public void Init() => this.InitBatchHandler();

    private void InitBatchHandler()
    {
      string loginUserId = LocalSettings.Settings.LoginUserId;
      this._projectGroupBatchHandler = new ProjectGroupBatchHandler(loginUserId, this._syncResult);
      this._projectBatchHandler = new ProjectBatchHandler(loginUserId, this._syncResult);
      this._taskBatchHandler = new TaskBatchHandler(loginUserId, this._syncResult);
      this._sortOrderBatchHandler = new TaskOrderBatchHandler(loginUserId, this._syncResult);
      this._syncOrderBatchHandler = new SyncOrderBatchHandler(loginUserId, this._syncResult);
      this._filterBatchHandler = new FilterBatchHandler(loginUserId, this._syncResult);
      this._tagBatchHandler = new TagBatchHandler(loginUserId, this._syncResult);
      this._eventBatchHandler = new EventBatchHandler(loginUserId, this._syncResult);
    }

    public async Task DoSync(int type, string projectId = "")
    {
      if (type == 0)
        this._needSync = true;
      await SyncService.SyncLock.LockAsync((Func<Task>) (async () =>
      {
        int num;
        if (num != 0 && type == 0)
        {
          if (!this._needSync)
            return;
          this._needSync = false;
        }
        try
        {
          EventHandler<SyncResult> syncStart = this.SyncStart;
          if (syncStart != null)
            syncStart((object) this, (SyncResult) null);
          this._syncResult.Clear();
          await this.SyncData(type, projectId);
        }
        finally
        {
          SyncResult e = this._syncResult.Copy();
          e.SyncType = type;
          EventHandler<SyncResult> syncFinished = this.SyncFinished;
          if (syncFinished != null)
            syncFinished((object) this, e);
        }
      }));
    }

    private static void SetSyncPoint() => LocalSettings.Settings.SyncPoint = DateTime.Now.Ticks;

    private async Task SyncData(int type, string projectId = "")
    {
      this._syncLog = new LogModel(TicketLogUtils.GetLogString("Start SyncData"));
      try
      {
        SyncService.NeedRecommit = false;
        this.PullOtherData(type);
        await this.PullColumns(projectId);
        await this.Pull(type);
        LogModel syncLog = this._syncLog;
        syncLog.Log = syncLog.Log + "\r\n\t" + TicketLogUtils.GetLogString("big sync: will post data");
        await this.Commit(type);
        if (SyncService.NeedRecommit)
          await this.Commit(type);
        this._syncLog.Log += "\r\n\tbig sync: did post data";
        UtilLog.Info(this._syncLog.Log);
        await this.HandleDirtyData(type);
        await this.TryUploadAttachment();
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
    }

    private async Task TryUploadAttachment() => await AttachmentUploadUtils.UploadAllFiles();

    private async Task HandleDirtyData(int type)
    {
      if (type != 1)
        return;
      try
      {
        await TaskDao.HandleDirtyData();
      }
      catch (Exception ex)
      {
      }
    }

    private async Task PullColumns(string projectId)
    {
      if (string.IsNullOrEmpty(projectId))
        await this.PullColumns();
      else
        await this.PullProjectColumns(projectId);
    }

    private async Task PullColumns()
    {
      await this.PullColumns(await UserDao.GetColumnCheckPoint(Utils.GetCurrentUserIdInt().ToString()));
    }

    private async Task PullColumns(long checkPoint)
    {
      SyncColumnBean bean = await Communicator.CheckRemoteColumnChanged(checkPoint);
      if (bean == null)
        return;
      this._syncLog.Log += string.Format("\r\n\tpullColumns u{0} a{1} d{2}", (object) bean.Update?.Count, (object) bean.Add?.Count, (object) bean.Deleted?.Count);
      int num = await ColumnBatchHandler.MergeWithServer(bean, this._syncLog) ? 1 : 0;
      await UserDao.SaveColumnCheckPoint(Utils.GetCurrentUserIdInt().ToString(), Utils.GetNowTimeStampInMills());
    }

    private async Task PullProjectColumns(string projectId)
    {
      if (string.IsNullOrEmpty(projectId))
        return;
      SyncResult syncResult = this._syncResult;
      syncResult.ColumnChanged = await ColumnBatchHandler.MergeWithServer(projectId);
      syncResult = (SyncResult) null;
    }

    private async Task PullOtherData(int type)
    {
      if (this._pullingOthers)
        return;
      this._pullingOthers = true;
      if (type == 1)
      {
        TickFocusManager.GetRemotePomos();
        TimerSyncService.PullRemoteTimers();
        SyncService.PullRemoteCalendars();
        EventArchiveSyncService.PullArchivedModels();
        CourseArchiveSyncService.PullArchivedModels();
        await Task.WhenAll(SyncService.SyncUserSettings(), this.PullNotificationCount(), LimitCache.InitLimitCache(), AvatarHelper.GetAllProjectAvatars(), SyncService.SyncPomoSettings(), SyncService.TryPullHabitCheckIns(), ScheduleService.GetRemoteSchedules(), EventArchiveSyncService.PullArchivedModelsAsync());
      }
      this._pullingOthers = false;
    }

    private static async Task TryPullHabitCheckIns()
    {
      if (await UserDao.GetUserSyncCheckPoint(LocalSettings.Settings.LoginUserId) <= 0L)
        return;
      await HabitSyncService.PullHabits(91);
      await HabitSectionsSyncService.PullHabitSections();
    }

    private static async Task<bool> PullRemoteTeams() => await TeamService.PullTeams();

    private static async Task SyncUserSettings()
    {
      await HabitService.PullRemoteConfig().ContinueWith(new Action<Task>(UtilRun.LogTask));
      await SettingsHelper.PullRemoteSettings().ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    private static async Task PullRemoteCalendars()
    {
      try
      {
        await CalendarService.PullSubscribeCalendars();
        await CalendarService.PullAccountCalendarsAndEvents();
        CalendarService.CheckExpiredAccounts();
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
      App.ReloadProjects();
    }

    private static async Task SyncPomoSettings()
    {
      int num = await PomoSettings.LoadRemoteSettings() ? 1 : 0;
    }

    private async Task PullNotificationCount()
    {
      NotificationUnreadCount notificationCount = await Communicator.GetNotificationCount();
      this._syncResult.NotificationCount = notificationCount == null || notificationCount.Activity <= 0 && notificationCount.Notification <= 0 ? new NotificationUnreadCount(0, 0) : notificationCount;
    }

    public void InitNotification() => this._syncResult.InitNotification();

    private async Task Commit(int type)
    {
      SyncService.SetSyncPoint();
      try
      {
        await ProjectBatchHandler.CommitToServer(false, this._syncLog);
        await ProjectGroupBatchHandler.CommitToServer(this._syncLog);
        await this.CommitMoveColumnProject(true);
        await this.CommitMoveOrRestoreProject(2);
        await this.CommitMoveOrRestoreProject(7);
        await ProjectBatchHandler.CommitToServer(true, this._syncLog);
        await ColumnBatchHandler.CommitToServer(this._syncLog);
        EventArchiveSyncService.PushLocalArchivedModels();
        CourseArchiveSyncService.PushLocalArchivedModels();
        ReminderDelayService.UploadDelayModels();
        await this.CommitFilter();
        await this.CommitTag();
        await this.CommitTask();
        await this.CommitAttachment();
        await SyncService.CommitTemplate();
        await this.CommitClearTrash();
        await SyncService.CommitTaskOrder();
        await this.CommitSyncOrder();
        await this.CommitTaskParent();
        await SyncService.CommitPomodoros(type);
        await SyncService.CommitEvents();
        await TimerSyncService.PostTimers();
        await HabitSyncService.CommitHabitAndCheckIns();
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
    }

    private async Task CommitAttachment()
    {
      List<AttachmentModel> updateAttachments = await AttachmentDao.GetNeedUpdateAttachments();
      SyncTaskBean addBean;
      SyncTaskBean updateBean;
      if (updateAttachments == null)
      {
        addBean = (SyncTaskBean) null;
        updateBean = (SyncTaskBean) null;
      }
      else if (updateAttachments.Count == 0)
      {
        addBean = (SyncTaskBean) null;
        updateBean = (SyncTaskBean) null;
      }
      else
      {
        Dictionary<string, List<AttachmentModel>> dictionary = new Dictionary<string, List<AttachmentModel>>();
        foreach (AttachmentModel attachmentModel in updateAttachments)
        {
          if (!string.IsNullOrEmpty(attachmentModel.taskId))
          {
            if (dictionary.ContainsKey(attachmentModel.taskId))
              dictionary[attachmentModel.taskId].Add(attachmentModel);
            else
              dictionary[attachmentModel.taskId] = new List<AttachmentModel>()
              {
                attachmentModel
              };
          }
        }
        addBean = new SyncTaskBean();
        updateBean = new SyncTaskBean();
        foreach (KeyValuePair<string, List<AttachmentModel>> keyValuePair in dictionary)
        {
          KeyValuePair<string, List<AttachmentModel>> taskAttachments = keyValuePair;
          TaskModel taskById = await TaskDao.GetTaskById(taskAttachments.Key);
          if (taskById != null && taskById.deleted == 0)
          {
            AttachmentModel[] array1 = taskAttachments.Value.Where<AttachmentModel>((Func<AttachmentModel, bool>) (a => a.id != a.refId && a.sync_status == 0.ToString())).ToArray<AttachmentModel>();
            AttachmentModel[] array2 = taskAttachments.Value.Where<AttachmentModel>((Func<AttachmentModel, bool>) (a => a.sync_status == 1.ToString())).ToArray<AttachmentModel>();
            if (array1.Length != 0)
              addBean.AddAttachments.Add(new TaskModel()
              {
                Attachments = array1,
                id = taskById.id,
                projectId = taskById.projectId
              });
            if (array2.Length != 0)
              updateBean.UpdateAttachments.Add(new TaskModel()
              {
                Attachments = array2,
                id = taskById.id,
                projectId = taskById.projectId
              });
          }
          taskAttachments = new KeyValuePair<string, List<AttachmentModel>>();
        }
        if (addBean.AddAttachments.Count > 0)
          await AttachmentDao.HandleCommitResult(addBean.AddAttachments, await Communicator.BatchUpdateTask(addBean));
        if (updateBean.UpdateAttachments.Count <= 0)
        {
          addBean = (SyncTaskBean) null;
          updateBean = (SyncTaskBean) null;
        }
        else
        {
          await AttachmentDao.HandleCommitResult(updateBean.UpdateAttachments, await Communicator.BatchUpdateTask(updateBean));
          addBean = (SyncTaskBean) null;
          updateBean = (SyncTaskBean) null;
        }
      }
    }

    private async Task CommitTaskParent()
    {
      List<string> addIds = (await SyncStatusDao.GetSyncStatusByType(4)).Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (s => s.EntityId)).ToList<string>();
      List<string> undoIds = CloseUndoHandler.GetAffectedTaskIds();
      List<SyncStatusModel> syncStatus = (await SyncStatusDao.GetSyncStatusByType(16)).Where<SyncStatusModel>((Func<SyncStatusModel, bool>) (s => !addIds.Contains(s.EntityId) && !undoIds.Contains(s.EntityId))).ToList<SyncStatusModel>();
      if (syncStatus.Count <= 0)
      {
        syncStatus = (List<SyncStatusModel>) null;
      }
      else
      {
        List<SyncTaskParentModel> taskParentModels = new List<SyncTaskParentModel>();
        foreach (SyncStatusModel status in syncStatus)
        {
          TaskModel task = await TaskDao.GetThinTaskById(status.EntityId);
          if (task != null)
          {
            if (task.parentId == task.id)
            {
              task.parentId = "";
              await TaskService.UpdateTaskParent(task);
            }
            if (task.deleted == 0 && Utils.IsEqualString(task.parentId, status.OldParentId))
            {
              int num = await SyncStatusDao.DeleteSyncStatus(status.EntityId, status.Type) ? 1 : 0;
              continue;
            }
            taskParentModels.Add(new SyncTaskParentModel()
            {
              taskId = task.id,
              parentId = task.deleted != 0 ? "" : task.parentId,
              projectId = task.projectId,
              oldParentId = status.OldParentId
            });
          }
          else
          {
            int num1 = await SyncStatusDao.DeleteSyncStatus(status.EntityId, status.Type) ? 1 : 0;
          }
          task = (TaskModel) null;
        }
        if (taskParentModels.Count > 0)
        {
          await ProjectBatchHandler.CommitToServer(true, this._syncLog);
          SyncTaskParentRes result = await Communicator.BatchPushTaskParent(taskParentModels);
          List<SyncStatusModel> extraStatus = (await SyncStatusDao.GetSyncStatusByType(16)).Where<SyncStatusModel>((Func<SyncStatusModel, bool>) (s => !taskParentModels.Exists((Predicate<SyncTaskParentModel>) (model => model.taskId == s.EntityId)))).ToList<SyncStatusModel>();
          Dictionary<string, SyncTaskParentEtag> id2etag = result.Id2etag;
          // ISSUE: explicit non-virtual call
          if ((id2etag != null ? (__nonvirtual (id2etag.Count) > 0 ? 1 : 0) : 0) != 0)
          {
            foreach (KeyValuePair<string, SyncTaskParentEtag> keyValuePair in result.Id2etag)
            {
              KeyValuePair<string, SyncTaskParentEtag> kv = keyValuePair;
              TaskModel task = await TaskDao.GetTaskById(kv.Key);
              if (task != null && !extraStatus.Exists((Predicate<SyncStatusModel>) (s => s.EntityId == task.id)))
              {
                task.etag = kv.Value.etag;
                task.parentId = kv.Value.parentId;
                TaskModel taskModel = task;
                string[] childIds = kv.Value.childIds;
                string str = (childIds != null ? (childIds.Length != 0 ? 1 : 0) : 0) != 0 ? JsonConvert.SerializeObject((object) kv.Value.childIds) : (string) null;
                taskModel.childrenString = str;
                await TaskService.UpdateTaskParent(task);
              }
              kv = new KeyValuePair<string, SyncTaskParentEtag>();
            }
            foreach (SyncStatusModel syncStatusModel in syncStatus.Where<SyncStatusModel>((Func<SyncStatusModel, bool>) (status => result.Id2etag.ContainsKey(status.EntityId))))
            {
              int num = await SyncStatusDao.DeleteSyncStatus(syncStatusModel.EntityId, syncStatusModel.Type) ? 1 : 0;
            }
          }
          Dictionary<string, string> id2error = result.Id2error;
          // ISSUE: explicit non-virtual call
          if ((id2error != null ? (__nonvirtual (id2error.Count) > 0 ? 1 : 0) : 0) != 0)
            syncStatus.ForEach((Action<SyncStatusModel>) (status =>
            {
              if (!result.Id2error.ContainsKey(status.EntityId))
                return;
              switch (result.Id2error[status.EntityId])
              {
                case "NO_PROJECT_PERMISSION":
                  App.Connection.DeleteAsync((object) status);
                  break;
                case "EXISTED":
                  App.Connection.DeleteAsync((object) status);
                  break;
                case "NOT_EXISTED":
                  App.Connection.DeleteAsync((object) status);
                  break;
              }
            }));
          extraStatus = (List<SyncStatusModel>) null;
        }
        syncStatus = (List<SyncStatusModel>) null;
      }
    }

    private static async Task CommitTemplate()
    {
      List<SyncStatusModel> localStatus = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (m => m.Type >= 13 && m.Type <= 15 && m.UserId == LocalSettings.Settings.LoginUserId)).ToListAsync();
      List<TaskTemplateModel> localTemplate = await TemplateDao.GetLocalTemplate();
      List<SyncStatusModel> syncStatusModelList = localStatus;
      // ISSUE: explicit non-virtual call
      if ((syncStatusModelList != null ? (__nonvirtual (syncStatusModelList.Count) > 0 ? 1 : 0) : 0) == 0)
      {
        localStatus = (List<SyncStatusModel>) null;
      }
      else
      {
        TaskTemplateSyncBean syncBean = new TaskTemplateSyncBean();
        TaskTemplateSyncBean noteTemplateSyncBean = new TaskTemplateSyncBean();
        foreach (SyncStatusModel syncStatusModel in localStatus)
        {
          SyncStatusModel status = syncStatusModel;
          TaskTemplateModel taskTemplateModel = localTemplate.FirstOrDefault<TaskTemplateModel>((Func<TaskTemplateModel, bool>) (t => t.Id == status.EntityId));
          TemplateKind templateKind;
          if (taskTemplateModel != null)
          {
            switch (status.Type)
            {
              case 13:
                string kind1 = taskTemplateModel.Kind;
                templateKind = TemplateKind.Note;
                string str1 = templateKind.ToString();
                if (kind1 == str1)
                {
                  noteTemplateSyncBean.Add.Add(taskTemplateModel);
                  continue;
                }
                syncBean.Add.Add(taskTemplateModel);
                continue;
              case 14:
                string kind2 = taskTemplateModel.Kind;
                templateKind = TemplateKind.Note;
                string str2 = templateKind.ToString();
                if (kind2 == str2)
                {
                  noteTemplateSyncBean.Update.Add(taskTemplateModel);
                  continue;
                }
                syncBean.Update.Add(taskTemplateModel);
                continue;
              case 15:
                string kind3 = taskTemplateModel.Kind;
                templateKind = TemplateKind.Note;
                string str3 = templateKind.ToString();
                if (kind3 == str3)
                {
                  noteTemplateSyncBean.Delete.Add(taskTemplateModel.Id);
                  continue;
                }
                syncBean.Delete.Add(taskTemplateModel.Id);
                continue;
              default:
                continue;
            }
          }
        }
        BatchUpdateResult result;
        if (!syncBean.IsEmpty())
        {
          result = await Communicator.PushTemplates(syncBean);
          if (result != null)
          {
            foreach (SyncStatusModel syncStatusModel in localStatus)
            {
              if (result.Id2etag.Keys.Contains<string>(syncStatusModel.EntityId))
              {
                int num = await App.Connection.DeleteAsync((object) syncStatusModel);
              }
            }
          }
          result = (BatchUpdateResult) null;
        }
        if (!noteTemplateSyncBean.IsEmpty())
        {
          result = await Communicator.PushTemplates(noteTemplateSyncBean, true);
          if (result != null)
          {
            foreach (SyncStatusModel syncStatusModel in localStatus)
            {
              if (result.Id2etag.Keys.Contains<string>(syncStatusModel.EntityId))
              {
                int num = await App.Connection.DeleteAsync((object) syncStatusModel);
              }
            }
          }
          result = (BatchUpdateResult) null;
        }
        noteTemplateSyncBean = (TaskTemplateSyncBean) null;
        localStatus = (List<SyncStatusModel>) null;
      }
    }

    private static async Task CommitTaskOrder()
    {
      SyncTaskOrderBean commitBean = await TaskOrderBatchHandler.CreateCommitBean();
      if (SyncService.InvalidSyncTaskOrderBean(commitBean))
        return;
      long lastPostPoint = DateTime.Now.Ticks;
      BatchTaskOrderUpdateResult result = await Communicator.BatchUpdateTaskOrder(commitBean);
      if (result == null)
        return;
      await TaskOrderBatchHandler.HandleCommitResult(result, lastPostPoint);
    }

    private async Task CommitSyncOrder()
    {
      SyncOrderBean syncOrderBean = await SyncOrderBatchHandler.CreateCommitBean();
      if (syncOrderBean.OrderByType == null)
        syncOrderBean = (SyncOrderBean) null;
      else if (syncOrderBean.OrderByType.Count <= 0)
      {
        syncOrderBean = (SyncOrderBean) null;
      }
      else
      {
        SyncOrderBean taskOrderBean = syncOrderBean.GetTaskOrderBean();
        if (taskOrderBean.OrderByType.Count > 0)
        {
          BatchSyncOrderUpdateResult result = await Communicator.BatchUpdateTaskSyncOrder(taskOrderBean);
          if (result != null)
            await SyncOrderBatchHandler.HandleCommitResult(result);
        }
        SyncOrderBean otherOrderBean = syncOrderBean.GetOtherOrderBean();
        if (otherOrderBean.OrderByType.Count <= 0)
        {
          syncOrderBean = (SyncOrderBean) null;
        }
        else
        {
          BatchSyncOrderUpdateResult result = await Communicator.BatchUpdateSyncOrder(otherOrderBean);
          if (result == null)
          {
            syncOrderBean = (SyncOrderBean) null;
          }
          else
          {
            await SyncOrderBatchHandler.HandleCommitResult(result);
            syncOrderBean = (SyncOrderBean) null;
          }
        }
      }
    }

    private static bool InvalidSyncTaskOrderBean(SyncTaskOrderBean taskOrderBean)
    {
      if (taskOrderBean?.taskOrderByDate != null && taskOrderBean.taskOrderByDate.Count != 0 || taskOrderBean?.taskOrderByPriority != null && taskOrderBean.taskOrderByPriority.Count != 0)
        return false;
      return taskOrderBean?.taskOrderByProject == null || taskOrderBean.taskOrderByProject.Count == 0;
    }

    private async Task Pull(int type)
    {
      long lastCheckPoint = await UserDao.GetUserSyncCheckPoint(LocalSettings.Settings.LoginUserId);
      if (lastCheckPoint < 0L)
        lastCheckPoint = 0L;
      LogModel syncLog1 = this._syncLog;
      syncLog1.Log = syncLog1.Log + "\r\n\t" + TicketLogUtils.GetLogString("big sync: will batch check \\" + lastCheckPoint.ToString());
      SyncBean syncBean = await Communicator.BatchCheck(lastCheckPoint);
      if (syncBean != null && !syncBean.IsEmpty())
      {
        this._syncLog.Log += "\r\n\tbig sync: will handle batch check result";
        try
        {
          await this.TryPullRemoteTeams(type, lastCheckPoint == 0L);
          await this._projectGroupBatchHandler.MergeWithServer((Collection<ProjectGroupModel>) syncBean.projectGroups, this._syncLog);
          await this._projectBatchHandler.MergeWithServer((Collection<ProjectModel>) syncBean.projectProfiles, this._syncLog);
          await this._filterBatchHandler.MergeWithServer((Collection<FilterModel>) syncBean.filters, this._syncLog);
          await this._tagBatchHandler.MergeWithServer(syncBean.tags, this._syncLog);
          await this._taskBatchHandler.MergeWithServer(syncBean.syncTaskBean, type, this._syncLog);
          await this._sortOrderBatchHandler.MergeWithServer(syncBean.syncTaskOrderBean);
          await this._syncOrderBatchHandler.MergeWithServer(syncBean.syncOrderBean);
          await this._syncOrderBatchHandler.MergeWithServer(syncBean.syncOrderBeanV3);
          await SyncService.TryPullRemoteHabits(lastCheckPoint == 0L);
          await TaskBatchHandler.HandleRepeatCompletedTasks(syncBean.syncTaskBean);
          await ReminderDelayService.MergeRemoteDelayModels(syncBean.remindChange);
          await SyncService.SaveCheckPoint(syncBean.checkPoint);
          await this.SaveChecks(syncBean.checks);
        }
        catch (Exception ex)
        {
          LogModel syncLog2 = this._syncLog;
          syncLog2.Log = syncLog2.Log + "\r\n\t" + ExceptionUtils.BuildExceptionMessage(ex);
        }
        this._syncLog.Log += "\r\n\tbig sync: did handle batch check result ";
      }
      else if (syncBean != null)
        await SyncService.SaveCheckPoint(syncBean.checkPoint);
      if (lastCheckPoint != 0L)
      {
        syncBean = (SyncBean) null;
      }
      else
      {
        ClosedListSyncService.PullCompletedTaskAtFirstLogin();
        syncBean = (SyncBean) null;
      }
    }

    private async Task SaveChecks(string checks)
    {
      if (string.IsNullOrEmpty(checks))
        return;
      try
      {
        JToken jtoken1;
        if (!JObject.Parse(checks).TryGetValue(nameof (checks), out jtoken1) || !(jtoken1 is JArray jarray))
          return;
        foreach (JToken jtoken2 in jarray)
        {
          JToken jtoken3;
          if (jtoken2 is JObject jobject && jobject.TryGetValue("action", out jtoken3) && jtoken3?.ToString() == "task.move")
          {
            JToken jtoken4;
            if (jobject.TryGetValue("data", out jtoken4))
            {
              try
              {
                MoveCheckModel moveCheckModel = JsonConvert.DeserializeObject<MoveCheckModel>(jtoken4.ToString());
                if (moveCheckModel != null)
                {
                  TaskBaseViewModel taskById = TaskCache.GetTaskById(moveCheckModel.id);
                  if (taskById != null)
                  {
                    if (taskById.ProjectId != moveCheckModel.projectId)
                    {
                      ProjectModel projectById = CacheManager.GetProjectById(moveCheckModel.projectId);
                      if (projectById != null)
                      {
                        if (!projectById.IsDeleted)
                          continue;
                      }
                      TaskDao.DeleteTaskInDb(moveCheckModel.id);
                    }
                  }
                }
              }
              catch (Exception ex)
              {
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    public static async Task TryPullRemoteHabits(bool first)
    {
      if (!first)
        return;
      await HabitSyncService.SyncHabitsOnLogin();
      HabitSectionsSyncService.SyncSections();
    }

    private async Task TryPullRemoteTeams(int type, bool first)
    {
      this._syncResult.TeamChanged = await SyncService.PullRemoteTeams();
    }

    public async Task PullProjectTasks(string projectId)
    {
      if (SyncService.TaskPullingProjectIds.Contains(projectId))
        return;
      SyncService.TaskPullingProjectIds.Add(projectId);
      try
      {
        Dictionary<string, ObservableCollection<TaskModel>> serverTasksDict = new Dictionary<string, ObservableCollection<TaskModel>>();
        ObservableCollection<TaskModel> observableCollection = await Communicator.PullServerTasksByProjectId(projectId);
        if (observableCollection == null)
          return;
        serverTasksDict.Add(projectId, observableCollection);
        await this._taskBatchHandler.MergeTasksOfOpenedProjects(serverTasksDict);
        serverTasksDict = (Dictionary<string, ObservableCollection<TaskModel>>) null;
      }
      finally
      {
        SyncService.TaskPullingProjectIds.Remove(projectId);
      }
    }

    private static async Task SaveCheckPoint(long checkPoint)
    {
      await UserDao.SaverUserSyncCheckPoint(LocalSettings.Settings.LoginUserId, checkPoint);
    }

    private static async Task CommitPomodoros(int type)
    {
      await FocusOptionUploader.SaveOption((List<FocusOptionModel>) null, type == 1, true);
      await PomoSyncService.CommitPomodoros();
    }

    private async Task CommitMoveOrRestoreProject(int moveOrRestore)
    {
      List<MoveOrRestoreProject> moveOrRestoreProjects = await MoveOrRestoreProjectBatchHandler.GetCommits(moveOrRestore);
      moveOrRestoreProjects?.RemoveAll((Predicate<MoveOrRestoreProject>) (m => UndoHelper.CanUndoIds.Contains(m.TaskId) || UndoHelper.UndoingIds.Contains(m.TaskId)));
      if (moveOrRestoreProjects == null)
        moveOrRestoreProjects = (List<MoveOrRestoreProject>) null;
      else if (moveOrRestoreProjects.Count <= 0)
      {
        moveOrRestoreProjects = (List<MoveOrRestoreProject>) null;
      }
      else
      {
        BatchUpdateResult result;
        if (moveOrRestore == 2)
        {
          result = await Communicator.BatchUpdateTaskProject(moveOrRestoreProjects);
          LogModel syncLog = this._syncLog;
          syncLog.Log = syncLog.Log + "\r\n" + string.Format("big sync:  move task count : {0} , success {1}", (object) moveOrRestoreProjects.Count, (object) result?.Id2etag?.Count);
        }
        else
        {
          result = await Communicator.BatchRestoreTask(moveOrRestoreProjects);
          LogModel syncLog = this._syncLog;
          syncLog.Log = syncLog.Log + "\r\n" + string.Format("big sync:  restore task count : {0} , success {1}", (object) moveOrRestoreProjects.Count, (object) result?.Id2etag?.Count);
        }
        if (result == null)
        {
          moveOrRestoreProjects = (List<MoveOrRestoreProject>) null;
        }
        else
        {
          MoveOrRestoreProjectBatchHandler.HandleCommitResult(result, moveOrRestore);
          moveOrRestoreProjects = (List<MoveOrRestoreProject>) null;
        }
      }
    }

    private async Task CommitTag()
    {
      BatchUpdateResult batchUpdateResult;
      do
      {
        SyncTagBean server = await TagBatchHandler.CommitToServer();
        if (server != null && !server.Empty)
        {
          LogModel syncLog = this._syncLog;
          syncLog.Log = syncLog.Log + "\r\n" + string.Format(" big sync: update tag count : a {0} u {1} d {2}", (object) server.Add.Count, (object) server.Update.Count, (object) server.Deleted.Count);
          batchUpdateResult = await Communicator.BatchUpdateTag(server);
          if (batchUpdateResult == null)
            goto label_2;
        }
        else
          goto label_6;
      }
      while (await TagBatchHandler.HandleCommitResult(batchUpdateResult.Id2etag, batchUpdateResult.Id2error, this._syncLog));
      goto label_8;
label_6:
      return;
label_2:
      return;
label_8:;
    }

    private async Task CommitClearTrash()
    {
      List<SyncStatusModel> clearTrashModels = await this._taskBatchHandler.GetNeedClearTrash();
      if (!clearTrashModels.Any<SyncStatusModel>())
        clearTrashModels = (List<SyncStatusModel>) null;
      else if (!Utils.IsNetworkAvailable())
      {
        clearTrashModels = (List<SyncStatusModel>) null;
      }
      else
      {
        await Communicator.ClearTrash();
        foreach (SyncStatusModel syncStatusModel in clearTrashModels)
        {
          int num = await SyncStatusDao.DeleteSyncStatus(syncStatusModel.EntityId, 8) ? 1 : 0;
        }
        clearTrashModels = (List<SyncStatusModel>) null;
      }
    }

    private async Task CommitFilter()
    {
      SyncFilterBean bean = await FilterBatchHandler.CommitToServer();
      if (bean == null)
        bean = (SyncFilterBean) null;
      else if (bean.Empty)
      {
        bean = (SyncFilterBean) null;
      }
      else
      {
        LogModel syncLog = this._syncLog;
        syncLog.Log = syncLog.Log + "\r\n" + string.Format(" big sync: update filter count : a {0} u {1} d {2}", (object) bean.Add.Count, (object) bean.Update.Count, (object) bean.Deleted.Count);
        BatchUpdateResult result = await Communicator.BatchUpdateFilter(bean);
        if (result == null)
        {
          bean = (SyncFilterBean) null;
        }
        else
        {
          await FilterBatchHandler.HandleCommitResult(result, bean.Deleted, this._syncLog);
          bean = (SyncFilterBean) null;
        }
      }
    }

    private static async Task CommitEvents()
    {
      List<SyncStatusModel> added = await EventBatchHandler.GetLocalAddedEvents();
      List<SyncStatusModel> updates = await EventBatchHandler.GetLocalModifiedEvents();
      List<SyncStatusModel> deleted = await EventBatchHandler.GetLocalDeletedEvents();
      List<SyncStatusModel> moved = await EventBatchHandler.GetLocalMoveEvents();
      if (!updates.Any<SyncStatusModel>() && !added.Any<SyncStatusModel>() && !deleted.Any<SyncStatusModel>() && !moved.Any<SyncStatusModel>())
      {
        added = (List<SyncStatusModel>) null;
        updates = (List<SyncStatusModel>) null;
        deleted = (List<SyncStatusModel>) null;
        moved = (List<SyncStatusModel>) null;
      }
      else
      {
        SyncEventBean bean = await EventTransfer.DescribeSyncTaskBean(added, updates, moved, deleted);
        SyncEventBean calDavBean = await EventTransfer.DescribeSyncTaskBean(added, updates, moved, deleted, true);
        if (bean.CalendarAccountBeans.Any<CalendarAccountBean>())
          await EventBatchHandler.HandleCommitResult(await Communicator.BatchUpdateCalendarEvents(bean), bean);
        if (calDavBean.CalendarAccountBeans.Any<CalendarAccountBean>())
          await EventBatchHandler.HandleCommitResult(await Communicator.BatchUpdateCalDavEvents(calDavBean), calDavBean, false);
        bean = (SyncEventBean) null;
        calDavBean = (SyncEventBean) null;
        added = (List<SyncStatusModel>) null;
        updates = (List<SyncStatusModel>) null;
        deleted = (List<SyncStatusModel>) null;
        moved = (List<SyncStatusModel>) null;
      }
    }

    private async Task CommitTask()
    {
      BlockingSet<string> undoTaskIds = new BlockingSet<string>();
      undoTaskIds.AddRange(UndoHelper.CanUndoIds);
      undoTaskIds.AddRange(UndoHelper.UndoingIds);
      undoTaskIds.AddRange((IEnumerable<string>) CloseUndoHandler.GetAffectedTaskIds());
      List<SyncStatusModel> createdTasks = (await this._taskBatchHandler.GetNeedPostCreatedTasks()).Where<SyncStatusModel>((Func<SyncStatusModel, bool>) (s => !undoTaskIds.Contains(s.EntityId))).ToList<SyncStatusModel>();
      List<SyncStatusModel> updatedTasks = (await this._taskBatchHandler.GetLocalContentUpdatedTasks()).Where<SyncStatusModel>((Func<SyncStatusModel, bool>) (s => !undoTaskIds.Contains(s.EntityId))).ToList<SyncStatusModel>();
      List<SyncStatusModel> deletedTasks = await this._taskBatchHandler.GetNeedPostDeletedTasks();
      List<SyncStatusModel> deleteForeverTasks = await this._taskBatchHandler.GetNeedPostDeleteForeverTasks();
      if (createdTasks.Count <= 0 && updatedTasks.Count <= 0 && deletedTasks.Count <= 0 && deleteForeverTasks.Count <= 0)
      {
        createdTasks = (List<SyncStatusModel>) null;
        updatedTasks = (List<SyncStatusModel>) null;
        deletedTasks = (List<SyncStatusModel>) null;
      }
      else
      {
        ReminderCalculator.AssembleReminders();
        this._syncLog.Log += string.Format("\r\n big sync: update task add:{0} updated:{1} ", (object) createdTasks.Count, (object) updatedTasks.Count);
        if (deletedTasks.Count > 0)
        {
          string str = deletedTasks.Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (t => t.EntityId)).Join<string>(";");
          LogModel syncLog = this._syncLog;
          syncLog.Log = syncLog.Log + "\r\n deleteIds : " + str;
        }
        foreach (SyncTaskBean taskBean in await TaskTransfer.DescribeSyncTaskBean(createdTasks, updatedTasks, deletedTasks, deleteForeverTasks))
        {
          if (taskBean.DeletedFromTrash.Count > 0)
          {
            string str = taskBean.DeletedFromTrash.Select<TaskProject, string>((Func<TaskProject, string>) (t => t.taskId)).Join<string>(";");
            LogModel syncLog = this._syncLog;
            syncLog.Log = syncLog.Log + "\r\n deleteForeverIds : " + str;
            await TaskBatchHandler.HandleCommitResult(taskBean, await Communicator.BatchDeleteTask(taskBean.DeletedFromTrash), this._syncLog);
          }
          if (!taskBean.Empty)
          {
            if (taskBean.Delete.Count > 0)
            {
              string str = taskBean.Delete.Select<TaskProject, string>((Func<TaskProject, string>) (t => t.taskId.Length <= 6 ? t.taskId : t.taskId.Substring(t.taskId.Length - 6, 6))).Join<string>(";");
              LogModel syncLog = this._syncLog;
              syncLog.Log = syncLog.Log + "\r\n deleteTaskIds : " + str;
            }
            await TaskBatchHandler.HandleCommitResult(taskBean, await Communicator.BatchUpdateTask(taskBean), this._syncLog);
          }
        }
        createdTasks = (List<SyncStatusModel>) null;
        updatedTasks = (List<SyncStatusModel>) null;
        deletedTasks = (List<SyncStatusModel>) null;
      }
    }

    private async Task CommitMoveColumnProject(bool checkReCommit)
    {
      List<MoveColumnArgs> columnMoveStatus = await MoveColumnProjectBatchHandler.GetCommits();
      if (columnMoveStatus == null)
        columnMoveStatus = (List<MoveColumnArgs>) null;
      else if (columnMoveStatus.Count <= 0)
      {
        columnMoveStatus = (List<MoveColumnArgs>) null;
      }
      else
      {
        BatchUpdateResult result = await Communicator.MoveColumnAsync(columnMoveStatus);
        LogModel syncLog = this._syncLog;
        syncLog.Log = syncLog.Log + "\r\n" + string.Format("big sync:  restore task count : {0} , success {1}", (object) columnMoveStatus.Count, (object) result?.Id2etag?.Count);
        if (result == null)
          columnMoveStatus = (List<MoveColumnArgs>) null;
        else if (!(await MoveColumnProjectBatchHandler.HandleCommitResult(result, columnMoveStatus) & checkReCommit))
        {
          columnMoveStatus = (List<MoveColumnArgs>) null;
        }
        else
        {
          await this.CommitMoveColumnProject(false);
          columnMoveStatus = (List<MoveColumnArgs>) null;
        }
      }
    }

    public void Clear()
    {
      this._projectGroupBatchHandler = (ProjectGroupBatchHandler) null;
      this._projectBatchHandler = (ProjectBatchHandler) null;
      this._taskBatchHandler = (TaskBatchHandler) null;
      this._sortOrderBatchHandler = (TaskOrderBatchHandler) null;
      this._syncOrderBatchHandler = (SyncOrderBatchHandler) null;
      this._filterBatchHandler = (FilterBatchHandler) null;
      this._tagBatchHandler = (TagBatchHandler) null;
      this._eventBatchHandler = (EventBatchHandler) null;
    }
  }
}
