// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.TaskItemLoadHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public static class TaskItemLoadHelper
  {
    public static async Task LoadIndicator(List<DisplayItemModel> models, bool loadPomo = true)
    {
      if (models == null || models.Count <= 0 || !loadPomo)
        return;
      List<PomodoroSummaryModel> pomosByTaskIds = await PomoSummaryDao.GetPomosByTaskIds(models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.IsTask)).Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (d => d.TaskId)).ToList<string>());
      Dictionary<string, PomodoroSummaryModel> dictionary = new Dictionary<string, PomodoroSummaryModel>();
      foreach (PomodoroSummaryModel pomodoroSummaryModel in pomosByTaskIds)
      {
        if (!string.IsNullOrEmpty(pomodoroSummaryModel.taskId) && !dictionary.ContainsKey(pomodoroSummaryModel.taskId))
          dictionary[pomodoroSummaryModel.taskId] = pomodoroSummaryModel;
      }
      foreach (DisplayItemModel model in models)
      {
        if (dictionary.Count > 0 && model.IsTask && dictionary.ContainsKey(model.Id))
        {
          model.PomoSummary = dictionary[model.Id];
          model.ShowPomo = !model.PomoSummary.IsEmpty();
        }
      }
    }

    public static async void LoadAssigneeName(DisplayItemModel model)
    {
      AssigneeSection section = (AssigneeSection) model.Section;
      if (string.IsNullOrEmpty(section.Assingee) || !(section.Assingee != "-1"))
        return;
      if (section.Assingee == LocalSettings.Settings.LoginUserId)
      {
        model.SourceViewModel.Title = Utils.GetString("Me");
      }
      else
      {
        string userName = await AvatarHelper.GetUserName(section.Assingee, section.ProjectId);
        if (string.IsNullOrEmpty(userName))
          return;
        model.SourceViewModel.Title = userName;
      }
    }

    private static async Task LoadAvatar(DisplayItemModel model)
    {
      if (!model.IsItem && !model.IsTaskOrNote || string.IsNullOrEmpty(model.Assignee) || !(model.Assignee != "-1"))
        return;
      DisplayItemModel displayItemModel = model;
      displayItemModel.AvatarUrl = await AvatarHelper.GetAvatarUrl(model.Assignee, model.ProjectId);
      displayItemModel = (DisplayItemModel) null;
    }

    public static async Task LoadShowReminder(DisplayItemModel model)
    {
      if (model == null)
        return;
      if (model.IsItem)
      {
        DateTime? startDate = model.StartDate;
        if (!startDate.HasValue)
          return;
        startDate = model.StartDate;
        DateTime now = DateTime.Now;
        if ((startDate.HasValue ? (startDate.GetValueOrDefault() > now ? 1 : 0) : 0) == 0)
          return;
        bool? isAllDay = model.IsAllDay;
        if (!isAllDay.HasValue)
          return;
        isAllDay = model.IsAllDay;
        if (isAllDay.Value)
          return;
        model.SetShowReminder(true);
      }
      else if (model.IsEvent || model.IsCourse)
      {
        DateTime? startDate = model.StartDate;
        DateTime now = DateTime.Now;
        if ((startDate.HasValue ? (startDate.GetValueOrDefault() >= now ? 1 : 0) : 0) == 0)
          return;
        model.SetShowReminder(!string.IsNullOrEmpty(model.ReminderString));
      }
      else
      {
        if (!model.IsTaskOrNote)
          return;
        DateTime? date = model.StartDate;
        if (model.Status == 0 && date.HasValue)
        {
          bool? isAllDay = model.IsAllDay;
          if (((int) isAllDay ?? 1) != 0)
          {
            DateTime? nullable = date;
            DateTime today = DateTime.Today;
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() < today ? 1 : 0) : 0) != 0)
              goto label_22;
          }
          isAllDay = model.IsAllDay;
          if (((int) isAllDay ?? 1) == 0)
          {
            DateTime? nullable = date;
            DateTime now = DateTime.Now;
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() < now ? 1 : 0) : 0) != 0)
              goto label_22;
          }
          if (model.Deleted == 0)
          {
            DateTime? remindTime = model.RemindTime;
            if (remindTime.HasValue)
            {
              remindTime = model.RemindTime;
              DateTime now = DateTime.Now;
              if ((remindTime.HasValue ? (remindTime.GetValueOrDefault() > now ? 1 : 0) : 0) != 0)
              {
                model.SetShowReminder(true);
                return;
              }
            }
            List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(model.Id);
            // ISSUE: explicit non-virtual call
            if (remindersByTaskId != null && __nonvirtual (remindersByTaskId.Count) > 0)
            {
              model.SetShowReminder(remindersByTaskId.Any<TaskReminderModel>((Func<TaskReminderModel, bool>) (r => DateTime.Now <= date.Value - TriggerUtils.ParseTrigger(r.trigger))));
              return;
            }
            model.SetShowReminder(false);
            return;
          }
        }
label_22:
        model.SetShowReminder(false);
      }
    }

    private static async void LoadCourseReminder(DisplayItemModel model)
    {
    }
  }
}
