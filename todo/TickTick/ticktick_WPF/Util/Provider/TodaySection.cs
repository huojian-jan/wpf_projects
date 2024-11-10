// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TodaySection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TodaySection : Section
  {
    public TodaySection()
      : base()
    {
      this.Name = Utils.GetString("Today");
      this.Ordinal = 2L;
      this.SectionDate = new DateTime?(DateTime.Today);
      this.SectionEntityId = DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) DateTimeFormatInfo.InvariantInfo);
      this.SectionId = "today";
    }

    public override bool CanSwitch(DisplayType displayType) => displayType != DisplayType.Habit;

    public override bool CanSort(string sortBy) => true;
  }
}
