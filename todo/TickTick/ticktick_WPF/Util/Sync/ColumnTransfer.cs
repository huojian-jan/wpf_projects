// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ColumnTransfer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class ColumnTransfer
  {
    public static async Task<SyncColumnBean> DescribeSyncColumnBean(
      Dictionary<string, string> changes)
    {
      SyncColumnBean bean = new SyncColumnBean();
      if (changes != null && changes.Any<KeyValuePair<string, string>>())
      {
        foreach (KeyValuePair<string, string> change in changes)
        {
          ColumnModel columnById = await ColumnDao.GetColumnById(change.Key);
          if (columnById != null)
          {
            switch (columnById.syncStatus)
            {
              case "init":
              case "new":
                bean.Add.Add(columnById);
                continue;
              case "update":
                bean.Update.Add(columnById);
                continue;
              case "delete":
                bean.Deleted.Add(new ColumnProjectModel()
                {
                  columnId = columnById.id,
                  projectId = columnById.projectId
                });
                continue;
              default:
                continue;
            }
          }
        }
      }
      SyncColumnBean syncColumnBean = bean;
      bean = (SyncColumnBean) null;
      return syncColumnBean;
    }

    public static async Task HandleCommitResult(
      Dictionary<string, string> id2Etag,
      Dictionary<string, string> id2Error,
      LogModel logModel)
    {
      await ColumnDao.SaveCommitResultBackToDb(id2Etag, id2Error, logModel);
    }
  }
}
