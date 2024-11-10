// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.SubscribeCalendarViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class SubscribeCalendarViewModel : SelectableItemViewModel
  {
    public readonly string AccountId;

    public SubscribeCalendarViewModel(string id, string title, string accountId = "")
    {
      this.Id = id;
      this.Title = title;
      this.Icon = SubscribeCalendarHelper.GetCalendarProjectIconById(accountId);
      this.Type = "subscribe_calendar";
      this.AccountId = accountId;
      this.ParentId = accountId;
    }
  }
}
