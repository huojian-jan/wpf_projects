// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TaskItemPomoSummaryControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class TaskItemPomoSummaryControl : UserControl, IComponentConnector
  {
    internal Path PomoImage;
    internal TextBlock PomoCountText;
    internal Run PomoCountRun;
    internal Run PomoSplitRun;
    internal Run EstimatePomoRun;
    internal Path TimerImage;
    internal TextBlock DurationText;
    internal Run DurationRun;
    internal Run DurationSplitRun;
    internal Run EstimateDurationRun;
    private bool _contentLoaded;

    public TaskItemPomoSummaryControl() => this.InitializeComponent();

    public void SetData(PomodoroSummaryModel pomoSummary)
    {
      if (pomoSummary == null)
      {
        this.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.Visibility = Visibility.Visible;
        (int num, long duration) = pomoSummary.GetFocusSummary();
        bool flag = pomoSummary.estimatedPomo > 0;
        bool showZero = pomoSummary.estimatedPomo <= 0 && pomoSummary.EstimatedDuration > 0L;
        this.PomoCountText.Visibility = num > 0 | flag ? Visibility.Visible : Visibility.Collapsed;
        if (this.PomoCountText.Visibility == Visibility.Visible)
        {
          this.PomoCountRun.Text = num.ToString() ?? "";
          this.PomoSplitRun.Text = pomoSummary.estimatedPomo > 0 ? "/" : "";
          this.EstimatePomoRun.Text = pomoSummary.estimatedPomo > 0 ? pomoSummary.estimatedPomo.ToString() ?? "" : "";
          this.TimerImage.Margin = new Thickness(0.0, 1.0, 2.0, 2.0);
        }
        else
          this.TimerImage.Margin = new Thickness(2.0, 1.0, 2.0, 2.0);
        this.DurationText.Visibility = duration >= 30L | showZero ? Visibility.Visible : Visibility.Collapsed;
        if (this.DurationText.Visibility != Visibility.Visible)
          return;
        this.DurationRun.Text = Utils.GetDurationString(duration, true, showZero) ?? "";
        this.DurationSplitRun.Text = showZero ? "/" : "";
        this.EstimateDurationRun.Text = showZero ? Utils.GetDurationString(pomoSummary.EstimatedDuration, true) : "";
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/taskitempomosummarycontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.PomoImage = (Path) target;
          break;
        case 2:
          this.PomoCountText = (TextBlock) target;
          break;
        case 3:
          this.PomoCountRun = (Run) target;
          break;
        case 4:
          this.PomoSplitRun = (Run) target;
          break;
        case 5:
          this.EstimatePomoRun = (Run) target;
          break;
        case 6:
          this.TimerImage = (Path) target;
          break;
        case 7:
          this.DurationText = (TextBlock) target;
          break;
        case 8:
          this.DurationRun = (Run) target;
          break;
        case 9:
          this.DurationSplitRun = (Run) target;
          break;
        case 10:
          this.EstimateDurationRun = (Run) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
