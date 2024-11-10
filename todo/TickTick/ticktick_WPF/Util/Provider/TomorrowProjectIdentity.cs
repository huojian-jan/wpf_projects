// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TomorrowProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Service;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TomorrowProjectIdentity : SmartProjectIdentity
  {
    public override string Id => "_special_id_tomorrow";

    public override string CatId => "tomorrow";

    public override TimeData GetTimeData()
    {
      return new TimeData()
      {
        StartDate = new DateTime?(DateTime.Today.AddDays(1.0)),
        IsAllDay = new bool?(true),
        IsDefault = true,
        Reminders = TimeData.GetDefaultAllDayReminders()
      };
    }

    public TomorrowProjectIdentity()
    {
      this.SortOption = SmartProjectService.GetSmartProjectSortOption("tomorrow", false);
    }

    public override string GetDisplayTitle() => Utils.GetString("Tomorrow");

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return ProjectIdentity.CreateSmartIdentity(project.Id);
    }
  }
}
