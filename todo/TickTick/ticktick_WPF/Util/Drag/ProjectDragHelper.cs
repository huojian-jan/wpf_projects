// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Drag.ProjectDragHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Drag
{
  public static class ProjectDragHelper
  {
    public static bool CanSwapItems(
      ProjectItemViewModel current,
      ProjectItemViewModel next,
      bool isUpDowm)
    {
      if (current != null && next != null)
      {
        switch (current)
        {
          case SmartProjectViewModel smart2:
            if (next is SmartProjectViewModel smart1)
              return ProjectDragHelper.CanSmartSwap(smart2) && ProjectDragHelper.CanSmartSwap(smart1);
            break;
          case FilterProjectViewModel _:
            if (next is FilterProjectViewModel)
              return true;
            break;
          case TagProjectViewModel projectViewModel3:
            if (next.InSubSection != projectViewModel3.InSubSection)
              return false;
            if (!projectViewModel3.IsParent)
              return next is TagProjectViewModel || next is EmptySubViewModel;
            if (next is EmptySubViewModel & isUpDowm)
              return true;
            if (!(next is TagProjectViewModel projectViewModel1))
              return false;
            if (!projectViewModel1.IsSubItem && !projectViewModel1.IsParent)
              return true;
            if (!projectViewModel1.IsParent)
              return false;
            return !isUpDowm || !projectViewModel1.Open;
          case ProjectGroupViewModel _:
            if (next is EmptySubViewModel & isUpDowm || next is ProjectGroupViewModel && (!isUpDowm || !next.Open))
              return true;
            return next is NormalProjectViewModel projectViewModel2 && !projectViewModel2.IsSubItem;
          case PtfAllViewModel _:
            return next is PtfAllViewModel;
        }
        if (ProjectDragHelper.IsProjectDragCategory(current) && ProjectDragHelper.IsProjectDragCategory(next))
          return true;
      }
      return false;
    }

    private static bool CanSmartSwap(SmartProjectViewModel smart)
    {
      return smart.Project is AllProject || smart.Project is TodayProject || smart.Project is TomorrowProject || smart.Project is WeekProject || smart.Project is AssignProject || smart.Project is InboxProject || smart.Project is SummaryProject;
    }

    private static bool IsProjectDragCategory(ProjectItemViewModel model)
    {
      switch (model)
      {
        case null:
          return false;
        case NormalProjectViewModel _:
        case EmptySubViewModel _:
          return true;
        case ProjectGroupViewModel _:
          return !(model is ClosedProjectGroupViewModel);
        default:
          return false;
      }
    }

    public static long GetNewSortOrder(string groupId, string teamId)
    {
      ProjectDragHelper.SortIdentity sortIdentity = ProjectDragHelper.GetPrimaryIdentities(teamId).FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (p => p.Identity == groupId));
      return sortIdentity != null && sortIdentity.Children.Any<ProjectDragHelper.SortIdentity>() ? sortIdentity.Children[0].SortOrder - 268435456L : -1L;
    }

    public static long GetNewSortOrder(
      bool upDown,
      string groupId,
      string projectId,
      string teamId)
    {
      ProjectDragHelper.SortIdentity sortIdentity1 = ProjectDragHelper.GetPrimaryIdentities(teamId).FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (p => p.Identity == groupId));
      ProjectDragHelper.SortIdentity sortIdentity2 = sortIdentity1 != null ? sortIdentity1.Children.FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (p => p.Identity == projectId)) : (ProjectDragHelper.SortIdentity) null;
      if (sortIdentity2 == null)
        return ProjectDragHelper.GetNewSortOrder(upDown, projectId, teamId);
      int index = sortIdentity1.Children.IndexOf(sortIdentity2);
      bool flag1 = index == 0;
      bool flag2 = index == sortIdentity1.Children.Count - 1;
      if (upDown)
      {
        if (flag2)
          return sortIdentity1.Children.Last<ProjectDragHelper.SortIdentity>().SortOrder + 268435456L;
        long sortOrder1 = sortIdentity1.Children[index].SortOrder;
        long sortOrder2 = sortIdentity1.Children[index + 1].SortOrder;
        return sortOrder1 + (sortOrder2 - sortOrder1) / 2L;
      }
      if (flag1)
        return sortIdentity1.Children.First<ProjectDragHelper.SortIdentity>().SortOrder - 268435456L;
      long sortOrder3 = sortIdentity1.Children[index].SortOrder;
      long sortOrder4 = sortIdentity1.Children[index - 1].SortOrder;
      return sortOrder3 + (sortOrder4 - sortOrder3) / 2L;
    }

    public static long GetNewSortOrder(bool upDown, string id, string teamId)
    {
      List<ProjectDragHelper.SortIdentity> primaryIdentities = ProjectDragHelper.GetPrimaryIdentities(teamId);
      ProjectDragHelper.SortIdentity sortIdentity = primaryIdentities.FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (p => p.Identity == id));
      if (sortIdentity == null)
        return -1;
      int index = primaryIdentities.IndexOf(sortIdentity);
      if (upDown)
      {
        if (index == primaryIdentities.Count - 1)
          return primaryIdentities.Last<ProjectDragHelper.SortIdentity>().SortOrder + 268435456L;
        long sortOrder1 = primaryIdentities[index].SortOrder;
        long sortOrder2 = primaryIdentities[index + 1].SortOrder;
        return sortOrder1 + (sortOrder2 - sortOrder1) / 2L;
      }
      if (index == 0)
        return primaryIdentities.First<ProjectDragHelper.SortIdentity>().SortOrder - 268435456L;
      long sortOrder3 = primaryIdentities[index].SortOrder;
      long sortOrder4 = primaryIdentities[index - 1].SortOrder;
      return sortOrder3 + (sortOrder4 - sortOrder3) / 2L;
    }

    public static long GetNewSortOrder(
      bool upDown,
      string groupId,
      bool isGroupAll,
      string teamId)
    {
      List<ProjectDragHelper.SortIdentity> primaryIdentities = ProjectDragHelper.GetPrimaryIdentities(teamId);
      List<ProjectDragHelper.SortIdentity> sortIdentities = ProjectDragHelper.GetSortIdentities(teamId);
      return !isGroupAll ? ProjectDragHelper.GetSortOrderOnGroup(upDown, groupId, (IList<ProjectDragHelper.SortIdentity>) primaryIdentities) : ProjectDragHelper.GetSortOrderOnGroupAll(upDown, groupId, primaryIdentities, sortIdentities);
    }

    private static long GetSortOrderOnGroup(
      bool upDown,
      string groupId,
      IList<ProjectDragHelper.SortIdentity> primary)
    {
      if (upDown)
      {
        ProjectDragHelper.SortIdentity sortIdentity = primary.FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (v => v.Identity == groupId));
        if (sortIdentity != null)
          return sortIdentity.Children.Count > 0 ? sortIdentity.Children[0].SortOrder - 268435456L : sortIdentity.SortOrder + 268435456L;
      }
      else
      {
        ProjectDragHelper.SortIdentity sortIdentity = primary.FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (v => v.Identity == groupId));
        if (sortIdentity != null)
        {
          int index = primary.IndexOf(sortIdentity);
          return index <= 0 ? sortIdentity.SortOrder - 268435456L : primary[index - 1].SortOrder + (primary[index].SortOrder - primary[index - 1].SortOrder) / 2L;
        }
      }
      return -1;
    }

    private static long GetSortOrderOnGroupAll(
      bool upDown,
      string groupId,
      List<ProjectDragHelper.SortIdentity> primary,
      List<ProjectDragHelper.SortIdentity> full)
    {
      if (upDown)
      {
        ProjectDragHelper.SortIdentity sortIdentity = primary.FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (v => v.Identity == groupId));
        if (sortIdentity != null)
        {
          int index = primary.IndexOf(sortIdentity);
          return index + 1 >= primary.Count ? sortIdentity.SortOrder + 268435456L : primary[index].SortOrder + (primary[index + 1].SortOrder - primary[index].SortOrder) / 2L;
        }
      }
      else
      {
        ProjectDragHelper.SortIdentity sortIdentity = full.FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (v => v.Identity == groupId));
        if (sortIdentity != null)
        {
          int num = full.IndexOf(sortIdentity);
          return full[num + sortIdentity.Children.Count].SortOrder + 268435456L;
        }
      }
      return -1;
    }

    private static List<ProjectDragHelper.SortIdentity> GetPrimaryIdentities(string teamId)
    {
      if (teamId == "c1a7e08345e444dea187e21a692f0d7a")
        teamId = (string) null;
      List<ProjectModel> projects = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p =>
      {
        if (p.Isinbox)
          return false;
        return string.IsNullOrEmpty(teamId) ? string.IsNullOrEmpty(p.teamId) : p.teamId == teamId;
      })).ToList<ProjectModel>();
      List<ProjectGroupModel> list1 = CacheManager.GetProjectGroups().Where<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (p => string.IsNullOrEmpty(teamId) ? string.IsNullOrEmpty(p.teamId) : p.teamId == teamId)).ToList<ProjectGroupModel>();
      List<ProjectDragHelper.SortIdentity> source = new List<ProjectDragHelper.SortIdentity>();
      if (projects.Any<ProjectModel>())
      {
        List<ProjectModel> list2 = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => string.IsNullOrEmpty(p.groupId) || p.groupId.ToUpper().ToString() == "NONE")).ToList<ProjectModel>();
        if (list2.Any<ProjectModel>())
          source.AddRange(list2.Select<ProjectModel, ProjectDragHelper.SortIdentity>((Func<ProjectModel, ProjectDragHelper.SortIdentity>) (project => new ProjectDragHelper.SortIdentity()
          {
            Identity = project.id,
            Name = project.name,
            SortOrder = project.sortOrder
          })));
      }
      if (list1.Count > 0)
        source.AddRange(list1.Select<ProjectGroupModel, ProjectDragHelper.SortIdentity>((Func<ProjectGroupModel, ProjectDragHelper.SortIdentity>) (group => new ProjectDragHelper.SortIdentity()
        {
          Identity = group.id,
          Name = group.name,
          SortOrder = group.sortOrder.GetValueOrDefault(),
          Children = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.groupId == group.id)).Select<ProjectModel, ProjectDragHelper.SortIdentity>((Func<ProjectModel, ProjectDragHelper.SortIdentity>) (p => new ProjectDragHelper.SortIdentity()
          {
            Identity = p.id,
            Name = p.name,
            SortOrder = p.sortOrder
          })).OrderBy<ProjectDragHelper.SortIdentity, long>((Func<ProjectDragHelper.SortIdentity, long>) (p => p.SortOrder)).ToList<ProjectDragHelper.SortIdentity>()
        })));
      return source.OrderBy<ProjectDragHelper.SortIdentity, long>((Func<ProjectDragHelper.SortIdentity, long>) (sort => sort.SortOrder)).ToList<ProjectDragHelper.SortIdentity>();
    }

    private static List<ProjectDragHelper.SortIdentity> GetSortIdentities(string teamId)
    {
      List<ProjectDragHelper.SortIdentity> sortIdentities = new List<ProjectDragHelper.SortIdentity>();
      List<ProjectDragHelper.SortIdentity> primaryIdentities = ProjectDragHelper.GetPrimaryIdentities(teamId);
      if (primaryIdentities.Any<ProjectDragHelper.SortIdentity>())
      {
        foreach (ProjectDragHelper.SortIdentity sortIdentity in primaryIdentities)
        {
          sortIdentities.Add(sortIdentity);
          if (sortIdentity.Children != null && sortIdentity.Children.Any<ProjectDragHelper.SortIdentity>())
          {
            foreach (ProjectDragHelper.SortIdentity child in sortIdentity.Children)
              sortIdentities.Add(child);
          }
        }
      }
      return sortIdentities;
    }

    public static long CalculateInsertSortOrder(
      bool upDown,
      string identity,
      string teamId,
      string outGroupId = "")
    {
      return ProjectDragHelper.CalculateInsertOrder(upDown, identity, (IList<ProjectDragHelper.SortIdentity>) ProjectDragHelper.GetSortIdentities(teamId), outGroupId);
    }

    public static long CalculateInsertTagSortOrder(bool upDown, string tagName)
    {
      IList<ProjectDragHelper.SortIdentity> tagSortIdentities = ProjectDragHelper.GetTagSortIdentities();
      return ProjectDragHelper.CalculateInsertOrder(upDown, tagName, tagSortIdentities);
    }

    private static IList<ProjectDragHelper.SortIdentity> GetTagSortIdentities()
    {
      int num = LocalSettings.Settings.SmartListTag == 2 ? 1 : 0;
      List<TagModel> source = CacheManager.GetTags();
      if (num != 0)
      {
        ConcurrentDictionary<string, int> countCache = TaskCountCache.CountData;
        source = source.Where<TagModel>((Func<TagModel, bool>) (tag => countCache.ContainsKey(tag.name) && countCache[tag.name] > 0)).ToList<TagModel>();
      }
      List<ProjectDragHelper.SortIdentity> tagSortIdentities = new List<ProjectDragHelper.SortIdentity>();
      if (source != null && source.Count > 0)
        tagSortIdentities.AddRange((IEnumerable<ProjectDragHelper.SortIdentity>) source.Select<TagModel, ProjectDragHelper.SortIdentity>((Func<TagModel, ProjectDragHelper.SortIdentity>) (tag => new ProjectDragHelper.SortIdentity()
        {
          Identity = tag.name,
          SortOrder = tag.sortOrder
        })).OrderBy<ProjectDragHelper.SortIdentity, long>((Func<ProjectDragHelper.SortIdentity, long>) (tag => tag.SortOrder)));
      return (IList<ProjectDragHelper.SortIdentity>) tagSortIdentities;
    }

    public static long GetTagMinSort()
    {
      IList<ProjectDragHelper.SortIdentity> tagSortIdentities = ProjectDragHelper.GetTagSortIdentities();
      return !tagSortIdentities.Any<ProjectDragHelper.SortIdentity>() ? 0L : tagSortIdentities.First<ProjectDragHelper.SortIdentity>().SortOrder;
    }

    public static long CalculateInsertFilterSortOrder(bool upDown, string identity)
    {
      List<FilterModel> filters = CacheManager.GetFilters();
      return filters != null && filters.Count > 0 ? ProjectDragHelper.CalculateInsertOrder(upDown, identity, (IList<ProjectDragHelper.SortIdentity>) filters.Select<FilterModel, ProjectDragHelper.SortIdentity>((Func<FilterModel, ProjectDragHelper.SortIdentity>) (filter => new ProjectDragHelper.SortIdentity()
      {
        Identity = filter.id,
        SortOrder = filter.sortOrder
      })).OrderBy<ProjectDragHelper.SortIdentity, long>((Func<ProjectDragHelper.SortIdentity, long>) (sort => sort.SortOrder)).ToList<ProjectDragHelper.SortIdentity>()) : 0L;
    }

    private static long CalculateInsertOrder(
      bool upDown,
      string identity,
      IList<ProjectDragHelper.SortIdentity> sortIdentities,
      string outGroupId = "")
    {
      if (sortIdentities.Count > 0)
      {
        ProjectDragHelper.SortIdentity sortIdentity = sortIdentities.FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (sort => sort.Identity == identity));
        if (sortIdentity != null)
        {
          int index1 = sortIdentities.IndexOf(sortIdentity);
          int index2 = index1 + (upDown ? 1 : -1);
          if (!string.IsNullOrEmpty(outGroupId))
          {
            if (upDown)
            {
              index1 = ProjectDragHelper.GetIndexOfItem(sortIdentities, outGroupId);
              index2 = ProjectDragHelper.GetNextIndexOfGroup(sortIdentities, outGroupId);
            }
            else
            {
              int nextIndexOfGroup = ProjectDragHelper.GetNextIndexOfGroup(sortIdentities, outGroupId);
              return sortIdentities[nextIndexOfGroup].SortOrder - 268435456L;
            }
          }
          if (index2 < 0)
            return sortIdentities.First<ProjectDragHelper.SortIdentity>().SortOrder - 268435456L;
          return index2 >= sortIdentities.Count ? sortIdentities.Last<ProjectDragHelper.SortIdentity>().SortOrder + 268435456L : sortIdentities[index1].SortOrder + (sortIdentities[index2].SortOrder - sortIdentities[index1].SortOrder) / 2L;
        }
      }
      return -1;
    }

    private static int GetNextIndexOfGroup(
      IList<ProjectDragHelper.SortIdentity> sortIdentities,
      string id)
    {
      if (sortIdentities != null && sortIdentities.Any<ProjectDragHelper.SortIdentity>())
      {
        ProjectDragHelper.SortIdentity sortIdentity = sortIdentities.FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (p => p.Identity == id));
        if (sortIdentity != null)
          return sortIdentities.IndexOf(sortIdentity) + sortIdentity.Children.Count;
      }
      return -1;
    }

    private static int GetIndexOfItem(
      IList<ProjectDragHelper.SortIdentity> sortIdentities,
      string id)
    {
      if (sortIdentities != null && sortIdentities.Any<ProjectDragHelper.SortIdentity>())
      {
        ProjectDragHelper.SortIdentity sortIdentity = sortIdentities.FirstOrDefault<ProjectDragHelper.SortIdentity>((Func<ProjectDragHelper.SortIdentity, bool>) (p => p.Identity == id));
        if (sortIdentity != null)
          return sortIdentities.IndexOf(sortIdentity);
      }
      return -1;
    }

    public static long? GetNewSortOrderInClosed(bool upDown, string projectId, string teamId)
    {
      List<ProjectModel> projectInTeams = CacheManager.GetProjectInTeams(teamId);
      List<ProjectModel> list = projectInTeams != null ? projectInTeams.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.closed.GetValueOrDefault())).ToList<ProjectModel>() : (List<ProjectModel>) null;
      if (list == null || list.Count == 0)
        return new long?();
      list.Sort((Comparison<ProjectModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
      ProjectModel projectModel = list.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
      if (projectModel == null)
        return new long?();
      int num = list.IndexOf(projectModel);
      if (upDown)
      {
        if (num == list.Count - 1)
          return new long?(projectModel.sortOrder + 268435456L);
        long sortOrder1 = projectModel.sortOrder;
        long sortOrder2 = list[num + 1].sortOrder;
        return new long?(sortOrder1 + (sortOrder2 - sortOrder1) / 2L);
      }
      if (num == 0)
        return new long?(projectModel.sortOrder - 268435456L);
      long sortOrder3 = projectModel.sortOrder;
      long sortOrder4 = list[num - 1].sortOrder;
      return new long?(sortOrder3 + (sortOrder4 - sortOrder3) / 2L);
    }

    public static long GetNewSortOrderOnCloseChanged(ProjectModel project)
    {
      bool isClosed = project.closed.GetValueOrDefault();
      List<ProjectModel> projectInTeams = CacheManager.GetProjectInTeams(project.teamId);
      List<ProjectModel> list = projectInTeams != null ? projectInTeams.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.closed.GetValueOrDefault() == isClosed)).ToList<ProjectModel>() : (List<ProjectModel>) null;
      // ISSUE: explicit non-virtual call
      if (list == null || __nonvirtual (list.Count) <= 0)
        return project.sortOrder;
      list.Sort((Comparison<ProjectModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
      ProjectModel projectModel = list[isClosed ? 0 : list.Count - 1];
      return projectModel.id == project.id ? project.sortOrder : projectModel.sortOrder + (isClosed ? -1L : 1L) * 268435456L;
    }

    private class SortIdentity
    {
      public List<ProjectDragHelper.SortIdentity> Children;
      public string Identity;
      public string Name;
      public long SortOrder;
      public long TeamIndex;
    }
  }
}
