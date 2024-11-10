// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SyncSortOrderModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class SyncSortOrderModel : BaseModel
  {
    private string _userId;
    private string _groupId = "all";

    public SyncSortOrderModel() => this.UserId = LocalSettings.Settings.LoginUserId;

    public SyncSortOrderModel(string type)
    {
      this.SortOrderType = type;
      this.Type = 1;
      this.UserId = LocalSettings.Settings.LoginUserId;
    }

    public string UserId
    {
      get => this._userId;
      set => this._userId = StringPool.GetOrCreate(value);
    }

    public string EntityId { get; set; }

    [NotNull]
    public long SortOrder { get; set; }

    [NotNull]
    public int Type { get; set; }

    [JsonIgnore]
    public int SyncStatus { get; set; }

    [NotNull]
    [Indexed]
    public string SortOrderType { get; set; }

    public int Deleted { get; set; }

    public string ColumnId { get; set; } = "";

    [NotNull]
    public string GroupId
    {
      get => this._groupId;
      set => this._groupId = StringPool.GetOrCreate(value);
    }

    public static void Copy(SyncSortOrderModel dest, SyncSortOrderModel from)
    {
      dest.UserId = from.UserId;
      dest.EntityId = from.EntityId;
      dest.SortOrder = from.SortOrder;
      dest.Type = from.Type;
      dest.SyncStatus = from.SyncStatus;
      dest.SortOrderType = from.SortOrderType;
      dest.Deleted = from.Deleted;
      dest.GroupId = from.GroupId;
    }

    public static SyncSortOrderModel Build(string type, TaskSortOrderInDateModel model)
    {
      return new SyncSortOrderModel(type)
      {
        Type = model.type,
        SyncStatus = 3,
        SortOrder = model.sortOrder,
        EntityId = model.taskid,
        GroupId = model.projectid
      };
    }

    public static SyncSortOrderModel Build(string type, TaskSortOrderInProjectModel model)
    {
      return new SyncSortOrderModel(type)
      {
        Type = EntityType.GetEntityTypeNum(model.EntityType),
        SyncStatus = 3,
        SortOrder = model.SortOrder,
        EntityId = model.EntityId,
        GroupId = model.ProjectId
      };
    }

    public static SyncSortOrderModel Build(string type, TaskSortOrderInPriorityModel model)
    {
      return new SyncSortOrderModel(type)
      {
        Type = EntityType.GetEntityTypeNum(model.EntityType),
        SyncStatus = 3,
        SortOrder = model.SortOrder,
        EntityId = model.EntityId,
        GroupId = model.CatId
      };
    }

    public static SyncSortOrderModel Build(string type, SyncSortOrderModel model)
    {
      return new SyncSortOrderModel(type)
      {
        Type = model.Type,
        SyncStatus = 3,
        SortOrder = model.SortOrder,
        EntityId = model.EntityId,
        GroupId = model.GroupId
      };
    }
  }
}
