// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TimeTableModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using ticktick_WPF.Notifier;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TimeTableModel : PreferenceBaseModel
  {
    public string currentTimetableId { get; set; }

    public bool displayInCalendar { get; set; }

    public bool isEnabled { get; set; }

    public bool displayInSmartProjects { get; set; }

    [JsonIgnore]
    public bool ShowInCal => this.isEnabled && this.displayInCalendar;

    [JsonIgnore]
    public bool ShowInSmart => this.isEnabled && this.displayInSmartProjects;

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      if (model is TimeTableModel timeTableModel)
      {
        if (timeTableModel.mtime > this.mtime)
        {
          this.currentTimetableId = timeTableModel.currentTimetableId;
          this.displayInCalendar = timeTableModel.displayInCalendar;
          this.displayInSmartProjects = timeTableModel.displayInSmartProjects;
          this.isEnabled = timeTableModel.isEnabled;
          this.mtime = timeTableModel.mtime;
          DataChangedNotifier.OnScheduleChanged();
          UtilLog.Info("TimeTableModel.SetRemoteValue");
        }
        else if (timeTableModel.mtime < this.mtime)
          return true;
      }
      return false;
    }
  }
}
