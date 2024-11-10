// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.AboutInfo
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
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class AboutInfo : UserControl, IComponentConnector
  {
    internal TextBlock VersionTextBlock;
    internal Run CopyRight;
    internal Image LogoImage;
    private bool _contentLoaded;

    public AboutInfo()
    {
      this.InitializeComponent();
      this.InitData();
    }

    private void InitData()
    {
      this.VersionTextBlock.Text = Application.ResourceAssembly.GetName().Version.ToString();
      this.CopyRight.Text = Utils.GetString("Copyright1")?.Replace("2023", DateTime.Today.ToString("yyyy"));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/aboutinfo.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.VersionTextBlock = (TextBlock) target;
          break;
        case 2:
          this.CopyRight = (Run) target;
          break;
        case 3:
          this.LogoImage = (Image) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
