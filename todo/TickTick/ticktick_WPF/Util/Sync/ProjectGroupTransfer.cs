// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ProjectGroupTransfer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class ProjectGroupTransfer
  {
    public static SyncProjectGroupBean DescribeSyncProjectGroupBean(
      List<ProjectGroupModel> localChanges)
    {
      SyncProjectGroupBean projectGroupBean = new SyncProjectGroupBean();
      HashSet<string> stringSet = new HashSet<string>();
      if (localChanges != null)
      {
        foreach (ProjectGroupModel localChange in localChanges)
        {
          if (stringSet.Contains(localChange.id))
            App.Connection.DeleteAsync((object) localChange);
          if (localChange.sync_status.Equals("SYNC_NEW"))
          {
            projectGroupBean.Add.Add(localChange);
            stringSet.Add(localChange.id);
          }
          else if (localChange.sync_status.Equals("SYNC_UPDATE") || localChange.sync_status.Equals("SYNC_INIT"))
          {
            projectGroupBean.Update.Add(localChange);
            stringSet.Add(localChange.id);
          }
        }
      }
      return projectGroupBean;
    }
  }
}
