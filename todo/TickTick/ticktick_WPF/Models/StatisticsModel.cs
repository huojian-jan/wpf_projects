// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.StatisticsModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class StatisticsModel
  {
    public int todayPomoCount { get; set; }

    public int totalPomoCount { get; set; }

    public long todayPomoDuration { get; set; }

    public long totalPomoDuration { get; set; }

    public int Date { get; set; }

    public void Add(StatisticsModel extra)
    {
      if (extra == null)
        return;
      this.todayPomoCount += extra.todayPomoCount;
      this.totalPomoCount += extra.totalPomoCount;
      this.todayPomoDuration += extra.todayPomoDuration;
      this.totalPomoDuration += extra.totalPomoDuration;
    }

    public void ClearToday()
    {
      this.todayPomoCount = 0;
      this.todayPomoDuration = 0L;
    }
  }
}
