// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.GuideProjectModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class GuideProjectModel
  {
    public string name { get; set; }

    public GuideProjectData metaData { get; set; }

    public string id { get; set; }

    public ProjectModel ToProject()
    {
      if (string.IsNullOrEmpty(this.id))
        this.id = Utils.GetGuid();
      if (this.metaData == null)
        return (ProjectModel) null;
      return new ProjectModel()
      {
        id = this.id,
        name = this.name,
        sortType = this.metaData.sortType,
        viewMode = this.metaData.viewMode,
        userid = LocalSettings.Settings.LoginUserId,
        sync_status = Constants.SyncStatus.SYNC_NEW.ToString(),
        kind = Constants.ProjectKind.TASK.ToString()
      };
    }

    public List<ColumnModel> ToColumns()
    {
      if (string.IsNullOrEmpty(this.id))
        this.id = Utils.GetGuid();
      GuideProjectData metaData = this.metaData;
      if (metaData == null)
        return (List<ColumnModel>) null;
      List<GuideProjectColumn> columns = metaData.columns;
      return columns == null ? (List<ColumnModel>) null : columns.Select<GuideProjectColumn, ColumnModel>((Func<GuideProjectColumn, ColumnModel>) (c => new ColumnModel()
      {
        id = c.id,
        projectId = this.id,
        name = c.name,
        sortOrder = new long?(c.sortOrder),
        syncStatus = "new",
        userId = LocalSettings.Settings.LoginUserId
      })).ToList<ColumnModel>();
    }

    public List<TaskModel> ToTasks()
    {
      if (string.IsNullOrEmpty(this.id))
        this.id = Utils.GetGuid();
      if (this.metaData?.tasks == null)
        return (List<TaskModel>) null;
      List<TaskModel> tasks = new List<TaskModel>();
      long num = 0;
      foreach (GuideProjectTask task in this.metaData.tasks)
      {
        string tId = task.id ?? Utils.GetGuid();
        TaskModel taskModel1 = new TaskModel();
        taskModel1.id = tId;
        taskModel1.title = task.title;
        taskModel1.content = task.content;
        taskModel1.desc = task.desc;
        taskModel1.columnId = task.columnId;
        taskModel1.parentId = task.parentId;
        taskModel1.projectId = this.id;
        taskModel1.repeatFlag = task.repeatFlag;
        taskModel1.repeatFrom = task.repeatFrom;
        taskModel1.timeZone = task.timeZone;
        taskModel1.startDate = task.startDate;
        taskModel1.dueDate = task.dueDate;
        taskModel1.isFloating = task.floating;
        taskModel1.isAllDay = task.allDay;
        taskModel1.priority = task.priority;
        taskModel1.tags = task.tags.ToArray();
        taskModel1.userId = LocalSettings.Settings.LoginUserId;
        taskModel1.creator = LocalSettings.Settings.LoginUserId;
        List<string> reminders = task.reminders;
        taskModel1.reminders = reminders != null ? reminders.Select<string, TaskReminderModel>((Func<string, TaskReminderModel>) (r => new TaskReminderModel()
        {
          id = Utils.GetGuid(),
          taskserverid = tId,
          trigger = r
        })).ToArray<TaskReminderModel>() : (TaskReminderModel[]) null;
        List<GuideProjectTaskItem> items = task.items;
        taskModel1.items = items != null ? items.Select<GuideProjectTaskItem, TaskDetailItemModel>((Func<GuideProjectTaskItem, TaskDetailItemModel>) (i => new TaskDetailItemModel()
        {
          id = Utils.GetGuid(),
          title = i.title,
          startDate = i.startDate,
          isAllDay = i.allDay,
          TaskServerId = tId
        })).ToArray<TaskDetailItemModel>() : (TaskDetailItemModel[]) null;
        taskModel1.Actions = task.actions == null ? (string) null : JsonConvert.SerializeObject((object) task.actions);
        taskModel1.Resources = task.resources == null ? (string) null : JsonConvert.SerializeObject((object) task.resources);
        taskModel1.label = task.label;
        taskModel1.sortOrder = num;
        TaskModel taskModel2 = taskModel1;
        tasks.Add(taskModel2);
        num += 268435456L;
      }
      return tasks;
    }
  }
}
