// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.Item.KanbanDragItem
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
using System.Windows.Markup;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Kanban.Item
{
  public class KanbanDragItem : UserControl, IComponentConnector
  {
    private KanbanItemContent _itemContent;
    private ListItemTagsControl _tagPanel;
    internal Grid ContentGrid;
    internal Border TagBd;
    private bool _contentLoaded;

    public KanbanDragItem() => this.InitializeComponent();

    private async void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      KanbanDragItem kanbanDragItem = this;
      if (!(kanbanDragItem.DataContext is DisplayItemModel dataContext) || dataContext.IsSection)
        return;
      if (kanbanDragItem._itemContent == null)
      {
        kanbanDragItem._itemContent = new KanbanItemContent();
        kanbanDragItem._itemContent.SetValue(Grid.RowProperty, (object) 0);
        kanbanDragItem._itemContent.SetValue(Grid.ColumnProperty, (object) 0);
        kanbanDragItem._itemContent.SetValue(Panel.ZIndexProperty, (object) -1);
        kanbanDragItem._itemContent.SetResourceReference(FrameworkElement.MinHeightProperty, (object) "Height30");
        kanbanDragItem._itemContent.TitleBox.Margin = new Thickness(8.0, 0.0, 0.0, 0.0);
        kanbanDragItem.ContentGrid.Children.Add((UIElement) kanbanDragItem._itemContent);
      }
      await Task.Delay(10);
      if (kanbanDragItem._tagPanel != null)
        return;
      kanbanDragItem._tagPanel = new ListItemTagsControl();
      kanbanDragItem.TagBd.Child = (UIElement) kanbanDragItem._tagPanel;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/kanban/item/kanbandragitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          break;
        case 2:
          this.ContentGrid = (Grid) target;
          break;
        case 3:
          this.TagBd = (Border) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
