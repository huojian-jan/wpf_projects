// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.SortOrder.ProjectPinSortOrderModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Service.SortOrder
{
  [Table("SyncSortOrderModel")]
  public class ProjectPinSortOrderModel : SyncSortOrderModel
  {
    public ProjectPinSortOrderModel() => this.SortOrderType = "projectPinned";

    public ProjectPinSortOrderModel(SyncSortOrderModel model)
    {
      this.SortOrderType = "projectPinned";
      SyncSortOrderModel.Copy((SyncSortOrderModel) this, model);
    }
  }
}
