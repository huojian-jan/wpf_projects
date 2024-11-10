// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ProjectTransfer
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

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class ProjectTransfer
  {
    public static async Task<SyncProjectBean> DescribeSyncProjectBean(
      Dictionary<string, string> localChanges)
    {
      SyncProjectBean syncBean = new SyncProjectBean();
      if (localChanges != null && localChanges.Count > 0)
      {
        List<string> teamIds = CacheManager.GetTeams().Select<TeamModel, string>((Func<TeamModel, string>) (t => t.id)).ToList<string>();
        List<ProjectModel> list;
        if (!UserDao.IsPro())
          list = (await ProjectDao.GetAllProjects(false)).ToList<ProjectModel>();
        else
          list = (await ProjectDao.GetAllProjects(false, false)).Where<ProjectModel>((Func<ProjectModel, bool>) (p => !TeamDao.IsTeamExpired(p.teamId))).ToList<ProjectModel>();
        int count = list.Where<ProjectModel>((Func<ProjectModel, bool>) (p =>
        {
          if (string.IsNullOrEmpty(p.teamId) || teamIds.Contains(p.teamId))
          {
            string syncStatus1 = p.sync_status;
            Constants.SyncStatus syncStatus2 = Constants.SyncStatus.SYNC_ERROR_UP_LIMIT;
            string str1 = syncStatus2.ToString();
            if (syncStatus1 != str1)
            {
              string syncStatus3 = p.sync_status;
              syncStatus2 = Constants.SyncStatus.SYNC_NEW;
              string str2 = syncStatus2.ToString();
              return syncStatus3 != str2;
            }
          }
          return false;
        })).ToList<ProjectModel>().Count;
        long canAddProjectCount = Utils.GetUserLimit(Constants.LimitKind.ProjectNumber) - (long) count;
        foreach (KeyValuePair<string, string> localChange in localChanges)
        {
          KeyValuePair<string, string> local = localChange;
          if (!local.Key.Contains("inbox"))
          {
            ProjectModel projectById = await ProjectDao.GetProjectById(local.Key);
            if (projectById != null)
            {
              if (local.Value == Constants.SyncStatus.SYNC_NEW.ToString() || local.Value == Constants.SyncStatus.SYNC_ERROR_UP_LIMIT.ToString())
              {
                if (canAddProjectCount > 0L)
                {
                  syncBean.Add.Add(projectById);
                  --canAddProjectCount;
                }
                else if (local.Value == Constants.SyncStatus.SYNC_NEW.ToString())
                {
                  projectById.sync_status = Constants.SyncStatus.SYNC_ERROR_UP_LIMIT.ToString();
                  int num = await App.Connection.UpdateAsync((object) projectById);
                }
              }
              else if (local.Value == Constants.SyncStatus.SYNC_UPDATE.ToString() || local.Value == Constants.SyncStatus.SYNC_INIT.ToString())
              {
                if (projectById.delete_status)
                  syncBean.Deleted.Add(local.Key);
                else
                  syncBean.Update.Add(projectById);
              }
            }
            local = new KeyValuePair<string, string>();
          }
        }
      }
      SyncProjectBean syncProjectBean = syncBean;
      syncBean = (SyncProjectBean) null;
      return syncProjectBean;
    }
  }
}
