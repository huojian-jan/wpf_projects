// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ClosedLoader.ClosedTaskWithFilterLoader
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Completed;

#nullable disable
namespace ticktick_WPF.Util.Sync.ClosedLoader
{
  public class ClosedTaskWithFilterLoader
  {
    public static ClosedTaskWithFilterLoader CompletionLoader = new ClosedTaskWithFilterLoader()
    {
      InCompletedProject = true
    };
    public bool InCompletedProject;
    public static ClosedTaskWithFilterLoader AbandonedLoader = new ClosedTaskWithFilterLoader()
    {
      InAbandonedProject = true
    };
    public bool InAbandonedProject;
    public static ClosedTaskWithFilterLoader CalClosedLoader = new ClosedTaskWithFilterLoader()
    {
      InCalProject = true
    };
    public bool InCalProject;
    public LoadStatus CompletedProjectStatus = new LoadStatus(false, new DateTime?(), DateTime.Now, 50);
    private List<string> _selectedProjectIds;
    private DateTime _pullToTime;
    private bool _localDrainOff;
    private List<KeyValuePair<DateTime, DateTime>> _dateKvList = new List<KeyValuePair<DateTime, DateTime>>();

    public event EventHandler<bool> Loaded;

    private async Task<bool> GetProjectLoadStatus(DateTime? earliestInList)
    {
      await this.SetTimeSpan();
      ClosedLoadStatus statusInAllList = await CompletionLoadStatusDao.GetStatusInAllList();
      if (!statusInAllList.IsAllLoaded)
      {
        DateTime? nullable;
        if (this.CompletedProjectStatus.FromTime.HasValue)
        {
          DateTime earliestPulledDate = statusInAllList.EarliestPulledDate;
          nullable = this.CompletedProjectStatus.FromTime;
          if ((nullable.HasValue ? (earliestPulledDate > nullable.GetValueOrDefault() ? 1 : 0) : 0) == 0)
            goto label_6;
        }
        this._pullToTime = Utils.IsEmptyDate(this.CompletedProjectStatus.ToTime) || statusInAllList.EarliestPulledDate < this.CompletedProjectStatus.ToTime ? statusInAllList.EarliestPulledDate : this.CompletedProjectStatus.ToTime;
        nullable = earliestInList;
        DateTime pullToTime = this._pullToTime;
        return (nullable.HasValue ? (nullable.GetValueOrDefault() > pullToTime ? 1 : 0) : 0) == 0;
      }
label_6:
      this.CompletedProjectStatus.DrainOff = true;
      return false;
    }

    private async Task SetTimeSpan()
    {
      ClosedFilterViewModel closedFilterViewModel = (ClosedFilterViewModel) null;
      if (this.InCompletedProject)
        closedFilterViewModel = CompletedProjectIdentity.Filter;
      if (this.InAbandonedProject)
        closedFilterViewModel = AbandonedProjectIdentity.Filter;
      if (closedFilterViewModel == null)
        return;
      CompletedFilterData completedFilterData = await closedFilterViewModel.GetCompletedFilterData();
      this._selectedProjectIds = completedFilterData.ProjectIds;
      this.CompletedProjectStatus.FromTime = completedFilterData.FromTime;
      this.CompletedProjectStatus.ToTime = completedFilterData.ToTime;
    }

    private async Task<bool> LoadTasks(
      DateTime? earliestInList = null,
      int loadTimes = 0,
      bool loadMore = false,
      bool checkNum = true)
    {
      ClosedTaskWithFilterLoader withFilterLoader = this;
      if (loadMore)
        withFilterLoader.CompletedProjectStatus.Count += 50;
      if (loadTimes >= 4)
        return true;
      if (!await withFilterLoader.GetProjectLoadStatus(earliestInList))
        return !withFilterLoader._localDrainOff & loadMore;
      List<TaskModel> tasks = await Communicator.GetClosedTasks("all", withFilterLoader.CompletedProjectStatus.FromTime, new DateTime?(withFilterLoader._pullToTime), 100);
      if (tasks == null)
        return false;
      if (tasks.Count > 0)
      {
        int num1 = await TaskService.MergeTasks((IEnumerable<TaskModel>) tasks) ? 1 : 0;
      }
      if (tasks.Count < 100)
        withFilterLoader.CompletedProjectStatus.DrainOff = true;
      ClosedLoadStatus statusByEntityId = await CompletionLoadStatusDao.GetLoadStatusByEntityId("all");
      if (!withFilterLoader.CompletedProjectStatus.DrainOff)
      {
        DateTime? earliestDate = (DateTime?) tasks.LastOrDefault<TaskModel>((Func<TaskModel, bool>) (t => t.completedTime.HasValue))?.completedTime;
        if (!earliestDate.HasValue)
          return tasks.Count > 0;
        DateTime earliestPulledDate = earliestDate.Value;
        earliestDate = new DateTime?(earliestPulledDate.AddSeconds(-1.0));
        if (!statusByEntityId.IsAllLoaded && statusByEntityId.EarliestPulledDate <= withFilterLoader._pullToTime)
        {
          earliestPulledDate = statusByEntityId.EarliestPulledDate;
          DateTime? nullable = earliestDate;
          if ((nullable.HasValue ? (earliestPulledDate > nullable.GetValueOrDefault() ? 1 : 0) : 0) != 0)
          {
            statusByEntityId.EarliestPulledDate = earliestDate.Value;
            await CompletionLoadStatusDao.SaveCompletionLoadStatus(statusByEntityId);
          }
        }
        if (checkNum)
        {
          List<string> selectedProjectIds = withFilterLoader._selectedProjectIds;
          // ISSUE: explicit non-virtual call
          // ISSUE: reference to a compiler-generated method
          if ((selectedProjectIds != null ? (__nonvirtual (selectedProjectIds.Count) > 0 ? 1 : 0) : 0) != 0 && !tasks.Any<TaskModel>(new Func<TaskModel, bool>(withFilterLoader.\u003CLoadTasks\u003Eb__16_1)))
          {
            ++loadTimes;
            int num2 = await withFilterLoader.LoadTasks(earliestDate, loadTimes) ? 1 : 0;
          }
        }
        earliestDate = new DateTime?();
      }
      else if (!statusByEntityId.IsAllLoaded && statusByEntityId.EarliestPulledDate <= withFilterLoader._pullToTime)
      {
        DateTime? nullable = (DateTime?) tasks.LastOrDefault<TaskModel>((Func<TaskModel, bool>) (t => t.completedTime.HasValue))?.completedTime;
        DateTime dateTime1 = nullable ?? statusByEntityId.EarliestPulledDate;
        ClosedLoadStatus closedLoadStatus1 = statusByEntityId;
        nullable = withFilterLoader.CompletedProjectStatus.FromTime;
        DateTime dateTime2 = nullable ?? dateTime1;
        closedLoadStatus1.EarliestPulledDate = dateTime2;
        ClosedLoadStatus closedLoadStatus2 = statusByEntityId;
        nullable = withFilterLoader.CompletedProjectStatus.FromTime;
        int num3 = !nullable.HasValue ? 1 : 0;
        closedLoadStatus2.IsAllLoaded = num3 != 0;
        await CompletionLoadStatusDao.SaveCompletionLoadStatus(statusByEntityId);
      }
      return tasks.Count > 0;
    }

    public async void TryLoadTasks(DateTime? earliestInList = null, bool loadMore = false, bool checkNum = true)
    {
      ClosedTaskWithFilterLoader withFilterLoader = this;
      EventHandler<bool> eventHandler = withFilterLoader.Loaded;
      if (eventHandler != null)
      {
        object sender = (object) withFilterLoader;
        eventHandler(sender, await withFilterLoader.LoadTasks(earliestInList, loadMore: loadMore));
        sender = (object) null;
      }
      eventHandler = (EventHandler<bool>) null;
    }

    public async Task<bool> LoadTasksInSpan(
      DateTime start,
      DateTime end,
      string prjId = "all",
      bool onlyCheckStart = false)
    {
      if (onlyCheckStart)
      {
        if (!await this.GetProjectLoadStatus(new DateTime?(start)))
          return false;
      }
      else if (this._dateKvList.Any<KeyValuePair<DateTime, DateTime>>((Func<KeyValuePair<DateTime, DateTime>, bool>) (kv => kv.Key >= start && kv.Value <= end)))
        return false;
      KeyValuePair<DateTime, DateTime> dateKv = new KeyValuePair<DateTime, DateTime>(start, end);
      this._dateKvList.Add(dateKv);
      ClosedLoadStatus statusByEntityId1 = await CompletionLoadStatusDao.GetLoadStatusByEntityId(prjId);
      if (statusByEntityId1.IsAllLoaded || statusByEntityId1.EarliestPulledDate < start)
        return false;
      bool savePullDate = false;
      DateTime toTime;
      if (statusByEntityId1.EarliestPulledDate > start.AddDays(100.0))
      {
        toTime = end;
      }
      else
      {
        toTime = statusByEntityId1.EarliestPulledDate;
        savePullDate = true;
      }
      bool tasksPulled = false;
      List<TaskModel> tasks = await Communicator.GetClosedTasks(prjId, new DateTime?(start), new DateTime?(toTime), 1200);
      if (tasks == null)
      {
        this._dateKvList.Remove(dateKv);
        return false;
      }
      if (tasks.Count > 0)
      {
        if (await TaskService.MergeTasks((IEnumerable<TaskModel>) tasks))
          tasksPulled = true;
      }
      while (tasks.Count >= 1200)
      {
        TaskModel taskModel = tasks.LastOrDefault<TaskModel>((Func<TaskModel, bool>) (t => t.completedTime.HasValue));
        if (taskModel != null && taskModel.completedTime.HasValue)
        {
          toTime = toTime == taskModel.completedTime.Value ? taskModel.completedTime.Value.AddSeconds(-1.0) : taskModel.completedTime.Value;
          if (!(toTime < start))
          {
            List<TaskModel> closedTasks = await Communicator.GetClosedTasks(prjId, new DateTime?(start), new DateTime?(toTime), 1200);
            if (await TaskService.MergeTasks((IEnumerable<TaskModel>) closedTasks))
              tasksPulled = true;
            List<TaskModel> taskModelList = closedTasks;
            // ISSUE: explicit non-virtual call
            if ((taskModelList != null ? (__nonvirtual (taskModelList.Count) > 0 ? 1 : 0) : 0) != 0)
            {
              tasks = closedTasks;
              closedTasks = (List<TaskModel>) null;
            }
            else
              break;
          }
          else
            break;
        }
        else
          break;
      }
      ClosedLoadStatus statusByEntityId2 = await CompletionLoadStatusDao.GetLoadStatusByEntityId(prjId);
      if (savePullDate | onlyCheckStart)
      {
        statusByEntityId2.EarliestPulledDate = start.AddSeconds(-1.0);
        await CompletionLoadStatusDao.SaveCompletionLoadStatus(statusByEntityId2);
      }
      return tasksPulled;
    }

    public bool NeedShowLoadMore(int completedCount)
    {
      this._localDrainOff = completedCount < this.CompletedProjectStatus.Count;
      return !this.CompletedProjectStatus.DrainOff || completedCount >= this.CompletedProjectStatus.Count;
    }

    public void Reset()
    {
      this.CompletedProjectStatus.Count = 50;
      this.CompletedProjectStatus.DrainOff = false;
    }
  }
}
