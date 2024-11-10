// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.NotificationHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class NotificationHelper
  {
    private static readonly Dictionary<string, string[]> ProjectNtfOptions = new Dictionary<string, string[]>();
    private static bool _initialized;

    public static async void InitProjectNtfOptions()
    {
      if (NotificationHelper._initialized)
        return;
      foreach (ProjectModel projectModel in CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (project => project.IsEnable())).ToList<ProjectModel>())
      {
        if (NotificationHelper.ProjectNtfOptions.ContainsKey(projectModel.id))
          NotificationHelper.ProjectNtfOptions[projectModel.id] = projectModel.notificationOptions;
        else
          NotificationHelper.ProjectNtfOptions.Add(projectModel.id, projectModel.notificationOptions);
      }
      NotificationHelper._initialized = true;
    }

    public static string[] GetNtfOptions(string projectId)
    {
      if (string.IsNullOrEmpty(projectId))
        return (string[]) null;
      return !NotificationHelper.ProjectNtfOptions.ContainsKey(projectId) ? (string[]) null : NotificationHelper.ProjectNtfOptions[projectId];
    }

    public static void OnProjectChanged(ProjectModel project)
    {
      if (project.IsEnable())
        NotificationHelper.SaveNtfOptions(project.id, project.notificationOptions);
      else
        NotificationHelper.ProjectNtfOptions.Remove(project.id);
    }

    public static void SaveNtfOptions(string projectId, string[] ntfOptions)
    {
      if (NotificationHelper.ProjectNtfOptions.ContainsKey(projectId))
        NotificationHelper.ProjectNtfOptions[projectId] = ntfOptions;
      else
        NotificationHelper.ProjectNtfOptions.Add(projectId, ntfOptions);
    }
  }
}
