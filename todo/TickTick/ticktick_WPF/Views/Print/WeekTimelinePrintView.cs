// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.WeekTimelinePrintView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Calendar.Week;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class WeekTimelinePrintView : Grid
  {
    private int _days;
    private SolidColorBrush _lineColor;

    public WeekTimelinePrintView(List<CalendarTimelineDayViewModel> pointModels, double height)
    {
      this.Height = height;
      if (pointModels == null || pointModels.Count == 0)
        return;
      this._days = pointModels.Count;
      this._lineColor = ThemeUtil.GetColorInString("#89191919");
      this.InitColumns();
      this.SetTopAndBotTask(pointModels);
      if (!pointModels.Any<CalendarTimelineDayViewModel>())
        return;
      this.SetTaskModels(pointModels);
    }

    private void InitColumns()
    {
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(48.0)
      });
      for (int index = 0; index < this._days; ++index)
      {
        this.ColumnDefinitions.Add(new ColumnDefinition()
        {
          Width = new GridLength(1.0, GridUnitType.Star)
        });
        Line line = new Line();
        line.Y1 = 0.0;
        line.Y2 = 1.0;
        line.HorizontalAlignment = HorizontalAlignment.Right;
        line.Stretch = Stretch.Fill;
        line.StrokeThickness = 1.0;
        line.Stroke = (Brush) this._lineColor;
        line.Margin = new Thickness(0.0, 0.0, 0.0, -1.0);
        Line element = line;
        element.SetValue(Grid.ColumnProperty, (object) index);
        this.Children.Add((UIElement) element);
      }
      Line line1 = new Line();
      line1.Y1 = 0.0;
      line1.Y2 = 1.0;
      line1.HorizontalAlignment = HorizontalAlignment.Right;
      line1.Margin = new Thickness(0.0, 0.0, 0.0, -1.0);
      line1.Stretch = Stretch.Fill;
      line1.StrokeThickness = 1.0;
      line1.Stroke = (Brush) this._lineColor;
      Line element1 = line1;
      element1.SetValue(Grid.ColumnProperty, (object) this._days);
      this.Children.Add((UIElement) element1);
      this.GetTextBlock("00:00").Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
      TextBlock textBlock = this.GetTextBlock("24:00");
      textBlock.Margin = new Thickness(0.0, this.Height - 20.0, 0.0, 0.0);
      if (!CalendarGeoHelper.TopFolded)
      {
        this.AddLineChildren(25);
        this.AddHourText(0, 24);
      }
      else
      {
        this.AddLineChildren(CalendarGeoHelper.GetEndHour() - CalendarGeoHelper.GetStartHour() + 3);
        this.AddHourText(CalendarGeoHelper.GetStartHour(), CalendarGeoHelper.GetEndHour() + (CalendarGeoHelper.GetEndHour() == 24 ? -1 : 0), 1);
        this.GetTextBlock("-" + (CalendarGeoHelper.GetStartHour() < 10 ? "0" : "") + CalendarGeoHelper.GetStartHour().ToString() + ":00").Margin = new Thickness(0.0, 100.0, 0.0, 0.0);
        this.GetTextBlock((CalendarGeoHelper.GetEndHour() < 10 ? "0" : "") + CalendarGeoHelper.GetEndHour().ToString() + ":00").Margin = new Thickness(0.0, this.Height - 100.0, 0.0, 0.0);
        textBlock.Text = "-24:00";
      }
    }

    private void AddHourText(int start, int end, int startIndex = 0)
    {
      for (int index = start + 1; index < end; ++index)
        this.GetTextBlock((index > 9 ? "" : "0") + index.ToString() + ":00").Margin = new Thickness(0.0, (double) (120 * (index - start + startIndex) - 8), 0.0, 0.0);
    }

    private TextBlock GetTextBlock(string text)
    {
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 16.0;
      textBlock.VerticalAlignment = VerticalAlignment.Top;
      textBlock.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock.Text = text;
      textBlock.Foreground = (Brush) ThemeUtil.GetColorInString("#E8191919");
      TextBlock element = textBlock;
      this.Children.Add((UIElement) element);
      return element;
    }

    private void AddLineChildren(int count)
    {
      Line line1 = new Line();
      line1.X1 = 0.0;
      line1.X2 = 1.0;
      line1.VerticalAlignment = VerticalAlignment.Top;
      line1.Stretch = Stretch.Fill;
      line1.StrokeThickness = 1.0;
      line1.Stroke = (Brush) this._lineColor;
      Line element1 = line1;
      element1.SetValue(Grid.ColumnProperty, (object) 0);
      element1.SetValue(Grid.ColumnSpanProperty, (object) (this._days + 1));
      this.Children.Add((UIElement) element1);
      for (int index = 1; index <= count; ++index)
      {
        Line line2 = new Line();
        line2.X1 = 0.0;
        line2.X2 = 1.0;
        line2.VerticalAlignment = VerticalAlignment.Top;
        line2.Margin = new Thickness(0.0, (double) (120 * index - 1), 0.0, 0.0);
        line2.Stretch = Stretch.Fill;
        line2.StrokeThickness = 1.0;
        line2.Stroke = (Brush) this._lineColor;
        Line element2 = line2;
        element2.SetValue(Grid.ColumnProperty, (object) 1);
        element2.SetValue(Grid.ColumnSpanProperty, (object) this._days);
        this.Children.Add((UIElement) element2);
      }
    }

    private void SetTopAndBotTask(List<CalendarTimelineDayViewModel> pointModels)
    {
      for (int index = 0; index < pointModels.Count; ++index)
      {
        List<CalendarDisplayModel> topTasks = pointModels[index].TopTasks;
        string str1 = topTasks != null ? topTasks.Aggregate<CalendarDisplayModel, string>("", (Func<string, CalendarDisplayModel, string>) ((current, topTask) =>
        {
          string[] strArray = new string[5]
          {
            current,
            null,
            null,
            null,
            null
          };
          DateTime? displayStartDate = topTask.DisplayStartDate;
          ref DateTime? local = ref displayStartDate;
          strArray[1] = local.HasValue ? local.GetValueOrDefault().ToString("HH:mm") : (string) null;
          strArray[2] = " ";
          strArray[3] = topTask.Title;
          strArray[4] = "\r\n";
          return string.Concat(strArray);
        })) : (string) null;
        int? nullable1;
        int? nullable2;
        if (str1 == null)
        {
          nullable1 = new int?();
          nullable2 = nullable1;
        }
        else
          nullable2 = new int?(str1.LastIndexOf("\r\n", StringComparison.Ordinal));
        int? nullable3 = nullable2;
        nullable1 = nullable3;
        int num1 = 0;
        if (nullable1.GetValueOrDefault() > num1 & nullable1.HasValue)
          str1 = str1.Remove(nullable3.Value);
        TextBlock textBlock1 = new TextBlock();
        textBlock1.Text = str1;
        textBlock1.FontSize = 18.0;
        textBlock1.LineHeight = 30.0;
        textBlock1.Foreground = (Brush) ThemeUtil.GetAlphaColor("#191919", 85);
        textBlock1.Padding = new Thickness(5.0, 10.0, 5.0, 10.0);
        textBlock1.VerticalAlignment = VerticalAlignment.Top;
        textBlock1.TextTrimming = TextTrimming.CharacterEllipsis;
        TextBlock element1 = textBlock1;
        element1.SetValue(Grid.ColumnProperty, (object) (index + 1));
        this.Children.Add((UIElement) element1);
        List<CalendarDisplayModel> botTasks = pointModels[index].BotTasks;
        string str2 = botTasks != null ? botTasks.Aggregate<CalendarDisplayModel, string>("", (Func<string, CalendarDisplayModel, string>) ((current, topTask) =>
        {
          string[] strArray = new string[5]
          {
            current,
            null,
            null,
            null,
            null
          };
          DateTime? displayStartDate = topTask.DisplayStartDate;
          ref DateTime? local = ref displayStartDate;
          strArray[1] = local.HasValue ? local.GetValueOrDefault().ToString("HH:mm") : (string) null;
          strArray[2] = " ";
          strArray[3] = topTask.Title;
          strArray[4] = "\r\n";
          return string.Concat(strArray);
        })) : (string) null;
        int? nullable4;
        if (str2 == null)
        {
          nullable1 = new int?();
          nullable4 = nullable1;
        }
        else
          nullable4 = new int?(str2.LastIndexOf("\r\n", StringComparison.Ordinal));
        int? nullable5 = nullable4;
        nullable1 = nullable5;
        int num2 = 0;
        if (nullable1.GetValueOrDefault() > num2 & nullable1.HasValue)
          str2 = str2.Remove(nullable5.Value);
        TextBlock textBlock2 = new TextBlock();
        textBlock2.Text = str2;
        textBlock2.FontSize = 18.0;
        textBlock2.LineHeight = 30.0;
        textBlock2.Foreground = (Brush) ThemeUtil.GetAlphaColor("#191919", 85);
        textBlock2.Padding = new Thickness(5.0, this.Height - 110.0, 5.0, 10.0);
        textBlock2.VerticalAlignment = VerticalAlignment.Top;
        textBlock2.TextTrimming = TextTrimming.CharacterEllipsis;
        TextBlock element2 = textBlock2;
        element2.SetValue(Grid.ColumnProperty, (object) (index + 1));
        this.Children.Add((UIElement) element2);
      }
    }

    private void SetTaskModels(List<CalendarTimelineDayViewModel> models)
    {
      for (int index = 0; index < models.Count; ++index)
      {
        if (models[index].Cells != null && models[index].Cells.Any<TaskCellViewModel>())
        {
          foreach (TaskCellViewModel cell in models[index].Cells)
          {
            double calendarHourHeight = LocalSettings.Settings.CalendarHourHeight;
            double num = cell.Height / calendarHourHeight * 120.0;
            CalendarPrintCell calendarPrintCell = new CalendarPrintCell();
            calendarPrintCell.Margin = new Thickness(6.0 + cell.HorizontalOffset, (CalendarGeoHelper.TopFolded ? 124.0 : 4.0) + 120.0 / calendarHourHeight * (cell.VerticalOffset - (CalendarGeoHelper.TopFolded ? 32.0 : 0.0)), 0.0, 0.0);
            calendarPrintCell.Height = num >= 10.0 ? num - 10.0 : 0.0;
            calendarPrintCell.DataContext = (object) cell;
            calendarPrintCell.Width = cell.Width >= 6.0 ? cell.Width - 6.0 : 0.0;
            calendarPrintCell.VerticalAlignment = VerticalAlignment.Top;
            calendarPrintCell.HorizontalAlignment = HorizontalAlignment.Left;
            CalendarPrintCell element = calendarPrintCell;
            element.SetValue(Grid.ColumnProperty, (object) (index + 1));
            this.Children.Add((UIElement) element);
          }
        }
      }
    }
  }
}
