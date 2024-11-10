// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskCountDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TaskCountDao
  {
    public static async Task<List<SortableTask>> GetAllSortableTasks()
    {
      return await App.Connection.QueryAsync<SortableTask>("select distinct(id) as TaskId, projectId as ProjectId, tag as Tag from taskmodel where userid = '" + LocalSettings.Settings.LoginUserId + "' and status = 0 and deleted = 0");
    }
  }
}
