// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.SelectableItem
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

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class SelectableItem : UserControl, IComponentConnector
  {
    internal ContentControl Container;
    internal ContentControl SubPopupPlacement;
    internal Border OpenIconGrid;
    internal Path OpenIcon;
    internal Path ItemIcon;
    internal Grid BatchGrid;
    private bool _contentLoaded;

    public SelectableItem() => this.InitializeComponent();

    private ItemSelection GetParent() => Utils.FindParent<ItemSelection>((DependencyObject) this);

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
    }

    private void OnOpenClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext is TagGroupViewModel || this.DataContext is TeamSectionViewModel || this.DataContext is ListGroupViewModel || this.DataContext is FilterGroupViewModel || this.DataContext is CalendarGroupViewModel)
        return;
      e.Handled = true;
      if (!this.OpenIcon.IsVisible || !(this.DataContext is SelectableItemViewModel dataContext))
        return;
      this.GetParent()?.OnFoldChildren(this, dataContext);
    }

    private void OnCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is SelectableItemViewModel dataContext) || !dataContext.IsSectionGroup)
        return;
      e.Handled = true;
      dataContext.Selected = !dataContext.Selected;
      dataContext.PartSelected = false;
      this.GetParent()?.OnSectionGroupSelected(dataContext);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/selectableitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
          break;
        case 2:
          this.Container = (ContentControl) target;
          break;
        case 3:
          this.SubPopupPlacement = (ContentControl) target;
          break;
        case 4:
          this.OpenIconGrid = (Border) target;
          this.OpenIconGrid.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenClick);
          break;
        case 5:
          this.OpenIcon = (Path) target;
          break;
        case 6:
          this.ItemIcon = (Path) target;
          break;
        case 7:
          this.BatchGrid = (Grid) target;
          break;
        case 8:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
