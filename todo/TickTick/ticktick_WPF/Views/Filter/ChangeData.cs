// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ChangeData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class ChangeData
  {
    public ChangeType Type { get; set; }

    public CardViewModel From { get; set; }

    public CardViewModel To { get; set; }
  }
}
