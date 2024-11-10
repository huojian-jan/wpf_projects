// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Update.DownloadProgressBar
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;

#nullable disable
namespace ticktick_WPF.Views.Update
{
  public class DownloadProgressBar : UserControl, IComponentConnector
  {
    private int _progress;
    internal Rectangle Indicator;
    private bool _contentLoaded;

    public DownloadProgressBar() => this.InitializeComponent();

    public int Progress
    {
      get => this._progress;
      set
      {
        this._progress = value;
        this.NotifyProgressChanged(this._progress);
      }
    }

    private void NotifyProgressChanged(int progress)
    {
      this.Indicator.Width = (double) (int) ((double) progress * this.ActualWidth / 100.0);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/update/downloadprogressbar.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.Indicator = (Rectangle) target;
      else
        this._contentLoaded = true;
    }
  }
}
