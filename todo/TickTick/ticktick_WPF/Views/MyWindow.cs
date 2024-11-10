// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MyWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class MyWindow : Window
  {
    public MyWindow()
    {
      this.Title = Utils.GetAppName();
      Binding binding = new Binding("FontFamily")
      {
        Source = (object) LocalSettings.Settings
      };
      this.SetBinding(Control.FontFamilyProperty, (BindingBase) binding);
      this.UseLayoutRounding = true;
    }
  }
}
