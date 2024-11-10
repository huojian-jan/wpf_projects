// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.TTMediaPlayer
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
using WPFMediaKit.DirectShow.Controls;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class TTMediaPlayer : UserControl, IComponentConnector
  {
    private string _url;
    internal Border Container;
    internal MediaUriElement MediaEle;
    private bool _contentLoaded;

    public TTMediaPlayer()
    {
      this.InitializeComponent();
      this.MediaEle.Loop = true;
      this.Cursor = Cursors.Hand;
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseClick);
    }

    private void OnMouseClick(object sender, MouseButtonEventArgs e)
    {
      if (string.IsNullOrEmpty(this._url))
        return;
      Process.Start(this._url);
    }

    public async void PlayVideo(string url)
    {
      if (this._url == url)
        return;
      this._url = url;
      if (string.IsNullOrEmpty(url))
      {
        ((UIElement) this.MediaEle).Visibility = Visibility.Hidden;
        this.MediaEle.Source = (Uri) null;
        this.MediaEle.Stop();
      }
      else
      {
        this.MediaEle.Source = new Uri(url);
        await Task.Delay(300);
        ((UIElement) this.MediaEle).Visibility = Visibility.Visible;
      }
    }

    public bool IsPlaying(string url) => this._url == url;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/ttmediaplayer.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.MediaEle = (MediaUriElement) target;
        else
          this._contentLoaded = true;
      }
      else
        this.Container = (Border) target;
    }
  }
}
