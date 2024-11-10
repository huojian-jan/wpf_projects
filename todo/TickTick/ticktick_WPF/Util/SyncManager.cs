// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.SyncManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.Views.Pomo;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public class SyncManager
  {
    private static readonly SyncService SyncService;
    private static string _delayUid = Utils.GetGuid();

    static SyncManager()
    {
      SyncManager.SyncService = new SyncService();
      SyncManager.SyncService.Init();
      SyncManager.SyncService.SyncFinished -= new EventHandler<SyncResult>(SyncManager.OnSyncFinished);
      SyncManager.SyncService.SyncFinished += new EventHandler<SyncResult>(SyncManager.OnSyncFinished);
      NetworkChange.NetworkAvailabilityChanged -= new NetworkAvailabilityChangedEventHandler(SyncManager.OnNetWorkChanged);
      NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(SyncManager.OnNetWorkChanged);
    }

    private static void OnNetWorkChanged(object sender, NetworkAvailabilityEventArgs e)
    {
      FocusOptionUploader.SaveOption((List<FocusOptionModel>) null, true, true);
    }

    private static void OnSyncFinished(object sender, SyncResult e)
    {
      ThreadUtil.DetachedRunOnUiThread((Action) (async () => MainWindowManager.HandleSyncFinished(e)));
    }

    public static async Task PullProjectTasks(string projectId)
    {
      await SyncManager.SyncService.PullProjectTasks(projectId);
    }

    public static async Task Sync(int type = 0)
    {
      if (string.IsNullOrEmpty(LocalSettings.Settings.LoginUserId))
        return;
      DateTime today;
      if (type == 0)
      {
        string lastCheckSyncDate = LocalSettings.Settings.LastCheckSyncDate;
        today = DateTime.Today;
        if (today.ToString("yyyyMMdd") != lastCheckSyncDate)
          type = 1;
      }
      if (type == 1)
      {
        LocalSettings settings = LocalSettings.Settings;
        today = DateTime.Today;
        string str = today.ToString("yyyyMMdd");
        settings.LastCheckSyncDate = str;
      }
      MainWindowManager.BeginSyncStory();
      Task.Run((Action) (() => SyncManager.SyncService.DoSync(type)));
    }

    public static void TryDelaySync(int delay = 4000)
    {
      DelayActionHandlerCenter.TryDoAction(SyncManager._delayUid, (EventHandler) ((o, e) => SyncManager.Sync()), delay);
    }

    public static void Clear() => SyncManager.SyncService.Clear();

    public static async void Init()
    {
      SyncManager.SyncService.Init();
      await Task.Delay(1000);
      SyncManager.Sync(1);
      WebSocketService.InitAsync().ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    private static async void LogUnSyncData()
    {
      StringBuilder builder = new StringBuilder();
      bool project = await ProjectDao.CheckUnSyncProjects();
      bool projectGroup = await ProjectGroupDao.CheckUnSyncItem();
      bool sync = await SyncStatusDao.CheckUnSyncItem();
      builder.Append(string.Format("\nproject sync {0}, group sync {1}, task sync {2}\n", (object) project, (object) projectGroup, (object) sync));
      if (!project)
      {
        Dictionary<string, string> needPostProject = await ProjectDao.GetNeedPostProject();
        builder.Append("------------------\n");
        builder.Append("project sync failed\n");
        if (needPostProject != null && needPostProject.Count > 0)
        {
          foreach (KeyValuePair<string, string> keyValuePair in needPostProject)
          {
            KeyValuePair<string, string> p = keyValuePair;
            ProjectModel projectById = await ProjectDao.GetProjectById(p.Key);
            builder.Append("project_id:" + p.Key + ", project_name:" + projectById?.name + ", status:" + p.Value + "\n");
            p = new KeyValuePair<string, string>();
          }
        }
      }
      if (!projectGroup)
      {
        builder.Append("------------------\n");
        builder.Append("group sync failed\n");
        List<ProjectGroupModel> postProjectGroup = await ProjectGroupDao.GetNeedPostProjectGroup();
        if (postProjectGroup != null && postProjectGroup.Count > 0)
        {
          foreach (ProjectGroupModel projectGroupModel in postProjectGroup)
            builder.Append("group_id:" + projectGroupModel.id + ", group_name:" + projectGroupModel.name + ", status:" + projectGroupModel.sync_status + "\n");
        }
      }
      if (sync)
      {
        builder = (StringBuilder) null;
      }
      else
      {
        builder.Append("------------------\n");
        builder.Append("task sync failed\n");
        List<SyncStatusModel> allSyncStatus = await SyncStatusDao.GetAllSyncStatus();
        if (allSyncStatus == null)
          builder = (StringBuilder) null;
        else if (allSyncStatus.Count <= 0)
        {
          builder = (StringBuilder) null;
        }
        else
        {
          foreach (SyncStatusModel model in allSyncStatus)
          {
            TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.EntityId);
            builder.Append(string.Format("task_id:{0}, task_name:{1}, status:{2}\n", (object) model.EntityId, thinTaskById == null ? (object) "null" : (object) thinTaskById.title, (object) model.Type));
          }
          builder = (StringBuilder) null;
        }
      }
    }
  }
}
