// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskSyncedJsonDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TaskSyncedJsonDao
  {
    public static async Task<List<TaskSyncedJsonModel>> GetTaskSyncModels()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TaskSyncedJsonModel>().Where((Expression<Func<TaskSyncedJsonModel, bool>>) (model => model.userId == userId)).ToListAsync();
    }

    public static async Task<TaskSyncedJsonModel> GetJsonByTaskId(string id)
    {
      string loginUserId = LocalSettings.Settings.LoginUserId;
      List<TaskSyncedJsonModel> taskSyncedJsonModelList = await App.Connection.QueryAsync<TaskSyncedJsonModel>("select * from TaskSyncedJsonModel where taskSid  = '" + id + "' and userId = '" + loginUserId + "'");
      // ISSUE: explicit non-virtual call
      if (taskSyncedJsonModelList == null || __nonvirtual (taskSyncedJsonModelList.Count) <= 0)
        return (TaskSyncedJsonModel) null;
      for (int index = 1; index < taskSyncedJsonModelList.Count; ++index)
        App.Connection.DeleteAsync((object) taskSyncedJsonModelList[index]);
      return taskSyncedJsonModelList[0];
    }

    public static async Task<IEnumerable<string>> GetJsonNotExitIds()
    {
      List<IdModel> source = await App.Connection.QueryAsync<IdModel>("select TaskModel.id as Id from TaskModel left join TaskSyncedJsonModel on TaskModel.id=TaskSyncedJsonModel.taskSid " + string.Format("where TaskModel.deleted = {0} and TaskModel.userid = '{1}' and TaskSyncedJsonModel.taskSid is null", (object) 0, (object) LocalSettings.Settings.LoginUserId));
      return source != null ? source.Select<IdModel, string>((Func<IdModel, string>) (i => i.Id)) : (IEnumerable<string>) null;
    }

    public static async Task SaveTaskSyncedJsons(TaskSyncedJsonBean bean)
    {
      if (bean.Empty)
        return;
      List<TaskSyncedJsonModel> addlist;
      if (bean.Added.Count > 0)
      {
        addlist = new List<TaskSyncedJsonModel>();
        foreach (TaskModel taskModel in bean.Added)
          addlist.Add(new TaskSyncedJsonModel()
          {
            taskSid = taskModel.id,
            userId = taskModel.userId,
            jsonString = JsonConvert.SerializeObject((object) taskModel)
          });
        if (addlist.Count > 0)
        {
          int num = await App.Connection.InsertAllAsync((IEnumerable) addlist);
          foreach (TaskSyncedJsonModel json in addlist)
            CacheManager.UpdateSyncTask(json);
        }
        addlist = (List<TaskSyncedJsonModel>) null;
      }
      if (bean.Updated.Count <= 0)
        return;
      addlist = new List<TaskSyncedJsonModel>();
      List<TaskSyncedJsonModel> updatelist = new List<TaskSyncedJsonModel>();
      foreach (TaskModel taskModel in bean.Updated)
      {
        TaskModel task = taskModel;
        List<TaskSyncedJsonModel> listAsync = await App.Connection.Table<TaskSyncedJsonModel>().Where((Expression<Func<TaskSyncedJsonModel, bool>>) (v => v.taskSid == task.id)).ToListAsync();
        if (listAsync.Count > 0)
        {
          TaskSyncedJsonModel taskSyncedJsonModel1 = listAsync[0];
          TaskSyncedJsonModel taskSyncedJsonModel2 = new TaskSyncedJsonModel();
          taskSyncedJsonModel2._Id = taskSyncedJsonModel1._Id;
          taskSyncedJsonModel2.taskSid = task.id;
          taskSyncedJsonModel2.userId = task.userId;
          taskSyncedJsonModel2.jsonString = JsonConvert.SerializeObject((object) task);
          updatelist.Add(taskSyncedJsonModel2);
        }
        else
          addlist.Add(new TaskSyncedJsonModel()
          {
            taskSid = task.id,
            userId = task.userId,
            jsonString = JsonConvert.SerializeObject((object) task)
          });
      }
      if (updatelist.Count > 0)
      {
        int num = await App.Connection.UpdateAllAsync((IEnumerable) updatelist);
        foreach (TaskSyncedJsonModel json in updatelist)
          CacheManager.UpdateSyncTask(json);
      }
      if (addlist.Count > 0)
      {
        int num = await App.Connection.InsertAllAsync((IEnumerable) addlist);
        foreach (TaskSyncedJsonModel json in addlist)
          CacheManager.UpdateSyncTask(json);
      }
      addlist = (List<TaskSyncedJsonModel>) null;
      updatelist = (List<TaskSyncedJsonModel>) null;
    }

    public static async Task DeleteTaskSyncedJsonPhysical(string taskSid)
    {
      List<TaskSyncedJsonModel> listAsync = await App.Connection.Table<TaskSyncedJsonModel>().Where((Expression<Func<TaskSyncedJsonModel, bool>>) (task => task.taskSid == taskSid)).ToListAsync();
      if (listAsync.Count <= 0)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }
  }
}
