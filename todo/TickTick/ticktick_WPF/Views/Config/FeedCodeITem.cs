// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.FeedCodeITem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class FeedCodeITem : UserControl, IComponentConnector
  {
    internal Grid Container;
    private bool _contentLoaded;

    public FeedCodeITem() => this.InitializeComponent();

    private void OnCopyClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is ProjectFeedViewModel dataContext))
        return;
      try
      {
        Clipboard.SetText(dataContext.Url);
        Utils.FindParent<SettingDialog>((DependencyObject) this)?.Toast(Utils.GetString("Copied"));
      }
      catch (Exception ex)
      {
      }
    }

    private void OnRemoveClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is ProjectFeedViewModel dataContext))
        return;
      Utils.FindParent<SubscribeCalendar>((DependencyObject) this)?.RemoveFeedItem(dataContext);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/feedcodeitem.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCopyClick);
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnRemoveClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
