// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.CalendarUtils
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
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Util
{
  public class CalendarUtils
  {
    public static string GetCalendarDefaultAddProjectId()
    {
      ProjectExtra projectExtra = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      if (projectExtra == null || !projectExtra.ProjectIds.Any<string>() || projectExtra.ProjectIds.Contains("#alllists") || projectExtra.ProjectIds.Contains(TaskDefaultDao.GetDefaultSafely().ProjectId) || projectExtra.ProjectIds.Count <= 0 || projectExtra.FilterIds.Count != 0)
        return TaskDefaultDao.GetDefaultSafely().ProjectId;
      List<ProjectModel> list = projectExtra.ProjectIds.Select<string, ProjectModel>(new Func<string, ProjectModel>(CacheManager.GetProjectById)).Where<ProjectModel>((Func<ProjectModel, bool>) (project => project != null && project.IsEnable())).ToList<ProjectModel>();
      if (list.Count <= 0)
        return TaskDefaultDao.GetDefaultSafely().ProjectId;
      list.Sort((Comparison<ProjectModel>) ((l, r) => l.CompareTo(r)));
      return list[0].id;
    }

    public static async Task<string> CheckAddTaskCanShown(string taskId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return (string) null;
      ProjectModel projectById = CacheManager.GetProjectById(thinTaskById.projectId);
      if (projectById == null)
        return (string) null;
      ProjectExtra projectExtra = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      if (projectExtra.IsAll)
        return projectById.inAll || thinTaskById.IsAssignToMe() ? (string) null : Utils.GetString("ProjectHideHintInCalendar");
      if (projectExtra.FilterIds.Any<string>())
      {
        FilterModel filterById = CacheManager.GetFilterById(projectExtra.FilterIds[0]);
        if (filterById != null && !TaskService.IsTaskMatchedFilter(Parser.ToNormalModel(filterById.rule), new TaskBaseViewModel(thinTaskById), true))
          return Utils.GetString("TaskNotShowHintInCalendar");
      }
      if (!projectExtra.ProjectIds.Any<string>() && !projectExtra.GroupIds.Any<string>() && !projectExtra.Tags.Any<string>() && !projectExtra.SubscribeCalendars.Any<string>() && !projectExtra.BindAccounts.Any<string>())
        return (string) null;
      if (projectExtra.ProjectIds.Contains("#alllists"))
        return (string) null;
      List<string> taskTags = TagSerializer.ToTags(thinTaskById.tag);
      return projectExtra.Tags.Contains("*withtags") && taskTags.Any<string>() || projectExtra.ProjectIds.Contains(thinTaskById.projectId) || projectExtra.GroupIds.Contains(projectById.groupId) || projectExtra.Tags.Any<string>((Func<string, bool>) (tag =>
      {
        List<string> stringList = taskTags;
        // ISSUE: explicit non-virtual call
        return stringList != null && __nonvirtual (stringList.Contains(tag.ToLower()));
      })) ? (string) null : Utils.GetString("TaskNotShowHintInCalendar");
    }
  }
}
