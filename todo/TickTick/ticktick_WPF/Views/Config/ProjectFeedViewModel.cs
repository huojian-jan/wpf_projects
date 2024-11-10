// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.ProjectFeedViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  internal class ProjectFeedViewModel
  {
    public const string UrlBase = "webcal://{0}/pub/calendar/feeds/{1}/basic.ics";

    public ProjectFeedViewModel(ProjectFeedCode code)
    {
      if (code.projectId == "all")
        this.Name = Utils.GetString("AllList");
      else if (code.projectId.StartsWith("inbox"))
      {
        this.Name = Utils.GetString("Inbox");
      }
      else
      {
        ProjectModel projectById = CacheManager.GetProjectById(code.projectId);
        this.Name = projectById == null ? Utils.GetString("UnknownProject") : projectById.name;
      }
      this.ProjectId = code.projectId;
      this.Url = string.Format("webcal://{0}/pub/calendar/feeds/{1}/basic.ics", (object) BaseUrl.Domain, (object) code.code);
      this.CreatedTime = code.createdTime ?? DateTime.Now;
    }

    public string Name { get; set; }

    public string Url { get; set; }

    public DateTime CreatedTime { get; set; }

    public string ProjectId { get; internal set; }
  }
}
