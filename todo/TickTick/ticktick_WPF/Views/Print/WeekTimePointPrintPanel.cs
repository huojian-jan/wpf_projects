﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.WeekTimePointPrintPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Calendar.Week;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class WeekTimePointPrintPanel : UserControl, IComponentConnector
  {
    internal WeekTimePointPrintPanel Root;
    internal Grid Container;
    internal Grid TopGrid;
    internal TextBlock TopEnd;
    internal StackPanel PointLinePanel;
    internal Grid ItemsPanel;
    internal Grid BotGrid;
    internal TextBlock BotStart;
    private bool _contentLoaded;

    public WeekTimePointPrintPanel() => this.InitializeComponent();

    public WeekTimePointPrintPanel(List<CalendarTimelineDayViewModel> pointModels)
    {
      this.InitializeComponent();
      if (pointModels == null)
        return;
      if (!CalendarGeoHelper.TopFolded)
      {
        this.TopGrid.Visibility = Visibility.Collapsed;
        this.BotGrid.Visibility = Visibility.Collapsed;
        this.AddLineChildren(0, 24);
      }
      else
      {
        this.AddLineChildren(CalendarGeoHelper.GetStartHour(), CalendarGeoHelper.GetEndHour());
        TextBlock topEnd = this.TopEnd;
        string str1 = CalendarGeoHelper.GetStartHour() < 10 ? "0" : "";
        int num = CalendarGeoHelper.GetStartHour();
        string str2 = num.ToString();
        string str3 = str1 + str2 + ":00";
        topEnd.Text = str3;
        TextBlock botStart = this.BotStart;
        string str4 = CalendarGeoHelper.GetEndHour() < 10 ? "0" : "";
        num = CalendarGeoHelper.GetEndHour();
        string str5 = num.ToString();
        string str6 = str4 + str5 + ":00";
        botStart.Text = str6;
        this.SetTopAndBotTask(pointModels);
      }
      if (!pointModels.Any<CalendarTimelineDayViewModel>())
        return;
      this.SetTaskModels(pointModels);
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
        textBlock1.FontSize = 20.0;
        textBlock1.LineHeight = 30.0;
        textBlock1.Foreground = (Brush) ThemeUtil.GetAlphaColor("#191919", 85);
        textBlock1.Padding = new Thickness(5.0, 10.0, 5.0, 10.0);
        textBlock1.VerticalAlignment = VerticalAlignment.Center;
        textBlock1.TextTrimming = TextTrimming.CharacterEllipsis;
        TextBlock element1 = textBlock1;
        element1.SetValue(Grid.ColumnProperty, (object) (index + 1));
        this.TopGrid.Children.Add((UIElement) element1);
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
        textBlock2.FontSize = 20.0;
        textBlock2.LineHeight = 30.0;
        textBlock2.Foreground = (Brush) ThemeUtil.GetAlphaColor("#191919", 85);
        textBlock2.Padding = new Thickness(5.0, 10.0, 5.0, 10.0);
        textBlock2.VerticalAlignment = VerticalAlignment.Center;
        textBlock2.TextTrimming = TextTrimming.CharacterEllipsis;
        TextBlock element2 = textBlock2;
        element2.SetValue(Grid.ColumnProperty, (object) (index + 1));
        this.BotGrid.Children.Add((UIElement) element2);
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
            calendarPrintCell.Margin = new Thickness(6.0 + cell.HorizontalOffset, 6.0 + 120.0 / calendarHourHeight * cell.VerticalOffset - (CalendarGeoHelper.TopFolded ? 120.0 : 0.0), 0.0, 0.0);
            calendarPrintCell.Height = num >= 10.0 ? num - 10.0 : 0.0;
            calendarPrintCell.DataContext = (object) cell;
            calendarPrintCell.Width = cell.Width >= 6.0 ? cell.Width - 6.0 : 0.0;
            calendarPrintCell.VerticalAlignment = VerticalAlignment.Top;
            calendarPrintCell.HorizontalAlignment = HorizontalAlignment.Left;
            CalendarPrintCell element = calendarPrintCell;
            element.SetValue(Grid.ColumnProperty, (object) index);
            this.ItemsPanel.Children.Add((UIElement) element);
          }
        }
      }
    }

    private void AddLineChildren(int start, int end)
    {
      for (int index = start + 1; index <= end; ++index)
        this.PointLinePanel.Children.Add((UIElement) new TimePointLine(index.ToString() + ":00", index != end));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/weektimepointprintpanel.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (WeekTimePointPrintPanel) target;
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          this.TopGrid = (Grid) target;
          break;
        case 4:
          this.TopEnd = (TextBlock) target;
          break;
        case 5:
          this.PointLinePanel = (StackPanel) target;
          break;
        case 6:
          this.ItemsPanel = (Grid) target;
          break;
        case 7:
          this.BotGrid = (Grid) target;
          break;
        case 8:
          this.BotStart = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}