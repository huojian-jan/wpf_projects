// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ShowCountDownModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util
{
  public class ShowCountDownModel : BaseModel
  {
    public string UserId { get; set; }

    public string ProjectId { get; set; }

    public bool ShowCountDown { get; set; }
  }
}
