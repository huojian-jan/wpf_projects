// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TimeCheckActionHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Timers;

#nullable disable
namespace ticktick_WPF.Util
{
  public class TimeCheckActionHandler
  {
    private DateTime? _actionTime;
    private readonly Timer _delayTimer;
    private readonly int _interval;

    public event EventHandler DoAction;

    public TimeCheckActionHandler(int interval = 3)
    {
      this._delayTimer = new Timer((double) (interval * 1000));
      this._delayTimer.Elapsed -= new ElapsedEventHandler(this.OnAction);
      this._delayTimer.Elapsed += new ElapsedEventHandler(this.OnAction);
      this._interval = interval;
    }

    private void OnAction(object sender, ElapsedEventArgs e)
    {
      this._delayTimer.Stop();
      EventHandler doAction = this.DoAction;
      if (doAction == null)
        return;
      doAction((object) this, (EventArgs) null);
    }

    public void TryDoAction()
    {
      if (this.CheckCanDoAction())
      {
        this._delayTimer.Stop();
        EventHandler doAction = this.DoAction;
        if (doAction == null)
          return;
        doAction((object) this, (EventArgs) null);
      }
      else
      {
        this._delayTimer.Stop();
        this._delayTimer.Start();
      }
    }

    private bool CheckCanDoAction()
    {
      if (!this._actionTime.HasValue)
      {
        this._actionTime = new DateTime?(DateTime.Now);
        return true;
      }
      if ((DateTime.Now - this._actionTime.Value).TotalSeconds <= (double) this._interval)
        return false;
      this._actionTime = new DateTime?(DateTime.Now);
      return true;
    }
  }
}
