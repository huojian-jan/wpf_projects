// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.BindAccountViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class BindAccountViewModel : BaseViewModel
  {
    public string Id { get; set; }

    public string Account { get; set; }

    public List<BindCalendarViewModel> Calendars { get; set; }

    public string Kind { get; set; }

    public static BindAccountViewModel Build(BindCalendarAccountModel model)
    {
      BindAccountViewModel accountViewModel = new BindAccountViewModel()
      {
        Id = model.Id,
        Account = model.Account,
        Kind = model.Kind
      };
      if (model.Site == "feishu")
        accountViewModel.Account = Utils.GetString("FeishuCalendar");
      else if (!string.IsNullOrEmpty(model.Description))
        accountViewModel.Account = model.Description;
      List<BindCalendarViewModel> calendarViewModelList1 = new List<BindCalendarViewModel>();
      if (model.Calendars != null && model.Calendars.Any<BindCalendarModel>())
      {
        foreach (BindCalendarModel calendar in model.Calendars)
        {
          List<BindCalendarViewModel> calendarViewModelList2 = calendarViewModelList1;
          BindCalendarViewModel calendarViewModel = new BindCalendarViewModel();
          calendarViewModel.Id = calendar.Id;
          calendarViewModel.Name = calendar.Name;
          calendarViewModel.Color = calendar.Color;
          ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
          ComboBoxViewModel comboBoxViewModel1 = new ComboBoxViewModel((object) "show", Utils.GetString("Show"), 32.0);
          comboBoxViewModel1.Selected = calendar.Show == "show";
          observableCollection.Add(comboBoxViewModel1);
          ComboBoxViewModel comboBoxViewModel2 = new ComboBoxViewModel((object) "calendar", Utils.GetString("ShowOnlyInCalendar"), 32.0);
          comboBoxViewModel2.Selected = calendar.Show == "calendar";
          observableCollection.Add(comboBoxViewModel2);
          ComboBoxViewModel comboBoxViewModel3 = new ComboBoxViewModel((object) "hidden", Utils.GetString("Hide"), 32.0);
          comboBoxViewModel3.Selected = calendar.Show == "hidden";
          observableCollection.Add(comboBoxViewModel3);
          calendarViewModel.StatusItems = observableCollection;
          calendarViewModelList2.Add(calendarViewModel);
        }
      }
      accountViewModel.Calendars = calendarViewModelList1;
      return accountViewModel;
    }

    public bool IsBindAccount()
    {
      return this.Kind == "caldav" || this.Kind == "api" || this.Kind == "exchange" || this.Kind == "icloud";
    }
  }
}
