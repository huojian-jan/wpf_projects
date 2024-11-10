// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.MultiDayPrintHeadView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class MultiDayPrintHeadView : Grid
  {
    public MultiDayPrintHeadView(DateTime date, int days, int month, bool showWeekends)
    {
      this.Height = 80.0;
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(48.0)
      });
      SolidColorBrush colorInString1 = ThemeUtil.GetColorInString("#89191919");
      SolidColorBrush colorInString2 = ThemeUtil.GetColorInString("#E8191919");
      for (int index = 0; index < days; ++index)
      {
        this.ColumnDefinitions.Add(new ColumnDefinition()
        {
          Width = new GridLength(1.0, GridUnitType.Star)
        });
        Line line = new Line();
        line.Y1 = 0.0;
        line.Y2 = 1.0;
        line.HorizontalAlignment = HorizontalAlignment.Right;
        line.Margin = new Thickness(0.0, 50.0, 0.0, -1.0);
        line.Stretch = Stretch.Fill;
        line.StrokeThickness = 1.0;
        line.Stroke = (Brush) colorInString1;
        Line element1 = line;
        element1.SetValue(Grid.ColumnProperty, (object) index);
        this.Children.Add((UIElement) element1);
        while (!showWeekends && TickTickUtils.DateUtils.IsWeekends(date))
          date = date.AddDays(1.0);
        TextBlock textBlock1 = new TextBlock();
        textBlock1.FontSize = 16.0;
        textBlock1.Margin = new Thickness(10.0, 10.0, 10.0, 0.0);
        textBlock1.VerticalAlignment = VerticalAlignment.Top;
        textBlock1.Text = date.ToString("ddd", (IFormatProvider) App.Ci);
        textBlock1.Foreground = (Brush) colorInString1;
        textBlock1.Opacity = TickTickUtils.DateUtils.IsWeekends(date) ? 0.6 : 1.0;
        TextBlock element2 = textBlock1;
        element2.SetValue(Grid.ColumnProperty, (object) (index + 1));
        this.Children.Add((UIElement) element2);
        TextBlock textBlock2 = new TextBlock();
        textBlock2.FontSize = 24.0;
        textBlock2.Margin = new Thickness(10.0, 0.0, 10.0, 10.0);
        textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
        textBlock2.Text = date.Day.ToString();
        textBlock2.Foreground = (Brush) colorInString2;
        textBlock2.Opacity = date.Month != month ? 0.6 : 1.0;
        TextBlock element3 = textBlock2;
        element3.SetValue(Grid.ColumnProperty, (object) (index + 1));
        this.Children.Add((UIElement) element3);
        date = date.AddDays(1.0);
      }
      Line line1 = new Line();
      line1.Y1 = 0.0;
      line1.Y2 = 1.0;
      line1.HorizontalAlignment = HorizontalAlignment.Right;
      line1.Margin = new Thickness(0.0, 50.0, 0.0, -1.0);
      line1.Stretch = Stretch.Fill;
      line1.StrokeThickness = 1.0;
      line1.Stroke = (Brush) colorInString1;
      Line element4 = line1;
      element4.SetValue(Grid.ColumnProperty, (object) days);
      this.Children.Add((UIElement) element4);
      Line line2 = new Line();
      line2.X1 = 0.0;
      line2.X2 = 1.0;
      line2.VerticalAlignment = VerticalAlignment.Bottom;
      line2.Stretch = Stretch.Fill;
      line2.StrokeThickness = 1.0;
      line2.Stroke = (Brush) colorInString1;
      Line element5 = line2;
      element5.SetValue(Grid.ColumnSpanProperty, (object) (days + 1));
      this.Children.Add((UIElement) element5);
    }
  }
}
