// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MoveToastHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views
{
  public static class MoveToastHelper
  {
    public static bool CheckTaskMatched(ProjectIdentity identity, TaskModel task)
    {
      return task == null || identity == null || LocalSettings.Settings.InSearch || MoveToastHelper.CheckTaskMatched(identity, new TaskBaseViewModel(task));
    }

    public static bool CheckTaskMatched(ProjectIdentity identity, TaskBaseViewModel task)
    {
      List<TaskBaseViewModel> taskBaseViewModelList = new List<TaskBaseViewModel>()
      {
        task
      };
      switch (identity)
      {
        case AllProjectIdentity _:
          ProjectModel projectById = ProjectAndTaskIdsCache.GetProjectById(task.ProjectId);
          return projectById != null && projectById.IsValid() && (projectById.inAll || task.IsAssignToMe());
        case TodayProjectIdentity _:
          List<TaskBaseViewModel> list1 = TaskViewModelHelper.GetTasks(taskBaseViewModelList, startEndPairs: new List<FilterDatePair>()
          {
            new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(1.0)))
          }, inTodayOrWeek: true, inAll: true).ToList<TaskBaseViewModel>();
          // ISSUE: explicit non-virtual call
          return list1 != null && __nonvirtual (list1.Count) > 0;
        case TomorrowProjectIdentity _:
          List<TaskBaseViewModel> list2 = TaskViewModelHelper.GetTasks(taskBaseViewModelList, startEndPairs: new List<FilterDatePair>()
          {
            new FilterDatePair(new DateTime?(DateTime.Today.AddDays(1.0)), new DateTime?(DateTime.Today.AddDays(2.0)))
          }, inAll: true).ToList<TaskBaseViewModel>();
          // ISSUE: explicit non-virtual call
          return list2 != null && __nonvirtual (list2.Count) > 0;
        case WeekProjectIdentity _:
          List<TaskBaseViewModel> list3 = TaskViewModelHelper.GetTasks(taskBaseViewModelList, startEndPairs: new List<FilterDatePair>()
          {
            new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(7.0)))
          }, inTodayOrWeek: true, inAll: true).ToList<TaskBaseViewModel>();
          // ISSUE: explicit non-virtual call
          return list3 != null && __nonvirtual (list3.Count) > 0;
        case GroupProjectIdentity groupProjectIdentity:
          List<TaskBaseViewModel> list4 = TaskViewModelHelper.GetTasks(taskBaseViewModelList, new List<string>()
          {
            groupProjectIdentity.GroupId
          }).ToList<TaskBaseViewModel>();
          // ISSUE: explicit non-virtual call
          return list4 != null && __nonvirtual (list4.Count) > 0;
        case AssignToMeProjectIdentity _:
          List<TaskBaseViewModel> list5 = TaskViewModelHelper.GetTasks(taskBaseViewModelList, assignTo: new List<string>()
          {
            "me"
          }).ToList<TaskBaseViewModel>();
          // ISSUE: explicit non-virtual call
          return list5 != null && __nonvirtual (list5.Count) > 0;
        default:
          TagProjectIdentity tagProjectIdentity = identity as TagProjectIdentity;
          if (tagProjectIdentity == null)
          {
            switch (identity)
            {
              case FilterProjectIdentity filterProjectIdentity:
                List<TaskBaseViewModel> tasksMatchedFilter = TaskService.GetTasksMatchedFilter(filterProjectIdentity.Filter, taskBaseViewModelList);
                // ISSUE: explicit non-virtual call
                return tasksMatchedFilter != null && __nonvirtual (tasksMatchedFilter.Count) > 0;
              case NormalProjectIdentity normalProjectIdentity:
                return task.ProjectId == normalProjectIdentity.Id;
              case SearchProjectIdentity _:
              case CompletedProjectIdentity _:
              case AbandonedProjectIdentity _:
                return true;
              default:
                return false;
            }
          }
          else
          {
            List<string> list6 = CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.name == tagProjectIdentity.Tag || t.parent == tagProjectIdentity.Tag)).Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>();
            List<TaskBaseViewModel> list7 = TaskViewModelHelper.GetTasks(taskBaseViewModelList, tags: list6, showComplete: !LocalSettings.Settings.HideComplete).ToList<TaskBaseViewModel>();
            // ISSUE: explicit non-virtual call
            return list7 != null && __nonvirtual (list7.Count) > 0;
          }
      }
    }
  }
}
