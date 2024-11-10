// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.LockViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class LockViewModel
  {
    public LockViewModel(AppLockModel model)
    {
      this.MinLock = model.MinLock;
      this.LockAfter = model.LockAfter;
      this.LockInterval = model.LockInterval;
      this.LockWidget = model.LockWidget;
    }

    public bool MinLock { get; set; }

    public bool LockAfter { get; set; }

    public int LockInterval { get; set; }

    public bool LockWidget { get; set; }
  }
}
