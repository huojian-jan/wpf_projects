// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.HolidayModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class HolidayModel : BaseModel
  {
    public int type { get; set; }

    public DateTime date { get; set; }

    public string name { get; set; }

    public string region { get; set; }
  }
}
