// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.SubscribeCalendarProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public sealed class SubscribeCalendarProjectIdentity : ProjectIdentity
  {
    public readonly CalendarSubscribeProfileModel Profile;

    public override string SortProjectId => (string) null;

    public SubscribeCalendarProjectIdentity(CalendarSubscribeProfileModel profile)
    {
      this.Profile = profile;
      this.Id = profile.Id;
      this.SortOption = SortOptionUtils.GetSortOptionBySortType(Constants.SortType.dueDate, false);
      this.CanDrag = false;
    }

    public override List<string> GetTags() => new List<string>();

    public override string GetDisplayTitle() => this.Profile?.Name;
  }
}
