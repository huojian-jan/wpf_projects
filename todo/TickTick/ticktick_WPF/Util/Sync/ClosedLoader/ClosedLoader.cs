// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader
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
using ticktick_WPF.Service;
using ticktick_WPF.Util.Provider;

#nullable disable
namespace ticktick_WPF.Util.Sync.ClosedLoader
{
  public class ClosedLoader
  {
    public ProjectIdentity _identity;
    public LoadStatus CompletedTaskLoadStatus = new LoadStatus(false, new DateTime?(), DateTime.Now, 5);
    private string _entityId;
    private bool _localDrainOff;
    private bool _loadRemote = true;

    public int GetCompletedLimit() => this.CompletedTaskLoadStatus.Count;

    public bool CheckDrainOff(int num)
    {
      this._localDrainOff = this.CompletedTaskLoadStatus.Count > num;
      return (!this._loadRemote || this.CompletedTaskLoadStatus.DrainOff) && this._localDrainOff;
    }

    private void ResetLoadStatus()
    {
      this.CompletedTaskLoadStatus.Count = 5;
      this.CompletedTaskLoadStatus.DrainOff = false;
    }

    public void AddLimit(ProjectIdentity projectIdentity)
    {
      if (projectIdentity != null && this._identity != projectIdentity)
        this._identity = projectIdentity;
      this.CompletedTaskLoadStatus.Count += 50;
    }

    public void SetIdentity(ProjectIdentity projectIdentity)
    {
      if (projectIdentity == null || this._identity == projectIdentity)
        return;
      if (this._identity?.Id != projectIdentity?.Id)
        this.ResetLoadStatus();
      this._identity = projectIdentity;
      this._loadRemote = !(this._identity is FilterProjectIdentity) && !(this._identity is MatrixQuadrantIdentity);
    }

    public async Task<bool> NeedPullCompletedTasks(ProjectIdentity identity = null)
    {
      return await ClosedListSyncService.NeedPullCompletedTasks(identity ?? this._identity);
    }

    public async Task<bool> TryLoadCompletedTasks(
      ProjectIdentity identity,
      bool loadMore = true,
      DateTime? localEarliestDate = null)
    {
      if (identity is BindAccountCalendarProjectIdentity || identity is SubscribeCalendarProjectIdentity || identity is SearchProjectIdentity || identity is TagProjectIdentity || identity is FilterProjectIdentity || identity is AssignToMeProjectIdentity || identity is CompletedProjectIdentity || identity is AbandonedProjectIdentity || identity is TrashProjectIdentity || identity is MatrixQuadrantIdentity)
        return false;
      this._entityId = string.Empty;
      string projectIds = string.Empty;
      bool isAll = false;
      DateTime? fromDate = identity.GetCompletedFromTime();
      if (!(identity is GroupProjectIdentity group))
      {
        if (identity is NormalProjectIdentity normalProjectIdentity)
        {
          projectIds = normalProjectIdentity.Id;
          this._entityId = normalProjectIdentity.Id;
        }
      }
      else
      {
        projectIds = await ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader.GetGroupIds(group);
        if (string.IsNullOrEmpty(projectIds))
          return false;
        this._entityId = group.GroupId;
      }
      group = (GroupProjectIdentity) null;
      if (string.IsNullOrEmpty(projectIds))
      {
        this._entityId = "all";
        isAll = true;
      }
      ClosedLoadStatus status = await CompletionLoadStatusDao.GetLoadStatusByEntityId(this._entityId);
      if (!status.IsAllLoaded && (this._localDrainOff || !localEarliestDate.HasValue || localEarliestDate.Value <= status.EarliestPulledDate))
      {
        List<TaskModel> tasks;
        if (isAll)
          tasks = await Communicator.GetClosedTasks("all", fromDate, new DateTime?(status.EarliestPulledDate), 50);
        else
          tasks = await Communicator.GetClosedTasks(projectIds, fromDate, new DateTime?(status.EarliestPulledDate), 50);
        if (tasks == null)
          return false;
        if (tasks.Count < 50)
        {
          status.IsAllLoaded = true;
          await CompletionLoadStatusDao.SaveCompletionLoadStatus(status);
          this.CompletedTaskLoadStatus.DrainOff = true;
        }
        else
        {
          DateTime dateTime = (DateTime?) tasks.LastOrDefault<TaskModel>((Func<TaskModel, bool>) (t => t.completedTime.HasValue))?.completedTime ?? DateTime.Now;
          if (!status.IsAllLoaded && status.EarliestPulledDate > dateTime)
          {
            status.EarliestPulledDate = dateTime.AddSeconds(-1.0);
            await CompletionLoadStatusDao.SaveCompletionLoadStatus(status);
          }
        }
        if (await TaskService.MergeTasks((IEnumerable<TaskModel>) tasks))
          return true;
        tasks = (List<TaskModel>) null;
      }
      else
        this.CompletedTaskLoadStatus.DrainOff = status.IsAllLoaded;
      return false;
    }

    private static async Task<string> GetGroupIds(GroupProjectIdentity group)
    {
      string projectIds = string.Empty;
      ObservableCollection<ProjectModel> projectsInGroup = await ProjectDao.GetProjectsInGroup(group.GroupId);
      if (projectsInGroup.Count > 0)
      {
        for (int index = 0; index < projectsInGroup.Count; ++index)
          projectIds = index >= projectsInGroup.Count - 1 ? projectIds + projectsInGroup[index].id : projectIds + projectsInGroup[index].id + ",";
      }
      string groupIds = projectIds;
      projectIds = (string) null;
      return groupIds;
    }
  }
}
