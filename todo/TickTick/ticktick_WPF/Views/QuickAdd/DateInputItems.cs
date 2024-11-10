// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.DateInputItems
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.ObjectModel;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class DateInputItems : BaseInputItems<DateTime>
  {
    public DateInputItems() => this.LoadData();

    protected override ObservableCollection<InputItemViewModel<DateTime>> InitData()
    {
      ObservableCollection<InputItemViewModel<DateTime>> observableCollection1 = new ObservableCollection<InputItemViewModel<DateTime>>();
      observableCollection1.Add(new InputItemViewModel<DateTime>(DateUtils.FormatMonthDay(DateTime.Today) + "  " + Utils.GetString("Today"), Utils.GetString("Today"), DateTime.Today));
      ObservableCollection<InputItemViewModel<DateTime>> observableCollection2 = observableCollection1;
      DateTime today1 = DateTime.Today;
      string title1 = DateUtils.FormatMonthDay(today1.AddDays(1.0)) + "  " + Utils.GetString("Tomorrow");
      string str1 = Utils.GetString("Tomorrow");
      today1 = DateTime.Today;
      DateTime entity1 = today1.AddDays(1.0);
      InputItemViewModel<DateTime> inputItemViewModel1 = new InputItemViewModel<DateTime>(title1, str1, entity1);
      observableCollection2.Add(inputItemViewModel1);
      ObservableCollection<InputItemViewModel<DateTime>> observableCollection3 = observableCollection1;
      string title2 = DateUtils.FormatMonthDay(DateTime.Today.AddDays(7.0)) + "  " + Utils.GetString("Next") + (Utils.IsDida() ? string.Empty : " ") + DateUtils.FormatWeekDayName(DateTime.Today.AddDays(7.0));
      string str2 = Utils.GetString("Next");
      string str3 = Utils.IsDida() ? string.Empty : " ";
      DateTime today2 = DateTime.Today;
      string str4 = DateUtils.FormatWeekDayName(today2.AddDays(7.0));
      string str5 = str2 + str3 + str4;
      today2 = DateTime.Today;
      DateTime entity2 = today2.AddDays(7.0);
      InputItemViewModel<DateTime> inputItemViewModel2 = new InputItemViewModel<DateTime>(title2, str5, entity2);
      observableCollection3.Add(inputItemViewModel2);
      ObservableCollection<InputItemViewModel<DateTime>> observableCollection4 = observableCollection1;
      DateTime today3 = DateTime.Today;
      string title3 = DateUtils.FormatMonthDay(today3.AddMonths(1)) + "  " + Utils.GetString("NextMonth");
      string str6 = Utils.GetString("NextMonth");
      today3 = DateTime.Today;
      DateTime entity3 = today3.AddMonths(1);
      InputItemViewModel<DateTime> inputItemViewModel3 = new InputItemViewModel<DateTime>(title3, str6, entity3);
      observableCollection4.Add(inputItemViewModel3);
      return observableCollection1;
    }
  }
}
