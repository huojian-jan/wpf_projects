// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskDefaultModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Tag;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class TaskDefaultModel : BaseModel
  {
    public string UserId { get; set; }

    public int DateMode { get; set; }

    public string Date { get; set; }

    public int Duration { get; set; }

    public string TimeReminders { get; set; }

    public string AllDayReminders { get; set; }

    public int Priority { get; set; }

    public int AddTo { get; set; }

    public string TagString { get; set; }

    public string projectId { get; set; }

    [Ignore]
    public List<string> Tags
    {
      get => TagSerializer.ToTags(this.TagString);
      set => this.TagString = TagSerializer.ToJsonContent(value);
    }

    [Ignore]
    public string ProjectId
    {
      get
      {
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == this.projectId));
        return projectModel == null || !projectModel.IsEnable() || projectModel.kind == Constants.ProjectKind.NOTE.ToString() ? LocalSettings.Settings.InServerBoxId : this.projectId;
      }
      set => this.projectId = value;
    }

    public DateTime? GetDefaultDateTime() => TaskDefaultModel.GetDefaultDate(this.Date);

    public static TaskDefaultModel BuildDefault()
    {
      return new TaskDefaultModel()
      {
        UserId = Utils.GetCurrentUserIdInt().ToString(),
        DateMode = 0,
        Priority = 0,
        AddTo = 0,
        Duration = 60,
        ProjectId = LocalSettings.Settings.InServerBoxId,
        TimeReminders = "TRIGGER:PT0S"
      };
    }

    public static DateTime? GetDefaultDate(string date)
    {
      switch (date)
      {
        case "today":
          return new DateTime?(DateTime.Today);
        case "tomorrow":
          return new DateTime?(DateTime.Today.AddDays(1.0));
        case "day_after_tomorrow":
          return new DateTime?(DateTime.Today.AddDays(2.0));
        case "NextWeek":
          return new DateTime?(DateTime.Today.AddDays(7.0));
        default:
          return new DateTime?();
      }
    }

    public bool Equal(TaskDefaultModel model)
    {
      return this.DateMode == model.DateMode && this.Date == model.Date && this.Duration == model.Duration && this.TimeReminders == model.TimeReminders && this.AllDayReminders == model.AllDayReminders && this.Priority == model.Priority && this.AddTo == model.AddTo && this.TagString == model.TagString && this.ProjectId == model.ProjectId;
    }
  }
}
