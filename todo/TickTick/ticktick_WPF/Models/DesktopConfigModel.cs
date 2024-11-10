// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.DesktopConfigModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class DesktopConfigModel : PreferenceBaseModel
  {
    public List<string> tabs { get; set; } = new List<string>();

    public override bool SetRemoteValue(PreferenceBaseModel remote)
    {
      if (remote is DesktopConfigModel desktopConfigModel)
      {
        if (desktopConfigModel.mtime > this.mtime)
        {
          this.tabs = desktopConfigModel.tabs;
          this.mtime = desktopConfigModel.mtime;
        }
        if (remote.mtime < this.mtime)
          return true;
      }
      return false;
    }
  }
}
