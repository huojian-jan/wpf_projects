// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.BindAccountCalendarListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class BindAccountCalendarListData : SortProjectData
  {
    public BindAccountCalendarListData(BindCalendarAccountModel account)
    {
      this.ShowProjectSort = false;
      this.ShowCustomSort = false;
      this.Account = account;
      this.ShowLoadMore = false;
      this.EmptyTitle = Utils.GetString(account.Expired ? "CalendarExpiredTitle" : "CalendarA");
      this.EmptyContent = Utils.GetString(account.Expired ? "CalendarExpiredSummary" : "CalendarB");
      this.EmptyPath = Utils.GetIconData("IcEmptyCalendar");
      this.AddTaskHint = string.Empty;
      BindCalendarModel defaultCalendar = SubscribeCalendarHelper.GetDefaultCalendar(account.Id);
      this.AddTaskHint = defaultCalendar == null || account.Expired ? string.Empty : string.Format(Utils.GetString("AddAgendaTo"), (object) defaultCalendar.Name);
    }

    public BindCalendarAccountModel Account { get; set; }

    public override string GetTitle()
    {
      if (this.Account.Site == "feishu")
        return Utils.GetString("FeishuCalendar");
      return !string.IsNullOrEmpty(this.Account.Description) ? this.Account.Description : this.Account.Account;
    }

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptyCalendarDrawingImage") as DrawingImage;
    }

    public override bool ShowCalendarExpired() => this.Account.Expired;
  }
}
