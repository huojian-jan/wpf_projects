// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ProjectDataAssembler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public static class ProjectDataAssembler
  {
    public static List<SelectableItemViewModel> AssembleProjects(
      bool onlyShowPermission = false,
      bool inCal = false)
    {
      List<ProjectModel> projects = CacheManager.GetProjectsWithoutInbox().Where<ProjectModel>((Func<ProjectModel, bool>) (p => !p.closed.HasValue || !p.closed.Value)).ToList<ProjectModel>();
      if (onlyShowPermission)
        projects = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsProjectPermit())).ToList<ProjectModel>();
      List<ProjectGroupModel> projectGroups = CacheManager.GetProjectGroups();
      List<TeamModel> source1 = CacheManager.GetTeams().Where<TeamModel>((Func<TeamModel, bool>) (team => !team.expired)).ToList<TeamModel>();
      if (!UserManager.IsTeamUser())
        source1 = new List<TeamModel>();
      List<ProjectModel> orphanProjects = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => string.IsNullOrEmpty(p.groupId) || p.groupId.Trim().ToUpper() == "NONE")).ToList<ProjectModel>();
      List<ProjectGroupViewModel> groupModels = projectGroups.Select<ProjectGroupModel, ProjectGroupViewModel>((Func<ProjectGroupModel, ProjectGroupViewModel>) (group =>
      {
        return new ProjectGroupViewModel(group)
        {
          SortOrder = group.sortOrder.GetValueOrDefault(),
          TeamId = group.teamId,
          InCalFilter = inCal,
          Children = new List<SelectableItemViewModel>((IEnumerable<SelectableItemViewModel>) projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.groupId == group.id)).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).Select<ProjectModel, SubProjectViewModel>((Func<ProjectModel, SubProjectViewModel>) (p =>
          {
            return new SubProjectViewModel(p.id, p.groupId, p.name, p.IsShareList(), p.IsNote)
            {
              SortOrder = p.sortOrder,
              InCalFilter = inCal
            };
          })).ToList<SubProjectViewModel>())
        };
      })).ToList<ProjectGroupViewModel>();
      groupModels = groupModels.Where<ProjectGroupViewModel>((Func<ProjectGroupViewModel, bool>) (g =>
      {
        List<SelectableItemViewModel> children = g.Children;
        // ISSUE: explicit non-virtual call
        return children != null && __nonvirtual (children.Count) > 0;
      })).ToList<ProjectGroupViewModel>();
      List<TeamSectionViewModel> list = source1.Select<TeamModel, TeamSectionViewModel>((Func<TeamModel, TeamSectionViewModel>) (team =>
      {
        return new TeamSectionViewModel(team.id, team.name)
        {
          Children = new List<SelectableItemViewModel>((IEnumerable<SelectableItemViewModel>) groupModels.Where<ProjectGroupViewModel>((Func<ProjectGroupViewModel, bool>) (group => group.TeamId == team.id)).ToList<ProjectGroupViewModel>()).Union<SelectableItemViewModel>((IEnumerable<SelectableItemViewModel>) new List<SelectableItemViewModel>((IEnumerable<SelectableItemViewModel>) new List<SelectableItemViewModel>((IEnumerable<SelectableItemViewModel>) orphanProjects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.teamId == team.id)).Select<ProjectModel, ProjectViewModel>((Func<ProjectModel, ProjectViewModel>) (p =>
          {
            return new ProjectViewModel(p.id, p.name, p.IsShareList(), p.IsNote)
            {
              SortOrder = p.sortOrder,
              InCalFilter = inCal
            };
          }))))).OrderBy<SelectableItemViewModel, long>((Func<SelectableItemViewModel, long>) (item => item.SortOrder)).ToList<SelectableItemViewModel>()
        };
      })).ToList<TeamSectionViewModel>();
      TeamSectionViewModel sectionViewModel1 = new TeamSectionViewModel("c1a7e08345e444dea187e21a692f0d7a", Utils.GetString("Personal"));
      sectionViewModel1.Children = new List<SelectableItemViewModel>((IEnumerable<SelectableItemViewModel>) orphanProjects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => string.IsNullOrEmpty(p.teamId) || CacheManager.GetTeamById(p.teamId) == null)).Select<ProjectModel, ProjectViewModel>((Func<ProjectModel, ProjectViewModel>) (p =>
      {
        return new ProjectViewModel(p.id, p.name, p.IsShareList(), p.IsNote)
        {
          SortOrder = p.sortOrder,
          InCalFilter = inCal
        };
      })).ToList<ProjectViewModel>()).Union<SelectableItemViewModel>((IEnumerable<SelectableItemViewModel>) new List<SelectableItemViewModel>((IEnumerable<SelectableItemViewModel>) groupModels.Where<ProjectGroupViewModel>((Func<ProjectGroupViewModel, bool>) (group => string.IsNullOrEmpty(group.TeamId))))).OrderBy<SelectableItemViewModel, long>((Func<SelectableItemViewModel, long>) (item => item.SortOrder)).ToList<SelectableItemViewModel>();
      TeamSectionViewModel sectionViewModel2 = sectionViewModel1;
      list.Add(sectionViewModel2);
      if (source1.Any<TeamModel>())
      {
        foreach (TeamSectionViewModel model in list)
          ProjectDataAssembler.SetTeamId((SelectableItemViewModel) model, model.Id);
      }
      List<SelectableItemViewModel> source2 = ProjectDataAssembler.ExpandItems(list);
      if (!source1.Any<TeamModel>() && source2.Any<SelectableItemViewModel>())
        source2.RemoveAt(0);
      return source2;
    }

    private static void SetTeamId(SelectableItemViewModel model, string teamId)
    {
      if (model == null)
        return;
      model.TeamId = teamId;
      if (model.Children == null)
        return;
      foreach (SelectableItemViewModel child in model.Children)
        ProjectDataAssembler.SetTeamId(child, teamId);
    }

    private static List<SelectableItemViewModel> ExpandItems(List<TeamSectionViewModel> items)
    {
      List<SelectableItemViewModel> models = new List<SelectableItemViewModel>();
      if (items != null && items.Any<TeamSectionViewModel>())
      {
        foreach (SelectableItemViewModel selectableItemViewModel in items)
          ProjectDataAssembler.AddItems(selectableItemViewModel, models);
      }
      return models;
    }

    private static void AddItems(SelectableItemViewModel item, List<SelectableItemViewModel> models)
    {
      models.Add(item);
      if (item.Children == null || !item.Children.Any<SelectableItemViewModel>())
        return;
      foreach (SelectableItemViewModel child in item.Children)
        ProjectDataAssembler.AddItems(child, models);
    }
  }
}
