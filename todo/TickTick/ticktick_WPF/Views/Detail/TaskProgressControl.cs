// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskProgressControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskProgressControl : UserControl, IComponentConnector
  {
    private TaskProgress _progress;
    internal Grid Container;
    private bool _contentLoaded;

    public event TaskProgressDelegate ProgressChanged;

    public TaskProgressControl()
    {
      this.InitializeComponent();
      this.InitViewModel();
    }

    private void InitViewModel()
    {
      this._progress = new TaskProgress()
      {
        Percent = 0,
        ShowPointer = false,
        Width = this.ActualWidth
      };
      this.Container.DataContext = (object) this._progress;
    }

    public async Task SetProgress(int percent, bool withAnim = false)
    {
      if (!withAnim)
      {
        this._progress.Percent = percent;
      }
      else
      {
        int diff = percent - this._progress.Percent;
        if (diff == 0)
          return;
        int step = Math.Abs(diff) / 20 + 1;
        for (int i = 0; i < 100; ++i)
        {
          await Task.Delay(1);
          int num = this._progress.Percent + (diff > 0 ? step : step * -1);
          if (diff > 0)
          {
            if (num >= percent)
            {
              this._progress.Percent = percent;
              break;
            }
            this._progress.Percent = num;
          }
          else if (diff < 0)
          {
            if (num <= percent)
            {
              this._progress.Percent = percent;
              break;
            }
            this._progress.Percent = num;
          }
        }
      }
    }

    public void SetPointerProgress(int percent) => this._progress.PreviewPercent = percent;

    public void ShowPointer() => this._progress.ShowPointer = true;

    public void HidePointer() => this._progress.ShowPointer = false;

    public int GetProgress() => this._progress.Percent;

    public int GetPointerProgress() => this._progress.PreviewPercent;

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this._progress.Width = e.NewSize.Width;
    }

    private async void ProgressPreviewMouseMove(object sender, MouseEventArgs e)
    {
      TaskProgressControl relativeTo = this;
      int num1 = (int) (e.GetPosition((IInputElement) relativeTo).X / relativeTo.ActualWidth * 100.0);
      int previewPercent = -1;
      if ((double) Math.Abs(relativeTo.GetProgress() - num1) < 2.001)
      {
        previewPercent = relativeTo.GetProgress();
      }
      else
      {
        int num2 = num1 % 10;
        if (num2 >= 0 && num2 <= 5)
          previewPercent = num1 - num2;
        else if (num2 >= 6 && num2 <= 9)
          previewPercent = num1 + (10 - num2);
      }
      if (previewPercent != -1)
        await relativeTo.TryShowProgressPointer(previewPercent);
      else
        relativeTo.HidePointer();
    }

    private async Task TryShowProgressPointer(int previewPercent)
    {
      TaskProgressControl taskProgressControl = this;
      taskProgressControl.SetPointerProgress(previewPercent);
      // ISSUE: reference to a compiler-generated method
      DelayActionHandlerCenter.TryDoAction("DetailShowProgressPointer", new EventHandler(taskProgressControl.\u003CTryShowProgressPointer\u003Eb__14_0), 200);
    }

    private void ProgressMouseLeave(object sender, MouseEventArgs e)
    {
      DelayActionHandlerCenter.RemoveAction("DetailShowProgressPointer");
      this.HidePointer();
    }

    private void ProgressClick(object sender, MouseButtonEventArgs e)
    {
      int pointerProgress = this.GetPointerProgress();
      int progress = this.GetProgress();
      TaskProgressDelegate progressChanged = this.ProgressChanged;
      if (progressChanged == null)
        return;
      progressChanged(pointerProgress, progress);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/taskprogresscontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          ((UIElement) target).PreviewMouseMove += new MouseEventHandler(this.ProgressPreviewMouseMove);
          ((UIElement) target).MouseLeave += new MouseEventHandler(this.ProgressMouseLeave);
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.ProgressClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
