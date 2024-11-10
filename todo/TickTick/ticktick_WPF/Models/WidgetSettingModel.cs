// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.WidgetSettingModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class WidgetSettingModel : BaseModel
  {
    public string id { get; set; }

    public string type { get; set; }

    public string identity { get; set; }

    public string userId { get; set; }

    public string themeId { get; set; }

    public float opacity { get; set; }

    public string displayOption { get; set; }

    public bool autoHide { get; set; }

    public string sortType { get; set; }

    public string groupType { get; set; }

    public bool hideComplete { get; set; }

    public double left { get; set; }

    public double top { get; set; }

    public double width { get; set; }

    public double height { get; set; }

    public bool isLocked { get; set; }
  }
}
