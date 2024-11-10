// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.SetDateDialogArgs
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls.Primitives;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public struct SetDateDialogArgs
  {
    public bool CalendarMode;
    public bool IsNote;
    public bool ItemMode;
    public UIElement Target;
    public double HOffset;
    public double VOffset;
    public PlacementMode Placement;
    public bool ShowQuickDate;
    public bool CanSkip;
    public bool ShowRemind;

    public SetDateDialogArgs(
      bool calendarMode = false,
      bool isNote = false,
      bool itemMode = false,
      UIElement target = null,
      double hOffset = 0.0,
      double vOffset = 0.0,
      PlacementMode placement = PlacementMode.Relative,
      bool showQuickDate = true,
      bool canSkip = true,
      bool showRemind = true)
    {
      this.CalendarMode = calendarMode;
      this.IsNote = isNote;
      this.ItemMode = itemMode;
      this.Target = target;
      this.HOffset = hOffset;
      this.VOffset = vOffset;
      this.Placement = placement;
      this.ShowQuickDate = showQuickDate;
      this.CanSkip = canSkip;
      this.ShowRemind = showRemind;
    }
  }
}
