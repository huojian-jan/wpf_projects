// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ClosedListSyncService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync.ClosedLoader;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class ClosedListSyncService
  {
    public static void ClearLoadDict()
    {
    }

    public static async Task<bool> NeedPullCompletedTasks(ProjectIdentity projectId)
    {
      if (projectId is BindAccountCalendarProjectIdentity || projectId is SubscribeCalendarProjectIdentity || projectId is SearchProjectIdentity || projectId is TagProjectIdentity || projectId is FilterProjectIdentity || projectId is AssignToMeProjectIdentity || projectId is CompletedProjectIdentity || projectId is AbandonedProjectIdentity || projectId is TrashProjectIdentity)
        return false;
      ClosedLoadStatus statusByEntityId = await CompletionLoadStatusDao.GetLoadStatusByEntityId(projectId.LoadMoreId);
      return !statusByEntityId.IsAllLoaded && (DateTime.Now - statusByEntityId.EarliestPulledDate).TotalMinutes < 5.0;
    }

    public static async void PullCompletedTaskAtFirstLogin()
    {
      List<TaskModel> closedTasks = await Communicator.GetClosedTasks(Utils.GetInboxId(), new DateTime?(), new DateTime?(DateTime.Now), 600);
      if (closedTasks == null)
        return;
      int num = await TaskService.MergeTasks((IEnumerable<TaskModel>) closedTasks) ? 1 : 0;
    }
  }
}
