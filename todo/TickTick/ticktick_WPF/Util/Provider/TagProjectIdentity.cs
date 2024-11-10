// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TagProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public sealed class TagProjectIdentity : ProjectIdentity
  {
    public override string ViewMode => this.TagModel?.viewMode ?? "list";

    public TagProjectIdentity(TagModel tag)
    {
      this.TagModel = tag;
      this.Tag = tag.name;
      this.SortOption = tag.GetSortOption();
    }

    public string Tag { get; }

    public TagModel TagModel { get; private set; }

    public override string CatId => "#" + this.Tag;

    public override string SortProjectId => "#" + this.Tag;

    public override string Id => this.Tag;

    public override string QueryId => this.Tag;

    public override bool LoadAll => true;

    public override string Title => this.Tag;

    public override string GetDisplayTitle() => TagDataHelper.GetTagDisplayName(this.Tag);

    public override List<string> GetTags()
    {
      return new List<string>() { this.Tag };
    }

    public override bool UseDefaultTags() => false;

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new TagProjectIdentity(((TagProjectIdentity) project).TagModel);
    }

    public override TimelineModel GetTimelineModel()
    {
      if (this.TagModel == null)
        return (TimelineModel) null;
      TimelineModel timeline = this.TagModel.Timeline;
      if (timeline != null)
        return timeline.Copy();
      this.TagModel.Timeline = new TimelineModel("project");
      this.CommitTimeline(this.TagModel.Timeline);
      return this.TagModel.Timeline.Copy();
    }

    public override void CommitTimeline(TimelineModel model)
    {
      if (model == null)
        return;
      TagModel tagByName = CacheManager.GetTagByName(this.Tag);
      if (tagByName == null || tagByName.Timeline != null && tagByName.Timeline.IsEquals(model))
        return;
      if ((tagByName.Timeline == null || tagByName.Timeline.SyncPropertyChanged(model)) && tagByName.status != 0)
        tagByName.status = 1;
      if (model.SortType == "priority")
        model.SortType = tagByName.Timeline?.SortType ?? "project";
      if (tagByName.SyncTimeline != null)
      {
        tagByName.SyncTimeline.SortType = model.SortType;
        tagByName.SyncTimeline.SortOption = model.sortOption;
      }
      else
        tagByName.SyncTimeline = new TimelineSyncModel()
        {
          SortType = model.SortType,
          SortOption = model.sortOption
        };
      tagByName.Timeline = model;
      this.TagModel = tagByName;
      TagDao.UpdateTag(tagByName);
    }

    public override List<SortTypeViewModel> GetTimelineSortTypes()
    {
      List<SortTypeViewModel> projectSortTypeModels = SortOptionHelper.GetSmartProjectSortTypeModels(inTimeline: true);
      if (!this.TagModel.IsParent())
        projectSortTypeModels.RemoveAll((Predicate<SortTypeViewModel>) (s => s.Id == "tag"));
      return projectSortTypeModels;
    }

    public override async Task SwitchViewMode(string viewMode)
    {
      TagModel tag = CacheManager.GetTagByName(this.Tag);
      if (tag == null)
      {
        tag = (TagModel) null;
      }
      else
      {
        await TagDao.SwitchViewModel(tag, viewMode);
        DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new TagProjectIdentity(tag));
        SyncManager.TryDelaySync();
        tag = (TagModel) null;
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

    public override Geometry GetProjectIcon()
    {
      return SpecialListUtils.GetIconBySmartType(SmartListType.Tag);
    }
  }
}
