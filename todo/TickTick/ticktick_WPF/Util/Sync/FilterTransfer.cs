// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.FilterTransfer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public static class FilterTransfer
  {
    public static SyncFilterBean DescribeSyncFilterBean(List<FilterModel> localChanges)
    {
      SyncFilterBean syncFilterBean = new SyncFilterBean();
      foreach (FilterModel localChange in localChanges)
      {
        if (localChange.IsLocalAdded())
          syncFilterBean.Add.Add(localChange);
        else if (localChange.IsLocalUpdated())
          syncFilterBean.Update.Add(localChange);
        else if (localChange.IsLocalDeleted())
          syncFilterBean.Deleted.Add(localChange.id);
      }
      return syncFilterBean;
    }
  }
}
