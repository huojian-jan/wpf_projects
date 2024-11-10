// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.TaskTransfer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Sync.Model;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public static class TaskTransfer
  {
    public static async Task<List<SyncTaskBean>> DescribeSyncTaskBean(
      List<SyncStatusModel> addedTasks,
      List<SyncStatusModel> updatedTasks,
      List<SyncStatusModel> deletedTasks,
      List<SyncStatusModel> deletedForeverTasks)
    {
      List<SyncTaskBean> syncTaskBeans = new List<SyncTaskBean>();
      int batchCount = new List<List<SyncStatusModel>>()
      {
        addedTasks,
        updatedTasks,
        deletedTasks,
        deletedForeverTasks
      }.Max<List<SyncStatusModel>>((Func<List<SyncStatusModel>, int>) (list => list.Count)) / 30 + 1;
      List<TaskModel> needPullItemsTasks = new List<TaskModel>();
      for (int i = 0; i < batchCount; ++i)
      {
        int start = i * 30;
        int end = (i + 1) * 30;
        SyncTaskBean syncTaskBean = new SyncTaskBean();
        int addEnd;
        int j;
        SyncStatusModel task1;
        if (start < addedTasks.Count)
        {
          addEnd = Math.Min(end, addedTasks.Count);
          for (j = start; j < addEnd; ++j)
          {
            task1 = addedTasks[j];
            TaskModel fullTaskById = await TaskDao.GetFullTaskById(task1.EntityId, true);
            if (fullTaskById != null)
            {
              TaskTransfer.HandleLocalDateChanged(fullTaskById);
              syncTaskBean.Add.Add(fullTaskById);
            }
            else
            {
              UtilLog.Info("big sync: update task // add not found " + task1.EntityId);
              App.Connection.DeleteAsync((object) task1);
            }
            task1 = (SyncStatusModel) null;
          }
        }
        if (start < updatedTasks.Count)
        {
          addEnd = Math.Min(end, updatedTasks.Count);
          for (j = start; j < addEnd; ++j)
          {
            task1 = updatedTasks[j];
            TaskModel fullTaskById = await TaskDao.GetFullTaskById(task1.EntityId, true);
            if (fullTaskById != null)
            {
              if (fullTaskById.kind == "CHECKLIST" && (fullTaskById.items == null || fullTaskById.items.Length == 0))
              {
                needPullItemsTasks.Add(fullTaskById);
                continue;
              }
              TaskTransfer.HandleLocalDateChanged(fullTaskById);
              syncTaskBean.Update.Add(fullTaskById);
            }
            else
            {
              UtilLog.Info("big sync: update task // update not found " + task1.EntityId);
              App.Connection.DeleteAsync((object) task1);
            }
            task1 = (SyncStatusModel) null;
          }
        }
        if (start < deletedTasks.Count)
        {
          addEnd = Math.Min(end, deletedTasks.Count);
          for (j = start; j < addEnd; ++j)
          {
            task1 = deletedTasks[j];
            TaskModel taskById = await TaskDao.GetTaskById(task1.EntityId);
            if (taskById != null)
              syncTaskBean.Delete.Add(new TaskProject()
              {
                taskId = taskById.id,
                projectId = taskById.projectId
              });
            else
              App.Connection.DeleteAsync((object) task1);
            task1 = (SyncStatusModel) null;
          }
        }
        if (start < deletedForeverTasks.Count)
        {
          addEnd = Math.Min(end, deletedForeverTasks.Count);
          for (j = start; j < addEnd; ++j)
          {
            task1 = deletedForeverTasks[j];
            TaskModel taskById = await TaskDao.GetTaskById(task1.EntityId);
            if (taskById != null)
            {
              TaskProject taskProject = new TaskProject()
              {
                taskId = taskById.id,
                projectId = taskById.projectId
              };
              if (CacheManager.GetProjectById(taskProject.projectId) == null)
                taskProject.projectId = Utils.GetInboxId();
              syncTaskBean.DeletedFromTrash.Add(taskProject);
            }
            else
            {
              UtilLog.Info("big sync: update task // deleteForever not found " + task1.EntityId);
              App.Connection.DeleteAsync((object) task1);
            }
            task1 = (SyncStatusModel) null;
          }
        }
        syncTaskBeans.Add(syncTaskBean);
        needPullItemsTasks.ForEach((Action<TaskModel>) (task => TaskService.GetRemoteTaskCheckItems(task.projectId, task.id)));
        syncTaskBean = (SyncTaskBean) null;
      }
      List<SyncTaskBean> syncTaskBeanList = syncTaskBeans;
      syncTaskBeans = (List<SyncTaskBean>) null;
      needPullItemsTasks = (List<TaskModel>) null;
      return syncTaskBeanList;
    }

    private static void HandleLocalDateChanged(TaskModel task)
    {
      DateTime? nullable1;
      if (task.isAllDay.HasValue && task.isAllDay.Value || task.Floating)
      {
        if (task.startDate.HasValue)
        {
          DateTime dateTime1 = DateUtils.ConvertTimeZone(task.startDate.Value, task.timeZone);
          TaskModel taskModel = task;
          DateTime dateTime2 = task.startDate.Value;
          ref DateTime local = ref dateTime2;
          nullable1 = task.startDate;
          TimeSpan timeSpan = nullable1.Value - dateTime1;
          DateTime? nullable2 = new DateTime?(local.Add(timeSpan));
          taskModel.startDate = nullable2;
        }
        nullable1 = task.dueDate;
        if (nullable1.HasValue)
        {
          nullable1 = task.dueDate;
          DateTime dateTime3 = DateUtils.ConvertTimeZone(nullable1.Value, task.timeZone);
          TaskModel taskModel = task;
          nullable1 = task.dueDate;
          DateTime dateTime4 = nullable1.Value;
          ref DateTime local = ref dateTime4;
          nullable1 = task.dueDate;
          TimeSpan timeSpan = nullable1.Value - dateTime3;
          DateTime? nullable3 = new DateTime?(local.Add(timeSpan));
          taskModel.dueDate = nullable3;
        }
      }
      if (task.items == null || !((IEnumerable<TaskDetailItemModel>) task.items).Any<TaskDetailItemModel>())
        return;
      foreach (TaskDetailItemModel taskDetailItemModel1 in task.items)
      {
        if (taskDetailItemModel1.isAllDay.HasValue && taskDetailItemModel1.isAllDay.Value || task.Floating)
        {
          nullable1 = taskDetailItemModel1.startDate;
          if (nullable1.HasValue)
          {
            TaskDetailItemModel taskDetailItemModel2 = taskDetailItemModel1;
            nullable1 = taskDetailItemModel1.startDate;
            DateTime? nullable4 = new DateTime?(TimeZoneUtils.ToLocalTime(nullable1.Value, task.timeZone));
            taskDetailItemModel2.startDate = nullable4;
            TaskDetailItemModel taskDetailItemModel3 = taskDetailItemModel1;
            nullable1 = taskDetailItemModel1.startDate;
            DateTime dateTime = nullable1.Value;
            ref DateTime local = ref dateTime;
            nullable1 = taskDetailItemModel1.startDate;
            string converterValue = UtcDateTimeConverter.GetConverterValue(nullable1.Value);
            string str = local.ToString(converterValue);
            taskDetailItemModel3.serverStartDate = str;
          }
        }
      }
    }
  }
}
