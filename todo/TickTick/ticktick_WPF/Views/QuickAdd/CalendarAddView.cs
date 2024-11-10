// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.CalendarAddView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class CalendarAddView : QuickAddView
  {
    private string _accountKind;

    public CalendarAddView(BindCalendarAccountModel calendar, bool focus = false)
      : base((IProjectTaskDefault) new BindAccountCalendarProjectIdentity(calendar), focus: focus)
    {
      this._accountKind = calendar?.Kind;
      this.MoreGrid.Visibility = Visibility.Collapsed;
    }

    public void SetAccountKind(string kind) => this._accountKind = kind;

    public event EventHandler<CalendarEventModel> CalendarEventAdded;

    public override void ShowSelectProjectPopup(bool enter = false)
    {
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) this.SetCalendarPopup, new ProjectExtra()
      {
        SubscribeCalendars = new List<string>()
        {
          this.Model.CalendarId
        }
      }, new ProjectSelectorExtra()
      {
        isCalendarMode = true,
        batchMode = false,
        accountId = this.Model.AccountId
      });
      projectOrGroupPopup.ItemSelect -= new EventHandler<SelectableItemViewModel>(this.CalendarItemSelect);
      projectOrGroupPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(this.CalendarItemSelect);
      projectOrGroupPopup.Show();
    }

    private void CalendarItemSelect(object sender, SelectableItemViewModel model)
    {
      this.SetCalendarPopup.IsOpen = false;
      this.CalendarSelect(model.Title, model.Id);
    }

    public override async Task DoAddTask(string title)
    {
      CalendarAddView sender = this;
      string str1 = sender._accountKind == "caldav" || sender._accountKind == "icloud" ? Guid.NewGuid().ToString() : Utils.GetGuid();
      string str2 = str1 + "@" + sender.Model.CalendarId;
      CalendarEventModel calEvent = new CalendarEventModel()
      {
        UserId = Utils.GetCurrentUserIdInt().ToString(),
        Id = str2,
        Uid = str1,
        Title = title,
        EventId = str1,
        CalendarId = sender.Model.CalendarId,
        Type = 0
      };
      DateTime? nullable1 = sender.Model.TimeData.StartDate;
      if (!nullable1.HasValue)
        sender.Model.TimeData.StartDate = new DateTime?(DateTime.Today);
      nullable1 = sender.Model.TimeData.DueDate;
      if (!nullable1.HasValue)
        sender.Model.TimeData.DueDate = new DateTime?(DateTime.Today);
      TimeData timeData1 = sender.Model.TimeData;
      int num1;
      if (timeData1 == null)
      {
        num1 = 0;
      }
      else
      {
        nullable1 = timeData1.StartDate;
        num1 = nullable1.HasValue ? 1 : 0;
      }
      if (num1 != 0)
      {
        TimeData timeData2 = sender.Model.TimeData;
        int num2;
        if (timeData2 == null)
        {
          num2 = 0;
        }
        else
        {
          nullable1 = timeData2.DueDate;
          num2 = nullable1.HasValue ? 1 : 0;
        }
        if (num2 != 0)
        {
          calEvent.IsAllDay = ((int) sender.Model.TimeData.IsAllDay ?? 1) != 0;
          TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
          if (calEvent.IsAllDay)
          {
            CalendarEventModel calendarEventModel1 = calEvent;
            TimeData timeData3 = sender.Model.TimeData;
            DateTime? nullable2;
            if (timeData3 == null)
            {
              nullable1 = new DateTime?();
              nullable2 = nullable1;
            }
            else
            {
              nullable1 = timeData3.StartDate;
              nullable2 = new DateTime?(nullable1.Value);
            }
            calendarEventModel1.DueStart = nullable2;
            CalendarEventModel calendarEventModel2 = calEvent;
            TimeData timeData4 = sender.Model.TimeData;
            DateTime? nullable3;
            if (timeData4 == null)
            {
              nullable1 = new DateTime?();
              nullable3 = nullable1;
            }
            else
            {
              nullable1 = timeData4.DueDate;
              nullable3 = new DateTime?(nullable1.Value);
            }
            calendarEventModel2.DueEnd = nullable3;
            nullable1 = calEvent.DueStart;
            DateTime dateTime1 = nullable1.Value;
            nullable1 = calEvent.DueEnd;
            DateTime dateTime2 = nullable1.Value;
            if (dateTime1 == dateTime2)
            {
              CalendarEventModel calendarEventModel3 = calEvent;
              nullable1 = calEvent.DueEnd;
              DateTime? nullable4 = new DateTime?(nullable1.Value.AddDays(1.0));
              calendarEventModel3.DueEnd = nullable4;
            }
            if (!string.IsNullOrEmpty(defaultSafely.AllDayReminders) && sender.Model.TimeData.IsDefault && sender._accountKind != "icloud")
              calEvent.Reminders = JsonConvert.SerializeObject((object) ((IEnumerable<string>) defaultSafely.AllDayReminders.Split(',')).Select<string, int>(new Func<string, int>(TriggerUtils.TriggerToReminder)));
          }
          else
          {
            CalendarEventModel calendarEventModel4 = calEvent;
            nullable1 = sender.Model.TimeData.StartDate;
            DateTime? nullable5 = new DateTime?(nullable1.Value);
            calendarEventModel4.DueStart = nullable5;
            CalendarEventModel calendarEventModel5 = calEvent;
            nullable1 = sender.Model.TimeData.DueDate;
            DateTime? nullable6 = new DateTime?(nullable1 ?? sender.Model.TimeData.StartDate.Value);
            calendarEventModel5.DueEnd = nullable6;
            if (!string.IsNullOrEmpty(defaultSafely.TimeReminders) && sender.Model.TimeData.IsDefault && sender._accountKind != "icloud")
              calEvent.Reminders = JsonConvert.SerializeObject((object) ((IEnumerable<string>) defaultSafely.TimeReminders.Split(',')).Select<string, int>(new Func<string, int>(TriggerUtils.TriggerToReminder)));
          }
          if (sender.Model.TimeData.Reminders.Any<TaskReminderModel>() && sender._accountKind != "icloud")
            calEvent.Reminders = JsonConvert.SerializeObject((object) sender.Model.TimeData.Reminders.Select<TaskReminderModel, string>((Func<TaskReminderModel, string>) (reminder => reminder.trigger)).ToList<string>().Select<string, int>(new Func<string, int>(TriggerUtils.TriggerToReminder)).ToList<int>());
          calEvent.RepeatFlag = sender.Model.TimeData.RepeatFlag;
        }
      }
      await CalendarService.AddEvent(calEvent);
      // ISSUE: reference to a compiler-generated method
      BindCalendarModel bindCalendarModel = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>(new Func<BindCalendarModel, bool>(sender.\u003CDoAddTask\u003Eb__8_0));
      if (bindCalendarModel != null && bindCalendarModel.Accessible)
      {
        EventHandler<CalendarEventModel> calendarEventAdded = sender.CalendarEventAdded;
        if (calendarEventAdded != null)
          calendarEventAdded((object) sender, calEvent);
      }
      sender.ResetAfterAddTask();
      sender.RefreshHint();
      DataChangedNotifier.NotifyCalendarChanged();
      SyncManager.TryDelaySync();
      calEvent = (CalendarEventModel) null;
    }
  }
}
