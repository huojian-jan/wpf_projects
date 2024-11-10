// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.HabitModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.Views.Habit;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class HabitModel : BaseModel
  {
    [JsonProperty(PropertyName = "id")]
    [Indexed]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    [JsonProperty(PropertyName = "iconRes")]
    public string IconRes { get; set; }

    [JsonProperty(PropertyName = "color")]
    public string Color { get; set; }

    [JsonProperty(PropertyName = "status")]
    [Indexed]
    public int Status { get; set; }

    [JsonProperty(PropertyName = "totalCheckIns")]
    public int TotalCheckIns { get; set; }

    [JsonProperty(PropertyName = "createdTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime CreatedTime { get; set; }

    [JsonProperty(PropertyName = "modifiedTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime ModifiedTime { get; set; }

    [JsonProperty(PropertyName = "archivedTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? ArchivedTime { get; set; }

    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }

    [JsonProperty(PropertyName = "goal")]
    public double Goal { get; set; }

    [JsonProperty(PropertyName = "step")]
    public double Step { get; set; }

    [JsonProperty(PropertyName = "unit")]
    public string Unit { get; set; }

    [JsonProperty(PropertyName = "etag")]
    public string Etag { get; set; }

    [JsonProperty(PropertyName = "sortOrder")]
    public long SortOrder { get; set; }

    [JsonProperty(PropertyName = "repeatRule")]
    public string RepeatRule { get; set; }

    [JsonProperty(PropertyName = "encouragement")]
    public string Encouragement { get; set; }

    [Ignore]
    [JsonProperty(PropertyName = "reminders")]
    public string[] Reminders
    {
      get
      {
        if (string.IsNullOrEmpty(this.Reminder))
          return new string[0];
        return this.Reminder.Split(',');
      }
      set
      {
        if (value != null && ((IEnumerable<string>) value).Any<string>())
          this.Reminder = ((IEnumerable<string>) value).Join<string>(",");
        else
          this.Reminder = string.Empty;
      }
    }

    [Ignore]
    [JsonProperty(PropertyName = "exDates")]
    public string[] ExDates
    {
      get
      {
        return !string.IsNullOrEmpty(this.ExDateString) ? JsonConvert.DeserializeObject<string[]>(this.ExDateString) : new string[0];
      }
      set
      {
        if (value == null || !((IEnumerable<string>) value).Any<string>())
          return;
        this.ExDateString = JsonConvert.SerializeObject((object) value);
      }
    }

    [JsonIgnore]
    public string ExDateString { get; set; }

    [JsonProperty(PropertyName = "recordEnable")]
    public bool? RecordEnable { get; set; } = new bool?(false);

    [JsonProperty(PropertyName = "sectionId")]
    public string SectionId { get; set; }

    [JsonProperty(PropertyName = "targetDays")]
    public int? TargetDays { get; set; }

    [JsonProperty(PropertyName = "targetStartDate")]
    public int? TargetStartDate { get; set; }

    public DateTime GetStartDate()
    {
      DateTime result;
      return this.TargetStartDate.HasValue && DateTime.TryParseExact(this.TargetStartDate.ToString(), "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ? result : this.CreatedTime;
    }

    [JsonProperty(PropertyName = "completedCycles")]
    public int? CompletedCycles { get; set; }

    [JsonIgnore]
    [Ignore]
    public List<CompletedCycle> CompletedCyclesList { get; set; }

    [JsonIgnore]
    public string CompletedCyclesListStr
    {
      get => JsonConvert.SerializeObject((object) this.CompletedCyclesList);
      set
      {
        if (value == null)
          return;
        this.CompletedCyclesList = JsonConvert.DeserializeObject<List<CompletedCycle>>(value);
      }
    }

    [JsonIgnore]
    public string Reminder { get; set; }

    [JsonIgnore]
    public int CheckStamp { get; set; }

    [JsonIgnore]
    [NotNull]
    [DefaultValue("2")]
    public int SyncStatus { get; set; }

    public bool IsBoolHabit() => this.Type?.ToLower() != HabitType.Real.ToString().ToLower();

    public static HabitModel GetDefaultHabit()
    {
      return new HabitModel()
      {
        Id = Utils.GetGuid(),
        Name = "",
        IconRes = "habit_daily_check_in",
        Color = HabitUtils.IconColorDict["habit_daily_check_in"],
        Type = HabitType.Boolean.ToString(),
        Goal = 1.0,
        Step = 1.0,
        Unit = Utils.GetString("Count"),
        RepeatRule = "RRULE:FREQ=DAILY;INTERVAL=1",
        RecordEnable = new bool?(false),
        CreatedTime = DateTime.Now
      };
    }

    public bool IsSkipToday()
    {
      return this.ExDates != null && ((IEnumerable<string>) this.ExDates).Contains<string>(DateTime.Today.ToString("yyyyMMdd"));
    }

    public void MergeSkip(List<string> localExDates)
    {
    }
  }
}
