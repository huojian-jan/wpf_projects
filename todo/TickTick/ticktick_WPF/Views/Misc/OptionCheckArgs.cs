// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.OptionCheckArgs
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public struct OptionCheckArgs
  {
    public string Group;
    public string UId;
    public bool Selected;

    public OptionCheckArgs(string g, string i, bool s)
    {
      this.Group = g;
      this.UId = i;
      this.Selected = s;
    }

    public OptionCheckArgs(string g, Guid i, bool s)
    {
      this.Group = g;
      this.UId = i.ToString();
      this.Selected = s;
    }
  }
}
