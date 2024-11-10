// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.ProjectGroupCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Cache
{
  public class ProjectGroupCache : CacheBase<ProjectGroupModel>
  {
    private bool _loaded;

    public override async Task Load()
    {
      ProjectGroupCache projectGroupCache = this;
      List<ProjectGroupModel> groups;
      if (projectGroupCache._loaded)
      {
        groups = (List<ProjectGroupModel>) null;
      }
      else
      {
        projectGroupCache._loaded = true;
        groups = (await ProjectGroupDao.GetProjectGroups()).ToList<ProjectGroupModel>();
        await projectGroupCache.CheckTimelineModel(groups);
        projectGroupCache.AssembleData((IEnumerable<ProjectGroupModel>) groups, (Func<ProjectGroupModel, string>) (group => group.id));
        groups = (List<ProjectGroupModel>) null;
      }
    }

    private async Task CheckTimelineModel(List<ProjectGroupModel> groups)
    {
      List<ProjectGroupModel> projectGroupModelList1 = groups;
      // ISSUE: explicit non-virtual call
      if ((projectGroupModelList1 != null ? (__nonvirtual (projectGroupModelList1.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      List<ProjectGroupModel> projectGroupModelList2 = new List<ProjectGroupModel>();
      foreach (ProjectGroupModel group in groups)
      {
        if (group.SyncTimeline.SortType != group.Timeline.SortType)
        {
          group.SyncTimeline.SortType = group.Timeline.SortType;
          projectGroupModelList2.Add(group);
        }
      }
      if (!projectGroupModelList2.Any<ProjectGroupModel>())
        return;
      int num = await App.Connection.UpdateAllAsync((IEnumerable) projectGroupModelList2);
    }
  }
}
