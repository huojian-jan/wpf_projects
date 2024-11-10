// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.SyncDataBean`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class SyncDataBean<T>
  {
    private List<T> addeds = new List<T>();
    private List<T> updateds = new List<T>();
    private List<T> deleteds = new List<T>();

    public List<T> Addeds
    {
      get => this.addeds;
      set => this.addeds = value;
    }

    public List<T> Updateds
    {
      get => this.updateds;
      set => this.updateds = value;
    }

    public List<T> Deleteds
    {
      get => this.deleteds;
      set => this.deleteds = value;
    }
  }
}
