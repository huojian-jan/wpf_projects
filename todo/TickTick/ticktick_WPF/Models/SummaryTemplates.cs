// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SummaryTemplates
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class SummaryTemplates : PreferenceBaseModel
  {
    public List<SummaryTemplate> templates { get; set; }

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      if (model is SummaryTemplates summaryTemplates)
      {
        if (summaryTemplates.mtime > this.mtime)
        {
          this.templates = summaryTemplates.templates;
          this.mtime = summaryTemplates.mtime;
        }
        else if (summaryTemplates.mtime < this.mtime)
          return true;
      }
      return false;
    }
  }
}
