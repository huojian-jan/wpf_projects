// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TimelineSettingsModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using ticktick_WPF.Notifier;
using ticktick_WPF.Views.Timeline;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TimelineSettingsModel : PreferenceBaseModel
  {
    [JsonProperty("color")]
    public string Color { get; set; } = TimelineColorType.List.GetName();

    [JsonProperty("showWeek")]
    public bool ShowWeek { get; set; }

    [JsonProperty("showGuide")]
    public bool ShowGuide { get; set; } = true;

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      if (model is TimelineSettingsModel timelineSettingsModel)
      {
        if (timelineSettingsModel.mtime > this.mtime)
        {
          this.Color = timelineSettingsModel.Color;
          this.ShowWeek = timelineSettingsModel.ShowWeek;
          this.ShowGuide = timelineSettingsModel.ShowGuide;
          this.mtime = timelineSettingsModel.mtime;
          DataChangedNotifier.NotifyTimelineSettingsChanged();
          UtilLog.Info("TimelineSettingsModel.SetRemoteValue");
        }
        else if (timelineSettingsModel.mtime < this.mtime)
          return true;
      }
      return false;
    }
  }
}
