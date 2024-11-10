// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Event.DragMouseEvent
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Input;

#nullable disable
namespace ticktick_WPF.Event
{
  public class DragMouseEvent
  {
    public DragMouseEvent(double x, double y)
    {
      this.X = x;
      this.Y = y;
    }

    public DragMouseEvent(MouseEventArgs arg) => this.MouseArg = arg;

    public MouseEventArgs MouseArg { get; set; }

    public object Data { get; set; }

    public double X { get; }

    public double Y { get; }
  }
}
