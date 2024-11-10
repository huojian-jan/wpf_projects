// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.MonthCellControl
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
using System.Windows.Shapes;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class MonthCellControl : UserControl, IComponentConnector
  {
    internal Ellipse HoverBackground;
    private bool _contentLoaded;

    public event EventHandler<DateTime> Select;

    public MonthCellControl(MonthViewModel model)
    {
      this.InitializeComponent();
      this.Model = model;
      this.DataContext = (object) this.Model;
    }

    public MonthViewModel Model { get; }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      this.HoverBackground.Visibility = Visibility.Visible;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      this.HoverBackground.Visibility = Visibility.Collapsed;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
      EventHandler<DateTime> select = this.Select;
      if (select == null)
        return;
      select((object) this, this.Model.Date);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/monthcellcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.HoverBackground = (Ellipse) target;
        else
          this._contentLoaded = true;
      }
      else
      {
        ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnMouseEnter);
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClick);
        ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      }
    }
  }
}
