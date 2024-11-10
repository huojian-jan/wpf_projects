// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.SyncBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ticktick_WPF.Models;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class SyncBean
  {
    public long checkPoint { get; set; }

    public SyncTaskBean syncTaskBean { get; set; }

    public ObservableCollection<ProjectModel> projectProfiles { get; set; }

    public ObservableCollection<ProjectGroupModel> projectGroups { get; set; }

    public ObservableCollection<FilterModel> filters { get; set; }

    public List<TagModel> tags { get; set; }

    public string inboxId { get; set; }

    public SyncTaskOrderBean syncTaskOrderBean { get; set; }

    public SyncOrderBean syncOrderBean { get; set; }

    public SyncOrderBean syncOrderBeanV3 { get; set; }

    public List<ReminderDelayModel> remindChange { get; set; }

    [JsonIgnore]
    public string checks { get; set; }

    public bool SyncError { get; set; }

    public bool IsEmpty()
    {
      return this.filters == null && this.projectGroups == null && this.projectProfiles == null && this.syncTaskBean == null && this.tags == null && (this.syncTaskBean == null || this.syncTaskBean.Empty) && this.checks == null;
    }
  }
}
