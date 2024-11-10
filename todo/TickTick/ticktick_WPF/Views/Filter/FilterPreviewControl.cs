// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.FilterPreviewControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class FilterPreviewControl : UserControl, IComponentConnector
  {
    private bool _contentLoaded;

    public FilterPreviewControl() => this.InitializeComponent();

    private void ClosePreview(object sender, MouseButtonEventArgs e)
    {
    }

    private void SaveFilter(object sender, RoutedEventArgs e)
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/filterpreviewcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          ((ButtonBase) target).Click += new RoutedEventHandler(this.SaveFilter);
        else
          this._contentLoaded = true;
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ClosePreview);
    }
  }
}
