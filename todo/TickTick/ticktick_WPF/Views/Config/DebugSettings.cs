// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.DebugSettings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class DebugSettings : UserControl, IComponentConnector
  {
    internal TextBlock CalendarTest;
    private bool _contentLoaded;

    public DebugSettings() => this.InitializeComponent();

    private void ABTestClick(object sender, RoutedEventArgs e)
    {
    }

    private async void ConfigTestClick(object sender, RoutedEventArgs e)
    {
    }

    private void CalendarClick(object sender, RoutedEventArgs e)
    {
      DateTime dateTime = new DateTime(2025, 7, 25);
      System.Globalization.Calendar calendar = CalendarConverter.GetCalendar("korean-lunar");
      int year = calendar.GetYear(dateTime);
      int month = calendar.GetMonth(dateTime);
      calendar.GetDayOfMonth(dateTime);
      int era = calendar.GetEra(dateTime);
      calendar.IsLeapMonth(year, month, era);
      this.CalendarTest.Text = new CalendarDisplay(dateTime, "korean-lunar").DisplayText();
    }

    private void LeapClick(object sender, RoutedEventArgs e)
    {
      string calendarType = "hebcal";
      System.Globalization.Calendar calendar = CalendarConverter.GetCalendar("hebcal");
      DateTime dateTime = new DateTime(1970, 1, 1);
      HashSet<string> stringSet = new HashSet<string>();
      for (int index = 0; index < 30000; ++index)
      {
        dateTime = dateTime.AddDays(1.0);
        int year = calendar.GetYear(dateTime);
        int month = calendar.GetMonth(dateTime);
        int dayOfMonth = calendar.GetDayOfMonth(dateTime);
        int era = calendar.GetEra(dateTime);
        if (calendar.IsLeapMonth(year, month, era) && dayOfMonth == 1)
        {
          string str = string.Format("{0} - {1} - {2}, {3}, {4}", (object) year, (object) month, (object) dayOfMonth, (object) dateTime.ToString("yyyyMMdd"), (object) new CalendarDisplay(dateTime, calendarType).DisplayText());
          if (stringSet.Add(str))
          {
            TextBlock calendarTest = this.CalendarTest;
            calendarTest.Text = calendarTest.Text + str + "\n";
          }
        }
      }
      this.CalendarTest.Text += "finish";
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/debugsettings.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ABTestClick);
          break;
        case 2:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ConfigTestClick);
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CalendarClick);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.LeapClick);
          break;
        case 5:
          this.CalendarTest = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
