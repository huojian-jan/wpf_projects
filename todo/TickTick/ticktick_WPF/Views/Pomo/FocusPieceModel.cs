// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusPieceModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Util;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusPieceModel
  {
    public string Id { get; set; }

    public int Type { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime BeginTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? EndTime { get; set; }

    public long GetDuration()
    {
      if (!Utils.IsEmptyDate(this.EndTime))
      {
        DateTime? endTime = this.EndTime;
        if (endTime.HasValue)
        {
          DateTime beginTime = this.BeginTime;
          endTime = this.EndTime;
          DateTime end = endTime.Value;
          return Utils.GetTotalSecond(beginTime, end);
        }
      }
      return 0;
    }

    public async Task<string> GetTaskId()
    {
      if (this.Type == 0)
        return this.Id;
      if (this.Type == 2)
      {
        TimerModel timerById = await TimerDao.GetTimerById(this.Id);
        if (timerById != null && timerById.ObjType == "task")
          return (await TaskDao.GetThinTaskById(timerById.ObjId))?.id;
      }
      return string.Empty;
    }
  }
}
