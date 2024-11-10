// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.KanbanHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public class KanbanHelper
  {
    public static bool IsProjectInKanbanMode(string projectId)
    {
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
      return projectModel != null && projectModel.viewMode == "kanban";
    }
  }
}
