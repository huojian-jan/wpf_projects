// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.AlternativeCalendar
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class AlternativeCalendar : PreferenceBaseModel
  {
    public string calendar { get; set; }

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      if (model is AlternativeCalendar alternativeCalendar)
      {
        if (alternativeCalendar.mtime > this.mtime)
        {
          this.calendar = alternativeCalendar.calendar;
          this.mtime = alternativeCalendar.mtime;
        }
        else if (alternativeCalendar.mtime < this.mtime)
          return true;
      }
      return false;
    }
  }
}
