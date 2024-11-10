// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.RecentlyColors
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class RecentlyColors : PreferenceBaseModel
  {
    public List<string> colors { get; set; }

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      if (model is RecentlyColors recentlyColors)
      {
        if (recentlyColors.mtime > this.mtime)
        {
          this.colors = recentlyColors.colors;
          this.mtime = recentlyColors.mtime;
        }
        else if (recentlyColors.mtime < this.mtime)
          return true;
      }
      return false;
    }
  }
}
