// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.HabitSectionsSyncService
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
using ticktick_WPF.Resource;
using ticktick_WPF.Service;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  internal class HabitSectionsSyncService
  {
    private static readonly SemaphoreLocker SyncLock = new SemaphoreLocker();
    private static bool _isWaitingUpdate = false;

    public static void SyncSections()
    {
      HabitSectionsSyncService.SyncLock.LockAsync((Func<Task>) (async () => HabitSectionsSyncService.PullHabitSections()));
    }

    public static async Task PullHabitSections()
    {
      if (!LocalSettings.Settings.ShowHabit)
        return;
      bool changed = await HabitService.PullAndMergeRemoteHabitSections();
      await HabitSectionsSyncService.BatchUpdateHabitSections();
      int num = changed ? 1 : 0;
      await HabitSectionCache.SetSections();
    }

    public static async Task BatchUpdateHabitSections()
    {
      if (HabitSectionsSyncService._isWaitingUpdate)
        return;
      HabitSectionsSyncService._isWaitingUpdate = true;
      await Task.Delay(500);
      List<HabitSectionModel> postHabitSection = await HabitSectionDao.GetNeedPostHabitSection();
      HabitSectionsSyncService._isWaitingUpdate = false;
      if (!postHabitSection.Any<HabitSectionModel>())
        return;
      SyncHabitSectionsBean bean = new SyncHabitSectionsBean();
      foreach (HabitSectionModel habitSectionModel in postHabitSection)
      {
        switch (habitSectionModel.SyncStatus)
        {
          case -1:
            bean.Delete.Add(habitSectionModel.Id);
            continue;
          case 0:
            bean.Add.Add(habitSectionModel);
            continue;
          case 1:
            bean.Update.Add(habitSectionModel);
            continue;
          default:
            continue;
        }
      }
      BatchUpdateResult model = await Communicator.UpdateHabitSections(bean);
      if (model != null)
      {
        try
        {
          foreach (string key in model.Id2etag.Keys)
          {
            string etag = model.Id2etag[key];
            await HabitService.SaveHabitSectionEtag(key, etag);
          }
          Collection<string> delete = bean.Delete;
          await HabitSectionDao.DeleteAllAsync(delete != null ? delete.ToList<string>() : (List<string>) null);
        }
        catch (Exception ex)
        {
        }
      }
      bean = (SyncHabitSectionsBean) null;
      model = (BatchUpdateResult) null;
    }
  }
}
