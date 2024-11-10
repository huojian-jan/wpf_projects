// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.ProjectCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Cache
{
  public class ProjectCache : CacheBase<ProjectModel>
  {
    private bool _loaded;

    public override async Task Load()
    {
      ProjectCache projectCache = this;
      List<ProjectModel> projects;
      if (projectCache._loaded)
      {
        projects = (List<ProjectModel>) null;
      }
      else
      {
        projectCache._loaded = true;
        projects = (await ProjectDao.GetAllProjects()).ToList<ProjectModel>();
        ProjectModel projectModel = projects.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.Isinbox));
        if (projectModel != null)
        {
          projectModel.name = Utils.GetString("Inbox");
          projectModel.sortOrder = long.MinValue;
        }
        else
        {
          int num = await Utils.CreatInbox();
          ProjectModel projectById = await ProjectDao.GetProjectById(LocalSettings.Settings.InServerBoxId);
          if (projectById != null)
            projects.Add(projectById);
        }
        await projectCache.CheckProjectSortOrders(projects);
        await projectCache.CheckProjectTimelineModel(projects);
        projectCache.AssembleData((IEnumerable<ProjectModel>) projects, (Func<ProjectModel, string>) (project => project.id));
        projectCache.LoadProjectAvatars(projects);
        projects = (List<ProjectModel>) null;
      }
    }

    private async Task CheckProjectTimelineModel(List<ProjectModel> projects)
    {
      List<ProjectModel> projectModelList1 = projects;
      // ISSUE: explicit non-virtual call
      if ((projectModelList1 != null ? (__nonvirtual (projectModelList1.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      List<ProjectModel> projectModelList2 = new List<ProjectModel>();
      foreach (ProjectModel project in projects)
      {
        if (project.Timeline != null && (project.SyncTimeline == null || project.SyncTimeline.SortType != project.Timeline.SortType))
        {
          if (project.SyncTimeline == null)
            project.SyncTimeline = new TimelineSyncModel();
          if (project.Timeline.SortType == "none" || string.IsNullOrEmpty(project.Timeline.SortType))
            project.Timeline.SortType = "sortOrder";
          project.SyncTimeline.SortType = project.Timeline.SortType;
          projectModelList2.Add(project);
        }
      }
      if (!projectModelList2.Any<ProjectModel>())
        return;
      int num = await App.Connection.UpdateAllAsync((IEnumerable) projectModelList2);
    }

    private async Task CheckProjectSortOrders(List<ProjectModel> projects)
    {
      List<ProjectModel> projectModelList = projects;
      // ISSUE: explicit non-virtual call
      if ((projectModelList != null ? (__nonvirtual (projectModelList.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      projects = projects.OrderBy<ProjectModel, string>((Func<ProjectModel, string>) (p => p.teamId)).ThenBy<ProjectModel, string>((Func<ProjectModel, string>) (p => p.groupId)).ThenBy<ProjectModel, bool?>((Func<ProjectModel, bool?>) (p => p.closed)).ThenBy<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).ToList<ProjectModel>();
      long? nullable1 = new long?();
      long? nullable2 = new long?();
      List<ProjectModel> needResort = new List<ProjectModel>();
      foreach (ProjectModel project in projects)
      {
        ProjectModel p = needResort.LastOrDefault<ProjectModel>();
        if (p != null)
        {
          if (project.sortOrder == p.sortOrder && project.teamId == p.teamId && project.groupId == p.groupId)
          {
            needResort.Add(project);
          }
          else
          {
            if (needResort.Count > 1)
            {
              if (!nullable1.HasValue || project.sortOrder > nullable1.Value)
                nullable2 = new long?(project.sortOrder);
              long? nullable3 = nullable1;
              nullable1 = new long?(nullable3 ?? nullable2.Value - 268435456L);
              nullable3 = nullable2;
              nullable2 = new long?(nullable3 ?? nullable1.Value + 268435456L);
              long num1 = (nullable2.Value - nullable1.Value) / (long) (needResort.Count + 1);
              for (int index = 0; index < needResort.Count; ++index)
              {
                ProjectModel projectModel = needResort[index];
                projectModel.sortOrder = nullable1.Value + num1 * (long) (index + 1);
                projectModel.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
              }
              int num2 = await App.Connection.UpdateAllAsync((IEnumerable) needResort);
            }
            needResort.Clear();
            needResort.Add(project);
            nullable1 = new long?(p.sortOrder);
            nullable2 = new long?();
          }
        }
        else
          needResort.Add(project);
        p = (ProjectModel) null;
      }
      if (needResort.Count > 1)
      {
        if (!nullable1.HasValue)
        {
          nullable2 = new long?(needResort[0].sortOrder - 268435456L);
          nullable1 = new long?(needResort[0].sortOrder - 268435456L);
        }
        nullable2 = new long?(nullable2 ?? nullable1.Value + 268435456L);
        long num3 = (nullable2.Value - nullable1.Value) / (long) (needResort.Count + 1);
        for (int index = 0; index < needResort.Count; ++index)
        {
          ProjectModel projectModel = needResort[index];
          projectModel.sortOrder = nullable1.Value + num3 * (long) (index + 1);
          projectModel.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
        }
        int num4 = await App.Connection.UpdateAllAsync((IEnumerable) needResort);
      }
      needResort = (List<ProjectModel>) null;
    }

    private void LoadProjectAvatars(List<ProjectModel> projects)
    {
      Utils.RunOnBackgroundThread(Application.Current?.Dispatcher, (Action) (async () =>
      {
        foreach (ProjectModel project in projects)
        {
          if (project.IsValid() && project.IsShareList())
            AvatarHelper.GetProjectUsersAsync(project.id);
        }
      }));
    }
  }
}
