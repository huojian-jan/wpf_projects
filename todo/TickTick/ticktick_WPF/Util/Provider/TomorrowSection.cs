// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TomorrowSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TomorrowSection : Section
  {
    public TomorrowSection()
      : base()
    {
      this.Name = Utils.GetString("Tomorrow");
      this.Ordinal = 2L;
      DateTime dateTime1 = DateTime.Now;
      dateTime1 = dateTime1.AddDays(1.0);
      this.SectionDate = new DateTime?(dateTime1.Date);
      this.SectionId = "tomorrow";
      DateTime dateTime2 = DateTime.Today;
      dateTime2 = dateTime2.AddDays(1.0);
      this.SectionEntityId = dateTime2.ToString("yyyyMMdd", (IFormatProvider) DateTimeFormatInfo.InvariantInfo);
    }

    public override bool CanSwitch(DisplayType displayType) => displayType != DisplayType.Habit;

    public override bool CanSort(string sortBy) => true;
  }
}
