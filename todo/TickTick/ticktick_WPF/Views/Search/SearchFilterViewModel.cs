// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchFilterViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchFilterViewModel : BaseViewModel
  {
    private DateFilter _dateFilter;
    private DateTime? _endDate;
    private ProjectOrGroupFilter _projectOrGroupFilter;
    private string _searchKey = string.Empty;
    private string _selectedProjectDisplayText = Utils.GetString("AllList");
    private List<string> _selectedProjectGroupIds = new List<string>();
    private List<string> _selectedProjectIds = new List<string>();
    private string _selectedTagDisplayText = Utils.GetString("AllTags");
    private List<int> _selectedPriorities = new List<int>();
    private List<string> _selectedAssignees = new List<string>();
    private List<string> _selectedTags = new List<string>();
    private List<string> _searchTags = new List<string>();
    private DateTime? _startDate;
    private StatusFilter _statusFilter;
    private TaskType _selectedType = TaskType.TaskAndNote;
    private bool _showPriorityFilter;
    private bool _showAssignFilter;
    private bool _showTaskTypeFilter;
    private bool _assignFilterEnable = true;
    private bool _taskTypeFilterEnable = true;

    public bool ShowMoreFilter
    {
      get
      {
        if (!this.ShowPriorityFilter || this.AssignFilterEnable && !this.ShowAssignFilter)
          return true;
        return this.TaskTypeFilterEnable && !this.ShowTaskTypeFilter;
      }
    }

    public bool ShowPriorityFilter
    {
      get => this._showPriorityFilter;
      set
      {
        this._showPriorityFilter = value;
        this.OnPropertyChanged(nameof (ShowPriorityFilter));
        this.OnPropertyChanged("ShowMoreFilter");
      }
    }

    public bool ShowAssignFilter
    {
      get => this._showAssignFilter;
      set
      {
        this._showAssignFilter = value;
        this.OnPropertyChanged(nameof (ShowAssignFilter));
        this.OnPropertyChanged("ShowMoreFilter");
      }
    }

    public bool ShowTaskTypeFilter
    {
      get => this._showTaskTypeFilter;
      set
      {
        this._showTaskTypeFilter = value;
        this.OnPropertyChanged(nameof (ShowTaskTypeFilter));
        this.OnPropertyChanged("ShowMoreFilter");
      }
    }

    public bool AssignFilterEnable
    {
      get => this._assignFilterEnable;
      set
      {
        this._assignFilterEnable = value;
        this.OnPropertyChanged(nameof (AssignFilterEnable));
        this.OnPropertyChanged("ShowMoreFilter");
      }
    }

    public bool TaskTypeFilterEnable
    {
      get => this._taskTypeFilterEnable;
      set
      {
        this._taskTypeFilterEnable = value;
        this.OnPropertyChanged(nameof (TaskTypeFilterEnable));
        this.OnPropertyChanged("ShowMoreFilter");
      }
    }

    public List<string> SearchTags
    {
      get => this._searchTags;
      set
      {
        this._searchTags = value;
        this.OnPropertyChanged(nameof (SearchTags));
      }
    }

    public List<string> SelectedTags
    {
      get => this._selectedTags;
      set
      {
        this._selectedTags = value;
        this.OnPropertyChanged(nameof (SelectedTags));
      }
    }

    public List<int> SelectedPriorities
    {
      get => this._selectedPriorities;
      set
      {
        this._selectedPriorities = value;
        this.OnPropertyChanged(nameof (SelectedPriorities));
      }
    }

    public List<string> SelectedAssignees
    {
      get => this._selectedAssignees;
      set
      {
        this._selectedAssignees = value;
        this.OnPropertyChanged(nameof (SelectedAssignees));
      }
    }

    public List<string> SelectedProjectIds
    {
      get => this._selectedProjectIds;
      set
      {
        this._selectedProjectIds = value;
        this.OnPropertyChanged(nameof (SelectedProjectIds));
      }
    }

    public List<string> SelectedProjectGroupIds
    {
      get => this._selectedProjectGroupIds;
      set
      {
        this._selectedProjectGroupIds = value;
        this.OnPropertyChanged(nameof (SelectedProjectGroupIds));
      }
    }

    public TaskType SelectedType
    {
      get => this._selectedType;
      set
      {
        this._selectedType = value;
        this.OnPropertyChanged(nameof (SelectedType));
      }
    }

    public ProjectOrGroupFilter ProjectOrGroupFilter
    {
      get => this._projectOrGroupFilter;
      set
      {
        this._projectOrGroupFilter = value;
        this.OnPropertyChanged(nameof (ProjectOrGroupFilter));
      }
    }

    public DateFilter DateFilter
    {
      get => this._dateFilter;
      set
      {
        this._dateFilter = value;
        this.OnPropertyChanged(nameof (DateFilter));
      }
    }

    public StatusFilter StatusFilter
    {
      get => this._statusFilter;
      set
      {
        this._statusFilter = value;
        this.OnPropertyChanged(nameof (StatusFilter));
      }
    }

    public DateTime? StartDate
    {
      get => this._startDate;
      set
      {
        this._startDate = value;
        this.OnPropertyChanged(nameof (StartDate));
      }
    }

    public DateTime? EndDate
    {
      get => this._endDate;
      set
      {
        this._endDate = value;
        this.OnPropertyChanged(nameof (EndDate));
      }
    }

    public string SelectedTagDisplayText
    {
      get => this._selectedTagDisplayText;
      set
      {
        this._selectedTagDisplayText = value;
        this.OnPropertyChanged(nameof (SelectedTagDisplayText));
      }
    }

    public string SelectedProjectDisplayText
    {
      get => this._selectedProjectDisplayText;
      set
      {
        this._selectedProjectDisplayText = value;
        this.OnPropertyChanged(nameof (SelectedProjectDisplayText));
      }
    }

    public string SearchKey
    {
      get => this._searchKey;
      set
      {
        this._searchKey = value;
        this.OnPropertyChanged(nameof (SearchKey));
      }
    }

    public bool Searched { get; set; }

    public SearchFilterModel ToSearchFilter()
    {
      SearchFilterModel searchFilter = new SearchFilterModel();
      searchFilter.DateFilter = this._dateFilter;
      switch (this._dateFilter)
      {
        case DateFilter.ThisWeek:
          searchFilter.Start = new DateTime?(DateTime.Today.AddDays((double) ((int) DateTime.Today.DayOfWeek * -1 + Utils.GetWeekFromDiff())));
          searchFilter.End = new DateTime?(searchFilter.Start.Value.AddDays(7.0));
          break;
        case DateFilter.ThisMonth:
          DateTime dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          int day = dateTime.Day;
          SearchFilterModel searchFilterModel1 = searchFilter;
          dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          DateTime? nullable1 = new DateTime?(dateTime.AddDays((double) (1 - day)));
          searchFilterModel1.Start = nullable1;
          SearchFilterModel searchFilterModel2 = searchFilter;
          dateTime = searchFilter.Start.Value;
          DateTime? nullable2 = new DateTime?(dateTime.AddMonths(1));
          searchFilterModel2.End = nullable2;
          break;
        case DateFilter.Custom:
          searchFilter.Start = this.StartDate;
          SearchFilterModel searchFilterModel3 = searchFilter;
          DateTime? endDate = this.EndDate;
          ref DateTime? local = ref endDate;
          DateTime? nullable3 = local.HasValue ? new DateTime?(local.GetValueOrDefault().AddDays(1.0)) : this.EndDate;
          searchFilterModel3.End = nullable3;
          break;
      }
      switch (this.StatusFilter)
      {
        case StatusFilter.Uncompleted:
          searchFilter.Status = new int?(0);
          break;
        case StatusFilter.Completed:
          searchFilter.Status = new int?(2);
          break;
        case StatusFilter.Abandoned:
          searchFilter.Status = new int?(-1);
          break;
      }
      if (this.ProjectOrGroupFilter == ProjectOrGroupFilter.Custom)
      {
        searchFilter.ProjectIds = this.SelectedProjectIds;
        if (this.SelectedProjectGroupIds.Count > 0)
        {
          foreach (string selectedProjectGroupId in this.SelectedProjectGroupIds)
          {
            List<ProjectModel> projectsInGroup = CacheManager.GetProjectsInGroup(selectedProjectGroupId);
            if (projectsInGroup.Count > 0)
              searchFilter.ProjectIds.AddRange(projectsInGroup.Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.id)));
          }
        }
      }
      searchFilter.Tags = this._selectedTags;
      searchFilter.Priorities = this._selectedPriorities;
      searchFilter.Assignees = this._selectedAssignees;
      searchFilter.TaskType = this._selectedType;
      searchFilter.GroupIds = this._selectedProjectGroupIds;
      searchFilter.Key = this._searchKey;
      return searchFilter;
    }
  }
}
