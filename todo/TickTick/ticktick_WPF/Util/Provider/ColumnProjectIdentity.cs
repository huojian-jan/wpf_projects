// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ColumnProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Kanban;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public sealed class ColumnProjectIdentity : ProjectIdentity
  {
    public readonly string ColumnId;

    public ColumnProjectIdentity(ProjectIdentity project, string columnId)
    {
      this.Id = columnId;
      this.ColumnId = columnId;
      this.Project = project;
      if (project != null)
      {
        SortOption sortOption = project.SortOption;
        this.SortOption = new SortOption()
        {
          groupBy = sortOption.orderBy == sortOption.groupBy || columnId == "note" && sortOption.orderBy != "tag" ? "none" : sortOption.orderBy,
          orderBy = sortOption.orderBy
        };
        int num;
        if (!project.IsNote && !(columnId == "note"))
        {
          ProjectModel projectById = CacheManager.GetProjectById(ColumnViewModel.GetProject(columnId));
          num = projectById != null ? (projectById.IsNote ? 1 : 0) : 0;
        }
        else
          num = 1;
        this.IsNote = num != 0;
      }
      else
        this.SortOption = SortOptionUtils.GetSortOptionBySortType(Constants.SortType.dueDate, true);
    }

    public override string LoadMoreId => this.Project?.CatId;

    public override string QueryId => this.Project?.CatId;

    public override string CatId => this.Project?.CatId;

    public override string Title => this.Project?.GetDisplayTitle();

    public override bool CanEdit
    {
      get
      {
        if (this.ColumnId.Contains("project:"))
        {
          ProjectModel projectById = CacheManager.GetProjectById(ColumnViewModel.GetProject(this.ColumnId));
          return projectById == null || projectById.IsEnable();
        }
        bool? nullable = this.Project is NormalProjectIdentity project1 ? project1.Project?.IsEnable() : new bool?();
        if (nullable.HasValue)
          return nullable.GetValueOrDefault();
        return !(this.Project is GroupProjectIdentity project2) || project2.CanAddTask();
      }
    }

    public ProjectIdentity Project { get; }

    public override string SortProjectId => this.Project?.SortProjectId;

    public override string GetProjectId()
    {
      if (this.ColumnId.StartsWith("assign:") && this.Project is GroupProjectIdentity project)
      {
        string assignee = this.GetAssignee();
        if (!string.IsNullOrEmpty(assignee) && assignee != "-1")
        {
          string id = ProjectDao.GetAssignProjectInGroup(project.GroupId, assignee)?.id;
          if (id != null)
            return id;
        }
      }
      if (this.ColumnId.StartsWith("project:"))
        return ColumnViewModel.GetProject(this.ColumnId);
      return this.Project?.GetProjectId();
    }

    public override string GetProjectName() => this.Project?.GetProjectName();

    public override string GetDisplayTitle() => this.Project?.GetDisplayTitle();

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new NormalProjectIdentity(CacheManager.GetProjectById(project.Id) ?? ((NormalProjectIdentity) project).Project);
    }

    public SortOption GetRealSortOption() => this.Project?.SortOption;

    public override bool CanAddTask()
    {
      if (this.ColumnId == "habit" || this.ColumnId == "course" || this.ColumnId == "calendar")
        return false;
      ProjectIdentity project = this.Project;
      return project != null && project.CanAddTask();
    }

    public override string GetSortProjectId()
    {
      SortOption sortOption = this.Project?.SortOption;
      return !(sortOption?.groupBy == "dueDate") && !(sortOption?.groupBy == "project") ? this.CatId : this.SortProjectId;
    }

    public override int GetPriority()
    {
      if (this.ColumnId.StartsWith("priority:"))
        return ColumnViewModel.GetPriority(this.ColumnId);
      ProjectIdentity project = this.Project;
      return project == null ? base.GetPriority() : project.GetPriority();
    }

    public override TimeData GetTimeData()
    {
      if (this.ColumnId.StartsWith("date:"))
      {
        DateTime? date = ColumnViewModel.GetDate(this.ColumnId);
        return new TimeData()
        {
          StartDate = date,
          IsAllDay = new bool?(true),
          IsDefault = true,
          Reminders = TimeData.GetDefaultAllDayReminders()
        };
      }
      TimeData timeData = this.Project?.GetTimeData();
      if (timeData != null && this.ColumnId == "note")
      {
        timeData.Reminders = (List<TaskReminderModel>) null;
        if (!(this.Project is TodayProjectIdentity) && !(this.Project is TomorrowProjectIdentity) && !(this.Project is WeekProjectIdentity))
          timeData = (TimeData) null;
      }
      return timeData;
    }

    public override string GetColumnId() => this.ColumnId;

    public override List<string> GetTags()
    {
      if (this.ColumnId.StartsWith("tag:"))
      {
        if (this.ColumnId == "tag:#notag")
          return new List<string>();
        return new List<string>()
        {
          this.ColumnId.Replace("tag:", "")
        };
      }
      return this.Project?.GetTags();
    }

    public override string GetAssignee()
    {
      if (!this.ColumnId.StartsWith("assign:"))
        return (string) null;
      return this.ColumnId == "assign:-1" ? (string) null : this.ColumnId.Replace("assign:", "");
    }
  }
}
