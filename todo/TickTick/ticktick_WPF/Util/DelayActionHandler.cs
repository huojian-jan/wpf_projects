// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DelayActionHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Timers;

#nullable disable
namespace ticktick_WPF.Util
{
  public class DelayActionHandler
  {
    private bool _cleared;
    private readonly object _locker = new object();
    private readonly Timer _delayTimer = new Timer()
    {
      AutoReset = false
    };

    public event EventHandler DoAction;

    public DelayActionHandler(int interval = 300)
    {
      this._delayTimer.Interval = (double) interval;
      this._delayTimer.Elapsed += new ElapsedEventHandler(this.OnAction);
    }

    private void OnAction(object sender, ElapsedEventArgs e)
    {
      lock (this._locker)
        this._delayTimer.Stop();
      EventHandler doAction = this.DoAction;
      if (doAction == null)
        return;
      doAction((object) this, (EventArgs) null);
    }

    public void ImmediatelyDoAction()
    {
      lock (this._locker)
        this._delayTimer.Stop();
      EventHandler doAction = this.DoAction;
      if (doAction == null)
        return;
      doAction((object) this, (EventArgs) null);
    }

    public void TryDoAction(int? interval = null)
    {
      lock (this._locker)
      {
        try
        {
          if (this._delayTimer == null)
            return;
          this._delayTimer.Interval = (double) interval ?? this._delayTimer.Interval;
          if (this._delayTimer.Enabled)
            return;
          this._delayTimer.Start();
        }
        catch
        {
        }
      }
    }

    public void CancelAction()
    {
      lock (this._locker)
        this._delayTimer.Stop();
    }

    public void SetAction(EventHandler handler)
    {
      lock (this._locker)
      {
        this.DoAction = (EventHandler) null;
        this._delayTimer.Stop();
        if (this._cleared)
        {
          this._cleared = false;
          this._delayTimer.Elapsed += new ElapsedEventHandler(this.OnAction);
        }
        this.DoAction += handler;
      }
    }

    public void StopAndClear()
    {
      lock (this._locker)
      {
        this._cleared = true;
        this.DoAction = (EventHandler) null;
        this._delayTimer.Stop();
        this._delayTimer.Elapsed -= new ElapsedEventHandler(this.OnAction);
      }
    }

    public void Dispose()
    {
      lock (this._locker)
      {
        this._cleared = true;
        this.DoAction = (EventHandler) null;
        this._delayTimer.Stop();
        this._delayTimer.Elapsed -= new ElapsedEventHandler(this.OnAction);
        this._delayTimer.Close();
      }
    }
  }
}
