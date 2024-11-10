// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.AttendModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class AttendModel : BaseModel
  {
    [Unique]
    public string attendId { get; set; }

    public string json { get; set; }
  }
}
