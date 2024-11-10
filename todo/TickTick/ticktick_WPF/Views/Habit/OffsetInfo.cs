// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.OffsetInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class OffsetInfo
  {
    public OffsetInfo(double offset, double unitHeight)
    {
      this.Title = offset.ToString("F0");
      this.UnitHeight = unitHeight;
    }

    public string Title { get; set; }

    public double UnitHeight { get; set; }
  }
}
