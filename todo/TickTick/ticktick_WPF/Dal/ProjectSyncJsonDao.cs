// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ProjectSyncJsonDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class ProjectSyncJsonDao
  {
    public static async Task<List<ProjectSyncedJsonModel>> GetProjectSyncedModels()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<ProjectSyncedJsonModel>().Where((Expression<Func<ProjectSyncedJsonModel, bool>>) (model => model.UserId == userId)).ToListAsync();
    }

    private static async Task<ProjectSyncedJsonModel> GetSavedJson(string projectId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<ProjectSyncedJsonModel>().Where((Expression<Func<ProjectSyncedJsonModel, bool>>) (model => model.UserId == userId && model.ProjectId == projectId)).FirstOrDefaultAsync();
    }

    public static async Task TryAddProjectJson(string projectId)
    {
      ProjectModel project = await ProjectDao.GetProjectById(projectId);
      ProjectSyncedJsonModel jsonModel;
      if (project == null)
      {
        jsonModel = (ProjectSyncedJsonModel) null;
      }
      else
      {
        int currentUserIdInt = Utils.GetCurrentUserIdInt();
        string userId = currentUserIdInt.ToString();
        ProjectSyncedJsonModel projectSyncedJsonModel1 = new ProjectSyncedJsonModel();
        currentUserIdInt = Utils.GetCurrentUserIdInt();
        projectSyncedJsonModel1.UserId = currentUserIdInt.ToString();
        projectSyncedJsonModel1.ProjectId = project.id;
        projectSyncedJsonModel1.JsonString = JsonConvert.SerializeObject((object) project);
        jsonModel = projectSyncedJsonModel1;
        ProjectSyncedJsonModel projectSyncedJsonModel2 = await App.Connection.Table<ProjectSyncedJsonModel>().Where((Expression<Func<ProjectSyncedJsonModel, bool>>) (p => p.ProjectId == project.id && p.UserId == userId)).FirstOrDefaultAsync();
        if (projectSyncedJsonModel2 == null)
        {
          int num = await App.Connection.InsertAsync((object) jsonModel);
          jsonModel = (ProjectSyncedJsonModel) null;
        }
        else
        {
          jsonModel._Id = projectSyncedJsonModel2._Id;
          int num = await App.Connection.UpdateAsync((object) jsonModel);
          jsonModel = (ProjectSyncedJsonModel) null;
        }
      }
    }

    public static async Task BatchDeleteProjects(List<string> projectIds)
    {
      if (projectIds == null || !projectIds.Any<string>())
        return;
      foreach (string projectId in projectIds)
      {
        ProjectSyncedJsonModel savedJson = await ProjectSyncJsonDao.GetSavedJson(projectId);
        if (savedJson != null)
        {
          int num = await App.Connection.DeleteAsync((object) savedJson);
        }
      }
    }

    public static async Task BatchDeleteProjects(List<ProjectModel> projects)
    {
      if (projects == null || !projects.Any<ProjectModel>())
        return;
      await ProjectSyncJsonDao.BatchDeleteProjects(projects.Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.id)).ToList<string>());
    }
  }
}
