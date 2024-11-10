// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskWindowDisplayArgs
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskWindowDisplayArgs
  {
    public UIElement Target;
    public double TargetWidth;
    public Point Point;
    public bool FocusTitle;
    public double AddHeight;
    public int QuadrantLevel;

    public TaskWindowDisplayArgs(
      UIElement target,
      double targetWidth,
      Point point,
      bool focus = false,
      double height = 0.0,
      int qLevel = 0)
    {
      this.Target = target;
      this.TargetWidth = targetWidth;
      this.Point = point;
      this.FocusTitle = focus;
      this.AddHeight = height;
      this.QuadrantLevel = qLevel;
    }

    public TaskWindowDisplayArgs(UIElement target, double height)
    {
      this.Target = target;
      this.AddHeight = height;
    }

    public TaskWindowDisplayArgs(UIElement target, double targetWidth, Point point, double height = 0.0)
    {
      this.Target = target;
      this.TargetWidth = targetWidth;
      this.Point = point;
      this.AddHeight = height;
    }

    public TaskWindowDisplayArgs(UIElement target, double targetWidth, bool focus, int qLevel)
    {
      this.Target = target;
      this.TargetWidth = targetWidth;
      this.FocusTitle = focus;
      this.QuadrantLevel = qLevel;
    }
  }
}
