// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.MultiDayLine
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public class MultiDayLine : Border
  {
    public double Offset;
    private DateTime _date;
    private Line _line;

    public DateTime Date
    {
      get => this._date;
      set
      {
        this._date = value;
        if (DateUtils.IsWeekends(this._date))
        {
          this.SetResourceReference(Border.BackgroundProperty, (object) "BaseColorOpacity2");
          this._line.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity4");
        }
        else
        {
          this.Background = (Brush) Brushes.Transparent;
          this._line.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
        }
      }
    }

    public MultiDayLine()
    {
      this.IsHitTestVisible = false;
      this.Margin = new Thickness(0.0, 41.0, 0.0, 0.0);
      this.HorizontalAlignment = HorizontalAlignment.Left;
      this.InitLine();
    }

    private void InitLine()
    {
      Line line = new Line();
      line.Y1 = 0.0;
      line.Y2 = 1.0;
      line.HorizontalAlignment = HorizontalAlignment.Right;
      line.Stretch = Stretch.Fill;
      line.StrokeThickness = 1.0;
      line.IsHitTestVisible = false;
      this._line = line;
      this._line.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
      this.Child = (UIElement) this._line;
    }
  }
}
