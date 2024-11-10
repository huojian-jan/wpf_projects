// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TaskMergeUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class TaskMergeUtils
  {
    public static async Task<ProjectModel> GetMergedProject(List<TaskModel> tasks)
    {
      List<string> projectIds = tasks.Select<TaskModel, string>((Func<TaskModel, string>) (task => task.projectId)).Distinct<string>().ToList<string>();
      if (projectIds.Count == 1)
      {
        ProjectModel mergedProject = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectIds[0]));
        if (mergedProject != null)
          return mergedProject;
      }
      return new ProjectModel()
      {
        id = Utils.GetInboxId(),
        name = Utils.GetString("Inbox")
      };
    }
  }
}
