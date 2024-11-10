// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.AppLockModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class AppLockModel : BaseModel
  {
    public string UserId { get; set; }

    public string Password { get; set; }

    public bool MinLock { get; set; }

    public bool LockAfter { get; set; }

    public int LockInterval { get; set; }

    public bool LockWidget { get; set; }

    public bool Locked { get; set; }

    public bool Equal(AppLockModel model)
    {
      return this.MinLock == model.MinLock && this.LockAfter == model.LockAfter && this.LockWidget == model.LockWidget && this.LockInterval == model.LockInterval;
    }
  }
}
