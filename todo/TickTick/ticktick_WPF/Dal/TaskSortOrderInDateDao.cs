// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskSortOrderInDateDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TaskSortOrderInDateDao
  {
    public static async Task<List<TaskSortOrderInDateModel>> GetOrderByProjectAndDate(
      string projectId,
      string date)
    {
      if (projectId.Contains("_special_id_"))
        projectId = "all";
      return await App.Connection.Table<TaskSortOrderInDateModel>().Where((Expression<Func<TaskSortOrderInDateModel, bool>>) (v => v.userid == LocalSettings.Settings.LoginUserId && v.projectid == projectId && v.date == date && v.deleted != 1)).ToListAsync();
    }

    public static async Task<List<TaskSortOrderInDateModel>> GetSortOrderInDateByProjectId(
      string projectId)
    {
      if (projectId.Contains("_special_id_"))
        projectId = "all";
      return await App.Connection.Table<TaskSortOrderInDateModel>().Where((Expression<Func<TaskSortOrderInDateModel, bool>>) (v => v.userid == LocalSettings.Settings.LoginUserId && v.projectid == projectId && v.deleted != 1)).ToListAsync();
    }

    public static async Task<List<TaskSortOrderInDateModel>> GetOrderByDateWithDeleted(string date)
    {
      return await App.Connection.Table<TaskSortOrderInDateModel>().Where((Expression<Func<TaskSortOrderInDateModel, bool>>) (v => v.userid == LocalSettings.Settings.LoginUserId && v.date == date)).ToListAsync();
    }

    public static async Task UpdateOrInsertTaskSortOrderInDateAsync(
      TaskSortOrderInDateModel taskSortOrderInDate)
    {
      if (taskSortOrderInDate.projectid.Contains("_special_id_"))
        taskSortOrderInDate.projectid = "all";
      List<TaskSortOrderInDateModel> listAsync = await App.Connection.Table<TaskSortOrderInDateModel>().Where((Expression<Func<TaskSortOrderInDateModel, bool>>) (v => v.userid == taskSortOrderInDate.userid && v.projectid == taskSortOrderInDate.projectid && v.taskid == taskSortOrderInDate.taskid)).ToListAsync();
      taskSortOrderInDate.modifiedTime = Utils.GetNowTimeStamp();
      taskSortOrderInDate.status = 1;
      if (listAsync.Count != 0)
      {
        taskSortOrderInDate._Id = listAsync[0]._Id;
        int num = await App.Connection.UpdateAsync((object) taskSortOrderInDate);
      }
      else
      {
        taskSortOrderInDate.status = 0;
        int num = await App.Connection.InsertAsync((object) taskSortOrderInDate);
      }
    }

    public static async Task DeleteTaskSortOrderInDateDbByProjectIdAsync(string id)
    {
      string str = id.Contains("_special_id_") ? "all" : id;
      int num = await App.Connection.ExecuteAsync("UPDATE TaskSortOrderInDateModel SET modifiedTime = ?, deleted = ?, status = ? WHERE projectid = ?", (object) Utils.GetNowTimeStamp(), (object) 1, (object) 1, (object) str);
    }

    private static async Task<int> DeleteTaskSortOrder(TaskSortOrderInDateModel item)
    {
      return await App.Connection.ExecuteAsync("DELETE FROM TaskSortOrderInDateModel WHERE projectid = ? and taskid = ? and userid = ? and date = ?", (object) item.projectid, (object) item.taskid, (object) item.userid, (object) item.date);
    }

    public static async Task SavePostResult(SyncDataBean<TaskSortOrderInDateModel> dateBean)
    {
      if (dateBean != null)
      {
        if (dateBean.Deleteds != null && dateBean.Deleteds.Count > 0)
        {
          foreach (TaskSortOrderInDateModel deleted in dateBean.Deleteds)
          {
            int num = await TaskSortOrderInDateDao.DeleteTaskSortOrder(deleted);
          }
        }
        if (dateBean.Updateds != null && dateBean.Updateds.Count > 0)
        {
          foreach (TaskSortOrderInDateModel updated in dateBean.Updateds)
            updated.status = 2;
          int num = await App.Connection.UpdateAllAsync((IEnumerable) dateBean.Updateds);
        }
      }
      TaskSortOrderInDateDao.ClearDeletedOrders();
    }

    private static async Task<int> ClearDeletedOrders()
    {
      return await App.Connection.ExecuteAsync("DELETE FROM TaskSortOrderInDateModel WHERE deleted = ? and status = ?", (object) 1, (object) 2);
    }

    public static async Task SaveRemoteChangesToDb(
      SyncDataBean<TaskSortOrderInDateModel> pullDataBean)
    {
      foreach (TaskSortOrderInDateModel added in pullDataBean.Addeds)
        await TaskSortOrderInDateDao.SaveTaskSortOrdersInDate(added);
      foreach (TaskSortOrderInDateModel updated in pullDataBean.Updateds)
        await TaskSortOrderInDateDao.SaveTaskSortOrdersInDate(updated);
      foreach (TaskSortOrderInDateModel deleted in pullDataBean.Deleteds)
      {
        int num = await TaskSortOrderInDateDao.DeleteTaskSortOrder(deleted);
      }
    }

    public static async Task<Dictionary<string, Dictionary<string, List<TaskSortOrderInDateModel>>>> GetNeedPostSortOrdersInDateMap()
    {
      Dictionary<string, Dictionary<string, List<TaskSortOrderInDateModel>>> map = new Dictionary<string, Dictionary<string, List<TaskSortOrderInDateModel>>>();
      foreach (TaskSortOrderInDateModel orderInDateModel in await TaskSortOrderInDateDao.GetNeedPostSortOrdersInDate(long.MaxValue))
      {
        Dictionary<string, List<TaskSortOrderInDateModel>> dictionary1 = (Dictionary<string, List<TaskSortOrderInDateModel>>) null;
        if (map.ContainsKey(orderInDateModel.date))
          dictionary1 = map[orderInDateModel.date];
        if (dictionary1 == null)
        {
          Dictionary<string, List<TaskSortOrderInDateModel>> dictionary2 = new Dictionary<string, List<TaskSortOrderInDateModel>>();
          List<TaskSortOrderInDateModel> orderInDateModelList = new List<TaskSortOrderInDateModel>()
          {
            orderInDateModel
          };
          dictionary2.Add(orderInDateModel.projectid, orderInDateModelList);
          map.Add(orderInDateModel.date, dictionary2);
        }
        else if (dictionary1.ContainsKey(orderInDateModel.projectid))
        {
          dictionary1[orderInDateModel.projectid].Add(orderInDateModel);
        }
        else
        {
          List<TaskSortOrderInDateModel> orderInDateModelList = new List<TaskSortOrderInDateModel>()
          {
            orderInDateModel
          };
          dictionary1.Add(orderInDateModel.projectid, orderInDateModelList);
        }
      }
      Dictionary<string, Dictionary<string, List<TaskSortOrderInDateModel>>> sortOrdersInDateMap = map;
      map = (Dictionary<string, Dictionary<string, List<TaskSortOrderInDateModel>>>) null;
      return sortOrdersInDateMap;
    }

    public static async Task<List<TaskSortOrderInDateModel>> GetNeedPostSortOrdersInDate(
      long topPoint)
    {
      return await Task.Run<List<TaskSortOrderInDateModel>>((Func<Task<List<TaskSortOrderInDateModel>>>) (async () => await App.Connection.Table<TaskSortOrderInDateModel>().Where((Expression<Func<TaskSortOrderInDateModel, bool>>) (v => v.userid == LocalSettings.Settings.LoginUserId && v.modifiedTime <= topPoint && v.status != 2)).ToListAsync()));
    }

    private static async Task SaveTaskSortOrdersInDate(TaskSortOrderInDateModel orderInDate)
    {
      if (orderInDate == null)
        return;
      orderInDate.modifiedTime = Utils.GetNowTimeStamp();
      int num1 = await TaskSortOrderInDateDao.DeleteTaskSortOrder(orderInDate);
      int num2 = await App.Connection.InsertAsync((object) orderInDate);
    }

    public static async Task BatchInsertOrderInDate(List<TaskSortOrderInDateModel> orderInDate)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) orderInDate);
    }

    public static async Task<List<TaskSortOrderInDateModel>> GetAllSortsInProject(string projectId)
    {
      return await App.Connection.Table<TaskSortOrderInDateModel>().Where((Expression<Func<TaskSortOrderInDateModel, bool>>) (model => model.projectid == projectId && model.deleted == 0)).ToListAsync();
    }

    public static async Task<List<TaskSortOrderInDateModel>> GetAllSorts()
    {
      return await App.Connection.Table<TaskSortOrderInDateModel>().Where((Expression<Func<TaskSortOrderInDateModel, bool>>) (model => model.userid == LocalSettings.Settings.LoginUserId)).ToListAsync();
    }

    public static async Task TryAddSplitTaskOrder(
      string projectId,
      string date,
      string baseTaskId,
      string newTaskId,
      bool below)
    {
      List<TaskSortOrderInDateModel> byProjectAndDate = await TaskSortOrderInDateDao.GetOrderByProjectAndDate(projectId, date);
      if (byProjectAndDate == null || byProjectAndDate.Count == 0)
        return;
      List<TaskSortOrderInDateModel> list = byProjectAndDate.OrderBy<TaskSortOrderInDateModel, long>((Func<TaskSortOrderInDateModel, long>) (o => o.sortOrder)).ToList<TaskSortOrderInDateModel>();
      int index1 = -1;
      for (int index2 = 0; index2 < list.Count; ++index2)
      {
        if (list[index2].taskid == baseTaskId)
        {
          index1 = index2;
          break;
        }
      }
      if (index1 < 0)
        return;
      long num1 = index1 != 0 || below ? (!(index1 == byProjectAndDate.Count - 1 & below) ? (list[index1].sortOrder + list[below ? index1 + 1 : index1 - 1].sortOrder) / 2L : list[byProjectAndDate.Count - 1].sortOrder + 268435456L) : list[0].sortOrder - 268435456L;
      int num2 = await App.Connection.InsertAsync((object) new TaskSortOrderInDateModel()
      {
        userid = LocalSettings.Settings.LoginUserId,
        taskid = newTaskId,
        date = date,
        projectid = projectId,
        sortOrder = num1,
        modifiedTime = Utils.GetNowTimeStamp(),
        status = 1,
        type = 1
      });
    }

    public static async Task SyncOnEventCalendarChanged(string originalId, string revisedId)
    {
      int num = await App.Connection.ExecuteAsync("UPDATE TaskSortOrderInDateModel SET taskid=?, status=? WHERE taskid=?", (object) revisedId, (object) 1, (object) originalId);
    }

    public static async Task UpdateSortOrderInDate(
      string id,
      long order,
      string date,
      string sortProjectId,
      int type = 1)
    {
      id = ArchivedDao.GetOriginalId(id);
      TaskSortOrderInDateModel orderInDateModel = await App.Connection.Table<TaskSortOrderInDateModel>().Where((Expression<Func<TaskSortOrderInDateModel, bool>>) (m => m.projectid == sortProjectId && id == m.taskid && m.date == date && m.deleted == 0)).FirstOrDefaultAsync();
      if (orderInDateModel != null)
      {
        orderInDateModel.sortOrder = order;
        orderInDateModel.modifiedTime = Utils.GetNowTimeStamp();
        orderInDateModel.status = 1;
        orderInDateModel.type = type;
        int num = await App.Connection.UpdateAsync((object) orderInDateModel);
      }
      else
      {
        int num1 = await App.Connection.InsertAsync((object) new TaskSortOrderInDateModel()
        {
          userid = LocalSettings.Settings.LoginUserId,
          taskid = id,
          date = date,
          projectid = sortProjectId,
          sortOrder = order,
          modifiedTime = Utils.GetNowTimeStamp(),
          status = 1,
          type = type
        });
      }
    }

    public static async Task<List<TaskSortOrderInDateModel>> BatchResetSortOrder(
      List<(string, DisplayType)> id2Types,
      string date,
      string sortProjectId)
    {
      List<TaskSortOrderInDateModel> result = new List<TaskSortOrderInDateModel>();
      if (id2Types == null || id2Types.Count == 0)
        return result;
      List<string> ids = id2Types.Select<(string, DisplayType), string>((Func<(string, DisplayType), string>) (i => ArchivedDao.GetOriginalId(i.Item1))).ToList<string>();
      List<TaskSortOrderInDateModel> models = await App.Connection.Table<TaskSortOrderInDateModel>().Where((Expression<Func<TaskSortOrderInDateModel, bool>>) (m => m.projectid == sortProjectId && ids.Contains(m.taskid) && m.date == date && m.deleted == 0)).ToListAsync();
      long sortOrder = 0;
      foreach ((string, DisplayType) id2Type in id2Types)
      {
        string id = ArchivedDao.GetOriginalId(id2Type.Item1);
        TaskSortOrderInDateModel model = models.FirstOrDefault<TaskSortOrderInDateModel>((Func<TaskSortOrderInDateModel, bool>) (m => m.taskid == id));
        if (model != null)
        {
          model.sortOrder = sortOrder;
          model.modifiedTime = Utils.GetNowTimeStamp();
          model.status = 1;
          model.type = EntityType.GetEntityTypeNum(id2Type.Item2);
          int num = await App.Connection.UpdateAsync((object) model);
        }
        else
        {
          model = new TaskSortOrderInDateModel()
          {
            userid = LocalSettings.Settings.LoginUserId,
            taskid = ArchivedDao.GetOriginalId(id2Type.Item1),
            date = date,
            projectid = sortProjectId,
            sortOrder = sortOrder,
            modifiedTime = Utils.GetNowTimeStamp(),
            status = 1,
            type = EntityType.GetEntityTypeNum(id2Type.Item2)
          };
          int num = await App.Connection.InsertAsync((object) model);
        }
        sortOrder += 268435456L;
        result.Add(model);
        model = (TaskSortOrderInDateModel) null;
      }
      return result;
    }

    public static async Task NewTaskAdded(TaskModel task, string catId)
    {
      if (task == null)
        return;
      if (catId.Contains("_special_id_"))
        catId = "all";
      DateTime? date = DateUtils.GetDateSectionDate(DateUtils.GetSectionCategory(task.startDate, task.dueDate, task.isAllDay));
      if (!date.HasValue || date.Value < DateTime.Today)
        return;
      List<TaskSortOrderInDateModel> byProjectAndDate = await TaskSortOrderInDateDao.GetOrderByProjectAndDate(catId, date.Value.ToString("yyyyMMdd"));
      if (byProjectAndDate == null || byProjectAndDate.Count == 0)
        return;
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      TaskSortOrderInDateModel orderInDateModel = new TaskSortOrderInDateModel()
      {
        userid = LocalSettings.Settings.LoginUserId,
        taskid = task.id,
        date = date.Value.ToString("yyyyMMdd"),
        projectid = catId,
        modifiedTime = Utils.GetNowTimeStamp(),
        status = 0,
        type = 1
      };
      if (defaultSafely.AddTo == 0)
      {
        long num = Math.Min(0L, byProjectAndDate.Select<TaskSortOrderInDateModel, long>((Func<TaskSortOrderInDateModel, long>) (o => o.sortOrder)).Min());
        orderInDateModel.sortOrder = num - 268435456L;
      }
      else
      {
        long num = Math.Max(0L, byProjectAndDate.Select<TaskSortOrderInDateModel, long>((Func<TaskSortOrderInDateModel, long>) (o => o.sortOrder)).Max());
        orderInDateModel.sortOrder = num + 268435456L;
      }
      int num1 = await App.Connection.InsertAsync((object) orderInDateModel);
    }
  }
}
