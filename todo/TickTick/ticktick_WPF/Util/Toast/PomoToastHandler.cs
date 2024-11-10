// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Toast.PomoToastHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Views.Pomo;

#nullable disable
namespace ticktick_WPF.Util.Toast
{
  public class PomoToastHandler : IToastHandler
  {
    public void Exec(ToastOptionModel optModel, List<KeyValuePair<string, object>> kvs)
    {
      switch (optModel.ToastType)
      {
        case ToastType.PomoCompleted:
        case ToastType.PomoRelaxCompleted:
          if (optModel.CurrentOptionName == "Complete")
          {
            if (TickFocusManager.Status == PomoStatus.WaitingWork)
              UserActCollectUtils.AddClickEvent("focus", "start_from", "reminder");
            if (TickFocusManager.Status != PomoStatus.WaitingWork && TickFocusManager.Status != PomoStatus.WaitingRelax)
              break;
            FocusTimer.BeginTimer();
            break;
          }
          if (!(optModel.CurrentOptionName == "Exit"))
            break;
          FocusTimer.Reset();
          break;
      }
    }
  }
}
