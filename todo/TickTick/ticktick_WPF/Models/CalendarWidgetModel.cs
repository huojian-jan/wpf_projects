// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CalendarWidgetModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;
using System.ComponentModel;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class CalendarWidgetModel : BaseModel
  {
    public string id { get; set; }

    public string userId { get; set; }

    public string themeId { get; set; }

    public float opacity { get; set; }

    public string displayOption { get; set; }

    public bool autoHide { get; set; }

    public bool hideComplete { get; set; }

    public double left { get; set; }

    public double top { get; set; }

    public double width { get; set; }

    public double height { get; set; }

    public bool isLocked { get; set; }

    [NotNull]
    [DefaultValue("0")]
    public int mode { get; set; }
  }
}
