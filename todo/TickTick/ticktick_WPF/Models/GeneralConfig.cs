// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.GeneralConfig
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class GeneralConfig : PreferenceBaseModel
  {
    public bool urlParseEnabled { get; set; } = true;

    public bool emailRemindEnabled { get; set; }

    public List<string> emailRemindItems { get; set; } = new List<string>()
    {
      "task"
    };

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      if (model is GeneralConfig generalConfig)
      {
        if (generalConfig.mtime > this.mtime)
        {
          this.urlParseEnabled = generalConfig.urlParseEnabled;
          this.emailRemindEnabled = generalConfig.emailRemindEnabled;
          this.emailRemindItems = generalConfig.emailRemindItems;
          this.mtime = generalConfig.mtime;
        }
        else if (generalConfig.mtime < this.mtime)
          return true;
      }
      return false;
    }
  }
}
