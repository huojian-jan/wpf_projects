// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CalendarViewConf
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class CalendarViewConf
  {
    public string cellColorType;
    public bool showChecklist;
    public bool showCompleted = true;
    public bool showFutureTask;
    public bool showFocusRecord;
    public bool? showHabit = new bool?(false);
  }
}
