// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.ItemDragNotifier
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public static class ItemDragNotifier
  {
    public static event EventHandler<MouseEventArgs> MouseMove;

    public static void NotifyMouseMove(MouseEventArgs e)
    {
      EventHandler<MouseEventArgs> mouseMove = ItemDragNotifier.MouseMove;
      if (mouseMove == null)
        return;
      mouseMove((object) null, e);
    }
  }
}
