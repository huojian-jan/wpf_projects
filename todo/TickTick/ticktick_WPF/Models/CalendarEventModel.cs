// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CalendarEventModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class CalendarEventModel : BaseModel
  {
    [JsonProperty("id")]
    [Indexed]
    public string Id { get; set; }

    [JsonProperty("uid")]
    public string Uid { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }

    [JsonProperty("dueStart")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? DueStart { get; set; }

    [JsonProperty("dueEnd")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? DueEnd { get; set; }

    [JsonProperty("originalStartTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? OriginalStartTime { get; set; }

    [JsonProperty("isAllDay")]
    public bool IsAllDay { get; set; }

    [JsonProperty("repeatFlag")]
    public string RepeatFlag { get; set; }

    [JsonProperty("etag")]
    public string Etag { get; set; }

    [Ignore]
    [JsonProperty("conference")]
    public ConferenceModel Conference { get; set; }

    [JsonIgnore]
    public string ConferenceInfo
    {
      get => JsonConvert.SerializeObject((object) this.Conference);
      set
      {
        if (string.IsNullOrEmpty(value))
          return;
        this.Conference = JsonConvert.DeserializeObject<ConferenceModel>(value);
      }
    }

    [JsonIgnore]
    public string CalendarId { get; set; }

    [JsonIgnore]
    public int Type { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    [JsonIgnore]
    public string OriginalEventId { get; set; }

    [Ignore]
    [JsonProperty("attendees")]
    public List<CalendarAttendeeModel> Attendees { get; set; }

    [JsonIgnore]
    public string AttendeeInfo { get; set; }

    [JsonIgnore]
    public int Deleted { get; set; }

    [JsonProperty("timezone")]
    public string TimeZone { get; set; }

    [Ignore]
    [JsonProperty("reminders")]
    public List<int> ReminderList { get; set; }

    [JsonIgnore]
    public string Reminders { get; set; }

    [Ignore]
    [JsonIgnore]
    public bool Editable { get; set; }

    [Ignore]
    [JsonProperty("eXDates")]
    public List<DateTime> ExDateList { get; set; }

    [JsonIgnore]
    public string ExDates { get; set; }

    [JsonIgnore]
    [Indexed]
    public string EventId { get; set; } = string.Empty;

    [Ignore]
    [JsonIgnore]
    public int Status { get; set; }

    public static CalendarEventModel Copy(CalendarEventModel model)
    {
      return new CalendarEventModel()
      {
        Id = model.Id,
        Uid = model.Uid,
        Title = model.Title,
        Content = model.Content,
        Conference = model.Conference,
        Location = model.Location,
        DueStart = model.DueStart,
        DueEnd = model.DueEnd,
        IsAllDay = model.IsAllDay,
        RepeatFlag = model.RepeatFlag,
        Etag = model.Etag,
        CalendarId = model.CalendarId,
        UserId = model.UserId,
        Attendees = model.Attendees,
        AttendeeInfo = model.AttendeeInfo,
        Deleted = model.Deleted,
        TimeZone = model.TimeZone,
        ReminderList = model.ReminderList,
        Reminders = model.Reminders,
        ExDateList = model.ExDateList,
        ExDates = model.ExDates,
        EventId = model.EventId
      };
    }

    public bool IsSkipped(List<CalendarEventModel> skipEvents, DateTime? date = null)
    {
      date = date ?? this.DueStart;
      return skipEvents.Any<CalendarEventModel>((Func<CalendarEventModel, bool>) (cal =>
      {
        if (cal.Id != this.Id && cal.OriginalStartTime.HasValue && date.HasValue)
        {
          DateTime dateTime = cal.OriginalStartTime.Value;
          DateTime date1 = dateTime.Date;
          dateTime = date.Value;
          DateTime date2 = dateTime.Date;
          if (date1 == date2)
          {
            string eventId = cal.EventId;
            return (eventId != null ? (eventId.StartsWith(this.EventId + "_") ? 1 : 0) : 0) != 0 || cal.Uid == this.Uid;
          }
        }
        return false;
      }));
    }
  }
}
