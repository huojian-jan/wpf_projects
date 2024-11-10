// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.CalendarInputItems
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class CalendarInputItems : BaseInputItems<string>
  {
    public readonly string AccountId;

    public CalendarInputItems(string accountId)
    {
      this.AccountId = accountId;
      this.LoadData();
    }

    protected override ObservableCollection<InputItemViewModel<string>> InitData()
    {
      ObservableCollection<InputItemViewModel<string>> observableCollection = new ObservableCollection<InputItemViewModel<string>>();
      BindCalendarAccountModel calendarAccountModel = CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (account => account.Id == this.AccountId));
      if (calendarAccountModel?.Calendars != null && calendarAccountModel.Calendars.Any<BindCalendarModel>())
      {
        foreach (BindCalendarModel calendar in calendarAccountModel.Calendars)
        {
          if (calendar.Accessible)
            observableCollection.Add(new InputItemViewModel<string>(calendar.Name, calendar.Id));
        }
      }
      return observableCollection;
    }
  }
}
