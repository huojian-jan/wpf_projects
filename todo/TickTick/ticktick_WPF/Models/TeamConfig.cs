// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TeamConfig
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class TeamConfig : PreferenceBaseModel
  {
    public bool autoAcceptInvite { get; set; } = true;

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      if (model is TeamConfig teamConfig)
      {
        if (teamConfig.mtime > this.mtime)
        {
          this.autoAcceptInvite = teamConfig.autoAcceptInvite;
          this.mtime = teamConfig.mtime;
        }
        else if (teamConfig.mtime < this.mtime)
          return true;
      }
      return false;
    }
  }
}
