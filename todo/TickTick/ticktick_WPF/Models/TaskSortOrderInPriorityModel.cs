// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskSortOrderInPriorityModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskSortOrderInPriorityModel : BaseModel
  {
    public string UserId { get; set; }

    public string CatId { get; set; }

    public string EntityType { get; set; }

    public string EntityId { get; set; }

    public int Priority { get; set; }

    public long SortOrder { get; set; }

    public int SyncStatus { get; set; }

    public int Deleted { get; set; }
  }
}
