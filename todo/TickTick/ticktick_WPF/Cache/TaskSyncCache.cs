// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.TaskSyncCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Cache
{
  public class TaskSyncCache : CacheBase<TaskSyncedJsonModel>
  {
    public override async Task Load() => TaskSyncCache.TryLoad();

    private static async Task TryLoad()
    {
      await Task.Delay(15000);
      Task.Run((Func<Task>) (async () =>
      {
        Stopwatch ws = new Stopwatch();
        ws.Restart();
        IEnumerable<string> jsonNotExitIds = await TaskSyncedJsonDao.GetJsonNotExitIds();
        List<string> list = jsonNotExitIds != null ? jsonNotExitIds.ToList<string>() : (List<string>) null;
        ws.Stop();
        UtilLog.Info("GetJsonNotExitIds : " + ws.ElapsedMilliseconds.ToString() + " " + (list != null ? new int?(list.Count<string>()) : new int?()).ToString());
        TaskSyncedJsonBean bean;
        if (list == null)
        {
          ws = (Stopwatch) null;
          bean = (TaskSyncedJsonBean) null;
        }
        else if (list.Count == 0)
        {
          ws = (Stopwatch) null;
          bean = (TaskSyncedJsonBean) null;
        }
        else
        {
          bean = new TaskSyncedJsonBean();
          foreach (string taskId in list)
          {
            TaskModel taskById = await TaskDao.GetTaskById(taskId);
            if (taskById != null)
              bean.Added.Add(await TaskDao.AssembleFullTask(taskById));
          }
          await TaskSyncedJsonDao.SaveTaskSyncedJsons(bean);
          ws = (Stopwatch) null;
          bean = (TaskSyncedJsonBean) null;
        }
      }));
    }
  }
}
