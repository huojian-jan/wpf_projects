// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.FocusConfig
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class FocusConfig : PreferenceBaseModel
  {
    public bool keepInSync { get; set; } = true;

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      if (model is FocusConfig focusConfig)
      {
        if (focusConfig.mtime > this.mtime)
        {
          this.keepInSync = focusConfig.keepInSync;
          this.mtime = focusConfig.mtime;
        }
        else if (focusConfig.mtime < this.mtime)
          return true;
      }
      return false;
    }
  }
}
