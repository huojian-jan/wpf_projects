// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.LocationModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class LocationModel : BaseModel
  {
    public string alias { get; set; }

    [Ignore]
    public LocationModel.Loc loc { get; set; }

    public float? radius { get; set; }

    public int? transitionType { get; set; }

    public string shortAddress { get; set; }

    public string address { get; set; }

    public string userId { get; set; }

    public string taskId { get; set; }

    public float longitude { get; set; }

    public float latitude { get; set; }

    public class Loc
    {
      public float longitude { get; set; }

      public float latitude { get; set; }
    }
  }
}
