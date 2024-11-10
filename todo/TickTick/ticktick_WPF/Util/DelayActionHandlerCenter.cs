// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DelayActionHandlerCenter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class DelayActionHandlerCenter
  {
    private static ConcurrentDictionary<string, DelayActionHandler> _actionHandlers = new ConcurrentDictionary<string, DelayActionHandler>();

    public static void TryDoAction(string uid, EventHandler handler, int interval = 300)
    {
      DelayActionHandler delayActionHandler1;
      if (!DelayActionHandlerCenter._actionHandlers.TryGetValue(uid, out delayActionHandler1))
      {
        delayActionHandler1 = new DelayActionHandler(interval);
        DelayActionHandlerCenter._actionHandlers.TryAdd(uid, delayActionHandler1);
      }
      delayActionHandler1.SetAction(handler);
      delayActionHandler1.DoAction += (EventHandler) ((o, e) =>
      {
        DelayActionHandler delayActionHandler2;
        if (!DelayActionHandlerCenter._actionHandlers.TryRemove(uid, out delayActionHandler2))
          return;
        delayActionHandler2.Dispose();
      });
      delayActionHandler1.TryDoAction();
    }

    public static void RemoveAction(string id)
    {
      DelayActionHandler delayActionHandler;
      if (!DelayActionHandlerCenter._actionHandlers.TryRemove(id, out delayActionHandler))
        return;
      delayActionHandler.StopAndClear();
    }
  }
}
