// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TaskOperationHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class TaskOperationHelper
  {
    public static async Task<OperationExtra> GetTaskAccessInfo(
      string taskId,
      bool canShowAddSubtask = true)
    {
      OperationExtra extra = new OperationExtra();
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task != null)
      {
        if (!string.IsNullOrEmpty(task.tag))
          task.tags = TagSerializer.ToTags(task.tag).ToArray();
        if (!string.IsNullOrEmpty(task.exDates))
          task.exDate = ExDateSerilizer.ToArray(task.exDates);
        extra.TaskId = taskId;
        extra.Priority = task.priority;
        extra.ProjectId = task.projectId;
        extra.ColumnId = task.columnId;
        extra.Assignee = task.assignee;
        extra.TaskType = task.kind == "NOTE" ? TaskType.Note : TaskType.Task;
        if (extra.TaskType == TaskType.Task)
          extra.IsAbandoned = new bool?(task.status == -1);
        List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.id);
        OperationExtra operationExtra1 = extra;
        TimeData timeData = new TimeData();
        timeData.StartDate = task.startDate;
        timeData.DueDate = task.dueDate;
        timeData.IsAllDay = new bool?(!task.startDate.HasValue || ((int) task.isAllDay ?? 1) != 0);
        timeData.Reminders = remindersByTaskId != null ? remindersByTaskId.ToList<TaskReminderModel>() : (List<TaskReminderModel>) null;
        timeData.RepeatFrom = task.repeatFrom;
        timeData.RepeatFlag = task.repeatFlag;
        timeData.HasTime = task.startDate.HasValue;
        string[] exDate = task.exDate;
        timeData.ExDates = exDate != null ? ((IEnumerable<string>) exDate).ToList<string>() : (List<string>) null;
        timeData.TimeZone = new TimeZoneViewModel(task.Floating, task.timeZone);
        operationExtra1.TimeModel = timeData;
        DateTime? nullable1 = task.startDate;
        if (!nullable1.HasValue)
        {
          TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
          DateTime? nullable2 = defaultSafely.GetDefaultDateTime();
          if (!nullable2.HasValue)
          {
            nullable2 = new DateTime?(DateTime.Today);
            task.isAllDay = new bool?(true);
            if (defaultSafely.DateMode == 1)
            {
              task.dueDate = new DateTime?(nullable2.Value.AddMinutes((double) defaultSafely.Duration));
              task.isAllDay = new bool?(Constants.DurationValue.IsAllDayValue(defaultSafely.Duration));
            }
            task.reminders = !task.isAllDay.HasValue || !task.isAllDay.Value ? TimeData.GetDefaultTimeReminders().ToArray() : TimeData.GetDefaultAllDayReminders().ToArray();
          }
          task.startDate = nullable2;
        }
        extra.ShowCopy = true;
        extra.ShowCopyLink = true;
        extra.ShowCreateSubTask = canShowAddSubtask && !string.IsNullOrEmpty(taskId) && TaskDao.GetTaskLevel(taskId, task.projectId) < 4;
        OperationExtra operationExtra2 = extra;
        TagSelectData tagSelectData = new TagSelectData();
        string[] tags = task.tags;
        tagSelectData.OmniSelectTags = (tags != null ? ((IEnumerable<string>) tags).ToList<string>() : (List<string>) null) ?? new List<string>();
        operationExtra2.Tags = tagSelectData;
        extra.ShowPomo = LocalSettings.Settings.EnableFocus && task.status == 0;
        extra.ShowSkip = !TaskService.IsNonRepeatTask(task);
        extra.ShowDate = string.IsNullOrEmpty(task.attendId) || AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) task);
        bool flag = AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) task);
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == task.projectId));
        extra.ShowAssignTo = projectModel != null && projectModel.IsShareList() && string.IsNullOrEmpty(task.attendId) | flag;
        extra.CanSwitch = task.kind == "NOTE" || task.status == 0 && !TaskCache.IsParentTask(task.id) && !TaskCache.IsChildTask(task.parentId, task.projectId) && string.IsNullOrEmpty(task.attendId);
        extra.ShowSwitch = task.status == 0 && string.IsNullOrEmpty(task.attendId);
        extra.FailedSwitchTips = Utils.GetString("CannotConvertRelatedTaskToNote");
        OperationExtra operationExtra3 = extra;
        DateTime? nullable3;
        if (task.status != 2)
        {
          nullable1 = new DateTime?();
          nullable3 = nullable1;
        }
        else
          nullable3 = task.completedTime;
        operationExtra3.CompleteTime = nullable3;
      }
      OperationExtra taskAccessInfo = extra;
      extra = (OperationExtra) null;
      return taskAccessInfo;
    }

    public static bool CheckIfAgendaAllowClearDate(string attendId, DependencyObject parent)
    {
      if (string.IsNullOrEmpty(attendId))
        return true;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("ClearDate"), Utils.GetString("AgendaClearDateHint"), Utils.GetString("PublicClear"), Utils.GetString("Cancel"));
      customerDialog.Owner = Window.GetWindow(parent);
      bool? nullable = customerDialog.ShowDialog();
      return nullable.HasValue && nullable.Value;
    }

    public static async Task<bool> CheckIfTaskAllowClearDate(string taskId, DependencyObject parent)
    {
      return TaskOperationHelper.CheckIfAgendaAllowClearDate((await TaskDao.GetThinTaskById(taskId)).attendId, parent);
    }

    public static async Task<bool> CheckIfAllowDeleteTask(string taskId, DependencyObject parent)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      return string.IsNullOrEmpty(thinTaskById.attendId) || await TaskOperationHelper.CheckIfAllowDeleteAgenda(thinTaskById, parent);
    }

    public static async Task<bool> CheckIfAllowDeleteAgenda(TaskModel task, DependencyObject parent)
    {
      if (task != null && !string.IsNullOrEmpty(task.attendId))
      {
        if (AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) task))
        {
          CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("DeleteAgenda"), Utils.GetString("DeleteAgendaHint"), Utils.GetString("Delete"), Utils.GetString("Cancel"));
          customerDialog.Owner = parent == null ? (Window) null : Window.GetWindow(parent);
          bool? nullable = customerDialog.ShowDialog();
          if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
            return false;
        }
        else
        {
          CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("DeleteAgenda"), Utils.GetString("DeleteAttendAgendaHint"), Utils.GetString("Delete"), Utils.GetString("Cancel"));
          customerDialog.Owner = parent == null ? (Window) null : Window.GetWindow(parent);
          bool? nullable = customerDialog.ShowDialog();
          if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
            return false;
        }
      }
      return true;
    }
  }
}
