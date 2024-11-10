// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarDetailViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarDetailViewModel : BaseViewModel
  {
    public const int AttendeeCount = 5;
    private bool _attendeeExpand;
    private string _calendarId;
    private string _calendarName;
    private string _content;
    private List<CalendarAttendeeModel> _displayAttendees;
    private DateTime? _dueDate;
    private bool _isAllDay;
    private string _location;
    private List<int> _reminders;
    private string _repeatFlag;
    private string _repeatText;
    private DateTime? _startDate;
    private string _title;
    public bool IsNew;
    public bool Organizer;

    public CalendarDetailViewModel(BindCalendarModel calendar)
    {
      this.Data = new CalendarEventModel();
      this.CalendarName = calendar.Name;
      this.CalendarId = calendar.Id;
      this.IsNew = true;
      this.Editable = true;
    }

    public CalendarDetailViewModel(CalendarEventModel model)
    {
      this.Data = model;
      this.Id = model.Id;
      this.Title = model.Title ?? string.Empty;
      this.Content = model.Content ?? string.Empty;
      this.StartDate = model.DueStart;
      this.DueDate = model.DueEnd;
      this.IsAllDay = model.IsAllDay;
      this.Location = model.Location;
      this.RepeatFlag = model.RepeatFlag;
      this.RepeatText = RRuleUtils.RRule2String(string.Empty, model.RepeatFlag, model.DueStart);
      this.Editable = model.Editable;
      this.Organizer = true;
      if (model.Attendees != null && model.Attendees.Any<CalendarAttendeeModel>())
      {
        this.ShowAttend = model.Attendees.Any<CalendarAttendeeModel>();
        this.AttendeeExpand = this.ShowAttend && model.Attendees.Count >= 5;
        this.ShowAttendeeExpand = this.AttendeeExpand;
        this.Attendees = model.Attendees.OrderByDescending<CalendarAttendeeModel, bool>((Func<CalendarAttendeeModel, bool>) (attendee => attendee.Organizer)).ToList<CalendarAttendeeModel>();
        this.DisplayAttendees = this.Attendees.Take<CalendarAttendeeModel>(5).ToList<CalendarAttendeeModel>();
        CalendarAttendeeModel calendarAttendeeModel = model.Attendees.FirstOrDefault<CalendarAttendeeModel>((Func<CalendarAttendeeModel, bool>) (a => a.Self));
        if (calendarAttendeeModel != null)
          this.Organizer = calendarAttendeeModel.Organizer;
      }
      if (this.ShowAttend)
      {
        List<CalendarAttendeeModel> attendees = this.Attendees;
        this.Attendees = attendees != null ? attendees.OrderByDescending<CalendarAttendeeModel, bool>((Func<CalendarAttendeeModel, bool>) (attendee => attendee.Organizer)).ToList<CalendarAttendeeModel>() : (List<CalendarAttendeeModel>) null;
      }
      List<CalendarAttendeeModel> attendees1 = this.Attendees;
      CalendarAttendeeModel calendarAttendeeModel1 = attendees1 != null ? attendees1.FirstOrDefault<CalendarAttendeeModel>((Func<CalendarAttendeeModel, bool>) (attendee => !attendee.Organizer)) : (CalendarAttendeeModel) null;
      if (calendarAttendeeModel1 != null)
        calendarAttendeeModel1.FirstGuest = true;
      this.Reminders = model.ReminderList ?? new List<int>();
      if (model.Type == 0)
      {
        BindCalendarModel bindCalendarModel = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == model.CalendarId));
        if (bindCalendarModel != null)
        {
          this.CalendarName = bindCalendarModel.Name;
          this.CalendarId = bindCalendarModel.Id;
        }
      }
      else
      {
        CalendarSubscribeProfileModel subscribeProfileModel = CacheManager.GetSubscribeCalendars().FirstOrDefault<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => cal.Id == model.CalendarId));
        if (subscribeProfileModel != null)
        {
          this.CalendarName = subscribeProfileModel.Name;
          this.CalendarId = subscribeProfileModel.Id;
        }
      }
      if (model.Conference == null)
        return;
      this.IsShowConference = true;
      if (model.Conference.EntryPoints == null || model.Conference.EntryPoints.Count == 1 && model.Conference.EntryPoints.First<EntryPointsModel>().EntryPointType == "more")
      {
        this.IsShowConference = false;
      }
      else
      {
        this.ConferenceName = model.Conference.Name;
        EntryPointsModel entryPointsModel = ((model.Conference.EntryPoints.FirstOrDefault<EntryPointsModel>((Func<EntryPointsModel, bool>) (entity => entity.EntryPointType == "video")) ?? model.Conference.EntryPoints.FirstOrDefault<EntryPointsModel>((Func<EntryPointsModel, bool>) (entity => entity.EntryPointType == "phone"))) ?? model.Conference.EntryPoints.FirstOrDefault<EntryPointsModel>((Func<EntryPointsModel, bool>) (entity => entity.EntryPointType == "more"))) ?? model.Conference.EntryPoints.FirstOrDefault<EntryPointsModel>();
        this.ConferenceType = entryPointsModel.EntryPointType;
        this.ConferenceUri = entryPointsModel.Uri;
      }
    }

    public bool Editable { get; set; }

    public bool ShowAttendeeExpand { get; set; }

    public CalendarEventModel Data { get; }

    public string Id { get; set; }

    public bool IsShowConference { get; set; }

    public string ConferenceName { get; set; }

    public string ConferenceType { get; set; }

    public string ConferenceUri { get; set; }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.Data.Title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public string Content
    {
      get => this._content;
      set
      {
        this._content = value;
        this.Data.Content = value;
        this.OnPropertyChanged(nameof (Content));
      }
    }

    public bool IsAllDay
    {
      get => this._isAllDay;
      set
      {
        this._isAllDay = value;
        this.Data.IsAllDay = value;
        this.OnPropertyChanged(nameof (IsAllDay));
      }
    }

    public DateTime? StartDate
    {
      get => this._startDate;
      set
      {
        this._startDate = value;
        this.Data.DueStart = value;
        this.OnPropertyChanged(nameof (StartDate));
      }
    }

    public DateTime? DueDate
    {
      get => this._dueDate;
      set
      {
        this._dueDate = value;
        this.Data.DueEnd = value;
        this.OnPropertyChanged(nameof (DueDate));
      }
    }

    public string RepeatFlag
    {
      get => this._repeatFlag;
      set
      {
        this._repeatFlag = value;
        this.Data.RepeatFlag = value;
        this.OnPropertyChanged(nameof (RepeatFlag));
      }
    }

    public string RepeatText
    {
      get => this._repeatText;
      set
      {
        this._repeatText = value;
        this.OnPropertyChanged(nameof (RepeatText));
      }
    }

    public string CalendarId
    {
      get => this._calendarId;
      set
      {
        this._calendarId = value;
        this.Data.CalendarId = value;
        this.OnPropertyChanged(nameof (CalendarId));
      }
    }

    public string CalendarName
    {
      get => this._calendarName;
      set
      {
        this._calendarName = value;
        this.OnPropertyChanged(nameof (CalendarName));
      }
    }

    public string Location
    {
      get => this._location;
      set
      {
        this._location = value;
        this.OnPropertyChanged(nameof (Location));
      }
    }

    public int Mode { get; set; }

    public List<CalendarAttendeeModel> Attendees { get; set; }

    public List<CalendarAttendeeModel> DisplayAttendees
    {
      get => this._displayAttendees;
      set
      {
        this._displayAttendees = value;
        this.OnPropertyChanged(nameof (DisplayAttendees));
      }
    }

    public bool ShowAttend { get; set; }

    public List<int> Reminders
    {
      get => this._reminders;
      set
      {
        this._reminders = value;
        this.Data.ReminderList = value;
        this.Data.Reminders = JsonConvert.SerializeObject((object) value);
        this.OnPropertyChanged(nameof (Reminders));
      }
    }

    public bool AttendeeExpand
    {
      get => this._attendeeExpand;
      set
      {
        this._attendeeExpand = value;
        this.OnPropertyChanged(nameof (AttendeeExpand));
      }
    }

    public async Task<string> GetSnoozeText()
    {
      List<int> reminders = this.Reminders;
      // ISSUE: explicit non-virtual call
      if ((reminders != null ? (__nonvirtual (reminders.Count) > 0 ? 1 : 0) : 0) != 0 && this.StartDate.HasValue)
      {
        string id = this.Id;
        if (!string.IsNullOrEmpty(id) && id.Contains("@"))
        {
          int length = id.IndexOf("@", StringComparison.Ordinal);
          id = id.Substring(0, length);
        }
        List<ReminderDelayModel> delayModelById = await ReminderDelayDao.GetDelayModelById(id, "calendar");
        // ISSUE: explicit non-virtual call
        if (delayModelById != null && __nonvirtual (delayModelById.Count) > 0)
        {
          ReminderDelayModel reminderDelayModel = delayModelById[0];
          if (reminderDelayModel != null && reminderDelayModel.NextTime.HasValue && reminderDelayModel.NextTime.Value > DateTime.Now)
          {
            bool flag = false;
            foreach (double reminder in this.Reminders)
            {
              DateTime dateTime = this.StartDate.Value.AddMinutes(reminder);
              DateTime? remindTime = reminderDelayModel.RemindTime;
              if ((remindTime.HasValue ? (dateTime == remindTime.GetValueOrDefault() ? 1 : 0) : 0) != 0)
              {
                flag = true;
                break;
              }
            }
            if (flag)
            {
              DateTime date = reminderDelayModel.NextTime.Value;
              if (!(date > DateTime.Now))
                return string.Empty;
              DateTime now = DateTime.Now;
              if (Math.Abs((now.Date - date.Date).TotalDays - -1.0) <= 0.001)
                return string.Format(Utils.GetString("PreviewSnoozeText"), (object) Utils.GetString("Tomorrow"));
              now = DateTime.Now;
              return Math.Abs((now.Date - date.Date).TotalDays) > 0.001 ? string.Format(Utils.GetString("PreviewSnoozeText"), (object) (DateUtils.FormatFullDate(date) + " " + DateUtils.FormatHourMinute(date))) : string.Format(Utils.GetString("PreviewSnoozeText"), (object) DateUtils.FormatHourMinute(date));
            }
          }
        }
      }
      return (string) null;
    }
  }
}
