// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ClosedLoader.CompletionLoadStatusDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util.Sync.ClosedLoader
{
  public static class CompletionLoadStatusDao
  {
    private static ClosedLoadStatus _allStatus;

    public static async Task<ClosedLoadStatus> GetLoadStatusByEntityId(string entityId)
    {
      return entityId == "all" ? await CompletionLoadStatusDao.GetStatusInAllList() : await CompletionLoadStatusDao.GetStatusByEntityId(entityId);
    }

    private static async Task<ClosedLoadStatus> GetStatusByEntityId(string entityId)
    {
      ClosedLoadStatus status = await App.Connection.Table<ClosedLoadStatus>().Where((Expression<Func<ClosedLoadStatus, bool>>) (s => s.UserId == LocalSettings.Settings.LoginUserId && s.EntityId == entityId)).FirstOrDefaultAsync();
      if (status == null)
      {
        status = new ClosedLoadStatus()
        {
          EntityId = entityId,
          EarliestPulledDate = DateTime.UtcNow,
          IsAllLoaded = false,
          IsNew = true,
          UserId = LocalSettings.Settings.LoginUserId
        };
        int num = await App.Connection.InsertAsync((object) status);
      }
      status.EarliestPulledDate = DateUtils.GetLocalTime(status.EarliestPulledDate);
      ClosedLoadStatus statusByEntityId = status;
      status = (ClosedLoadStatus) null;
      return statusByEntityId;
    }

    public static async Task SaveCompletionLoadStatus(ClosedLoadStatus status)
    {
      status.EarliestPulledDate = status.EarliestPulledDate.ToUniversalTime();
      if (status.IsNew)
      {
        ClosedLoadStatus closedLoadStatus = await App.Connection.Table<ClosedLoadStatus>().Where((Expression<Func<ClosedLoadStatus, bool>>) (s => s.UserId == LocalSettings.Settings.LoginUserId && s.EntityId == status.EntityId)).FirstOrDefaultAsync();
        if (closedLoadStatus == null)
        {
          int num1 = await App.Connection.InsertAsync((object) status);
        }
        else
        {
          status._Id = closedLoadStatus._Id;
          int num2 = await App.Connection.UpdateAsync((object) status);
        }
      }
      else
      {
        int num = await App.Connection.UpdateAsync((object) status);
      }
    }

    public static async Task<ClosedLoadStatus> GetStatusInAllList()
    {
      if (CompletionLoadStatusDao._allStatus == null || CompletionLoadStatusDao._allStatus.IsNew || CompletionLoadStatusDao._allStatus.UserId != LocalSettings.Settings.LoginUserId)
      {
        CompletionLoadStatusDao._allStatus = await CompletionLoadStatusDao.GetStatusByEntityId("all");
        if (CompletionLoadStatusDao._allStatus.IsAllLoaded)
          CompletionLoadStatusDao._allStatus.IsAllLoaded = false;
        if (Utils.IsEmptyDate(CompletionLoadStatusDao._allStatus.EarliestPulledDate))
          CompletionLoadStatusDao._allStatus.EarliestPulledDate = DateTime.UtcNow;
      }
      return CompletionLoadStatusDao._allStatus;
    }

    public static async Task DeleteStatusByEntityId(string entityId)
    {
      List<ClosedLoadStatus> listAsync = await App.Connection.Table<ClosedLoadStatus>().Where((Expression<Func<ClosedLoadStatus, bool>>) (s => s.UserId == LocalSettings.Settings.LoginUserId && s.EntityId == entityId)).ToListAsync();
      if (listAsync == null)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task ResetPullTime(DateTime time)
    {
      List<ClosedLoadStatus> listAsync = await App.Connection.Table<ClosedLoadStatus>().Where((Expression<Func<ClosedLoadStatus, bool>>) (s => s.UserId == LocalSettings.Settings.LoginUserId)).ToListAsync();
      foreach (ClosedLoadStatus closedLoadStatus in listAsync)
      {
        if (closedLoadStatus.EarliestPulledDate < time)
        {
          closedLoadStatus.EarliestPulledDate = time;
          closedLoadStatus.IsAllLoaded = false;
        }
      }
      int num = await App.Connection.UpdateAllAsync((IEnumerable) listAsync);
    }
  }
}
