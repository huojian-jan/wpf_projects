// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ConditionNormalItem
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
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class ConditionNormalItem : Grid, IComponentConnector
  {
    internal ConditionNormalItem Root;
    internal Grid OpenIconGrid;
    internal Path OpenIcon;
    private bool _contentLoaded;

    public ConditionNormalItem() => this.InitializeComponent();

    private void OnOpenClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext is FilterListItemViewModel dataContext)
        Utils.FindParent<ConditionEditDialog>((DependencyObject) this).OnOpenClick(dataContext);
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/conditionnormalitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (ConditionNormalItem) target;
          break;
        case 2:
          this.OpenIconGrid = (Grid) target;
          this.OpenIconGrid.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenClick);
          break;
        case 3:
          this.OpenIcon = (Path) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
