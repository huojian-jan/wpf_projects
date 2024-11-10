// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Completed.ClosedFilterViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Views.Completed
{
  public class ClosedFilterViewModel : BaseViewModel
  {
    private ProjectOrGroupFilter _projectOrGroupFilter;
    private DateFilter _dateFilter;
    private List<string> _selectedProjectGroupIds = new List<string>();
    private List<string> _selectedProjectIds = new List<string>();
    private DateTime? _startDate;
    private DateTime? _endDate;
    private string _selectedProjectDisplayText = Utils.GetString("AllList");
    private string _selectedStatusDisplayText = Utils.GetString("AllStatus");
    private string _selectedPriorityDisplayText = Utils.GetString("all_priorities");
    private bool _showAll = true;
    public bool IsCompleted = true;

    public string PersonalOrAllText
    {
      get
      {
        return Utils.GetString(this.ShowAll ? (this.IsCompleted ? "AllCompleted" : "AllWontDo") : (this.IsCompleted ? "CompletedbyMe" : "WontDobyMe"));
      }
    }

    public bool ShowAll
    {
      get => this._showAll;
      set
      {
        this._showAll = value;
        this.OnPropertyChanged("PersonalOrAllText");
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

    public string SelectedProjectDisplayText
    {
      get => this._selectedProjectDisplayText;
      set
      {
        this._selectedProjectDisplayText = value;
        this.OnPropertyChanged(nameof (SelectedProjectDisplayText));
      }
    }

    public string SelectedStatusDisplayText
    {
      get => this._selectedStatusDisplayText;
      set
      {
        this._selectedStatusDisplayText = value;
        this.OnPropertyChanged(nameof (SelectedStatusDisplayText));
      }
    }

    public string SelectedPriorityDisplayText
    {
      get => this._selectedPriorityDisplayText;
      set
      {
        this._selectedPriorityDisplayText = value;
        this.OnPropertyChanged(nameof (SelectedPriorityDisplayText));
      }
    }

    public bool Changed { get; set; } = true;

    public async Task<CompletedFilterData> GetCompletedFilterData()
    {
      CompletedFilterData data = new CompletedFilterData();
      DateTime dateTime1 = DateTime.Now;
      int dayOfWeek = (int) dateTime1.DayOfWeek;
      int weekFromDiff = Utils.GetWeekFromDiff();
      switch (this.DateFilter)
      {
        case DateFilter.All:
          data.FromTime = new DateTime?();
          CompletedFilterData completedFilterData1 = data;
          dateTime1 = DateTime.Now;
          dateTime1 = dateTime1.Date;
          DateTime dateTime2 = dateTime1.AddDays(1.0);
          completedFilterData1.ToTime = dateTime2;
          break;
        case DateFilter.ThisWeek:
          CompletedFilterData completedFilterData2 = data;
          dateTime1 = DateTime.Today;
          DateTime? nullable1 = new DateTime?(dateTime1.AddDays((double) (weekFromDiff - dayOfWeek)));
          completedFilterData2.FromTime = nullable1;
          CompletedFilterData completedFilterData3 = data;
          dateTime1 = DateTime.Today;
          DateTime dateTime3 = dateTime1.AddDays((double) (weekFromDiff - dayOfWeek + 7));
          completedFilterData3.ToTime = dateTime3;
          break;
        case DateFilter.LastWeek:
          CompletedFilterData completedFilterData4 = data;
          dateTime1 = DateTime.Today;
          DateTime? nullable2 = new DateTime?(dateTime1.AddDays((double) (weekFromDiff - dayOfWeek - 7)));
          completedFilterData4.FromTime = nullable2;
          CompletedFilterData completedFilterData5 = data;
          dateTime1 = DateTime.Today;
          DateTime dateTime4 = dateTime1.AddDays((double) (weekFromDiff - dayOfWeek));
          completedFilterData5.ToTime = dateTime4;
          break;
        case DateFilter.ThisMonth:
          dateTime1 = DateTime.Now;
          dateTime1 = dateTime1.Date;
          int day = dateTime1.Day;
          CompletedFilterData completedFilterData6 = data;
          dateTime1 = DateTime.Now;
          dateTime1 = dateTime1.Date;
          DateTime? nullable3 = new DateTime?(dateTime1.AddDays((double) (1 - day)));
          completedFilterData6.FromTime = nullable3;
          CompletedFilterData completedFilterData7 = data;
          dateTime1 = DateTime.Now;
          dateTime1 = dateTime1.Date;
          dateTime1 = dateTime1.AddDays((double) (1 - day));
          DateTime dateTime5 = dateTime1.AddMonths(1);
          completedFilterData7.ToTime = dateTime5;
          break;
        case DateFilter.Custom:
          data.FromTime = this.StartDate;
          CompletedFilterData completedFilterData8 = data;
          dateTime1 = this.EndDate ?? DateTime.Now;
          dateTime1 = dateTime1.Date;
          DateTime dateTime6 = dateTime1.AddDays(1.0);
          completedFilterData8.ToTime = dateTime6;
          break;
      }
      switch (this.ProjectOrGroupFilter)
      {
        case ProjectOrGroupFilter.All:
          data.ProjectIds = new List<string>();
          this._selectedProjectIds = new List<string>();
          break;
        case ProjectOrGroupFilter.Custom:
          data.ProjectIds = this.SelectedProjectIds;
          if (this.SelectedProjectGroupIds.Count > 0)
          {
            foreach (string selectedProjectGroupId in this.SelectedProjectGroupIds)
            {
              ObservableCollection<ProjectModel> projectsInGroup = await ProjectDao.GetProjectsInGroup(selectedProjectGroupId);
              if (projectsInGroup.Count > 0)
                data.ProjectIds = data.ProjectIds.Union<string>(projectsInGroup.Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.id))).ToList<string>();
            }
            break;
          }
          break;
      }
      CompletedFilterData completedFilterData9 = data;
      data = (CompletedFilterData) null;
      return completedFilterData9;
    }
  }
}
