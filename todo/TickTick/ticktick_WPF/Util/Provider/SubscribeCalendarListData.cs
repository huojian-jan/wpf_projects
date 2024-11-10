// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.SubscribeCalendarListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class SubscribeCalendarListData : SortProjectData
  {
    public SubscribeCalendarListData(CalendarSubscribeProfileModel profile)
    {
      this.ShowProjectSort = false;
      this.ShowCustomSort = false;
      this.Profile = profile;
      this.ShowLoadMore = false;
      this.EmptyTitle = Utils.GetString("CalendarA");
      this.EmptyContent = Utils.GetString("CalendarB");
      this.EmptyPath = Utils.GetIconData("IcEmptyCalendar");
      this.AddTaskHint = string.Empty;
    }

    public CalendarSubscribeProfileModel Profile { get; set; }

    public override string GetTitle() => this.Profile.Name;

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptyCalendarDrawingImage") as DrawingImage;
    }

    public override bool ShowCalendarExpired() => this.Profile.Expired;
  }
}
