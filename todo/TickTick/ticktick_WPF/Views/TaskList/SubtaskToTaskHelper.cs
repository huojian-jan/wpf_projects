// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.SubtaskToTaskHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public static class SubtaskToTaskHelper
  {
    public static async Task<TaskPrimaryProperty> GetTaskPrimaryProperty(
      TaskPrimaryProperty primaryTaskProperty,
      ProjectIdentity projectIdentity,
      DisplayItemModel dropTarget,
      bool below,
      Section dropSection,
      bool checkSection)
    {
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      TaskPrimaryProperty defaultProperty = new TaskPrimaryProperty()
      {
        Priority = new int?(defaultSafely.Priority),
        ProjectId = Utils.GetInboxId(),
        TimeData = TimeData.BuildFromDefault(defaultSafely)
      };
      TimeData timeData = primaryTaskProperty.TimeData;
      bool? isAllDay;
      int num;
      if (timeData == null)
      {
        num = 0;
      }
      else
      {
        isAllDay = timeData.IsAllDay;
        num = isAllDay.HasValue ? 1 : 0;
      }
      if (num != 0)
      {
        isAllDay = primaryTaskProperty.TimeData.IsAllDay;
        if (!isAllDay.Value)
          defaultProperty.TimeData.Reminders = TimeData.GetDefaultTimeReminders();
      }
      TaskPrimaryProperty taskPrimaryProperty = await SubtaskToTaskHelper.MergeTaskProperty(primaryTaskProperty, defaultProperty, await SubtaskToTaskHelper.GetDropDefaultProperty(projectIdentity.SortOption.groupBy, dropTarget, below, dropSection, checkSection));
      defaultProperty = (TaskPrimaryProperty) null;
      return taskPrimaryProperty;
    }

    public static async Task<TaskPrimaryProperty> MergeTaskProperty(
      TaskPrimaryProperty taskProperty,
      TaskPrimaryProperty defaultProperty,
      TaskPrimaryProperty dropProperty)
    {
      TaskPrimaryProperty result = new TaskPrimaryProperty()
      {
        Priority = defaultProperty.Priority,
        ProjectId = defaultProperty.ProjectId,
        TaskStatus = dropProperty.TaskStatus,
        AssigneeId = dropProperty.AssigneeId,
        SortOrder = dropProperty.SortOrder,
        ParentId = taskProperty.ParentId
      };
      SubtaskToTaskHelper.MergePriority(taskProperty, dropProperty, result);
      SubtaskToTaskHelper.MergeProjectId(taskProperty, dropProperty, result);
      SubtaskToTaskHelper.MergeTimeData(defaultProperty, taskProperty, dropProperty, result);
      SubtaskToTaskHelper.MergeTags(taskProperty, dropProperty, result);
      return result;
    }

    private static void MergeTags(
      TaskPrimaryProperty taskProperty,
      TaskPrimaryProperty dropProperty,
      TaskPrimaryProperty result)
    {
      result.Tags = new List<string>();
      if (taskProperty.Tags != null && taskProperty.Tags.Any<string>())
        result.Tags.AddRange((IEnumerable<string>) taskProperty.Tags);
      if (dropProperty.Tags == null || !dropProperty.Tags.Any<string>())
        return;
      result.Tags.AddRange((IEnumerable<string>) dropProperty.Tags);
    }

    private static void MergeTimeData(
      TaskPrimaryProperty defaultProperty,
      TaskPrimaryProperty taskProperty,
      TaskPrimaryProperty dropProperty,
      TaskPrimaryProperty result)
    {
      result.TimeData = new TimeData();
      if (taskProperty.TimeData != null && !Utils.IsEmptyDate(taskProperty.TimeData.StartDate))
      {
        result.TimeData.StartDate = taskProperty.TimeData.StartDate;
        result.TimeData.DueDate = taskProperty.TimeData.DueDate;
        result.TimeData.IsAllDay = taskProperty.TimeData.IsAllDay;
        if (!string.IsNullOrEmpty(taskProperty.TimeData.RepeatFlag))
        {
          result.TimeData.RepeatFrom = taskProperty.TimeData.RepeatFrom;
          result.TimeData.RepeatFlag = taskProperty.TimeData.RepeatFlag;
        }
        if (taskProperty.TimeData.Reminders != null && taskProperty.TimeData.Reminders.Any<TaskReminderModel>())
        {
          result.TimeData.Reminders = new List<TaskReminderModel>();
          foreach (TaskReminderModel reminder in taskProperty.TimeData.Reminders)
            result.TimeData.Reminders.Add(new TaskReminderModel()
            {
              trigger = reminder.trigger
            });
        }
      }
      DateTime? nullable1;
      if (dropProperty.TimeData != null)
      {
        result.TimeData.StartDate = dropProperty.TimeData.StartDate;
        result.TimeData.IsAllDay = dropProperty.TimeData.IsAllDay;
        TimeData timeData1 = taskProperty.TimeData;
        int num1;
        if (timeData1 == null)
        {
          num1 = 0;
        }
        else
        {
          nullable1 = timeData1.StartDate;
          num1 = nullable1.HasValue ? 1 : 0;
        }
        if (num1 != 0)
        {
          result.TimeData.IsAllDay = taskProperty.TimeData.IsAllDay;
          result.TimeData.StartDate = taskProperty.TimeData.StartDate;
          nullable1 = dropProperty.TimeData.StartDate;
          if (nullable1.HasValue)
          {
            nullable1 = dropProperty.TimeData.StartDate;
            DateTime date1 = nullable1.Value.Date;
            nullable1 = taskProperty.TimeData.StartDate;
            DateTime dateTime1 = nullable1.Value;
            DateTime date2 = dateTime1.Date;
            int totalDays = (int) (date1 - date2).TotalDays;
            TimeData timeData2 = result.TimeData;
            nullable1 = result.TimeData.StartDate;
            dateTime1 = nullable1.Value;
            DateTime? nullable2 = new DateTime?(dateTime1.AddDays((double) totalDays));
            timeData2.StartDate = nullable2;
            TimeData timeData3 = taskProperty.TimeData;
            int num2;
            if (timeData3 == null)
            {
              num2 = 0;
            }
            else
            {
              nullable1 = timeData3.DueDate;
              num2 = nullable1.HasValue ? 1 : 0;
            }
            if (num2 != 0)
            {
              nullable1 = taskProperty.TimeData.DueDate;
              DateTime dateTime2 = nullable1.Value;
              nullable1 = taskProperty.TimeData.StartDate;
              DateTime dateTime3 = nullable1.Value;
              double totalMinutes = (dateTime2 - dateTime3).TotalMinutes;
              TimeData timeData4 = result.TimeData;
              nullable1 = result.TimeData.StartDate;
              dateTime1 = nullable1.Value;
              DateTime? nullable3 = new DateTime?(dateTime1.AddMinutes(totalMinutes));
              timeData4.DueDate = nullable3;
            }
          }
        }
      }
      if (result.TimeData != null && (result.TimeData.Reminders == null || !result.TimeData.Reminders.Any<TaskReminderModel>()))
      {
        nullable1 = result.TimeData.StartDate;
        if (nullable1.HasValue && !Utils.IsEmptyDate(result.TimeData.StartDate))
        {
          TimeData timeData = taskProperty.TimeData;
          bool? isAllDay;
          int num;
          if (timeData == null)
          {
            num = 0;
          }
          else
          {
            isAllDay = timeData.IsAllDay;
            num = isAllDay.HasValue ? 1 : 0;
          }
          if (num != 0 && taskProperty.TimeData?.Reminders != null && taskProperty.TimeData.Reminders.Any<TaskReminderModel>())
          {
            isAllDay = taskProperty.TimeData.IsAllDay;
            if (isAllDay.Value)
              result.TimeData.Reminders = taskProperty.TimeData.Reminders;
          }
        }
      }
      if (result.TimeData != null && !Utils.IsEmptyDate(result.TimeData.StartDate) && (result.TimeData.Reminders == null || !result.TimeData.Reminders.Any<TaskReminderModel>()))
        result.TimeData.Reminders = defaultProperty.TimeData.Reminders;
      result.TimeData.TimeZone = taskProperty.TimeData == null ? defaultProperty.TimeData.TimeZone : taskProperty.TimeData.TimeZone;
    }

    private static void MergeProjectId(
      TaskPrimaryProperty taskProperty,
      TaskPrimaryProperty dropProperty,
      TaskPrimaryProperty result)
    {
      if (!string.IsNullOrEmpty(taskProperty.ProjectId))
        result.ProjectId = taskProperty.ProjectId;
      if (string.IsNullOrEmpty(dropProperty.ProjectId))
        return;
      result.ProjectId = dropProperty.ProjectId;
    }

    private static void MergePriority(
      TaskPrimaryProperty taskProperty,
      TaskPrimaryProperty dropProperty,
      TaskPrimaryProperty result)
    {
      int? priority;
      if (taskProperty.Priority.HasValue)
      {
        priority = taskProperty.Priority;
        int num = 0;
        if (!(priority.GetValueOrDefault() == num & priority.HasValue))
          result.Priority = taskProperty.Priority;
      }
      priority = dropProperty.Priority;
      if (priority.HasValue)
        result.Priority = dropProperty.Priority;
      priority = result.Priority;
      if (priority.HasValue)
        return;
      result.Priority = new int?(0);
    }

    public static TaskPrimaryProperty GetProjectDefaultProperty(IProjectTaskDefault project)
    {
      TaskPrimaryProperty projectDefaultProperty = new TaskPrimaryProperty();
      switch (project)
      {
        case FilterProjectIdentity filterProjectIdentity:
          projectDefaultProperty.Priority = new int?(filterProjectIdentity.GetPriority());
          projectDefaultProperty.ProjectId = filterProjectIdentity.GetProjectId();
          projectDefaultProperty.Tags = filterProjectIdentity.GetTags();
          projectDefaultProperty.TimeData = filterProjectIdentity.GetTimeData();
          break;
        case TagProjectIdentity tagProjectIdentity:
          projectDefaultProperty.Tags = tagProjectIdentity.GetTags();
          break;
        case NormalProjectIdentity normalProjectIdentity:
          projectDefaultProperty.ProjectId = normalProjectIdentity.GetProjectId();
          break;
        case GroupProjectIdentity groupProjectIdentity:
          projectDefaultProperty.ProjectId = groupProjectIdentity.GetProjectId();
          break;
        default:
          projectDefaultProperty.TimeData = project.GetTimeData();
          break;
      }
      return projectDefaultProperty;
    }

    private static async Task<TaskPrimaryProperty> GetDropDefaultProperty(
      string groupBy,
      DisplayItemModel dropTarget,
      bool below,
      Section dropSection,
      bool checkSection)
    {
      TaskPrimaryProperty taskDefault = new TaskPrimaryProperty();
      if (dropSection is CompletedSection)
      {
        taskDefault.TaskStatus = 2;
        return taskDefault;
      }
      TaskPrimaryProperty taskPrimaryProperty;
      switch (groupBy)
      {
        case "tag":
          if (checkSection && dropSection != null && dropSection is TagSection tagSection)
          {
            taskDefault.Tags = TagSerializer.ToTags(tagSection.GetTag());
            break;
          }
          break;
        case "project":
        case "sortOrder":
          if (dropTarget != null)
          {
            taskDefault.ProjectId = dropTarget.ProjectId;
            taskPrimaryProperty = taskDefault;
            taskPrimaryProperty.SortOrder = await ProjectSortOrderDao.GetSiblingSortOrderInProject(dropTarget.ProjectId, dropTarget.TaskId, !below);
            taskPrimaryProperty = (TaskPrimaryProperty) null;
            break;
          }
          break;
        case "priority":
          if (checkSection && dropTarget != null)
          {
            taskDefault.ProjectId = dropTarget.ProjectId;
            taskPrimaryProperty = taskDefault;
            taskPrimaryProperty.SortOrder = await ProjectSortOrderDao.GetSiblingSortOrderInProject(dropTarget.ProjectId, dropTarget.TaskId, !below);
            taskPrimaryProperty = (TaskPrimaryProperty) null;
            taskDefault.Priority = new int?(dropTarget.Priority);
            break;
          }
          break;
        case "dueDate":
          if (checkSection)
          {
            DisplayItemModel displayItemModel = dropTarget;
            DateTime? startDate;
            int num;
            if (displayItemModel == null)
            {
              num = 0;
            }
            else
            {
              startDate = displayItemModel.StartDate;
              num = startDate.HasValue ? 1 : 0;
            }
            if (num != 0)
            {
              TaskPrimaryProperty taskPrimaryProperty1 = taskDefault;
              TimeData timeData = new TimeData();
              startDate = dropTarget.StartDate;
              timeData.StartDate = new DateTime?(startDate.Value.Date);
              timeData.IsAllDay = new bool?(true);
              taskPrimaryProperty1.TimeData = timeData;
            }
            if (dropSection != null)
            {
              taskDefault.TimeData = new TimeData()
              {
                StartDate = dropSection.GetStartDate(),
                IsAllDay = new bool?(true)
              };
              break;
            }
            break;
          }
          break;
        case "assignee":
          if (checkSection && dropSection != null)
          {
            taskDefault.AssigneeId = dropSection.GetAssignee();
            break;
          }
          break;
      }
      return taskDefault;
    }
  }
}
