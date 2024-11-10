// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskCompletionRateDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TaskCompletionRateDao
  {
    private static ConcurrentDictionary<string, TaskCompletionRate> _cacheRates = new ConcurrentDictionary<string, TaskCompletionRate>();

    public static async Task WarmUp()
    {
      (await App.Connection.Table<TaskCompletionRate>().ToListAsync()).AsParallel<TaskCompletionRate>().ForAll<TaskCompletionRate>((Action<TaskCompletionRate>) (item =>
      {
        item.IsFromDb = true;
        TaskCompletionRateDao._cacheRates.TryAdd(item.TaskId, item);
      }));
    }

    public static async Task SaveToDb()
    {
      List<TaskCompletionRate> all = TaskCompletionRateDao._cacheRates.Values.ToList<TaskCompletionRate>();
      int num1 = await App.Connection.UpdateAllAsync((IEnumerable) all.Where<TaskCompletionRate>((Func<TaskCompletionRate, bool>) (item => item.IsFromDb)));
      int num2 = await App.Connection.InsertAllAsync((IEnumerable) all.Where<TaskCompletionRate>((Func<TaskCompletionRate, bool>) (item => !item.IsFromDb)));
      all = (List<TaskCompletionRate>) null;
    }

    public static string GetRateStrById(string id)
    {
      TaskCompletionRate rateById = TaskCompletionRateDao.GetRateById(id);
      return rateById == null || rateById.TotalCount == 0 ? string.Empty : string.Format("{0}/{1}", (object) rateById.CompletedCount, (object) rateById.TotalCount);
    }

    private static TaskCompletionRate GetRateById(string taskId)
    {
      if (string.IsNullOrEmpty(taskId))
        return (TaskCompletionRate) null;
      TaskCompletionRate taskCompletionRate;
      return !TaskCompletionRateDao._cacheRates.TryGetValue(taskId, out taskCompletionRate) ? (TaskCompletionRate) null : taskCompletionRate;
    }

    public static string GetRateStrByIdInDb(string id, string projectId)
    {
      if (string.IsNullOrEmpty(id))
        return string.Empty;
      List<TaskBaseViewModel> children = TaskCache.GetChildren(id, projectId);
      int completed = children.Count<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (item => item.Status != 0));
      int total = children.Count;
      ConcurrentDictionary<string, TaskCompletionRate> cacheRates = TaskCompletionRateDao._cacheRates;
      string key1 = id;
      TaskCompletionRate addValue = new TaskCompletionRate();
      addValue.TaskId = id;
      addValue.CompletedCount = completed;
      addValue.TotalCount = total;
      Func<string, TaskCompletionRate, TaskCompletionRate> updateValueFactory = (Func<string, TaskCompletionRate, TaskCompletionRate>) ((key, old) =>
      {
        old.CompletedCount = completed;
        old.TotalCount = total;
        return old;
      });
      cacheRates.AddOrUpdate(key1, addValue, updateValueFactory);
      return total > 0 ? string.Format("{0}/{1}", (object) completed, (object) total) : string.Empty;
    }

    public static async Task<List<string>> FetchRateByModels(List<DisplayItemModel> taskModels)
    {
      List<string> changedList = new List<string>();
      int skip = 0;
      List<ChildTaskCompletedModel> fullList = new List<ChildTaskCompletedModel>();
      List<string> ids = taskModels.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (t => t.Id)).ToList<string>();
      while (skip < ids.Count)
      {
        List<string> list = ids.Skip<string>(skip).Take<string>(300).ToList<string>();
        skip += 300;
        if (list.Any<string>())
        {
          SQLiteAsyncConnection connection = App.Connection;
          string sql = "select parentId, status, projectId from TaskModel " + string.Format("where {0} = {1} ", (object) "deleted", (object) 0) + "and parentId in (" + string.Join(",", list.Select<string, string>((Func<string, string>) (id => "'" + id + "'"))) + ")";
          object[] objArray = Array.Empty<object>();
          fullList.AddRange((IEnumerable<ChildTaskCompletedModel>) await connection.QueryAsync<ChildTaskCompletedModel>(sql, objArray));
        }
      }
      foreach (string str in fullList.Where<ChildTaskCompletedModel>((Func<ChildTaskCompletedModel, bool>) (item => !string.IsNullOrEmpty(item.parentId))).Select<ChildTaskCompletedModel, string>((Func<ChildTaskCompletedModel, string>) (item => item.parentId)).Distinct<string>())
      {
        string parentId = str;
        ids.Remove(parentId);
        DisplayItemModel parentModel = taskModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (t => t.Id == parentId));
        if (parentModel != null && !string.IsNullOrEmpty(parentModel.ProjectId))
        {
          int num1 = fullList.Where<ChildTaskCompletedModel>((Func<ChildTaskCompletedModel, bool>) (item => item.projectId == parentModel.ProjectId)).Where<ChildTaskCompletedModel>((Func<ChildTaskCompletedModel, bool>) (item => item.parentId == parentId)).Count<ChildTaskCompletedModel>((Func<ChildTaskCompletedModel, bool>) (item => item.status != 0));
          int num2 = fullList.Where<ChildTaskCompletedModel>((Func<ChildTaskCompletedModel, bool>) (item => item.projectId == parentModel.ProjectId)).Count<ChildTaskCompletedModel>((Func<ChildTaskCompletedModel, bool>) (item => item.parentId == parentId));
          TaskCompletionRate taskCompletionRate;
          if (TaskCompletionRateDao._cacheRates.TryGetValue(parentId, out taskCompletionRate))
          {
            if (num2 != taskCompletionRate.TotalCount || num1 != taskCompletionRate.CompletedCount)
            {
              taskCompletionRate.TotalCount = num2;
              taskCompletionRate.CompletedCount = num1;
              changedList.Add(parentId);
            }
          }
          else
          {
            TaskCompletionRateDao._cacheRates[parentId] = new TaskCompletionRate()
            {
              TaskId = parentId,
              CompletedCount = num1,
              TotalCount = num2
            };
            changedList.Add(parentId);
          }
        }
      }
      foreach (string key in ids)
      {
        if (TaskCompletionRateDao._cacheRates.TryRemove(key, out TaskCompletionRate _))
          changedList.Add(key);
      }
      List<string> stringList = changedList;
      changedList = (List<string>) null;
      fullList = (List<ChildTaskCompletedModel>) null;
      ids = (List<string>) null;
      return stringList;
    }
  }
}
