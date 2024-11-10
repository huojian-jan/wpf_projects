// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ProjectUsersMode
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class ProjectUsersMode : BaseModel
  {
    private string _projectId;

    public string projectId
    {
      get => this._projectId;
      set
      {
        this._projectId = value;
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.ProjectId == value));
        this.SortOrder = projectModel != null ? projectModel.sortOrder : 0L;
      }
    }

    public List<ShareUserModel> shareUsers { get; set; }

    public long SortOrder { get; set; }
  }
}
