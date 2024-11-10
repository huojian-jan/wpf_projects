// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.DragEventManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Event;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public static class DragEventManager
  {
    public static event EventHandler<DragMouseEvent> CheckItemDragEvent;

    public static event EventHandler<string> CheckItemDrop;

    public static void NotifySubtaskDrop(string itemId)
    {
      EventHandler<string> checkItemDrop = DragEventManager.CheckItemDrop;
      if (checkItemDrop == null)
        return;
      checkItemDrop((object) null, itemId);
    }

    public static void NotifyDragEvent(DragMouseEvent mouseEvent)
    {
      EventHandler<DragMouseEvent> checkItemDragEvent = DragEventManager.CheckItemDragEvent;
      if (checkItemDragEvent == null)
        return;
      checkItemDragEvent((object) null, mouseEvent);
    }
  }
}
