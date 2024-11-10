// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.ReminderDelayService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  public static class ReminderDelayService
  {
    public static async Task UploadDelayModels()
    {
      List<ReminderDelayModel> models = await ReminderDelayDao.GetNeedSyncModelsAsync();
      models = models.Where<ReminderDelayModel>((Func<ReminderDelayModel, bool>) (m =>
      {
        DateTime? nextTime = m.NextTime;
        DateTime now = DateTime.Now;
        return nextTime.HasValue && nextTime.GetValueOrDefault() > now;
      })).ToList<ReminderDelayModel>();
      if (models.Count == 0)
      {
        models = (List<ReminderDelayModel>) null;
      }
      else
      {
        ReminderDelaySyncModel syncModel = new ReminderDelaySyncModel();
        foreach (ReminderDelayModel reminderDelayModel in models)
        {
          switch (reminderDelayModel.SyncStatus)
          {
            case -1:
              syncModel.delete.Add(reminderDelayModel.ObjId);
              continue;
            case 0:
              syncModel.add.Add(reminderDelayModel);
              continue;
            default:
              continue;
          }
        }
        if (await Communicator.UpdateReminderDelayModels(syncModel))
        {
          using (List<ReminderDelayModel>.Enumerator enumerator = models.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              ReminderDelayModel current = enumerator.Current;
              switch (current.SyncStatus)
              {
                case -1:
                  BaseDao<ReminderDelayModel>.DeleteAsync(current);
                  continue;
                case 0:
                  current.SyncStatus = 2;
                  BaseDao<ReminderDelayModel>.UpdateAsync(current);
                  continue;
                default:
                  continue;
              }
            }
            models = (List<ReminderDelayModel>) null;
          }
        }
        else
        {
          UtilLog.Info("UpdateReminderDelayModels error");
          models = (List<ReminderDelayModel>) null;
        }
      }
    }

    public static async Task MergeRemoteDelayModels(List<ReminderDelayModel> remotes)
    {
      Dictionary<string, ReminderDelayModel> localDict;
      if (remotes == null)
      {
        localDict = (Dictionary<string, ReminderDelayModel>) null;
      }
      else
      {
        List<ReminderDelayModel> allModelsAsync = await ReminderDelayDao.GetAllModelsAsync();
        localDict = new Dictionary<string, ReminderDelayModel>();
        foreach (ReminderDelayModel model in allModelsAsync)
        {
          DateTime? nextTime = model.NextTime;
          DateTime now = DateTime.Now;
          if ((nextTime.HasValue ? (nextTime.GetValueOrDefault() < now ? 1 : 0) : 0) != 0 || string.IsNullOrEmpty(model.ObjId) || localDict.ContainsKey(model.ObjId))
          {
            int num = await BaseDao<ReminderDelayModel>.DeleteAsync(model);
          }
          else
            localDict[model.ObjId] = model;
        }
        foreach (ReminderDelayModel remote in remotes)
        {
          if (!string.IsNullOrEmpty(remote.ObjId) && localDict.ContainsKey(remote.ObjId))
          {
            ReminderDelayModel model = localDict[remote.ObjId];
            localDict.Remove(remote.ObjId);
            DateTime? nullable1 = model.RemindTime;
            DateTime? nullable2 = remote.RemindTime;
            if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
            {
              nullable2 = model.NextTime;
              nullable1 = remote.NextTime;
              if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() == nullable1.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
                continue;
            }
            int num = await BaseDao<ReminderDelayModel>.DeleteAsync(model);
          }
          remote.SyncStatus = 2;
          remote.UserId = LocalSettings.Settings.LoginUserId;
          int num1 = await BaseDao<ReminderDelayModel>.InsertAsync(remote);
        }
        foreach (ReminderDelayModel model in localDict.Values)
        {
          if (model.SyncStatus == 2)
          {
            int num = await BaseDao<ReminderDelayModel>.DeleteAsync(model);
          }
        }
        localDict = (Dictionary<string, ReminderDelayModel>) null;
      }
    }
  }
}
