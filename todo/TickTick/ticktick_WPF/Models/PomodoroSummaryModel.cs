// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.PomodoroSummaryModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Util;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class PomodoroSummaryModel : BaseModel
  {
    public string id { get; set; }

    [Indexed]
    public string taskId { get; set; }

    [JsonProperty("pomoCount")]
    public int count { get; set; }

    [JsonIgnore]
    public long duration { get; set; }

    [JsonProperty("pomoDuration")]
    public long PomoDuration { get; set; }

    public int estimatedPomo { get; set; }

    public string userId { get; set; }

    [JsonProperty("estimatedDuration")]
    public long EstimatedDuration { get; set; }

    [JsonProperty("stopwatchDuration")]
    public long StopwatchDuration { get; set; }

    [Ignore]
    public List<object[]> focuses { get; set; }

    [JsonIgnore]
    public string focusesString
    {
      get => JsonConvert.SerializeObject((object) this.focuses);
      set
      {
        try
        {
          this.focuses = JsonConvert.DeserializeObject<List<object[]>>(value) ?? new List<object[]>();
        }
        catch (Exception ex)
        {
          this.focuses = new List<object[]>();
        }
      }
    }

    public PomodoroSummaryModel Copy(string copyTaskSid, bool clear = false)
    {
      PomodoroSummaryModel pomodoroSummaryModel = (PomodoroSummaryModel) this.MemberwiseClone();
      pomodoroSummaryModel.id = Utils.GetGuid();
      pomodoroSummaryModel.taskId = copyTaskSid;
      if (clear)
      {
        pomodoroSummaryModel.count = 0;
        pomodoroSummaryModel.duration = 0L;
        pomodoroSummaryModel.StopwatchDuration = 0L;
        pomodoroSummaryModel.PomoDuration = 0L;
        pomodoroSummaryModel.focuses = (List<object[]>) null;
      }
      return pomodoroSummaryModel;
    }

    public bool IsEmpty()
    {
      if (this.count > 0 || this.PomoDuration + this.StopwatchDuration >= 30L || this.estimatedPomo > 0 || this.EstimatedDuration > 0L)
        return false;
      List<object[]> focuses = this.focuses;
      // ISSUE: explicit non-virtual call
      return (focuses != null ? (__nonvirtual (focuses.Count) > 0 ? 1 : 0) : 0) == 0;
    }

    internal void CheckNegative()
    {
      if (this.count < 0)
        this.count = 0;
      if (this.PomoDuration < 0L)
        this.PomoDuration = 0L;
      if (this.StopwatchDuration >= 0L)
        return;
      this.StopwatchDuration = 0L;
    }

    public void AddFocuses(object[] focus, bool checkExist = false)
    {
      if (this.focuses != null)
      {
        if (checkExist && this.focuses.Any<object[]>((Func<object[], bool>) (f => f[0] as string == focus[0] as string)))
          return;
        this.focuses.Add(focus);
      }
      else
        this.focuses = new List<object[]>() { focus };
    }

    public (int, long) GetFocusSummary()
    {
      int count = this.count;
      long num1 = this.PomoDuration + this.StopwatchDuration;
      List<object[]> focuses = this.focuses;
      // ISSUE: explicit non-virtual call
      if ((focuses != null ? (__nonvirtual (focuses.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        foreach (object[] objArray in this.focuses.Where<object[]>((Func<object[], bool>) (focus => focus.Length == 3)))
        {
          long? nullable = objArray[2] as long?;
          long valueOrDefault = nullable.GetValueOrDefault();
          num1 += valueOrDefault;
          nullable = objArray[1] as long?;
          long num2 = 1;
          if (!(nullable.GetValueOrDefault() == num2 & nullable.HasValue))
            count += valueOrDefault >= 300L ? 1 : 0;
        }
      }
      return (count, num1);
    }

    public (int, long, long) GetPomoFocusSummary()
    {
      int count = this.count;
      long pomoDuration = this.PomoDuration;
      long stopwatchDuration = this.StopwatchDuration;
      List<object[]> focuses = this.focuses;
      // ISSUE: explicit non-virtual call
      if ((focuses != null ? (__nonvirtual (focuses.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        foreach (object[] objArray in this.focuses.Where<object[]>((Func<object[], bool>) (focus => focus.Length == 3)))
        {
          long? nullable1 = objArray[2] as long?;
          long valueOrDefault = nullable1.GetValueOrDefault();
          long? nullable2 = objArray[1] as long?;
          nullable1 = nullable2;
          long num = 1;
          if (!(nullable1.GetValueOrDefault() == num & nullable1.HasValue))
            count += valueOrDefault >= 300L ? 1 : 0;
          if (nullable2.HasValue)
          {
            switch (nullable2.GetValueOrDefault())
            {
              case 0:
                pomoDuration += valueOrDefault;
                continue;
              case 1:
                stopwatchDuration += valueOrDefault;
                continue;
              default:
                continue;
            }
          }
        }
      }
      return (count, pomoDuration, stopwatchDuration);
    }
  }
}
