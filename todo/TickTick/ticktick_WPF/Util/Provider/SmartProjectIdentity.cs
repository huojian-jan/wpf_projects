// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.SmartProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class SmartProjectIdentity : ProjectIdentity
  {
    public static SmartProjectIdentity BuildSmartProject(string smartId)
    {
      if (smartId != null)
      {
        switch (smartId.Length)
        {
          case 15:
            if (smartId == "_special_id_all")
              return (SmartProjectIdentity) new AllProjectIdentity();
            break;
          case 16:
            if (smartId == "_special_id_week")
              return (SmartProjectIdentity) new WeekProjectIdentity();
            break;
          case 17:
            switch (smartId[13])
            {
              case 'o':
                if (smartId == "_special_id_today")
                  return (SmartProjectIdentity) new TodayProjectIdentity();
                break;
              case 'r':
                if (smartId == "_special_id_trash")
                  return (SmartProjectIdentity) new TrashProjectIdentity();
                break;
            }
            break;
          case 19:
            if (smartId == "_special_id_summary")
              return (SmartProjectIdentity) new SummaryProjectIdentity();
            break;
          case 20:
            switch (smartId[12])
            {
              case 'a':
                if (smartId == "_special_id_assigned")
                  return (SmartProjectIdentity) new AssignToMeProjectIdentity();
                break;
              case 't':
                if (smartId == "_special_id_tomorrow")
                  return (SmartProjectIdentity) new TomorrowProjectIdentity();
                break;
            }
            break;
          case 21:
            switch (smartId[12])
            {
              case 'a':
                if (smartId == "_special_id_abandoned")
                  return (SmartProjectIdentity) new AbandonedProjectIdentity();
                break;
              case 'c':
                if (smartId == "_special_id_completed")
                  return (SmartProjectIdentity) new CompletedProjectIdentity();
                break;
            }
            break;
        }
      }
      return (SmartProjectIdentity) null;
    }

    public override string ViewMode
    {
      get => SmartProjectService.GetSmartProjectViewMode(this.Id) ?? "list";
    }

    public override async Task SwitchViewMode(string viewMode)
    {
      SmartProjectIdentity smartProjectIdentity = this;
      if ((smartProjectIdentity.Id == "_special_id_today" || smartProjectIdentity.Id == "_special_id_tomorrow" || smartProjectIdentity.Id == "_special_id_week") && viewMode == "timeline")
        return;
      SmartProjectService.SaveSmartProjectViewMode(smartProjectIdentity.Id, viewMode);
      DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) SmartProjectIdentity.BuildSmartProject(smartProjectIdentity.Id));
    }

    public override List<string> GetSwitchViewModes()
    {
      string id = this.Id;
      if (id == "_special_id_today" || id == "_special_id_tomorrow" || id == "_special_id_week")
        return new List<string>() { "list", "kanban" };
      return new List<string>()
      {
        "list",
        "kanban",
        "timeline"
      };
    }

    public override TimelineModel GetTimelineModel()
    {
      return SmartProjectService.GetSmartProjectTimeline(this.Id).Copy();
    }

    public override void CommitTimeline(TimelineModel model)
    {
      if (model == null)
        return;
      if (model.SortType == "priority")
        model.SortType = "project";
      SmartProjectService.SaveSmartProjectTimeline(this.Id, model);
    }

    public override List<SortTypeViewModel> GetTimelineSortTypes()
    {
      return SortOptionHelper.GetSmartProjectSortTypeModels(inTimeline: true);
    }

    public override Geometry GetProjectIcon()
    {
      return SpecialListUtils.GetIconBySmartType(SpecialListUtils.GetSmartTypeById(this.Id));
    }
  }
}
