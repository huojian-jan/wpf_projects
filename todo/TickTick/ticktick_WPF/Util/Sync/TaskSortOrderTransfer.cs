// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.TaskSortOrderTransfer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sync.Model;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class TaskSortOrderTransfer
  {
    public static TaskSortOrder ConvertLocalToRemote(TaskSortOrderInDateModel localOrder)
    {
      return new TaskSortOrder()
      {
        id = localOrder.taskid,
        order = new long?(localOrder.sortOrder),
        type = localOrder.type
      };
    }

    public static TaskSortOrder ConvertLocalToRemote(TaskSortOrderInProjectModel localOrder)
    {
      return new TaskSortOrder()
      {
        id = localOrder.EntityId,
        order = new long?(localOrder.SortOrder),
        type = EntityType.GetEntityTypeNum(localOrder.EntityType)
      };
    }

    public static TaskSortOrder ConvertLocalToRemote(TaskSortOrderInPriorityModel localOrder)
    {
      return new TaskSortOrder()
      {
        id = localOrder.EntityId,
        order = new long?(localOrder.SortOrder),
        type = EntityType.GetEntityTypeNum(localOrder.EntityType)
      };
    }
  }
}
