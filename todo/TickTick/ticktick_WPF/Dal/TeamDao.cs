// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TeamDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Team;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TeamDao
  {
    public static bool IsTeamValid()
    {
      List<TeamModel> teams = CacheManager.GetTeams();
      return teams != null && teams.Any<TeamModel>((Func<TeamModel, bool>) (t => !t.expired));
    }

    public static async Task<bool> IsTeamPro()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<TeamModel> listAsync = await App.Connection.Table<TeamModel>().Where((Expression<Func<TeamModel, bool>>) (v => v.userId == userId && v.teamPro)).ToListAsync();
      return listAsync != null && listAsync.Any<TeamModel>((Func<TeamModel, bool>) (t => t.IsPro()));
    }

    public static async Task<List<TeamModel>> GetAllTeams()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TeamModel>().Where((Expression<Func<TeamModel, bool>>) (v => v.userId == userId)).OrderByDescending<DateTime>((Expression<Func<TeamModel, DateTime>>) (v => v.joinedTime)).ToListAsync();
    }

    public static async Task<bool> MergeTeams(List<TeamModel> teams)
    {
      bool result = false;
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<TeamModel> listAsync = await App.Connection.Table<TeamModel>().Where((Expression<Func<TeamModel, bool>>) (v => v.userId == userId)).ToListAsync();
      if (teams.Count > 0)
      {
        // ISSUE: explicit non-virtual call
        if (listAsync != null && __nonvirtual (listAsync.Count) > 0)
        {
          List<string> localTeamIds = listAsync.Select<TeamModel, string>((Func<TeamModel, string>) (team => team.id)).ToList<string>();
          List<string> teamIds = teams.Select<TeamModel, string>((Func<TeamModel, string>) (team => team.id)).ToList<string>();
          List<TeamModel> list1 = teams.Where<TeamModel>((Func<TeamModel, bool>) (team => !localTeamIds.Contains(team.id))).ToList<TeamModel>();
          if (list1.Count > 0)
          {
            result = true;
            TeamDao.AddTeam(list1);
          }
          List<TeamModel> list2 = listAsync.Where<TeamModel>((Func<TeamModel, bool>) (team => !teamIds.Contains(team.id))).ToList<TeamModel>();
          if (list2.Count > 0)
          {
            result = true;
            TeamDao.DeleteTeam(list2);
          }
          foreach (TeamModel teamModel1 in teams.Where<TeamModel>((Func<TeamModel, bool>) (team => localTeamIds.Contains(team.id))))
          {
            TeamModel team = teamModel1;
            TeamModel teamModel2 = listAsync.FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (model => model.id == team.id));
            if (teamModel2 != null)
            {
              team.open = teamModel2.open;
              team._Id = teamModel2._Id;
              TeamDao.UpdateTeam(team);
              if (teamModel2.name != team.name || teamModel2.IsPro() != team.IsPro())
                result = true;
            }
          }
        }
        else
        {
          result = true;
          TeamDao.AddTeam(teams);
        }
      }
      else
      {
        // ISSUE: explicit non-virtual call
        if (listAsync != null && __nonvirtual (listAsync.Count) > 0)
        {
          result = true;
          TeamDao.DeleteTeam(listAsync);
        }
      }
      return result;
    }

    private static async void DeleteTeam(List<TeamModel> teams)
    {
      if (teams == null || teams.Count <= 0)
        return;
      foreach (TeamModel team in teams)
      {
        int num = await App.Connection.DeleteAsync((object) team);
        CacheManager.DeleteTeam(team);
      }
    }

    public static async Task UpdateTeam(TeamModel team)
    {
      if (team == null)
        return;
      team.name = team.name.Trim();
      team.userId = Utils.GetCurrentUserIdInt().ToString();
      int num = await App.Connection.UpdateAsync((object) team);
      CacheManager.UpdateTeam(team);
    }

    private static async void AddTeam(List<TeamModel> teams)
    {
      if (teams == null)
        return;
      List<TeamModel> list = teams.Where<TeamModel>((Func<TeamModel, bool>) (t => t.expired)).ToList<TeamModel>();
      if (list.Count > 0)
        new TeamExpiredRemindDialog(list).ShowDialog();
      foreach (TeamModel team in teams)
      {
        team.name = team.name.Trim();
        team.expiredChecked = team.expired;
        team.userId = Utils.GetCurrentUserIdInt().ToString();
        int num = await App.Connection.InsertAsync((object) team);
        CacheManager.UpdateTeam(team);
      }
    }

    public static async void SaveOpenStatus(string id, bool open)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      TeamModel team = await App.Connection.Table<TeamModel>().Where((Expression<Func<TeamModel, bool>>) (t => t.id == id && t.userId == userId)).FirstOrDefaultAsync();
      if (team == null)
      {
        team = (TeamModel) null;
      }
      else
      {
        team.open = open;
        team.modifiedTime = DateTime.Now;
        int num = await App.Connection.UpdateAsync((object) team);
        CacheManager.UpdateTeam(team);
        team = (TeamModel) null;
      }
    }

    public static bool IsTeamExpired(string teamId)
    {
      return CacheManager.GetTeams().Where<TeamModel>((Func<TeamModel, bool>) (team => team.expired)).Select<TeamModel, string>((Func<TeamModel, string>) (team => team.id)).ToList<string>().Contains(teamId);
    }

    public static long GetNewProjectOrderInTeam(string teamId)
    {
      long projectOrderInTeam = 0;
      ProjectModel projectModel = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.teamId == teamId)).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (t => t.sortOrder)).FirstOrDefault<ProjectModel>();
      if (projectModel != null)
        projectOrderInTeam = projectModel.sortOrder - 268435456L;
      ProjectGroupModel projectGroupModel = CacheManager.GetProjectGroups().Where<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (p => p.teamId == teamId)).OrderBy<ProjectGroupModel, long?>((Func<ProjectGroupModel, long?>) (t => t.sortOrder)).FirstOrDefault<ProjectGroupModel>();
      if (projectGroupModel != null)
      {
        long? sortOrder = projectGroupModel.sortOrder;
        long num = projectOrderInTeam;
        if (sortOrder.GetValueOrDefault() < num & sortOrder.HasValue)
          projectOrderInTeam = projectGroupModel.sortOrder ?? -268435456L;
      }
      return projectOrderInTeam;
    }

    public static long GetNewProjectInPersonal()
    {
      long projectInPersonal = 0;
      ProjectModel projectModel = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => string.IsNullOrEmpty(p.teamId))).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (t => t.sortOrder)).FirstOrDefault<ProjectModel>();
      if (projectModel != null)
        projectInPersonal = projectModel.sortOrder - 268435456L;
      ProjectGroupModel projectGroupModel = CacheManager.GetProjectGroups().Where<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (p => string.IsNullOrEmpty(p.teamId))).OrderBy<ProjectGroupModel, long?>((Func<ProjectGroupModel, long?>) (t => t.sortOrder)).FirstOrDefault<ProjectGroupModel>();
      if (projectGroupModel != null)
      {
        long? sortOrder = projectGroupModel.sortOrder;
        long num = projectInPersonal;
        if (sortOrder.GetValueOrDefault() < num & sortOrder.HasValue)
          projectInPersonal = projectGroupModel.sortOrder ?? -268435456L;
      }
      return projectInPersonal;
    }
  }
}
