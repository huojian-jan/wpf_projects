// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Month.WeekDayNameView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Month
{
  public class WeekDayNameView : Grid
  {
    private readonly Dictionary<int, TextBlock> _dayNames = new Dictionary<int, TextBlock>();
    private readonly Line _hLine;
    private bool? _showWeekend;

    public WeekDayNameView()
    {
      this.Height = 41.0;
      Line line = new Line();
      line.X1 = 0.0;
      line.X2 = 1.0;
      line.Stretch = Stretch.Fill;
      line.StrokeThickness = 1.0;
      line.VerticalAlignment = VerticalAlignment.Bottom;
      this._hLine = line;
      this._hLine.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
      this.Children.Add((UIElement) this._hLine);
      this.SetColumns();
      this.SetDayNames();
    }

    public void ResetColumns(bool force = false)
    {
      if (!force && this._showWeekend.HasValue && this._showWeekend.Value == LocalSettings.Settings.ShowCalWeekend)
        return;
      this.SetColumns();
      this.SetDayNames();
    }

    private void SetColumns()
    {
      this._showWeekend = new bool?(LocalSettings.Settings.ShowCalWeekend);
      int index1 = LocalSettings.Settings.ShowCalWeekend ? 7 : 5;
      if (index1 != this.ColumnDefinitions.Count)
      {
        int count = this.ColumnDefinitions.Count;
        if (index1 > count)
        {
          for (int index2 = 0; index2 < index1 - count; ++index2)
          {
            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
              Width = new GridLength(1.0, GridUnitType.Star)
            });
            TextBlock textBlock = new TextBlock();
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Margin = new Thickness(0.0, 4.0, 0.0, 0.0);
            textBlock.FontSize = 12.0;
            TextBlock element = textBlock;
            element.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
            element.SetValue(Grid.ColumnProperty, (object) (count + index2));
            this._dayNames[count + index2] = element;
            this.Children.Add((UIElement) element);
          }
        }
        else
        {
          this.ColumnDefinitions.RemoveRange(index1, count - index1);
          for (int key = index1; key < count; ++key)
          {
            TextBlock element;
            if (this._dayNames.TryGetValue(key, out element))
            {
              this._dayNames.Remove(key);
              this.Children.Remove((UIElement) element);
            }
          }
        }
      }
      this._hLine.SetValue(Grid.ColumnSpanProperty, (object) index1);
    }

    private void SetDayNames()
    {
      DateTime dateTime = Utils.GetWeekStart(DateTime.Today);
      int num = LocalSettings.Settings.ShowCalWeekend ? 7 : 5;
      for (int key = 0; key < num; ++key)
      {
        if (!LocalSettings.Settings.ShowCalWeekend)
        {
          while (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
            dateTime = dateTime.AddDays(1.0);
        }
        TextBlock textBlock;
        if (this._dayNames.TryGetValue(key, out textBlock))
        {
          textBlock.Text = dateTime.ToString("ddd", (IFormatProvider) App.Ci);
          dateTime = dateTime.AddDays(1.0);
        }
      }
    }
  }
}
