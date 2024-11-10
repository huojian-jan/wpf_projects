// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskPinnedSortOrderModelComparer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskPinnedSortOrderModelComparer : IEqualityComparer<SyncSortOrderModel>
  {
    public bool Equals(SyncSortOrderModel x, SyncSortOrderModel y)
    {
      if (x == null && y == null)
        return true;
      return x != null && y != null && x.EntityId == y.EntityId;
    }

    public int GetHashCode(SyncSortOrderModel obj) => obj.EntityId.GetHashCode();
  }
}
