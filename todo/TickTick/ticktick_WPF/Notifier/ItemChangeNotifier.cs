// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.ItemChangeNotifier
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public static class ItemChangeNotifier
  {
    public static void NotifyDeletedUndo(TaskDetailItemModel item)
    {
      TaskChangeNotifier.OnCheckItemChanged(item.id);
    }

    public static void NotifyItemDeleted(TaskDetailItemModel item, object sender = null)
    {
      TaskChangeNotifier.OnCheckItemChanged(item.id);
    }

    public static void NotifyItemDateChanged(TaskDetailItemModel item)
    {
      TaskChangeNotifier.OnCheckItemChanged(item.id, true);
    }

    public static void NotifyItemStatusChanged(string id)
    {
      TaskChangeNotifier.OnCheckItemChanged(id, true);
    }
  }
}
