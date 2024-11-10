// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.UserPreferenceModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Models
{
  public class UserPreferenceModel
  {
    public MatrixModel matrix { get; set; }

    [JsonProperty("timeline")]
    public TimelineSettingsModel Timeline { get; set; }

    public DesktopConfigModel desktop_conf { get; set; }

    public DesktopTabBar desktopTabBars { get; set; }

    [JsonProperty("timetable")]
    public TimeTableModel TimeTable { get; set; }

    [JsonProperty("smartProjectsOption")]
    public SmartProjectsOption SmartProjectsOption { get; set; }

    [JsonProperty("general_conf")]
    public GeneralConfig GeneralConfig { get; set; }

    [JsonProperty("teamConf")]
    public TeamConfig TeamConfig { get; set; }

    [JsonProperty("focusConf")]
    public FocusConfig FocusConfig { get; set; }

    [JsonProperty("recentlyColors")]
    public RecentlyColors RecentlyColors { get; set; }

    [JsonProperty("summaryTemplates")]
    public SummaryTemplates summaryTemplates { get; set; }

    [JsonProperty("alternativeCalendar")]
    public AlternativeCalendar alternativeCalendar { get; set; }

    public long mtime { get; set; }

    public long GetMaxMTime()
    {
      List<long> source = new List<long>();
      source.Add(this.mtime);
      source.Add(this.matrix.mtime);
      source.Add(this.Timeline.mtime);
      source.Add(this.desktop_conf.mtime);
      TimeTableModel timeTable = this.TimeTable;
      source.Add(timeTable != null ? timeTable.mtime : 0L);
      GeneralConfig generalConfig = this.GeneralConfig;
      source.Add(generalConfig != null ? generalConfig.mtime : 0L);
      DesktopTabBar desktopTabBars = this.desktopTabBars;
      source.Add(desktopTabBars != null ? desktopTabBars.mtime : 0L);
      SummaryTemplates summaryTemplates = this.summaryTemplates;
      source.Add(summaryTemplates != null ? summaryTemplates.mtime : 0L);
      return source.Max();
    }
  }
}
