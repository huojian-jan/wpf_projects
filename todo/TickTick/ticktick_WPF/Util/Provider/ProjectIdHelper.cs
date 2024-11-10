// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ProjectIdHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Service;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public static class ProjectIdHelper
  {
    public static ProjectIdentity GetProjectIdentity(string projectId)
    {
      ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
      if (project == null)
        return (ProjectIdentity) null;
      NormalProjectIdentity normalProjectIdentity = new NormalProjectIdentity(project);
      normalProjectIdentity.SortOption = project.GetSortOption();
      NormalProjectIdentity projectIdentity = normalProjectIdentity;
      if (project.Isinbox)
        projectIdentity.SortOption = SmartProjectService.GetSmartProjectSortOption("inbox", false);
      return (ProjectIdentity) projectIdentity;
    }

    public static ProjectIdentity GetGroupIdentity(string groupId)
    {
      ProjectGroupModel group = CacheManager.GetProjectGroups().FirstOrDefault<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (g => g.id == groupId));
      return group == null ? (ProjectIdentity) null : (ProjectIdentity) new GroupProjectIdentity(group, CacheManager.GetProjectsInGroup(groupId));
    }

    public static ProjectIdentity GetFilterIdentity(string id)
    {
      FilterModel filter = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == id));
      if (filter == null)
        return (ProjectIdentity) null;
      FilterProjectIdentity filterIdentity = new FilterProjectIdentity(filter);
      filterIdentity.SortOption = filter.GetSortOption();
      return (ProjectIdentity) filterIdentity;
    }

    public static ProjectIdentity GetCalendarIdentity(string id)
    {
      CalendarSubscribeProfileModel profile = CacheManager.GetSubscribeCalendars().FirstOrDefault<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (c => c.Id == id));
      if (profile != null)
        return (ProjectIdentity) new SubscribeCalendarProjectIdentity(profile);
      BindCalendarAccountModel account = CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (c => c.Id == id));
      return account != null ? (ProjectIdentity) new BindAccountCalendarProjectIdentity(account) : (ProjectIdentity) null;
    }

    public static ProjectIdentity GetTagIdentity(string name)
    {
      TagModel tag = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (f => f.name == name));
      return tag != null ? (ProjectIdentity) new TagProjectIdentity(tag) : (ProjectIdentity) null;
    }
  }
}
