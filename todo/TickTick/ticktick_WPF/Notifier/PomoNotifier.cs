// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.PomoNotifier
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Views.Pomo;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public class PomoNotifier
  {
    public static void NotifyChanged(string pomoId, PomoChangeType type)
    {
      PomoChangeArgs e = new PomoChangeArgs()
      {
        Id = pomoId,
        ChangeType = type
      };
      EventHandler<PomoChangeArgs> changed = PomoNotifier.Changed;
      if (changed != null)
        changed((object) null, e);
      TickFocusManager.LoadStatistics();
    }

    public static void NotifyChanged(List<string> ids, PomoChangeType type)
    {
      PomoChangeArgs e = new PomoChangeArgs()
      {
        Ids = ids.ToList<string>(),
        ChangeType = type
      };
      EventHandler<PomoChangeArgs> changed = PomoNotifier.Changed;
      if (changed != null)
        changed((object) null, e);
      TickFocusManager.LoadStatistics();
    }

    public static void NotifyServiceChanged()
    {
      EventHandler serviceChanged = PomoNotifier.ServiceChanged;
      if (serviceChanged != null)
        serviceChanged((object) null, (EventArgs) null);
      TickFocusManager.LoadStatistics();
    }

    public static void NotifyTimerChanged()
    {
      EventHandler timerChanged = PomoNotifier.TimerChanged;
      if (timerChanged == null)
        return;
      timerChanged((object) null, (EventArgs) null);
    }

    public static void NotifyLinkChanged(string previewTimerId, string timerId)
    {
      EventHandler<PomoLinkArgs> linkChanged = PomoNotifier.LinkChanged;
      if (linkChanged == null)
        return;
      linkChanged((object) null, new PomoLinkArgs()
      {
        PreviewTimerId = previewTimerId,
        NewTimerId = timerId
      });
    }

    public static void NotifyFocusNoteChanged(object sender)
    {
      EventHandler focusingNoteChanged = PomoNotifier.FocusingNoteChanged;
      if (focusingNoteChanged == null)
        return;
      focusingNoteChanged(sender, (EventArgs) null);
    }

    public static event EventHandler<PomoChangeArgs> Changed;

    public static event EventHandler<PomoLinkArgs> LinkChanged;

    public static event EventHandler ServiceChanged;

    public static event EventHandler TimerChanged;

    public static event EventHandler PomoCommit;

    public static event EventHandler FocusingNoteChanged;

    public static void NotifyPomoCommit()
    {
      EventHandler pomoCommit = PomoNotifier.PomoCommit;
      if (pomoCommit == null)
        return;
      pomoCommit((object) null, (EventArgs) null);
    }
  }
}
