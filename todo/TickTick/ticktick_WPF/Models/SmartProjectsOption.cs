// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SmartProjectsOption
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Views.MainListView;

#nullable disable
namespace ticktick_WPF.Models
{
  public class SmartProjectsOption : PreferenceBaseModel
  {
    [JsonProperty("smartProjects")]
    public List<SmartProjectOption> SmartProjects { get; set; }

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      if (model is SmartProjectsOption smartProjectsOption)
      {
        if (smartProjectsOption.mtime > this.mtime)
        {
          this.mtime = smartProjectsOption.mtime;
          this.SaveProjects(smartProjectsOption.SmartProjects);
          ListViewContainer.ReloadTasks();
        }
        else if (smartProjectsOption.mtime < this.mtime)
          return true;
      }
      return false;
    }

    private void SaveProjects(List<SmartProjectOption> remotes)
    {
      if (this.SmartProjects == null)
      {
        this.SmartProjects = remotes;
      }
      else
      {
        foreach (SmartProjectOption remote1 in remotes)
        {
          SmartProjectOption remote = remote1;
          this.SmartProjects.FirstOrDefault<SmartProjectOption>((Func<SmartProjectOption, bool>) (m => m.Name == remote.Name))?.SaveRemote(remote);
        }
      }
    }

    public string GetViewModeByName(string name)
    {
      List<SmartProjectOption> smartProjects = this.SmartProjects;
      if (smartProjects == null)
        return (string) null;
      return smartProjects.FirstOrDefault<SmartProjectOption>((Func<SmartProjectOption, bool>) (s => s.Name == name))?.ViewMode;
    }

    public SortOption GetSortOptionByName(string name)
    {
      List<SmartProjectOption> smartProjects = this.SmartProjects;
      if (smartProjects == null)
        return (SortOption) null;
      return smartProjects.FirstOrDefault<SmartProjectOption>((Func<SmartProjectOption, bool>) (s => s.Name == name))?.SortOption;
    }

    public SmartProjectOption GetSmartProjectByName(string name)
    {
      List<SmartProjectOption> smartProjects = this.SmartProjects;
      return smartProjects == null ? (SmartProjectOption) null : smartProjects.FirstOrDefault<SmartProjectOption>((Func<SmartProjectOption, bool>) (s => s.Name == name));
    }

    public TimelineModel GetTimeline(string name)
    {
      SmartProjectOption smartProjectByName = this.GetSmartProjectByName(name);
      TimelineModel timeline = smartProjectByName?.Timeline;
      if (timeline == null)
      {
        timeline = name == "inbox" ? new TimelineModel("sortOrder") : new TimelineModel("project");
        if (smartProjectByName != null)
          smartProjectByName.Timeline = timeline;
      }
      return timeline;
    }
  }
}
