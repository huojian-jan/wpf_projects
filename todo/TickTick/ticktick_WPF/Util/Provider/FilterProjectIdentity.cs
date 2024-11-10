// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.FilterProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public sealed class FilterProjectIdentity : ProjectIdentity
  {
    public FilterProjectIdentity(FilterModel filter)
    {
      this.Filter = filter;
      this.TaskDefault = FilterViewModel.CalculateTaskDefault(this.Filter.rule);
      this.SortOption = filter.GetSortOption();
    }

    public FilterModel Filter { get; }

    public string FilterId => this.Filter?.id;

    public override string CatId => this.Filter?.id;

    public override string Id => this.Filter?.id;

    public override string SortProjectId => this.Filter?.id;

    public override string QueryId => this.Filter?.id;

    public override bool LoadAll => false;

    public override bool IsNote
    {
      get
      {
        FilterTaskDefault taskDefault = this.TaskDefault;
        return taskDefault != null && taskDefault.IsNote;
      }
    }

    public bool OnlyNote
    {
      get
      {
        FilterTaskDefault taskDefault = this.TaskDefault;
        return taskDefault != null && taskDefault.OnlyNote;
      }
    }

    private FilterTaskDefault TaskDefault { get; }

    public override string Title => this.Filter?.name;

    public override string ViewMode => this.Filter?.viewMode ?? "list";

    public override string GetProjectId() => this.TaskDefault.ProjectModel.id;

    public override string GetProjectName() => this.TaskDefault.ProjectModel.name;

    public override TimeData GetTimeData()
    {
      if (this.GetIsNote())
        return new TimeData();
      return new TimeData()
      {
        StartDate = this.TaskDefault.DefaultDate,
        Reminders = TimeData.GetDefaultAllDayReminders(),
        IsDefault = true,
        IsAllDay = new bool?(true)
      };
    }

    public int? GetDefaultPriority() => this.TaskDefault.Priority;

    public override int GetPriority()
    {
      return this.TaskDefault.Priority ?? TaskDefaultDao.GetDefaultSafely().Priority;
    }

    public override string GetDisplayTitle() => this.Filter?.name;

    public override Geometry GetProjectIcon()
    {
      return SpecialListUtils.GetIconBySmartType(SmartListType.Filter);
    }

    public override List<string> GetTags() => this.TaskDefault.DefaultTags;

    public override bool UseDefaultTags() => false;

    public override bool GetIsNote() => this.TaskDefault.IsNote;

    public bool FilterNote() => this.TaskDefault.OnlyNote;

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new FilterProjectIdentity(CacheManager.GetFilterById(project.CatId) ?? ((FilterProjectIdentity) project).Filter);
    }

    public override TimelineModel GetTimelineModel()
    {
      if (this.Filter == null)
        return (TimelineModel) null;
      TimelineModel timeline = this.Filter.Timeline;
      if (timeline != null)
        return timeline.Copy();
      this.Filter.Timeline = new TimelineModel(Constants.SortType.project.ToString());
      this.CommitTimeline(this.Filter.Timeline);
      return this.Filter.Timeline.Copy();
    }

    public override void CommitTimeline(TimelineModel model)
    {
      if (model == null)
        return;
      FilterModel filterById = CacheManager.GetFilterById(this.Id);
      if (filterById == null || filterById.Timeline != null && filterById.Timeline.IsEquals(model))
        return;
      if (model.SortType == "priority")
        model.SortType = filterById.Timeline?.SortType ?? "project";
      bool needSync = filterById.Timeline == null || filterById.Timeline.SyncPropertyChanged(model);
      filterById.Timeline = model;
      this.Filter.Timeline = model;
      if (filterById.SyncTimeline != null)
      {
        filterById.SyncTimeline.SortType = model.SortType;
        filterById.SyncTimeline.SortOption = model.sortOption;
      }
      else
        filterById.SyncTimeline = new TimelineSyncModel()
        {
          SortType = model.SortType,
          SortOption = model.sortOption
        };
      FilterDao.UpdateFilter(filterById, needSync);
    }

    public override List<SortTypeViewModel> GetTimelineSortTypes()
    {
      return SortOptionHelper.GetSmartProjectSortTypeModels(this.IsNote, true);
    }

    public override async Task SwitchViewMode(string viewMode)
    {
      FilterModel filter = CacheManager.GetFilterById(this.Id);
      if (filter == null)
      {
        filter = (FilterModel) null;
      }
      else
      {
        filter.viewMode = viewMode;
        switch (viewMode)
        {
          case "kanban":
            if (filter.SortOption?.groupBy == "none")
            {
              filter.SortOption.groupBy = "project";
              break;
            }
            break;
          case "timeline":
            TimelineModel.CheckTimelineEmpty((ITimeline) filter, Constants.SortType.project);
            break;
        }
        await FilterDao.UpdateFilter(filter);
        DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new FilterProjectIdentity(filter));
        SyncManager.TryDelaySync();
        filter = (FilterModel) null;
      }
    }

    public override List<string> GetSwitchViewModes()
    {
      return new List<string>()
      {
        "list",
        "kanban",
        "timeline"
      };
    }

    public override string CheckTitleValid(string name)
    {
      if (string.IsNullOrEmpty(name))
        return Utils.GetString("AddOrEditProjectNameCantNull");
      if (name.StartsWith("#"))
        return Utils.GetString("ListNameBeginError");
      if (!NameUtils.IsValidNameNoCheckSharp(name, false))
        return Utils.GetString("ListNameCantContain");
      return CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id != this.Filter.id && f.name == name)) != null ? Utils.GetString("AddOrEditProjectNameRepeat") : (string) null;
    }

    public override async void SaveTitle(string text)
    {
      this.Filter.name = text.Trim();
      await FilterSyncJsonDao.TrySaveFilter(this.Filter.id);
      await FilterDao.UpdateFilter(this.Filter);
      SyncManager.Sync();
      ListViewContainer.ReloadProjectData(false);
    }
  }
}
