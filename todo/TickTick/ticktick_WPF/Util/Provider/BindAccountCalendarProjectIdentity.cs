// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.BindAccountCalendarProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public sealed class BindAccountCalendarProjectIdentity : ProjectIdentity
  {
    private readonly BindCalendarModel _defaultCalendar;
    public readonly BindCalendarAccountModel Account;

    public override string SortProjectId => (string) null;

    public BindAccountCalendarProjectIdentity(BindCalendarAccountModel account)
    {
      this.Account = account;
      this.Id = account.Id;
      this.SortOption = SortOptionUtils.GetSortOptionBySortType(Constants.SortType.dueDate, false);
      this._defaultCalendar = SubscribeCalendarHelper.GetDefaultCalendar(account.Id);
      this.CanDrag = false;
    }

    public override bool IsCalendar() => true;

    public override string GetAccountId() => this.Account.Id;

    public override string GetProjectId() => this._defaultCalendar?.Id;

    public override string GetProjectName() => this._defaultCalendar?.Name;

    public override List<string> GetTags() => new List<string>();

    public override TimeData GetTimeData()
    {
      return new TimeData()
      {
        StartDate = new DateTime?(DateTime.Today),
        IsAllDay = new bool?(true),
        IsDefault = true
      };
    }

    public override string GetDisplayTitle()
    {
      if (!string.IsNullOrEmpty(this.Account?.Description))
        return this.Account.Description;
      return this.Account?.Account;
    }
  }
}
