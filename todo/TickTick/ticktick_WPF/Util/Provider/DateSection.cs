// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.DateSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class DateSection
  {
    public Section Later;
    public Section Nodate;
    public Section Outdated;
    public Section Today;
    public Section Tomorrow;
    public Section Week;

    public DateSection()
    {
      string name1 = Utils.GetString("later");
      DateTime dateTime1 = DateTime.Now;
      dateTime1 = dateTime1.AddDays(8.0);
      DateTime? sectionDate1 = new DateTime?(dateTime1.Date);
      this.Later = (Section) new LaterSection(name1, 1, sectionDate1, "later");
      this.Nodate = (Section) new NodateSection(Utils.GetString("NoDate"), 0, new DateTime?(), "nodate");
      OutdatedSection outdatedSection = new OutdatedSection();
      outdatedSection.Ordinal = 8L;
      this.Outdated = (Section) outdatedSection;
      TodaySection todaySection = new TodaySection();
      todaySection.Ordinal = 4L;
      this.Today = (Section) todaySection;
      TomorrowSection tomorrowSection = new TomorrowSection();
      tomorrowSection.Ordinal = 3L;
      this.Tomorrow = (Section) tomorrowSection;
      string name2 = Utils.GetString("Next7Day");
      DateTime dateTime2 = DateTime.Now;
      dateTime2 = dateTime2.AddDays(2.0);
      DateTime? sectionDate2 = new DateTime?(dateTime2.Date);
      this.Week = (Section) new WeekSection(name2, 2, sectionDate2, "week");
      // ISSUE: explicit constructor call
      base.\u002Ector();
    }
  }
}
