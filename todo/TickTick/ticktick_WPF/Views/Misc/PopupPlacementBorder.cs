// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.PopupPlacementBorder
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

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class PopupPlacementBorder : Button, IComponentConnector
  {
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(nameof (IsDropDownOpen), typeof (bool), typeof (PopupPlacementBorder), new PropertyMetadata((object) false));
    public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(nameof (Radius), typeof (double), typeof (PopupPlacementBorder), new PropertyMetadata((object) 4.0));
    internal PopupPlacementBorder Root;
    private bool _contentLoaded;

    public PopupPlacementBorder()
    {
      this.InitializeComponent();
      this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
    }

    public bool IsDropDownOpen
    {
      get => (bool) this.GetValue(PopupPlacementBorder.IsDropDownOpenProperty);
      set => this.SetValue(PopupPlacementBorder.IsDropDownOpenProperty, (object) value);
    }

    public double Radius
    {
      get => (double) this.GetValue(PopupPlacementBorder.RadiusProperty);
      set => this.SetValue(PopupPlacementBorder.RadiusProperty, (object) value);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/popupplacementborder.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.Root = (PopupPlacementBorder) target;
      else
        this._contentLoaded = true;
    }
  }
}
