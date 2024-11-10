// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.WeekProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Service;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class WeekProjectIdentity : SmartProjectIdentity
  {
    public WeekProjectIdentity()
    {
      this.SortOption = SmartProjectService.GetSmartProjectSortOption("week", false);
    }

    public override string Id => "_special_id_week";

    public override string CatId => "n7ds";

    public override TimeData GetTimeData()
    {
      return new TimeData()
      {
        StartDate = new DateTime?(DateTime.Today),
        IsAllDay = new bool?(true),
        IsDefault = true,
        Reminders = TimeData.GetDefaultAllDayReminders()
      };
    }

    public override string GetDisplayTitle() => Utils.GetString("Next7Day");

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return ProjectIdentity.CreateSmartIdentity(project.Id);
    }

    public override DateTime? GetCompletedFromTime() => new DateTime?(DateTime.Today);
  }
}
