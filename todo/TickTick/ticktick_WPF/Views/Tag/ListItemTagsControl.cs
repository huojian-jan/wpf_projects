// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.ListItemTagsControl
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
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class ListItemTagsControl : UserControl, IComponentConnector, IStyleConnector
  {
    private string _mouseDownTag;
    internal ListItemTagsControl Root;
    internal ItemsControl Items;
    private bool _contentLoaded;

    public event EventHandler<string> TagDelete;

    public ListItemTagsControl() => this.InitializeComponent();

    private void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is Grid grid && grid.DataContext is TagViewModel dataContext)
      {
        EventHandler<string> tagDelete = this.TagDelete;
        if (tagDelete != null)
          tagDelete((object) this, dataContext.Tag);
      }
      e.Handled = true;
    }

    private void OnTagClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is Grid grid && grid.DataContext is TagViewModel dataContext && !dataContext.IsMore && this._mouseDownTag == dataContext.Tag)
      {
        ListViewContainer parent = Utils.FindParent<ListViewContainer>((DependencyObject) this);
        if (parent != null)
          parent.SelectTagProject(dataContext.Tag);
        else
          App.SelectTagProject(dataContext.Tag);
      }
      e.Handled = true;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Grid grid) || !(grid.DataContext is TagViewModel dataContext) || dataContext.IsMore)
        return;
      this._mouseDownTag = dataContext.Tag;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/listitemtagscontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 4)
          this.Items = (ItemsControl) target;
        else
          this._contentLoaded = true;
      }
      else
        this.Root = (ListItemTagsControl) target;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
      {
        if (connectionId != 3)
          return;
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteClick);
      }
      else
      {
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTagClick);
        ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
      }
    }
  }
}
