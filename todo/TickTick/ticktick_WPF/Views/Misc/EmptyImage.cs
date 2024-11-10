// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.EmptyImage
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
namespace ticktick_WPF.Views.Misc
{
  public class EmptyImage : Viewbox, IComponentConnector
  {
    internal Image Image;
    internal Path Path;
    private bool _contentLoaded;

    public EmptyImage()
    {
      this.InitializeComponent();
      this.Width = 200.0;
      this.Height = 200.0;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/emptyimage.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.Path = (Path) target;
        else
          this._contentLoaded = true;
      }
      else
        this.Image = (Image) target;
    }
  }
}
