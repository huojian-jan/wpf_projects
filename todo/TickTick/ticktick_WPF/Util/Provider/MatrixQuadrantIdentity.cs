// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.MatrixQuadrantIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class MatrixQuadrantIdentity : ProjectIdentity
  {
    public QuadrantModel Quadrant { get; }

    public MatrixQuadrantIdentity(QuadrantModel quadrant)
    {
      this.Quadrant = quadrant;
      this.SortOption = this.Quadrant.GetSortOption();
      this.TaskDefault = FilterViewModel.CalculateTaskDefault(quadrant.rule);
      this.IsNote = this.TaskDefault.IsNote;
    }

    public override string CatId => this.Quadrant?.id;

    public override string SortProjectId => this.Quadrant?.id;

    public override string Id => this.Quadrant?.id;

    public override string QueryId => this.Quadrant?.id;

    public override bool LoadAll => false;

    public override bool IsNote
    {
      get
      {
        FilterTaskDefault taskDefault = this.TaskDefault;
        return taskDefault != null && taskDefault.IsNote;
      }
    }

    private FilterTaskDefault TaskDefault { get; }

    public override string GetProjectId() => this.TaskDefault.ProjectModel.id;

    public override string GetProjectName() => this.TaskDefault.ProjectModel.name;

    public override bool GetIsNote() => this.TaskDefault.IsNote;

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

    public override List<string> GetTags() => this.TaskDefault.DefaultTags;

    public override bool UseDefaultTags() => false;

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new MatrixQuadrantIdentity(((MatrixQuadrantIdentity) project).Quadrant);
    }
  }
}
