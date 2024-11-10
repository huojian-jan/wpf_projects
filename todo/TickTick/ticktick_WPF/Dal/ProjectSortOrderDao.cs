// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ProjectSortOrderDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

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
using ticktick_WPF.Util.Sync.Model;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class ProjectSortOrderDao
  {
    public static async Task<long> GetSiblingSortOrderInProject(
      string projectId,
      string taskId,
      bool previous)
    {
      List<TaskModel> tasksInProjectAsync = await TaskDao.GetTasksInProjectAsync(projectId);
      if (tasksInProjectAsync != null && tasksInProjectAsync.Any<TaskModel>())
      {
        List<TaskModel> list = tasksInProjectAsync.OrderBy<TaskModel, long>((Func<TaskModel, long>) (t => t.sortOrder)).ToList<TaskModel>();
        TaskModel taskModel = list.FirstOrDefault<TaskModel>((Func<TaskModel, bool>) (t => t.id == taskId));
        if (taskModel != null)
        {
          int num = list.IndexOf(taskModel);
          return !previous ? (num < list.Count - 1 ? (taskModel.sortOrder + list[num + 1].sortOrder) / 2L : taskModel.sortOrder + 268435456L) : (num > 0 ? (taskModel.sortOrder + list[num - 1].sortOrder) / 2L : taskModel.sortOrder - 268435456L);
        }
      }
      return 0;
    }

    public static long GetNextTaskSortOrderInProject(
      string projectId,
      long baseOrder,
      string parentId = null,
      bool top = false)
    {
      List<long> sortOrdersInProject = TaskCache.GetTaskSortOrdersInProject(projectId, parentId);
      if (sortOrdersInProject == null || sortOrdersInProject.Count == 0)
        return 0;
      if (top)
      {
        long num = sortOrdersInProject.OrderByDescending<long, long>((Func<long, long>) (i => i)).ToList<long>().FirstOrDefault<long>((Func<long, bool>) (s => s < baseOrder));
        return num < baseOrder ? num + (baseOrder - num) / 2L : baseOrder - 268435456L;
      }
      sortOrdersInProject.Sort();
      long num1 = sortOrdersInProject.FirstOrDefault<long>((Func<long, bool>) (s => s > baseOrder));
      return num1 > baseOrder ? baseOrder + (num1 - baseOrder) / 2L : baseOrder + 268435456L;
    }

    public static long GetNewTaskSortOrderInProject(string projectId, string parentId = null, bool? isTop = null)
    {
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      List<long> sortOrdersInProject = TaskCache.GetTaskSortOrdersInProject(projectId, parentId);
      if (sortOrdersInProject == null || sortOrdersInProject.Count == 0)
        return 0;
      if (((int) isTop ?? (defaultSafely.AddTo == 0 ? 1 : 0)) != 0)
      {
        long sortOrderInProject = sortOrdersInProject.Min() - 268435456L;
        if (sortOrderInProject == 0L)
          sortOrderInProject = -1L;
        return sortOrderInProject;
      }
      long sortOrderInProject1 = sortOrdersInProject.Max() + 268435456L;
      if (sortOrderInProject1 == 0L)
        sortOrderInProject1 = 1L;
      return sortOrderInProject1;
    }

    public static async Task<List<TaskSortOrderInProjectModel>> GetProjectSortOrdersInProjects(
      List<string> projects)
    {
      return await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (order => order.Deleted == 0 && projects.Contains(order.ProjectId) && order.UserId == LocalSettings.Settings.LoginUserId)).ToListAsync();
    }

    public static async Task<List<TaskSortOrderInProjectModel>> GetProjectSortOrdersInIdentity()
    {
      return await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (order => order.Deleted == 0 && order.UserId == LocalSettings.Settings.LoginUserId)).ToListAsync();
    }

    private static async Task<List<TaskSortOrderInProjectModel>> GetSortOrdersInProject(
      string projectId)
    {
      return await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (order => order.ProjectId == projectId && order.UserId == LocalSettings.Settings.LoginUserId)).OrderBy<long>((Expression<Func<TaskSortOrderInProjectModel, long>>) (order => order.SortOrder)).ToListAsync();
    }

    public static async Task<List<TaskSortOrderInProjectModel>> GetLocalSortOrders()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (v => v.UserId == userId && v.SyncStatus == 2 && v.Deleted == 0)).ToListAsync();
    }

    public static async Task SaveRemoteChangesToDb(SyncDataBean<TaskSortOrderInProjectModel> bean)
    {
      if (bean.Addeds.Any<TaskSortOrderInProjectModel>())
      {
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) bean.Addeds);
      }
      if (!bean.Updateds.Any<TaskSortOrderInProjectModel>())
        return;
      int num2 = await App.Connection.UpdateAllAsync((IEnumerable) bean.Updateds);
    }

    public static async Task DeleteRemoteDeleted(List<string> projects)
    {
      if (!projects.Any<string>())
        return;
      foreach (string project in projects)
      {
        List<TaskSortOrderInProjectModel> sortOrdersInProject = await ProjectSortOrderDao.GetSortOrdersInProject(project);
        if (sortOrdersInProject != null && sortOrdersInProject.Any<TaskSortOrderInProjectModel>())
        {
          foreach (TaskSortOrderInProjectModel orderInProjectModel in sortOrdersInProject)
          {
            if (orderInProjectModel.SyncStatus != 0)
            {
              int num = await App.Connection.DeleteAsync((object) orderInProjectModel);
            }
          }
        }
      }
    }

    public static async Task<List<TaskSortOrderInProjectModel>> GetNeedPostSortOrdersInProject()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (v => v.UserId == userId && v.SyncStatus != 2)).ToListAsync();
    }

    public static async Task<Dictionary<string, Dictionary<string, List<TaskSortOrderInProjectModel>>>> GetNeedPostSortOrderInProjectMap()
    {
      Dictionary<string, Dictionary<string, List<TaskSortOrderInProjectModel>>> result = new Dictionary<string, Dictionary<string, List<TaskSortOrderInProjectModel>>>();
      List<TaskSortOrderInProjectModel> sortOrdersInProject = await ProjectSortOrderDao.GetNeedPostSortOrdersInProject();
      if (sortOrdersInProject != null && sortOrdersInProject.Any<TaskSortOrderInProjectModel>())
      {
        foreach (TaskSortOrderInProjectModel orderInProjectModel in sortOrdersInProject)
        {
          string projectId = orderInProjectModel.ProjectId;
          string key = "all";
          if (!result.ContainsKey(projectId))
            result.Add(projectId, new Dictionary<string, List<TaskSortOrderInProjectModel>>());
          if (!result[projectId].ContainsKey(key))
            result[projectId].Add(key, new List<TaskSortOrderInProjectModel>());
          result[projectId][key].Add(orderInProjectModel);
        }
      }
      Dictionary<string, Dictionary<string, List<TaskSortOrderInProjectModel>>> orderInProjectMap = result;
      result = (Dictionary<string, Dictionary<string, List<TaskSortOrderInProjectModel>>>) null;
      return orderInProjectMap;
    }

    public static async Task SavePostResult(
      SyncDataBean<TaskSortOrderInProjectModel> projectBean)
    {
      if (projectBean != null)
      {
        if (projectBean.Deleteds != null && projectBean.Deleteds.Count > 0)
        {
          foreach (TaskSortOrderInProjectModel deleted in projectBean.Deleteds)
            await ProjectSortOrderDao.DeleteTaskSortOrder(deleted);
        }
        if (projectBean.Updateds != null && projectBean.Updateds.Count > 0)
        {
          foreach (TaskSortOrderInProjectModel updated in projectBean.Updateds)
            updated.SyncStatus = 2;
          int num = await App.Connection.UpdateAllAsync((IEnumerable) projectBean.Updateds);
        }
      }
      ProjectSortOrderDao.ClearDeletedOrders();
    }

    private static async void ClearDeletedOrders()
    {
      List<TaskSortOrderInProjectModel> listAsync = await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (v => v.Deleted != 0 && v.SyncStatus == 2)).ToListAsync();
      if (listAsync == null || listAsync.Count <= 0)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    private static async Task DeleteTaskSortOrder(TaskSortOrderInProjectModel item)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<TaskSortOrderInProjectModel> listAsync = await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (v => v.ProjectId == item.ProjectId && v.EntityId == item.EntityId && v.UserId == userId)).ToListAsync();
      if (listAsync.Count <= 0)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task DeleteAllOrders()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<TaskSortOrderInProjectModel> listAsync = await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (order => order.UserId == userId)).ToListAsync();
      if (listAsync == null || !listAsync.Any<TaskSortOrderInProjectModel>())
        return;
      listAsync.ForEach((Action<TaskSortOrderInProjectModel>) (order =>
      {
        order.SyncStatus = 1;
        order.Deleted = 1;
      }));
      int num = await App.Connection.UpdateAllAsync((IEnumerable) listAsync);
    }

    public static async Task<List<TaskSortOrderInProjectModel>> BatchResetSortOrder(
      string projectId,
      List<string> ids)
    {
      List<TaskSortOrderInProjectModel> result = new List<TaskSortOrderInProjectModel>();
      if (ids == null)
        return result;
      int first = 0;
      long second = 268435456L * (long) ids.Count;
      for (int i = 0; i < ids.Count; ++i)
      {
        string id = ids[i];
        TaskSortOrderInProjectModel item = await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (model => model.ProjectId == projectId && id == model.EntityId)).FirstOrDefaultAsync();
        if (item != null)
        {
          item.SortOrder = Math.Min((long) first, second) + Math.Abs((long) first - second) * (long) i / (long) ids.Count;
          item.SyncStatus = 1;
          int num = await App.Connection.UpdateAsync((object) item);
        }
        else
        {
          item = new TaskSortOrderInProjectModel()
          {
            EntityId = id,
            ProjectId = projectId,
            SortOrder = Math.Min((long) first, second) + Math.Abs((long) first - second) * (long) i / (long) ids.Count,
            EntityType = "task",
            SyncStatus = 0,
            UserId = LocalSettings.Settings.LoginUserId
          };
          int num = await App.Connection.InsertAsync((object) item);
        }
        result.Add(item);
        item = (TaskSortOrderInProjectModel) null;
      }
      return result;
    }

    public static async Task UpdateSortOrderInProject(
      string entityId,
      long sortOrder,
      string projectId,
      string type = "task")
    {
      TaskSortOrderInProjectModel orderInProjectModel = await App.Connection.Table<TaskSortOrderInProjectModel>().Where((Expression<Func<TaskSortOrderInProjectModel, bool>>) (m => m.ProjectId == projectId && entityId == m.EntityId)).FirstOrDefaultAsync();
      if (orderInProjectModel != null)
      {
        orderInProjectModel.SortOrder = sortOrder;
        orderInProjectModel.SyncStatus = 1;
        int num = await App.Connection.UpdateAsync((object) orderInProjectModel);
      }
      else
      {
        int num1 = await App.Connection.InsertAsync((object) new TaskSortOrderInProjectModel()
        {
          EntityId = entityId,
          ProjectId = projectId,
          SortOrder = sortOrder,
          EntityType = type,
          SyncStatus = 0,
          UserId = LocalSettings.Settings.LoginUserId
        });
      }
    }

    public static async Task NewTaskAdded(TaskModel task)
    {
      if (task == null)
        return;
      List<TaskSortOrderInProjectModel> sortOrdersInProject = await ProjectSortOrderDao.GetSortOrdersInProject(task.projectId);
      if (sortOrdersInProject == null || sortOrdersInProject.Count == 0)
        return;
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      TaskSortOrderInProjectModel orderInProjectModel = new TaskSortOrderInProjectModel()
      {
        EntityId = task.id,
        ProjectId = task.projectId,
        EntityType = nameof (task),
        SyncStatus = 0,
        UserId = LocalSettings.Settings.LoginUserId
      };
      if (defaultSafely.AddTo == 0)
      {
        long num = Math.Min(0L, sortOrdersInProject.Select<TaskSortOrderInProjectModel, long>((Func<TaskSortOrderInProjectModel, long>) (o => o.SortOrder)).Min());
        orderInProjectModel.SortOrder = num - 268435456L;
      }
      else
      {
        long num = Math.Max(0L, sortOrdersInProject.Select<TaskSortOrderInProjectModel, long>((Func<TaskSortOrderInProjectModel, long>) (o => o.SortOrder)).Max());
        orderInProjectModel.SortOrder = num + 268435456L;
      }
      int num1 = await App.Connection.InsertAsync((object) orderInProjectModel);
    }

    public static async Task TryAddSplitTaskOrder(
      string projectId,
      string baseTaskId,
      string insertTaskId,
      bool below)
    {
      List<TaskSortOrderInProjectModel> sortOrdersInProject = await ProjectSortOrderDao.GetSortOrdersInProject(projectId);
      if (sortOrdersInProject == null || sortOrdersInProject.Count == 0)
        return;
      List<TaskSortOrderInProjectModel> list = sortOrdersInProject.OrderBy<TaskSortOrderInProjectModel, long>((Func<TaskSortOrderInProjectModel, long>) (o => o.SortOrder)).ToList<TaskSortOrderInProjectModel>();
      int index1 = -1;
      for (int index2 = 0; index2 < list.Count; ++index2)
      {
        if (list[index2].EntityId == baseTaskId)
        {
          index1 = index2;
          break;
        }
      }
      if (index1 < 0)
        return;
      long num1 = index1 != 0 || below ? (!(index1 == sortOrdersInProject.Count - 1 & below) ? MathUtil.LongAvg(list[index1].SortOrder, list[below ? index1 + 1 : index1 - 1].SortOrder) : list[sortOrdersInProject.Count - 1].SortOrder + 268435456L) : list[0].SortOrder - 268435456L;
      int num2 = await App.Connection.InsertAsync((object) new TaskSortOrderInProjectModel()
      {
        EntityId = insertTaskId,
        ProjectId = projectId,
        EntityType = "task",
        SyncStatus = 0,
        UserId = LocalSettings.Settings.LoginUserId,
        SortOrder = num1
      });
    }
  }
}
