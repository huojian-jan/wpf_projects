// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.SyncResult
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class SyncResult
  {
    public int SyncType { get; set; }

    public bool ColumnChanged { get; set; }

    public bool TeamChanged { get; set; }

    public NotificationUnreadCount NotificationCount { get; set; }

    public bool RemoteDataChanged
    {
      get
      {
        return this.RemoteProjectGroupsChanged || this.RemoteProjectsChanged || this.RemoteFiltersChanged || this.RemoteTasksChanged || this.RemoteTagsChanged;
      }
    }

    public List<ProjectModel> AddedProjects { get; set; } = new List<ProjectModel>();

    public List<ProjectModel> UpdatedProjects { get; set; } = new List<ProjectModel>();

    public List<ProjectModel> DeletedProjects { get; set; } = new List<ProjectModel>();

    public bool RemoteProjectsChanged
    {
      get
      {
        return this.AddedProjects.Count > 0 || this.UpdatedProjects.Count > 0 || this.DeletedProjects.Count > 0;
      }
    }

    public bool RemoteGroupChanged
    {
      get
      {
        return this.AddedProjectGroups.Any<ProjectGroupModel>() || this.UpdatedProjectGroups.Any<ProjectGroupModel>() || this.DeletedProjectGroups.Any<ProjectGroupModel>();
      }
    }

    public bool RemoteTagChanged
    {
      get
      {
        return this.AddedTags.Any<TagModel>() || this.UpdatedTags.Any<TagModel>() || this.DeletedTags.Any<TagModel>();
      }
    }

    public List<ProjectGroupModel> AddedProjectGroups { get; set; } = new List<ProjectGroupModel>();

    public List<ProjectGroupModel> UpdatedProjectGroups { get; set; } = new List<ProjectGroupModel>();

    public List<ProjectGroupModel> DeletedProjectGroups { get; set; } = new List<ProjectGroupModel>();

    private bool RemoteProjectGroupsChanged
    {
      get
      {
        return this.AddedProjectGroups.Count > 0 || this.UpdatedProjectGroups.Count > 0 || this.DeletedProjectGroups.Count > 0;
      }
    }

    public List<FilterModel> AddedFilters { get; set; } = new List<FilterModel>();

    public List<FilterModel> UpdatedFilters { get; set; } = new List<FilterModel>();

    public List<string> DeletedFilterIds { get; set; } = new List<string>();

    public bool RemoteFiltersChanged
    {
      get
      {
        return this.AddedFilters.Count > 0 || this.UpdatedFilters.Count > 0 || this.DeletedFilterIds.Count > 0;
      }
    }

    public List<TaskModel> AddedTasks { get; set; } = new List<TaskModel>();

    public List<TaskModel> UpdatedTasks { get; set; } = new List<TaskModel>();

    public List<TaskProject> DeletedTasks { get; set; } = new List<TaskProject>();

    public List<TaskProject> DeletedInTrashTasks { get; set; } = new List<TaskProject>();

    public List<TaskProject> DeletedForeverTasks { get; set; } = new List<TaskProject>();

    public List<TagModel> AddedTags { get; set; } = new List<TagModel>();

    public List<TagModel> UpdatedTags { get; set; } = new List<TagModel>();

    public List<TagModel> DeletedTags { get; set; } = new List<TagModel>();

    public bool RemoteTasksChanged
    {
      get
      {
        return this.AddedTasks.Count > 0 || this.UpdatedTasks.Count > 0 || this.DeletedTasks.Count > 0 || this.DeletedInTrashTasks.Count > 0 || this.DeletedForeverTasks.Count > 0 || this.TaskSortChanged;
      }
    }

    private bool RemoteTagsChanged
    {
      get => this.AddedTags.Count > 0 || this.UpdatedTags.Count > 0 || this.DeletedTags.Count > 0;
    }

    public bool TaskSortChanged { get; set; }

    public void Clear()
    {
      this.AddedProjects.Clear();
      this.UpdatedProjects.Clear();
      this.DeletedProjects.Clear();
      this.AddedProjectGroups.Clear();
      this.UpdatedProjectGroups.Clear();
      this.AddedFilters.Clear();
      this.UpdatedFilters.Clear();
      this.DeletedFilterIds.Clear();
      this.AddedTags.Clear();
      this.UpdatedTags.Clear();
      this.DeletedTags.Clear();
      this.TaskSortChanged = false;
    }

    public void InitNotification() => this.NotificationCount = new NotificationUnreadCount(0, 0);

    public SyncResult Copy()
    {
      return new SyncResult()
      {
        AddedProjects = this.AddedProjects.ToList<ProjectModel>(),
        UpdatedProjects = this.UpdatedProjects.ToList<ProjectModel>(),
        DeletedProjects = this.DeletedProjects.ToList<ProjectModel>(),
        AddedProjectGroups = this.AddedProjectGroups.ToList<ProjectGroupModel>(),
        UpdatedProjectGroups = this.UpdatedProjectGroups.ToList<ProjectGroupModel>(),
        AddedFilters = this.AddedFilters.ToList<FilterModel>(),
        UpdatedFilters = this.UpdatedFilters.ToList<FilterModel>(),
        DeletedFilterIds = this.DeletedFilterIds.ToList<string>(),
        AddedTags = this.AddedTags.ToList<TagModel>(),
        UpdatedTags = this.UpdatedTags.ToList<TagModel>(),
        DeletedTags = this.DeletedTags.ToList<TagModel>(),
        UpdatedTasks = this.UpdatedTasks.ToList<TaskModel>(),
        AddedTasks = this.AddedTasks.ToList<TaskModel>(),
        DeletedTasks = this.DeletedTasks.ToList<TaskProject>(),
        DeletedForeverTasks = this.DeletedForeverTasks.ToList<TaskProject>(),
        DeletedInTrashTasks = this.DeletedInTrashTasks.ToList<TaskProject>(),
        TaskSortChanged = this.TaskSortChanged,
        NotificationCount = this.NotificationCount,
        TeamChanged = this.TeamChanged
      };
    }
  }
}
