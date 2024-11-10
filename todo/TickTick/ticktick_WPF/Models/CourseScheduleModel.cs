// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CourseScheduleModel
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
  public class CourseScheduleModel : BaseModel
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("copyOriginId")]
    public string CopyOriginId { get; set; }

    [Ignore]
    [JsonProperty("courses")]
    public List<CourseModel> Courses { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    [JsonProperty("createdTime")]
    public DateTime CreatedTime { get; set; }

    [JsonProperty("etag")]
    public string Etag { get; set; }

    [JsonProperty("lessonTimes")]
    [Ignore]
    public Dictionary<int, string[]> LessonTimes { get; set; }

    [JsonIgnore]
    public string LessonTimesStr { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("schoolId")]
    public string SchoolId { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    [JsonProperty("startDate")]
    public DateTime StartDate { get; set; }

    [JsonProperty("sortOrder")]
    public long SortOrder { get; set; }

    [JsonProperty("weekCount")]
    public int WeekCount { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    [JsonIgnore]
    public int SyncStatus { get; set; }

    [JsonProperty("reminders")]
    [Ignore]
    public string[] Reminders { get; set; }

    [JsonIgnore]
    public string RemindersStr { get; set; }

    public bool AllTimeSet()
    {
      try
      {
        Dictionary<int, string[]> source = JsonConvert.DeserializeObject<Dictionary<int, string[]>>(this.LessonTimesStr);
        // ISSUE: explicit non-virtual call
        if (source != null && __nonvirtual (source.Count) > 0)
        {
          int num = source.Keys.Max();
          if (source.Count < num)
            return false;
        }
        return source != null && source.All<KeyValuePair<int, string[]>>((Func<KeyValuePair<int, string[]>, bool>) (kv => kv.Value != null && kv.Value.Length == 2));
      }
      catch (Exception ex)
      {
        return false;
      }
    }
  }
}
