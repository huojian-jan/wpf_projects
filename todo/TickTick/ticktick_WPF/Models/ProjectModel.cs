// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ProjectModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Twitter;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class ProjectModel : BaseModel, IDroppable, ITimeline
  {
    private string _color;
    private string _notificationString;
    private string _sync_status;

    [Indexed]
    public string id { get; set; }

    public string userid { get; set; }

    public string name { get; set; } = string.Empty;

    public bool Isinbox { get; set; }

    public bool Ishide { get; set; }

    public bool muted { get; set; } = true;

    public string permission { get; set; }

    public string sortType { get; set; }

    [JsonProperty("sortOption")]
    [Ignore]
    public SortOption SortOption { get; set; }

    [JsonIgnore]
    public string SortOptionString
    {
      get
      {
        return this.SortOption != null ? JsonConvert.SerializeObject((object) this.SortOption) : string.Empty;
      }
      set
      {
        this.SortOption = string.IsNullOrEmpty(value) ? (SortOption) null : JsonConvert.DeserializeObject<SortOption>(value);
      }
    }

    [JsonProperty("timeline")]
    [Ignore]
    public TimelineSyncModel SyncTimeline { get; set; }

    [JsonIgnore]
    public string SyncTimelineModel
    {
      get
      {
        return this.SyncTimeline != null ? JsonConvert.SerializeObject((object) this.SyncTimeline) : string.Empty;
      }
      set
      {
        this.SyncTimeline = string.IsNullOrEmpty(value) ? new TimelineSyncModel() : JsonConvert.DeserializeObject<TimelineSyncModel>(value);
      }
    }

    [Ignore]
    [JsonIgnore]
    public ticktick_WPF.Models.TimelineModel Timeline { get; set; }

    [JsonIgnore]
    public string TimelineModel
    {
      get
      {
        return this.Timeline != null ? JsonConvert.SerializeObject((object) this.Timeline) : string.Empty;
      }
      set
      {
        this.Timeline = string.IsNullOrEmpty(value) ? new ticktick_WPF.Models.TimelineModel() : JsonConvert.DeserializeObject<ticktick_WPF.Models.TimelineModel>(value);
      }
    }

    public string color
    {
      get => this._color;
      set => this._color = value;
    }

    [JsonIgnore]
    public string NotificationString
    {
      get => this._notificationString;
      set => this._notificationString = value;
    }

    [Ignore]
    public string[] notificationOptions
    {
      get => this._notificationString?.Split(';');
      set
      {
        if (value == null)
          return;
        this._notificationString = value.Length == 0 ? string.Empty : (value.Length > 1 ? ((IEnumerable<string>) value).Join<string>(";") : value[0]);
      }
    }

    [Indexed]
    public string sync_status
    {
      get => this._sync_status;
      set
      {
        Constants.SyncStatus syncStatus1;
        if (this.IsNew())
        {
          string str1 = value;
          syncStatus1 = Constants.SyncStatus.SYNC_UPDATE;
          string str2 = syncStatus1.ToString();
          if (str1 == str2)
            return;
        }
        string syncStatus2 = this._sync_status;
        syncStatus1 = Constants.SyncStatus.SYNC_DONE;
        string str3 = syncStatus1.ToString();
        if (syncStatus2 != str3)
        {
          string str4 = value;
          syncStatus1 = Constants.SyncStatus.SYNC_INIT;
          string str5 = syncStatus1.ToString();
          if (str4 == str5)
            return;
        }
        this._sync_status = value;
      }
    }

    [Indexed]
    public bool delete_status { get; set; }

    public bool isOwner { get; set; } = true;

    public bool inAll { get; set; }

    public bool? openToTeam { get; set; } = new bool?(false);

    public bool? needAudit { get; set; } = new bool?(true);

    public string teamMemberPermission { get; set; } = "write";

    public long sortOrder { get; set; }

    public int userCount { get; set; } = 1;

    public int source { get; set; } = 1;

    public string etag { get; set; }

    [Ignore]
    private DateTime? ModifiedTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? modifiedTime
    {
      get
      {
        return !Utils.IsEmptyDate(this.ModifiedTime) ? this.ModifiedTime : new DateTime?(DateTime.Now);
      }
      set => this.ModifiedTime = value;
    }

    public bool? closed { get; set; }

    public string groupId { get; set; }

    public string viewMode { get; set; }

    public string teamId { get; set; }

    public string kind { get; set; }

    [JsonIgnore]
    [DefaultValue("1")]
    public bool ShowAddColumn { get; set; } = true;

    [Ignore]
    public bool CanDrop => this.IsEnable();

    [Ignore]
    [JsonIgnore]
    public string ProjectId => this.id;

    [Ignore]
    [JsonIgnore]
    public DateTime? DefaultDate => new DateTime?();

    [Ignore]
    [JsonIgnore]
    public bool IsCompleted => false;

    [Ignore]
    [JsonIgnore]
    public bool IsAbandoned => false;

    [Ignore]
    [JsonIgnore]
    public bool IsDeleted => false;

    [Ignore]
    [JsonIgnore]
    public List<string> Tags => new List<string>();

    [Ignore]
    [JsonIgnore]
    public int Priority => 0;

    [Ignore]
    [JsonIgnore]
    public bool Multiple => false;

    [JsonIgnore]
    public bool IsNote => this.kind == Constants.ProjectKind.NOTE.ToString();

    [JsonIgnore]
    [Ignore]
    public bool OpenToTeam
    {
      get => !string.IsNullOrEmpty(this.teamId) && this.openToTeam.GetValueOrDefault();
    }

    public void SetSyncStatus(string status) => this._sync_status = status;

    public bool IsProjectPermit()
    {
      if (this.permission == null || this.permission != "read" && this.permission != "comment")
        return true;
      return this.openToTeam.GetValueOrDefault() && this.teamMemberPermission != "read" && this.teamMemberPermission != "comment" && CacheManager.GetTeamById(this.teamId) != null;
    }

    public bool IsEnable() => this.IsValid() && this.IsProjectPermit();

    public bool IsValid() => !this.closed.GetValueOrDefault() && !this.IsDeleted;

    public bool IsBelongGroup(string group)
    {
      return string.IsNullOrEmpty(group) || group == "NONE" ? string.IsNullOrEmpty(this.groupId) || this.groupId == "NONE" : this.groupId != null && this.groupId.Equals(group);
    }

    public bool IsEquals(ProjectModel project)
    {
      return project.name == this.name && project.sortType == this.sortType && project.IsShareList() == this.IsShareList();
    }

    public int CompareTo(ProjectModel projectB)
    {
      if (projectB == null || this.Isinbox)
        return -1;
      if (projectB.Isinbox)
        return 1;
      long? sortOrder = (long?) CacheManager.GetGroupById(this.groupId)?.sortOrder;
      long num1 = sortOrder ?? this.sortOrder;
      sortOrder = (long?) CacheManager.GetGroupById(projectB.groupId)?.sortOrder;
      long num2 = sortOrder ?? projectB.sortOrder;
      return num1 != num2 ? num1.CompareTo(num2) : this.sortOrder.CompareTo(projectB.sortOrder);
    }

    public bool InGroup() => !string.IsNullOrEmpty(this.groupId) && this.groupId != "NONE";

    public bool IsOverLimit()
    {
      return this.sync_status == Constants.SyncStatus.SYNC_ERROR_UP_LIMIT.ToString();
    }

    public bool IsNew()
    {
      return this._sync_status == Constants.SyncStatus.SYNC_NEW.ToString() || this._sync_status == Constants.SyncStatus.SYNC_ERROR_UP_LIMIT.ToString();
    }

    public SortOption GetSortOption()
    {
      if (this.SortOption == null)
      {
        if (string.IsNullOrEmpty(this.sortType))
          this.sortType = "sortOrder";
        return SortOptionUtils.GetSortOptionBySortType(Utils.LoadSortType(this.sortType), this.viewMode == "kanban");
      }
      return new SortOption()
      {
        groupBy = this.SortOption.groupBy,
        orderBy = this.SortOption.orderBy
      };
    }

    public bool IsShareList()
    {
      if (this.userCount > 1)
        return true;
      return this.openToTeam.GetValueOrDefault() && !string.IsNullOrEmpty(this.teamId);
    }
  }
}
