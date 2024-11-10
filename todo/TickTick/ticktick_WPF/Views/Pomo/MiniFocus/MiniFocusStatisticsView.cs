// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MiniFocus.MiniFocusStatisticsView
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MiniFocus
{
  public class MiniFocusStatisticsView : UserControl, IComponentConnector
  {
    public static string MiniStatisticsTodayPomo = Utils.GetString("TodayPomo");
    public static string MiniStatisticsTodayDuration = Utils.GetString("DailyFocused");
    public static string MiniStatisticsWeekPomo = Utils.GetString("WeeklyPomo");
    public static string MiniStatisticsWeekDuration = Utils.GetString("WeeklyFocused");
    private List<string> _typeNames;
    internal Grid Container;
    internal Border LeftBorder;
    internal StackPanel StatisticsTitle1;
    internal TextBlock Text1;
    internal Path StatisticsPath1;
    internal Border ValBorder1;
    internal TextBlock FirstVal;
    internal Run FirstRun1;
    internal Run FirstRun2;
    internal Run FirstRun3;
    internal Run FirstRun4;
    internal Border RightBorder;
    internal StackPanel StatisticsTitle2;
    internal TextBlock Text2;
    internal Border ValBorder2;
    internal Run SecondRun1;
    internal Run SecondRun2;
    internal Run SecondRun3;
    internal Run SecondRun4;
    private bool _contentLoaded;

    public MiniFocusStatisticsView()
    {
      this.InitializeComponent();
      this._typeNames = new List<string>()
      {
        MiniFocusStatisticsView.MiniStatisticsTodayDuration,
        MiniFocusStatisticsView.MiniStatisticsTodayPomo,
        MiniFocusStatisticsView.MiniStatisticsWeekDuration,
        MiniFocusStatisticsView.MiniStatisticsWeekPomo
      };
      this.SetData();
      this.SetTitleEnable();
    }

    public void SetTitleEnable()
    {
      this.StatisticsTitle2.IsHitTestVisible = TickFocusManager.IsPomo;
      this.StatisticsTitle1.IsHitTestVisible = TickFocusManager.IsPomo;
    }

    public bool TitleMouseOver
    {
      get => this.StatisticsTitle1.IsMouseOver || this.StatisticsTitle2.IsMouseOver;
    }

    public bool PopupOpened { get; set; }

    public void SetData()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        int[] statisticsTypesSafely = TickFocusManager.GetStatisticsTypesSafely();
        int num1 = statisticsTypesSafely[0] < 0 || statisticsTypesSafely[0] > 3 ? 0 : statisticsTypesSafely[0];
        int num2 = statisticsTypesSafely[1] < 0 || statisticsTypesSafely[1] > 3 ? 2 : statisticsTypesSafely[1];
        this.Text1.Text = this._typeNames[num1];
        this.Text2.Text = this._typeNames[num2];
        PomoStat pomoStat = TickFocusManager.PomoStatistics;
        if (pomoStat == null)
          return;
        SetStatistics(this.FirstRun1, this.FirstRun2, this.FirstRun3, this.FirstRun4, num1);
        SetStatistics(this.SecondRun1, this.SecondRun2, this.SecondRun3, this.SecondRun4, num2);

        void SetStatistics(Run run1, Run run2, Run run3, Run run4, int type)
        {
          switch (type)
          {
            case 0:
              SetDurationStatistics(run1, run2, run3, run4, pomoStat.TodayDuration);
              break;
            case 1:
              SetPomoStatistics(run1, run2, run3, run4, pomoStat.TodayPomos);
              break;
            case 2:
              SetDurationStatistics(run1, run2, run3, run4, pomoStat.WeeklyDuration);
              break;
            case 3:
              SetPomoStatistics(run1, run2, run3, run4, pomoStat.WeeklyPomos);
              break;
          }
        }
      }));

      static void SetPomoStatistics(Run run1, Run run2, Run run3, Run run4, int pomos)
      {
        run1.Text = pomos.ToString();
        run2.Text = "";
        run3.Text = "";
        run4.Text = "";
      }

      static void SetDurationStatistics(Run run1, Run run2, Run run3, Run run4, long duration)
      {
        long num1 = duration / 60L + (long) (duration % 60L >= 30L);
        long num2 = num1 / 60L;
        long num3 = num1 % 60L;
        run1.Text = num2 > 0L ? num2.ToString() ?? "" : "";
        run2.Text = num2 > 0L ? "h " : "";
        run3.Text = num2 <= 0L || num3 != 0L ? num3.ToString() ?? "" : "";
        run4.Text = num2 <= 0L || num3 != 0L ? "m" : "";
      }
    }

    private void OnStatisticsTitle1Click(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      int[] statisticsTypesSafely = TickFocusManager.GetStatisticsTypesSafely();
      PomoStatisticsSetView statisticsSetView = new PomoStatisticsSetView(statisticsTypesSafely[0], statisticsTypesSafely[1]);
      statisticsSetView.TypeChanged += (EventHandler) ((o, args) => this.SetData());
      statisticsSetView.PopupClosed += (EventHandler) ((o, args) => this.PopupOpened = false);
      statisticsSetView.Show((UIElement) this.StatisticsTitle1);
      this.PopupOpened = true;
    }

    private void OnStatisticsTitle2Click(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      int[] statisticsTypesSafely = TickFocusManager.GetStatisticsTypesSafely();
      PomoStatisticsSetView statisticsSetView = new PomoStatisticsSetView(statisticsTypesSafely[1], statisticsTypesSafely[0]);
      statisticsSetView.TypeChanged += (EventHandler) ((o, args) => this.SetData());
      statisticsSetView.PopupClosed += (EventHandler) ((o, args) => this.PopupOpened = false);
      statisticsSetView.Show((UIElement) this.StatisticsTitle2);
      this.PopupOpened = true;
    }

    private void OnStatisticsClick(object sender, MouseButtonEventArgs e)
    {
      if (this.StatisticsTitle2.IsMouseOver || this.StatisticsTitle1.IsMouseOver)
        return;
      TickFocusManager.OpenStatisticsInWeb();
      UserActCollectUtils.AddClickEvent("focus_mini", "om", "statistics");
    }

    public void SetSize(double rate, double textRate)
    {
      this.Container.Width = 194.0 * rate;
      this.LeftBorder.Width = 110.0 * rate;
      this.LeftBorder.Margin = new Thickness(-7.0 * rate, 0.0, 0.0, 0.0);
      this.StatisticsTitle1.Margin = new Thickness(14.0 * rate, 8.0 * rate, 0.0, 0.0);
      this.Text1.MaxWidth = 80.0 * rate;
      this.Text1.LineHeight = 11.0 * textRate;
      this.Text1.FontSize = 9.5 * textRate;
      this.StatisticsPath1.Width = 10.0 * textRate;
      this.StatisticsPath1.Height = 10.0 * textRate;
      this.ValBorder1.Width = 93.0 * rate;
      this.ValBorder1.Margin = new Thickness(4.0 * rate, 0.0, 0.0, 0.0);
      this.FirstVal.Margin = new Thickness(0.0, 4.0 * rate, 0.0, 8.0 * rate);
      this.FirstVal.FontSize = 14.0 * textRate;
      this.FirstRun2.FontSize = 10.0 * textRate;
      this.RightBorder.Margin = new Thickness(85.0 * rate, 0.0, 0.0, 0.0);
      this.ValBorder2.Margin = new Thickness(97.0 * rate, 0.0, 0.0, 0.0);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/minifocus/minifocusstatisticsview.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Container = (Grid) target;
          break;
        case 2:
          this.LeftBorder = (Border) target;
          break;
        case 3:
          this.StatisticsTitle1 = (StackPanel) target;
          this.StatisticsTitle1.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnStatisticsTitle1Click);
          break;
        case 4:
          this.Text1 = (TextBlock) target;
          break;
        case 5:
          this.StatisticsPath1 = (Path) target;
          break;
        case 6:
          this.ValBorder1 = (Border) target;
          break;
        case 7:
          this.FirstVal = (TextBlock) target;
          this.FirstVal.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStatisticsClick);
          break;
        case 8:
          this.FirstRun1 = (Run) target;
          break;
        case 9:
          this.FirstRun2 = (Run) target;
          break;
        case 10:
          this.FirstRun3 = (Run) target;
          break;
        case 11:
          this.FirstRun4 = (Run) target;
          break;
        case 12:
          this.RightBorder = (Border) target;
          break;
        case 13:
          this.StatisticsTitle2 = (StackPanel) target;
          this.StatisticsTitle2.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnStatisticsTitle2Click);
          break;
        case 14:
          this.Text2 = (TextBlock) target;
          break;
        case 15:
          this.ValBorder2 = (Border) target;
          break;
        case 16:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStatisticsClick);
          break;
        case 17:
          this.SecondRun1 = (Run) target;
          break;
        case 18:
          this.SecondRun2 = (Run) target;
          break;
        case 19:
          this.SecondRun3 = (Run) target;
          break;
        case 20:
          this.SecondRun4 = (Run) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
